using System.Linq;

// Conversation session
public class SocialSession
{
    public NPC NPC { get; set; }
    public string RequestId { get; set; }
    public string DeckId { get; set; }  // THREE PARALLEL SYSTEMS: Social engagement deck ID
    public int CurrentMomentum { get; set; } = 0;
    public int CurrentDoubt { get; set; } = 0;
    public int MaxDoubt { get; set; } = 10;
    public int MaxInitiative { get; set; } = 10;
    public int TurnNumber { get; set; }
    public bool LetterGenerated { get; set; }
    public bool SituationCardDrawn { get; set; }
    // HIGHLANDER PRINCIPLE: ONE deck manages ALL card state
    // DO NOT create separate piles - they violate HIGHLANDER
    public SocialSessionCardDeck Deck { get; set; }
    public TokenMechanicsManager TokenManager { get; set; }
    public MomentumManager MomentumManager { get; set; }
    public PersonalityRuleEnforcer PersonalityEnforcer { get; set; }  // Enforces NPC personality rules
    public string RequestText { get; set; } // Text displayed when NPC presents a request

    // New 5-Resource System (Initiative, Cadence, Momentum, Doubt, Understanding)
    public int CurrentInitiative { get; set; } = 0; // Starts at 0, ACCUMULATES between LISTEN (never resets to base)
    public int Cadence { get; set; } = 0; // Range -10 to +10, conversation balance tracking
    public int CurrentUnderstanding { get; set; } = 0; // NEW: Sophistication/connection depth - unlocks tiers, PERSISTS through LISTEN

    // Depth Tier Unlock System - tiers unlock at UNDERSTANDING thresholds (not momentum) and persist
    public List<int> UnlockedTiers { get; set; } = new List<int> { 1 }; // Tier 1 (depths 1-2) always unlocked

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

    // Conversation turn history
    public List<SocialTurn> TurnHistory { get; set; } = new List<SocialTurn>();

    // Stranger conversation properties
    public bool IsStrangerConversation { get; set; } = false;
    public int? StrangerLevel { get; set; } // 1-3, affects XP multiplier
    public ConnectionState CurrentState = ConnectionState.NEUTRAL;
    public ConnectionState InitialState = ConnectionState.NEUTRAL;

    // NO COMPATIBILITY PROPERTIES - update all references immediately!
    public void AddDoubt(int amount)
    {
        CurrentDoubt = Math.Clamp(CurrentDoubt + amount, 0, MaxDoubt);
    }

    // NEW: Cadence effects (corrected mechanics from game document)
    public bool ShouldApplyCadenceDoubtPenalty()
    {
        return Cadence > 0;
    }

    public int GetCadenceDoubtPenalty()
    {
        return Math.Max(0, Cadence); // +1 Doubt per positive point
    }

    public bool ShouldApplyCadenceBonusDraw()
    {
        return Cadence < 0;
    }

    public int GetCadenceBonusDrawCount()
    {
        return Math.Abs(Math.Min(0, Cadence)); // +1 draw per negative point
    }

    // NEW: Linear cadence scaling system - no arbitrary thresholds
    public int GetDrawCount()
    {
        int baseDraw = 3; // Base draw is 3 cards
        int cadenceBonus = Math.Abs(Math.Min(0, Cadence)); // Linear scaling: +1 per negative Cadence point
        return baseDraw + cadenceBonus;
    }

    /// <summary>
    /// Get the cadence effect of LISTEN action for UI display
    /// Single source of truth for LISTEN cadence mechanics
    /// </summary>
    public int GetListenCadenceEffect()
    {
        return -1; // LISTEN reduces cadence by 1
    }

    // NEW: Initiative system methods (replacing Focus methods)
    public int GetCurrentInitiative()
    {
        return CurrentInitiative;
    }

    // PROPER 4-RESOURCE SYSTEM METHODS
    public bool CanAffordCard(int initiativeCost)
    {
        return CurrentInitiative >= initiativeCost;
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
    /// Delegates to ConversationTier which is the SINGLE SOURCE OF TRUTH
    /// </summary>
    public static int GetTierUnlockThreshold(int tier)
    {
        return SocialTier.GetUnlockThreshold(tier);
    }

    /// <summary>
    /// Check current UNDERSTANDING and unlock tiers at thresholds
    /// Tiers persist once unlocked (never lock again)
    /// CRITICAL: Uses Understanding (not Momentum) for tier unlocking
    /// </summary>
    public void CheckAndUnlockTiers()
    {
        bool tiersChanged = false;

        // Check each tier (skip Tier 1 as it's always unlocked)
        foreach (SocialTier tier in SocialTier.AllTiers.Skip(1))
        {
            if (CurrentUnderstanding >= tier.UnderstandingThreshold && !UnlockedTiers.Contains(tier.TierNumber))
            {
                if (!UnlockedTiers.Contains(tier.TierNumber))
                    UnlockedTiers.Add(tier.TierNumber);
                tiersChanged = true;
            }
        }

        if (tiersChanged)
        { }
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
    }

    public bool ShouldEnd()
    {
        // End if doubt at maximum
        return CurrentDoubt >= MaxDoubt;
    }

}
