
using System.Linq;

// Conversation session
public class ConversationSession
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public NPC NPC { get; set; }
    public string RequestId { get; set; }
    public string ConversationTypeId { get; set; }
    // Connection State preserved but only for determining starting resources
    public ConnectionState CurrentState { get; set; }
    public ConnectionState InitialState { get; set; }
    public int CurrentMomentum { get; set; } = 0;
    public int CurrentDoubt { get; set; } = 0;
    public int MaxDoubt { get; set; } = 10;
    public int TurnNumber { get; set; }
    public bool LetterGenerated { get; set; }
    public bool RequestCardDrawn { get; set; }
    public int? RequestUrgencyCounter { get; set; }
    public bool RequestCardPlayed { get; set; }
    // HIGHLANDER PRINCIPLE: ONE deck manages ALL card state
    // DO NOT create separate piles - they violate HIGHLANDER
    public SessionCardDeck Deck { get; set; }
    public TokenMechanicsManager TokenManager { get; set; }
    public MomentumManager MomentumManager { get; set; }
    public PersonalityRuleEnforcer PersonalityEnforcer { get; set; }  // Enforces NPC personality rules
    public string RequestText { get; set; } // Text displayed when NPC presents a request

    // New 4-Resource System (Initiative, Cadence, Momentum, Doubt)
    public int CurrentInitiative { get; set; } = 0; // Starts at 0, ACCUMULATES between LISTEN (never resets to base)
    public int Cadence { get; set; } = 0; // Range -5 to +5, conversation balance tracking


    // Doubt system continues to exist but now has tax effect
    public bool PreventNextDoubtIncrease { get; set; } = false;

    // Visible momentum system for deterministic gameplay
    // HiddenMomentum removed - now using visible CurrentMomentum

    public List<CardInstance> ObservationCards { get; set; } = new();

    // NPC-specific observation cards (from NPC's ObservationDeck)
    public List<CardInstance> NPCObservationCards { get; set; } = new();


    // Conversation turn history
    public List<ConversationTurn> TurnHistory { get; set; } = new List<ConversationTurn>();

    // Stranger conversation properties
    public bool IsStrangerConversation { get; set; } = false;
    public int? StrangerLevel { get; set; } // 1-3, affects XP multiplier

    // NO COMPATIBILITY PROPERTIES - update all references immediately!
    // Use Deck.HandCards for read-only access to hand cards
    // Use Deck.HandSize for hand count

    // NEW: Doubt tax calculation for momentum (20% reduction per doubt point)
    public int GetEffectiveMomentumGain(int baseMomentum)
    {
        decimal reduction = CurrentDoubt * 0.20m;
        return (int)(baseMomentum * (1 - reduction));
    }


    public bool CanReachMomentumThreshold(int threshold)
    {
        return CurrentMomentum >= threshold;
    }

    public int GetMomentumNeeded(int threshold)
    {
        return Math.Max(0, threshold - CurrentMomentum);
    }

    public void AddMomentum(int amount)
    {
        CurrentMomentum = Math.Max(0, CurrentMomentum + amount);
    }

    public void AddDoubt(int amount)
    {
        CurrentDoubt = Math.Clamp(CurrentDoubt + amount, 0, MaxDoubt);
    }

    // NEW: Cadence effects (corrected mechanics from game document)
    public bool ShouldApplyCadenceDoubtPenalty() => Cadence > 0;
    public int GetCadenceDoubtPenalty() => Math.Max(0, Cadence); // +1 Doubt per positive point
    public bool ShouldApplyCadenceBonusDraw() => Cadence < 0;
    public int GetCadenceBonusDrawCount() => Math.Abs(Math.Min(0, Cadence)); // +1 draw per negative point



    // NEW: Fixed card draw system (corrected per game document)
    public int GetDrawCount()
    {
        int baseDraw = 3; // Base draw is 3 cards
        int cadenceBonus = GetCadenceBonusDrawCount(); // +1 per negative Cadence point
        return baseDraw + cadenceBonus;
    }

    // NEW: Initiative system methods (replacing Focus methods)
    public int GetCurrentInitiative() => CurrentInitiative;


    // PROPER 4-RESOURCE SYSTEM METHODS
    public bool CanAffordCard(int initiativeCost) => CurrentInitiative >= initiativeCost;


    // NEW: Cadence management methods (corrected per game document)
    public void ApplyCadenceFromSpeak()
    {
        Cadence = Math.Min(5, Cadence + 1); // Player speaking increases cadence (+1, max +5)
    }

    public void ApplyCadenceFromListen()
    {
        Cadence = Math.Max(-5, Cadence - 2); // Listening decreases cadence (-2, min -5)
    }

    // NEW: Doubt reduction method
    public void ReduceDoubt(int amount)
    {
        CurrentDoubt = Math.Max(0, CurrentDoubt - amount);
    }


    // NEW: Initiative management methods
    public bool CanAffordCardInitiative(int initiativeCost)
    {
        return CurrentInitiative >= initiativeCost;
    }

    public bool SpendInitiative(int amount)
    {
        if (amount > CurrentInitiative)
        {
            return false;
        }

        CurrentInitiative -= amount;
        return true;
    }

    public void AddInitiative(int amount)
    {
        CurrentInitiative += amount; // Can accumulate without limit
    }

    // NEW: Conversation time cost per game document formula
    public int GetConversationTimeCost()
    {
        int statementsInSpoken = Deck.SpokenCards.Count(card =>
            card.ConversationCardTemplate?.Persistence == PersistenceType.Statement);
        return 1 + statementsInSpoken; // 1 base segment + Statements in Spoken
    }


    public bool IsHandOverflowing()
    {
        return Deck.HandSize > 10; // Check hand size from deck
    }

    public bool ShouldEnd()
    {
        // End if doubt at maximum
        return CurrentDoubt >= MaxDoubt;
    }

    public ConversationOutcome CheckThresholds()
    {
        if (CurrentDoubt >= MaxDoubt)
        {
            return new ConversationOutcome
            {
                Success = false,
                FinalMomentum = CurrentMomentum,
                TokensEarned = 0,
                Reason = "Doubt overwhelmed conversation"
            };
        }

        // Conversation ended normally without hitting thresholds
        return new ConversationOutcome
        {
            Success = true,
            FinalMomentum = CurrentMomentum,
            TokensEarned = CalculateTokenReward(),
            Reason = "Conversation ended"
        };
    }

    private int CalculateTokenReward()
    {
        // Token rewards now based on momentum achievement or conversation success
        // This will be handled by ConversationFacade logic
        return 1; // Base reward for successful conversation
    }

    public void ExecuteListen(TokenMechanicsManager tokenManager, ObligationQueueManager queueManager, GameWorld gameWorld)
    {
        // Implementation handled by ConversationFacade
    }

    public CardPlayResult ExecuteSpeak(HashSet<CardInstance> selectedCards)
    {
        // Implementation handled by ConversationFacade
        return new CardPlayResult
        {
            MomentumGenerated = 0,
            Results = new List<SingleCardResult>()
        };
    }

    public static ConversationSession StartConversation(NPC npc, ObligationQueueManager queueManager, TokenMechanicsManager tokenManager,
        List<CardInstance> observationCards, string requestId, string conversationTypeId, PlayerResourceState playerResourceState, GameWorld gameWorld, MomentumManager momentumManager)
    {
        // Use properly typed parameters
        List<CardInstance> obsCards = observationCards ?? new List<CardInstance>();
        GameWorld world = gameWorld;

        // Determine initial state
        ConnectionState initialState = ConversationRules.DetermineInitialState(npc, queueManager);

        // Create empty session deck (cards will be added separately)
        SessionCardDeck sessionDeck = SessionCardDeck.CreateFromTemplates(new List<ConversationCard>(), npc.ID);

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
            ConversationTypeId = conversationTypeId,
            CurrentState = initialState,
            InitialState = initialState,
            // Resources initialized to defaults
            CurrentMomentum = 0,
            CurrentDoubt = 0,
            TurnNumber = 0,
            Deck = sessionDeck, // HIGHLANDER: Deck manages ALL piles internally
            TokenManager = tokenManager,
            MomentumManager = momentumManager,
            ObservationCards = obsCards,
            NPCObservationCards = npcObservationCards
        };

        return session;
    }

}
