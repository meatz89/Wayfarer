using System.Collections.Generic;

/// <summary>
/// Mental Tactical System Card - represents investigation/puzzle-solving actions
/// Parallel to ConversationCard but for Mental tactical challenges
/// </summary>
public class MentalCard
{
    // Core identity
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }

    // Universal tactical card properties
    public CardType CardType { get; init; } = CardType.Mental;
    public CardTier Tier { get; init; }  // Foundation/Standard/Decisive - determines resource flow
    public int Depth { get; init; }
    public PlayerStatType BoundStat { get; init; }

    // Mental-specific tactical properties
    public int AttentionCost { get; init; } = 0;  // Action economy cost
    public Method Method { get; init; } = Method.Standard;  // Rhythm effect
    public MentalCategory Category { get; init; }  // Card category determines thematic approach and effects

    // Universal card properties (apply across all tactical systems)
    public RiskLevel RiskLevel { get; init; } = RiskLevel.Cautious;
    public Visibility Visibility { get; init; } = Visibility.Moderate;
    public ExertionLevel ExertionLevel { get; init; } = ExertionLevel.Light;
    public MethodType MethodType { get; init; } = MethodType.Direct;

    // Strategic resource costs (calculated at parse time from categorical properties via MentalCardEffectCatalog)
    public int StaminaCost { get; init; } = 0;
    public int DirectHealthCost { get; init; } = 0;
    public int CoinCost { get; init; } = 0;

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
/// Method determines how card affects ObserveActBalance (rhythm)
/// Parallel to Delivery in Conversation system
/// </summary>
public enum Method
{
    Careful,   // Negative Balance shift, safe, slow
    Standard,  // Neutral Balance
    Bold,      // Positive Balance shift, risky, fast
    Reckless   // Extreme positive Balance, very risky
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
