
// Conversation session
public class ConversationSession
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public NPC NPC { get; set; }
    public ConversationType ConversationType { get; set; }
    public EmotionalState CurrentState { get; set; }
    public EmotionalState InitialState { get; set; }
    public int CurrentFlow { get; set; }
    public int CurrentPatience { get; set; }
    public int MaxPatience { get; set; }
    public int TurnNumber { get; set; }
    public bool LetterGenerated { get; set; }
    public bool RequestCardDrawn { get; set; }
    public int? RequestUrgencyCounter { get; set; }
    public bool RequestCardPlayed { get; set; }
    public SessionCardDeck Deck { get; set; }
    public HandDeck Hand { get; set; }
    public HashSet<CardInstance> HandCards
    {
        get
        {
            if (Hand?.Cards != null) return Hand.Cards;
            return new HashSet<CardInstance>();
        }
    }
    public List<CardInstance> PlayedCards { get; set; } = new();
    public List<CardInstance> DiscardedCards { get; set; } = new();
    public TokenMechanicsManager TokenManager { get; set; }
    public FlowManager FlowManager { get; set; }
    public RapportManager RapportManager { get; set; }

    // New presence and atmosphere system
    public int FlowBattery { get; set; } = 0; // -3 to +3
    public int CurrentPresence { get; set; } = 0; // Current spent presence
    public int MaxPresence { get; set; } = 5; // Based on state
    public AtmosphereType CurrentAtmosphere { get; set; } = AtmosphereType.Neutral;

    public List<CardInstance> ObservationCards { get; set; } = new();

    // New helper methods
    public int GetAvailablePresence()
    {
        return Math.Max(0, GetEffectivePresenceCapacity() - CurrentPresence);
    }

    public int GetEffectivePresenceCapacity()
    {
        int baseCapacity = CurrentState switch
        {
            EmotionalState.DESPERATE => 3,
            EmotionalState.TENSE => 4,
            EmotionalState.NEUTRAL => 5,
            EmotionalState.OPEN => 5,
            EmotionalState.CONNECTED => 6,
            _ => 5
        };

        // Prepared atmosphere adds +1 capacity
        if (CurrentAtmosphere == AtmosphereType.Prepared)
            baseCapacity += 1;

        return baseCapacity;
    }

    public int GetDrawCount()
    {
        int baseCount = CurrentState switch
        {
            EmotionalState.DESPERATE => 1,
            EmotionalState.TENSE => 2,
            EmotionalState.NEUTRAL => 2,
            EmotionalState.OPEN => 3,
            EmotionalState.CONNECTED => 3,
            _ => 2
        };

        // AtmosphereType modifiers
        if (CurrentAtmosphere == AtmosphereType.Receptive)
            baseCount += 1;
        else if (CurrentAtmosphere == AtmosphereType.Pressured)
            baseCount = Math.Max(1, baseCount - 1);

        return baseCount;
    }

    public void RefreshPresence()
    {
        CurrentPresence = 0;
        MaxPresence = GetEffectivePresenceCapacity();
    }

    public bool IsHandOverflowing()
    {
        return HandCards.Count > 10; // Simplified overflow check
    }

    public bool ShouldEnd()
    {
        // End if patience exhausted or at desperate with -3 flow
        return CurrentPatience <= 0 || (CurrentState == EmotionalState.DESPERATE && FlowBattery <= -3);
    }

    public ConversationOutcome CheckThresholds()
    {
        if (CurrentFlow >= 100)
        {
            return new ConversationOutcome
            {
                Success = true,
                FinalFlow = CurrentFlow,
                FinalState = CurrentState,
                TokensEarned = CalculateTokenReward(),
                Reason = "Flow threshold reached"
            };
        }

        if (CurrentPatience <= 0)
        {
            return new ConversationOutcome
            {
                Success = false,
                FinalFlow = CurrentFlow,
                FinalState = CurrentState,
                TokensEarned = 0,
                Reason = "Patience exhausted"
            };
        }

        // Conversation ended normally without hitting thresholds
        return new ConversationOutcome
        {
            Success = true,
            FinalFlow = CurrentFlow,
            FinalState = CurrentState,
            TokensEarned = CalculateTokenReward(),
            Reason = "Conversation ended"
        };
    }

    private int CalculateTokenReward()
    {
        if (CurrentFlow >= 100) return 3;
        if (CurrentFlow >= 75) return 2;
        if (CurrentFlow >= 50) return 1;
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
            TotalFlow = 0,
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
        EmotionalState initialState = ConversationRules.DetermineInitialState(npc, queueManager);

        // Create session deck from NPC's conversation cards
        SessionCardDeck sessionDeck = SessionCardDeck.CreateFromTemplates(npc.ConversationDeck?.GetAllCards() ?? new List<ConversationCard>(), npc.ID);

        // Add observation cards if provided
        foreach (CardInstance obsCard in obsCards)
        {
            sessionDeck.AddCard(obsCard);
        }

        // Create session with proper initialization
        ConversationSession session = new ConversationSession
        {
            NPC = npc,
            ConversationType = convType,
            CurrentState = initialState,
            InitialState = initialState,
            CurrentFlow = 0,
            CurrentPatience = 10,
            MaxPatience = 10,
            TurnNumber = 0,
            Deck = sessionDeck,
            Hand = new HandDeck(),
            TokenManager = tokenManager,
            ObservationCards = obsCards
        };

        return session;
    }

    public static ConversationSession StartExchange(NPC npc, PlayerResourceState playerResourceState, TokenMechanicsManager tokenManager,
        List<string> spotDomainTags, ObligationQueueManager queueManager, GameWorld gameWorld)
    {
        // Create session deck from NPC's exchange cards
        List<ConversationCard> exchangeCards = npc.ExchangeDeck?.GetAllCards() ?? new List<ConversationCard>();
        SessionCardDeck sessionDeck = SessionCardDeck.CreateFromTemplates(exchangeCards, npc.ID);

        // Determine initial state  
        EmotionalState initialState = ConversationRules.DetermineInitialState(npc, queueManager);

        ConversationSession session = new ConversationSession
        {
            NPC = npc,
            ConversationType = ConversationType.Commerce,
            CurrentState = initialState,
            InitialState = initialState,
            CurrentFlow = 0,
            CurrentPatience = 10,
            MaxPatience = 10,
            TurnNumber = 0,
            Deck = sessionDeck,
            Hand = new HandDeck(),
            TokenManager = tokenManager
        };

        return session;
    }
}
