
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
    public int MaxInitiative { get; set; } = 10;
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

    // New 5-Resource System (Initiative, Cadence, Momentum, Doubt, Understanding)
    public int CurrentInitiative { get; set; } = 0; // Starts at 0, ACCUMULATES between LISTEN (never resets to base)
    public int Cadence { get; set; } = 0; // Range -10 to +10, conversation balance tracking
    public int CurrentUnderstanding { get; set; } = 0; // NEW: Sophistication/connection depth - unlocks tiers, PERSISTS through LISTEN

    // Depth Tier Unlock System - tiers unlock at UNDERSTANDING thresholds (not momentum) and persist
    public HashSet<int> UnlockedTiers { get; set; } = new HashSet<int> { 1 }; // Tier 1 (depths 1-2) always unlocked

    // Doubt system continues to exist but now has tax effect
    public bool PreventNextDoubtIncrease { get; set; } = false;

    // Statement History Tracking - count of Statement cards played per stat type
    public Dictionary<PlayerStatType, int> StatementCounts { get; set; } = new Dictionary<PlayerStatType, int>
    {
        { PlayerStatType.Insight, 0 },
        { PlayerStatType.Rapport, 0 },
        { PlayerStatType.Authority, 0 },
        { PlayerStatType.Diplomacy, 0 },
        { PlayerStatType.Cunning, 0 }
    };

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

    // DELETED: GetEffectiveMomentumGain() - Doubt Tax system NOT in specification
    // Doubt only matters when it reaches 10 (conversation ends), not as a tax on momentum


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



    // NEW: Linear cadence scaling system - no arbitrary thresholds
    public int GetDrawCount()
    {
        int baseDraw = 3; // Base draw is 3 cards
        int cadenceBonus = Math.Abs(Math.Min(0, Cadence)); // Linear scaling: +1 per negative Cadence point
        return baseDraw + cadenceBonus;
    }

    // NEW: Initiative system methods (replacing Focus methods)
    public int GetCurrentInitiative() => CurrentInitiative;


    // PROPER 4-RESOURCE SYSTEM METHODS
    public bool CanAffordCard(int initiativeCost) => CurrentInitiative >= initiativeCost;


    // NEW: Cadence management methods (corrected per game document)
    public void ApplyCadenceFromSpeak()
    {
        Cadence = Math.Min(10, Cadence + 1); // Player speaking increases cadence (+1, max +10)
    }

    public void ApplyCadenceFromListen()
    {
        Cadence = Math.Max(-10, Cadence - 3); // Listening decreases cadence (-3, min -10)
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
        CurrentInitiative = Math.Clamp(CurrentInitiative + amount, 0, MaxInitiative);
    }

    // NEW: Understanding management methods
    public void AddUnderstanding(int amount)
    {
        CurrentUnderstanding = Math.Max(0, CurrentUnderstanding + amount);
        // Check for tier unlocks whenever Understanding changes
        CheckAndUnlockTiers();
    }

    public int GetCurrentUnderstanding() => CurrentUnderstanding;

    // NEW: Conversation time cost per game document formula
    public int GetConversationTimeCost()
    {
        return 1 + GetTotalStatements(); // 1 base segment + total Statements played
    }

    // TIER UNLOCK SYSTEM METHODS

    /// <summary>
    /// Get the maximum depth accessible based on unlocked tiers
    /// Tier 1 = depths 1-2, Tier 2 = depths 3-4, Tier 3 = depths 5-6, Tier 4 = depths 7-8
    /// </summary>
    public int GetUnlockedMaxDepth()
    {
        return UnlockedTiers.Max() * 2; // Tier number × 2 = max depth for that tier
    }

    /// <summary>
    /// Get the Understanding threshold required to unlock a specific tier
    /// Tier 1 = 0 (always unlocked), Tier 2 = 6, Tier 3 = 12, Tier 4 = 18
    /// SINGLE SOURCE OF TRUTH for tier unlock thresholds
    /// </summary>
    public static int GetTierUnlockThreshold(int tier)
    {
        return tier switch
        {
            1 => 0,   // Tier 1 always unlocked
            2 => 6,   // Tier 2 unlocks at Understanding 6
            3 => 12,  // Tier 3 unlocks at Understanding 12
            4 => 18,  // Tier 4 unlocks at Understanding 18
            _ => 0
        };
    }

    /// <summary>
    /// Check current UNDERSTANDING and unlock tiers at thresholds
    /// Tiers persist once unlocked (never lock again)
    /// CRITICAL: Uses Understanding (not Momentum) for tier unlocking
    /// </summary>
    public void CheckAndUnlockTiers()
    {
        bool tiersChanged = false;

        // Check each tier using the centralized threshold logic
        for (int tier = 2; tier <= 4; tier++)
        {
            int threshold = GetTierUnlockThreshold(tier);
            if (CurrentUnderstanding >= threshold && !UnlockedTiers.Contains(tier))
            {
                UnlockedTiers.Add(tier);
                tiersChanged = true;
                int minDepth = (tier - 1) * 2 + 1;
                int maxDepth = tier * 2;
                Console.WriteLine($"[ConversationSession] TIER {tier} UNLOCKED at understanding {CurrentUnderstanding}! Depths {minDepth}-{maxDepth} now accessible.");
            }
        }

        if (tiersChanged)
        {
            Console.WriteLine($"[ConversationSession] Unlocked tiers: {string.Join(", ", UnlockedTiers.OrderBy(t => t))}. Max accessible depth: {GetUnlockedMaxDepth()}");
        }
    }

    // STATEMENT HISTORY TRACKING METHODS

    /// <summary>
    /// Get the count of Statement cards played for a specific stat type
    /// </summary>
    public int GetStatementCount(PlayerStatType stat)
    {
        return StatementCounts.TryGetValue(stat, out int count) ? count : 0;
    }

    /// <summary>
    /// Get the total number of Statement cards played across all stat types
    /// </summary>
    public int GetTotalStatements()
    {
        return StatementCounts.Values.Sum();
    }

    /// <summary>
    /// Increment the Statement counter for a specific stat type
    /// Called when a Statement card is played
    /// </summary>
    public void IncrementStatementCount(PlayerStatType stat)
    {
        if (StatementCounts.ContainsKey(stat))
        {
            StatementCounts[stat]++;
        }
        else
        {
            StatementCounts[stat] = 1;
        }

        Console.WriteLine($"[ConversationSession] Statement count for {stat}: {StatementCounts[stat]} (Total: {GetTotalStatements()})");
    }


    public bool IsHandOverflowing()
    {
        return Deck.HandSize > 7; // 7-card hand limit for refined system
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
