using System;

/// <summary>
/// Catalog of Physical card effect calculations from categorical properties.
/// Parser calls this at load time to calculate card base effects from card properties.
///
/// Parallel to CardEffectCatalog in Social system - converts categorical properties
/// (Discipline, Category, Approach, Stat, Depth) into numeric base effects.
///
/// PERFECT INFORMATION: Returns base values only - bonuses calculated at runtime in resolver.
/// </summary>
public static class PhysicalCardEffectCatalog
{
    /// <summary>
    /// Get base effects from categorical properties.
    /// Called by parser at card load time.
    /// </summary>
    public static PhysicalBaseEffects GetBaseEffectsFromProperties(
        PhysicalDiscipline discipline,
        PhysicalCategory category,
        Approach approach,
        PlayerStatType stat,
        int depth)
    {
        return new PhysicalBaseEffects
        {
            BaseBreakthrough = CalculateBreakthrough(discipline, depth, category),
            BaseDanger = CalculateDanger(approach, depth),
            BasePositionCost = CalculatePositionCost(depth)
        };
    }

    /// <summary>
    /// Calculate base Breakthrough (victory resource) from categorical properties.
    /// Breakthrough scales with depth and modified by category.
    /// </summary>
    private static int CalculateBreakthrough(PhysicalDiscipline discipline, int depth, PhysicalCategory category)
    {
        // Base breakthrough by depth tier
        int baseValue = depth switch
        {
            <= 2 => 2,   // Foundation: 2-3 Breakthrough
            <= 4 => 5,   // Standard: 5-6 Breakthrough
            <= 6 => 9,   // Advanced: 9-10 Breakthrough
            _ => 13      // Master: 13-15 Breakthrough
        };

        // Category modifiers
        int categoryModifier = category switch
        {
            PhysicalCategory.Aggressive => 1,   // High breakthrough, high danger
            PhysicalCategory.Tactical => 0,     // Balanced
            PhysicalCategory.Defensive => -1,   // Low breakthrough, low danger
            PhysicalCategory.Evasive => 0,      // Moderate breakthrough
            PhysicalCategory.Endurance => 0,    // Sustained breakthrough
            _ => 0
        };

        return baseValue + categoryModifier;
    }

    /// <summary>
    /// Calculate base Danger (consequence resource) from categorical properties.
    /// Danger determined primarily by Approach (risk level).
    /// </summary>
    private static int CalculateDanger(Approach approach, int depth)
    {
        int baseDanger = approach switch
        {
            Approach.Methodical => 0,    // Careful, safe
            Approach.Standard => 1,      // Moderate risk
            Approach.Aggressive => 2,    // High risk, high reward
            Approach.Reckless => 3,      // Extreme risk
            _ => 1
        };

        // Higher depth cards have slightly more danger (complexity)
        if (depth >= 5) baseDanger += 1;

        return baseDanger;
    }

    /// <summary>
    /// Calculate base Position cost (builder resource cost) from depth.
    /// Higher depth cards cost more Position to play.
    /// </summary>
    private static int CalculatePositionCost(int depth)
    {
        return depth switch
        {
            <= 2 => 0,   // Foundation: free (generate Position instead via GetPositionGeneration)
            <= 4 => 2,   // Standard: 2 Position
            <= 6 => 4,   // Advanced: 4 Position
            _ => 6       // Master: 6 Position
        };
    }

    /// <summary>
    /// Get base Progress from properties (used by resolver, not parser).
    /// Temporary method for existing resolver compatibility.
    /// </summary>
    public static int GetProgressFromProperties(int depth, PhysicalCategory category)
    {
        // Simple calculation for now - use full calculation once parser updated
        return depth switch
        {
            <= 2 => 2,
            <= 4 => 5,
            <= 6 => 9,
            _ => 13
        };
    }

    /// <summary>
    /// Get base Danger from properties (used by resolver, not parser).
    /// Temporary method for existing resolver compatibility.
    /// </summary>
    public static int GetDangerFromProperties(int depth, Approach approach)
    {
        return CalculateDanger(approach, depth);
    }

    /// <summary>
    /// Get Position cost from depth (called by parser).
    /// </summary>
    public static int GetPositionCostFromDepth(int depth)
    {
        return CalculatePositionCost(depth);
    }

    /// <summary>
    /// Get Stamina cost from categorical properties (called by parser).
    /// </summary>
    public static int GetStaminaCost(Approach approach, int depth, ExertionLevel exertion)
    {
        int baseCost = exertion switch
        {
            ExertionLevel.Light => 0,
            ExertionLevel.Moderate => 2,
            ExertionLevel.Heavy => 5,
            _ => 0
        };

        // Aggressive and Reckless approaches cost more stamina
        if (approach == Approach.Aggressive || approach == Approach.Reckless)
        {
            baseCost += 1;
        }

        return baseCost;
    }

    /// <summary>
    /// Get Health cost from categorical properties (called by parser).
    /// </summary>
    public static int GetHealthCost(Approach approach, RiskLevel risk, int depth)
    {
        int baseCost = risk switch
        {
            RiskLevel.Safe => 0,
            RiskLevel.Cautious => 0,
            RiskLevel.Risky => 2,
            RiskLevel.Dangerous => 5,
            _ => 0
        };

        // Reckless approach increases health cost
        if (approach == Approach.Reckless)
        {
            baseCost += 2;
        }

        return baseCost;
    }

    /// <summary>
    /// Get Coin cost from categorical properties (called by parser).
    /// </summary>
    public static int GetCoinCost(PhysicalCategory category, int depth)
    {
        // Most physical cards don't cost coins
        // Could add specialized equipment cards that require coin investment
        return 0;
    }

    /// <summary>
    /// Get XP reward from depth (called by parser).
    /// XP is ONLY calculated at parse time from depth, NEVER at runtime.
    /// Domain only APPLIES this pre-calculated value.
    /// </summary>
    public static int GetXPReward(int depth)
    {
        // XP = depth (simple, direct progression)
        // Foundation (1-2): 1-2 XP
        // Standard (3-4): 3-4 XP
        // Advanced (5-6): 5-6 XP
        // Master (7-8): 7-8 XP
        return depth;
    }
}

/// <summary>
/// Container for base Physical card effects calculated from categorical properties.
/// These are the BASE values before any bonuses are applied.
/// </summary>
public class PhysicalBaseEffects
{
    public int BaseBreakthrough { get; set; }
    public int BaseDanger { get; set; }
    public int BasePositionCost { get; set; }
}
