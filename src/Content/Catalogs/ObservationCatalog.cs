using System;

/// <summary>
/// Catalog translating categorical observation properties to mechanical values
/// Pattern: Categorical (fiction) → Mechanical (game design)
/// </summary>
public static class ObservationCatalog
{
    /// <summary>
    /// Get focus cost for examination depth
    /// Deeper examination requires more mental effort
    /// </summary>
    public static int GetFocusCostForDepth(ExaminationDepth depth)
    {
        return depth switch
        {
            ExaminationDepth.Glance => 2,      // Quick look, surface details
            ExaminationDepth.Careful => 5,     // Thorough examination
            ExaminationDepth.Exhaustive => 8,  // Expert-level analysis
            ExaminationDepth.Insight => 12,    // Requires high Insight stat
            _ => throw new InvalidOperationException($"Unknown depth: {depth}")
        };
    }

    /// <summary>
    /// Get time cost for examination depth
    /// Deeper examination takes more time
    /// </summary>
    public static int GetTimeCostForDepth(ExaminationDepth depth)
    {
        return depth switch
        {
            ExaminationDepth.Glance => 1,
            ExaminationDepth.Careful => 2,
            ExaminationDepth.Exhaustive => 3,
            ExaminationDepth.Insight => 2,  // High skill = efficient
            _ => 1
        };
    }

    /// <summary>
    /// Get default focus cost when not specified
    /// </summary>
    public static int GetDefaultFocusCost()
    {
        return 3;
    }

    /// <summary>
    /// Get default time cost when not specified
    /// </summary>
    public static int GetDefaultTimeCost()
    {
        return 1;
    }

    /// <summary>
    /// Get item find chance modifier based on examination thoroughness
    /// </summary>
    public static int GetFindChanceModifier(ExaminationDepth depth)
    {
        return depth switch
        {
            ExaminationDepth.Glance => 0,      // No bonus for quick look
            ExaminationDepth.Careful => 10,    // +10% for careful examination
            ExaminationDepth.Exhaustive => 25, // +25% for exhaustive search
            ExaminationDepth.Insight => 35,    // +35% with high insight
            _ => 0
        };
    }
}

/// <summary>
/// Categorical depth levels for examination points
/// </summary>
public enum ExaminationDepth
{
    Glance,      // Quick surface-level look
    Careful,     // Thorough examination
    Exhaustive,  // Expert-level analysis
    Insight      // Requires high Insight skill
}
