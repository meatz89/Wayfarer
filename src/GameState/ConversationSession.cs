
using System.Linq;

// Conversation session
public class ConversationSession
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public NPC NPC { get; set; }
    public ConversationType ConversationType { get; set; }
    public ConnectionState CurrentState { get; set; }
    public ConnectionState InitialState { get; set; }
    public int CurrentPatience { get; set; }
    public int MaxPatience { get; set; }
    public int TurnNumber { get; set; }
    public bool LetterGenerated { get; set; }
    public bool RequestCardDrawn { get; set; }
    public int? RequestUrgencyCounter { get; set; }
    public bool RequestCardPlayed { get; set; }
    // HIGHLANDER PRINCIPLE: ONE deck manages ALL card state
    // DO NOT create separate piles - they violate HIGHLANDER
    public SessionCardDeck Deck { get; set; }
    public TokenMechanicsManager TokenManager { get; set; }
    public FlowManager FlowManager { get; set; }
    public RapportManager RapportManager { get; set; }
    public PersonalityRuleEnforcer PersonalityEnforcer { get; set; }  // Enforces NPC personality rules
    public string RequestText { get; set; } // Text displayed when NPC presents a request

    // New focus and atmosphere system
    public int FlowBattery { get; set; } = 0; // -3 to +3
    public int CurrentFocus { get; set; } = 0; // Current spent focus
    public int MaxFocus { get; set; } = 5; // Based on state
    public AtmosphereType CurrentAtmosphere { get; set; } = AtmosphereType.Neutral;

    // Hidden momentum system - improves luck after failures
    public int HiddenMomentum { get; set; } = 0; // Invisible bad luck protection

    public List<CardInstance> ObservationCards { get; set; } = new();

    // NPC-specific observation cards (from NPC's ObservationDeck)
    public List<CardInstance> NPCObservationCards { get; set; } = new();

    // DELETED: RequestPile - now in Deck.RequestCards
    
    // Conversation turn history
    public List<ConversationTurn> TurnHistory { get; set; } = new List<ConversationTurn>();

    // Stranger conversation properties
    public bool IsStrangerConversation { get; set; } = false;
    public int? StrangerLevel { get; set; } // 1-3, affects XP multiplier

    // NO COMPATIBILITY PROPERTIES - update all references immediately!
    // Use Deck.Hand.Cards for hand cards
    // Use Deck.Hand for the hand pile

    // New helper methods
    public int GetAvailableFocus()
    {
        return Math.Max(0, GetEffectiveFocusCapacity() - CurrentFocus);
    }

    public int GetEffectiveFocusCapacity()
    {
        int baseCapacity = CurrentState switch
        {
            ConnectionState.DISCONNECTED => 3,
            ConnectionState.GUARDED => 4,
            ConnectionState.NEUTRAL => 5,
            ConnectionState.RECEPTIVE => 5,
            ConnectionState.TRUSTING => 6,
            _ => 5
        };

        // Prepared atmosphere adds +1 capacity
        if (CurrentAtmosphere == AtmosphereType.Prepared)
            baseCapacity += 1;

        return baseCapacity;
    }

    public int GetDrawCount()
    {
        // Use configured draw counts from GameRules
        int baseCount = GameRules.StandardRuleset.GetListenDrawCount(CurrentState);

        // AtmosphereType modifiers
        if (CurrentAtmosphere == AtmosphereType.Receptive)
            baseCount += 1;
        else if (CurrentAtmosphere == AtmosphereType.Pressured)
            baseCount = Math.Max(1, baseCount - 1);

        return baseCount;
    }

    public void RefreshFocus()
    {
        CurrentFocus = 0;
        MaxFocus = GetEffectiveFocusCapacity();
    }

    public bool IsHandOverflowing()
    {
        return Deck.Hand.Count > 10; // Check hand size from deck
    }

    public bool ShouldEnd()
    {
        // End if patience exhausted or at disconnected with -3 flow
        return CurrentPatience <= 0 || (CurrentState == ConnectionState.DISCONNECTED && FlowBattery <= -3);
    }

    public ConversationOutcome CheckThresholds()
    {
        if (CurrentPatience <= 0)
        {
            return new ConversationOutcome
            {
                Success = false,
                FinalFlow = FlowBattery,
                FinalState = CurrentState,
                TokensEarned = 0,
                Reason = "Patience exhausted"
            };
        }

        // Conversation ended normally without hitting thresholds
        return new ConversationOutcome
        {
            Success = true,
            FinalFlow = FlowBattery,
            FinalState = CurrentState,
            TokensEarned = CalculateTokenReward(),
            Reason = "Conversation ended"
        };
    }

    private int CalculateTokenReward()
    {
        // FlowBattery is already -3 to +3
        if (FlowBattery >= 3) return 3;
        if (FlowBattery >= 2) return 2;
        if (FlowBattery >= 1) return 1;
        return 0;
    }

    public void ExecuteListen(TokenMechanicsManager tokenManager, ObligationQueueManager queueManager, GameWorld gameWorld)
    {
        // Implementation handled by ConversationOrchestrator
    }

    public CardPlayResult ExecuteSpeak(HashSet<CardInstance> selectedCards)
    {
        // Implementation handled by ConversationOrchestrator
        return new CardPlayResult
        {
            FinalFlow = 0,
            Results = new List<SingleCardResult>()
        };
    }

    public static ConversationSession StartConversation(NPC npc, ObligationQueueManager queueManager, TokenMechanicsManager tokenManager,
        List<CardInstance> observationCards, ConversationType conversationType, PlayerResourceState playerResourceState, GameWorld gameWorld)
    {
        // Use properly typed parameters
        List<CardInstance> obsCards = observationCards ?? new List<CardInstance>();
        ConversationType convType = conversationType;
        GameWorld world = gameWorld;

        // Determine initial state
        ConnectionState initialState = ConversationRules.DetermineInitialState(npc, queueManager);

        // Create session deck from NPC's progression cards (this is legacy - should use CardDeckManager)
        SessionCardDeck sessionDeck = SessionCardDeck.CreateFromTemplates(npc.ProgressionDeck?.GetAllCards() ?? new List<ConversationCard>(), npc.ID);

        // Add observation cards if provided
        foreach (CardInstance obsCard in obsCards)
        {
            sessionDeck.AddCard(obsCard);
        }
        
        // Load NPC observation cards from their ObservationDeck
        List<CardInstance> npcObservationCards = new List<CardInstance>();
        if (npc.ObservationDeck != null && npc.ObservationDeck.Any())
        {
            foreach (ConversationCard obsCard in npc.ObservationDeck.GetAllCards())
            {
                npcObservationCards.Add(new CardInstance(obsCard));
            }
        }

        // Create session with proper initialization
        ConversationSession session = new ConversationSession
        {
            NPC = npc,
            ConversationType = convType,
            CurrentState = initialState,
            InitialState = initialState,
            // FlowBattery initialized separately
            CurrentPatience = 10,
            MaxPatience = 10,
            TurnNumber = 0,
            Deck = sessionDeck, // HIGHLANDER: Deck manages ALL piles internally
            TokenManager = tokenManager,
            ObservationCards = obsCards,
            NPCObservationCards = npcObservationCards
        };

        return session;
    }

}
