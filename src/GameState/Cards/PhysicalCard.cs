/// <summary>
/// Physical Tactical System Card - represents physical challenge actions
/// Parallel to MentalCard but for Physical tactical challenges
/// </summary>
public class PhysicalCard
{
    // Core identity
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }

    // Universal tactical card properties
    public CardTier Tier { get; init; }  // Foundation/Standard/Decisive - determines resource flow
    public int Depth { get; init; }
    public PlayerStatType BoundStat { get; init; }

    // Physical-specific tactical properties
    public int ExertionCost { get; init; } = 0;  // Action economy cost
    public Approach Approach { get; init; } = Approach.Standard;  // Rhythm effect
    public PhysicalCategory Category { get; init; }  // Card category determines thematic approach and effects
    public PhysicalDiscipline Discipline { get; init; } = PhysicalDiscipline.Combat;  // Specialization for bonus matching

    // Universal card properties (apply across all tactical systems)
    public RiskLevel RiskLevel { get; init; } = RiskLevel.Cautious;
    public Visibility Visibility { get; init; } = Visibility.Moderate;
    public ExertionLevel ExertionLevel { get; init; } = ExertionLevel.Light;
    public MethodType MethodType { get; init; } = MethodType.Direct;

    // Strategic resource costs (calculated at parse time from categorical properties via PhysicalCardEffectCatalog)
    public int StaminaCost { get; init; } = 0;
    public int DirectHealthCost { get; init; } = 0;
    public int CoinCost { get; init; } = 0;
    public int XPReward { get; init; } = 0;  // Pre-calculated XP from depth (parse time, not runtime)

    // Base tactical effects (calculated at parse time from categorical properties via PhysicalCardEffectCatalog)
    // These are BASE values BEFORE bonuses - resolver adds bonuses at runtime but NEVER recalculates base
    public int BaseBreakthrough { get; init; } = 0;  // Base victory resource gain (before stat bonuses)
    public int BaseDanger { get; init; } = 0;  // Base consequence resource gain (before exertion penalties)

    // Simple requirement properties (NOT objects - parser calculates costs/effects from categorical properties)
    public EquipmentCategory EquipmentCategory { get; init; } = EquipmentCategory.None;
    public Dictionary<PlayerStatType, int> StatThresholds { get; init; } = new Dictionary<PlayerStatType, int>();
    public int MinimumHealth { get; init; } = 0;
    public int MinimumStamina { get; init; } = 0;

    /// <summary>
    /// Get Exertion generation for Foundation cards (parallel to Attention generation)
    /// Foundation cards (Depth 1-2) generate +1 Exertion
    /// </summary>
    public int GetExertionGeneration()
    {
        return Depth <= 2 ? 1 : 0;
    }

    /// <summary>
    /// Check if player can access this card with their current stats
    /// </summary>
    public bool CanAccessWithStats(PlayerStats playerStats)
    {
        if (playerStats == null) return true;
        return playerStats.GetLevel(BoundStat) >= Depth;
    }
}

/// <summary>
/// Approach determines how card affects AssessExecuteBalance (rhythm)
/// Parallel to Method in Mental system
/// </summary>
public enum Approach
{
    Methodical,   // Negative Balance shift, safe, slow
    Standard,     // Neutral Balance
    Aggressive,   // Positive Balance shift, risky, fast
    Reckless      // Extreme positive Balance, very risky
}

/// <summary>
/// Action type for Physical tactical system
/// Determines action-based balance shift (combines with Approach for total balance change)
/// Assess (draw) = -2 balance, Execute (play) = +1 balance
/// </summary>
public enum PhysicalActionType
{
    Assess,   // Draw action: -2 balance
    Execute   // Play action: +1 balance
}
