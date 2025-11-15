/// <summary>
/// Mental Tactical System Card - represents obligation/puzzle-solving actions
/// Parallel to ConversationCard but for Mental tactical challenges
/// </summary>
public class MentalCard
{
    // Core identity
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }

    // Universal tactical card properties
    public CardTier Tier { get; init; }  // Foundation/Standard/Decisive - determines resource flow
    public int Depth { get; init; }
    public PlayerStatType BoundStat { get; init; }

    // Mental-specific tactical properties
    public int AttentionCost { get; init; } = 0;  // Action economy cost
    public Method Method { get; init; } = Method.Standard;  // Rhythm effect
    public MentalCategory Category { get; init; }  // Card category determines thematic approach and effects
    public ObligationDiscipline Discipline { get; init; } = ObligationDiscipline.Research;  // Specialization for bonus matching

    // Universal card properties (apply across all tactical systems)
    // NOTE: RiskLevel removed - Mental obligations have mental strain (ExertionLevel), not physical risk
    public Visibility Visibility { get; init; } = Visibility.Moderate;
    public ExertionLevel ExertionLevel { get; init; } = ExertionLevel.Light;
    public MethodType MethodType { get; init; } = MethodType.Direct;

    // Strategic resource costs (calculated at parse time from categorical properties via MentalCardEffectCatalog)
    // NOTE: Mental cards have NO health/stamina costs - Mental obligations cost Focus at SESSION level only
    // Individual cards cost ZERO permanent resources (only session resources: Attention, Progress, Exposure)
    public int CoinCost { get; init; } = 0;  // Rare - bribes, equipment purchases
    public int XPReward { get; init; } = 0;  // Pre-calculated XP from depth (parse time, not runtime)

    // Base tactical effects (calculated at parse time from categorical properties via MentalCardEffectCatalog)
    // These are BASE values BEFORE bonuses - resolver adds bonuses at runtime but NEVER recalculates base
    public int BaseProgress { get; init; } = 0;  // Base victory resource gain (before stat bonuses)
    public int BaseExposure { get; init; } = 0;  // Base consequence resource gain (before exertion penalties)

    // Simple requirement properties (NOT objects - parser calculates costs/effects from categorical properties)
    public EquipmentCategory EquipmentCategory { get; init; } = EquipmentCategory.None;
    public Dictionary<PlayerStatType, int> StatThresholds { get; init; } = new Dictionary<PlayerStatType, int>();
    public int MinimumHealth { get; init; } = 0;
    public int MinimumStamina { get; init; } = 0;

    /// <summary>
    /// Get Attention generation for Foundation cards (parallel to Initiative generation)
    /// Foundation cards (Depth 1-2) generate +1 Attention
    /// </summary>
    public int GetAttentionGeneration()
    {
        return Depth <= 2 ? 1 : 0;
    }

    // CanAccessWithStats method deleted - tier-based system not needed for Mental cards (no stat filtering)
}

/// <summary>
/// Method determines how card affects ObserveActBalance (rhythm)
/// Parallel to Delivery in Conversation system
/// </summary>
public enum Method
{
    Careful,   // Safe, slow
    Standard,  // Neutral
    Bold,      // Risky, fast
    Reckless   // Extreme, very risky
}

/// <summary>
/// Action type for Mental tactical system
/// Determines action-based balance shift (combines with Method for total balance change)
/// Observe (draw) = -2 balance, Act (play) = +1 balance
/// </summary>
public enum MentalActionType
{
    Observe,  // Draw action: -2 balance
    Act       // Play action: +1 balance
}
