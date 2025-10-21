using System;

/// <summary>
/// Catalog of Mental card effect calculations from categorical properties.
/// Parser calls this at load time to calculate card base effects from card properties.
///
/// Parallel to CardEffectCatalog in Social system - converts categorical properties
/// (Discipline, Category, Method, Stat, Depth) into numeric base effects.
///
/// PERFECT INFORMATION: Returns base values only - bonuses calculated at runtime in resolver.
/// </summary>
public static class MentalCardEffectCatalog
{
    /// <summary>
    /// Get base effects from categorical properties.
    /// Called by parser at card load time.
    /// </summary>
    public static MentalBaseEffects GetBaseEffectsFromProperties(
        ObligationDiscipline discipline,
        MentalCategory category,
        Method method,
        PlayerStatType stat,
        int depth)
    {
        return new MentalBaseEffects
        {
            BaseProgress = CalculateProgress(discipline, depth, category),
            BaseExposure = CalculateExposure(method, depth),
            BaseAttentionCost = CalculateAttentionCost(depth)
        };
    }

    /// <summary>
    /// Calculate base Progress (victory resource) from categorical properties.
    /// Progress scales with depth and modified by category.
    /// </summary>
    private static int CalculateProgress(ObligationDiscipline discipline, int depth, MentalCategory category)
    {
        // Base progress by depth tier
        int baseValue = depth switch
        {
            <= 2 => 2,   // Foundation: 2-3 Progress
            <= 4 => 5,   // Standard: 5-6 Progress
            <= 6 => 9,   // Advanced: 9-10 Progress
            _ => 13      // Master: 13-15 Progress
        };

        // Category modifiers
        int categoryModifier = category switch
        {
            MentalCategory.Analytical => 1,      // High progress, structured thinking
            MentalCategory.Synthesis => 1,       // High progress, connecting clues
            MentalCategory.Observational => 0,   // Moderate progress, gathering info
            MentalCategory.Physical => -1,       // Lower progress, hands-on obligation
            MentalCategory.Social => 0,          // Moderate progress, questioning
            _ => 0
        };

        return baseValue + categoryModifier;
    }

    /// <summary>
    /// Calculate base Exposure (consequence resource) from categorical properties.
    /// Exposure determined primarily by Method (risk level).
    /// </summary>
    private static int CalculateExposure(Method method, int depth)
    {
        int baseExposure = method switch
        {
            Method.Careful => 0,     // Safe, cautious
            Method.Standard => 1,    // Moderate risk
            Method.Bold => 2,        // High risk, high reward
            Method.Reckless => 3,    // Extreme risk
            _ => 1
        };

        // Higher depth obligations have slightly more exposure (complexity)
        if (depth >= 5) baseExposure += 1;

        return baseExposure;
    }

    /// <summary>
    /// Calculate base Attention cost (builder resource cost) from depth.
    /// Higher depth cards cost more Attention to play.
    /// </summary>
    private static int CalculateAttentionCost(int depth)
    {
        return depth switch
        {
            <= 2 => 0,   // Foundation: free (generate Attention instead via GetAttentionGeneration)
            <= 4 => 2,   // Standard: 2 Attention
            <= 6 => 4,   // Advanced: 4 Attention
            _ => 6       // Master: 6 Attention
        };
    }

    /// <summary>
    /// Get base Progress from properties (used by resolver, not parser).
    /// Temporary method for existing resolver compatibility.
    /// </summary>
    public static int GetProgressFromProperties(int depth, MentalCategory category)
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
    /// Get base Exposure from properties (used by resolver, not parser).
    /// Temporary method for existing resolver compatibility.
    /// </summary>
    public static int GetExposureFromProperties(int depth, Method method)
    {
        return CalculateExposure(method, depth);
    }

    /// <summary>
    /// Get Attention cost from depth (called by parser).
    /// </summary>
    public static int GetAttentionCostFromDepth(int depth)
    {
        return CalculateAttentionCost(depth);
    }

    /// <summary>
    /// Get Coin cost from categorical properties (called by parser).
    /// </summary>
    public static int GetCoinCost(MentalCategory category, int depth)
    {
        // Some obligation cards might require resources (bribes, equipment)
        // For now, most mental cards don't cost coins
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
/// Container for base Mental card effects calculated from categorical properties.
/// These are the BASE values before any bonuses are applied.
/// </summary>
public class MentalBaseEffects
{
    public int BaseProgress { get; set; }
    public int BaseExposure { get; set; }
    public int BaseAttentionCost { get; set; }
}
