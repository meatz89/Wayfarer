using System;

/// <summary>
/// Physical Card Effect Catalog - Automatic effect derivation from categorical properties
/// Parallel to CardEffectCatalog for Conversation system
/// Reduces manual JSON specification by deriving costs/effects from Type + Depth + TechniqueType
/// </summary>
public static class PhysicalCardEffectCatalog
{
    /// <summary>
    /// Get automatic PositionCost based on card depth
    /// Foundation (1-2): Cost 0 (generates Position)
    /// Standard (3-4): Cost 2
    /// Advanced (5-6): Cost 4
    /// Master (7+): Cost 6
    /// </summary>
    public static int GetPositionCostFromDepth(int depth)
    {
        return depth switch
        {
            1 or 2 => 0,  // Foundation cards generate, don't cost
            3 or 4 => 2,  // Standard
            5 or 6 => 4,  // Advanced
            _ => 6        // Master
        };
    }

    /// <summary>
    /// Get automatic Breakthrough effect based on depth and physical category
    /// Category determines approach and breakthrough generation
    /// </summary>
    public static int GetProgressFromProperties(int depth, PhysicalCategory category)
    {
        int baseProgress = depth switch
        {
            1 or 2 => 0,  // Foundation cards don't generate progress
            3 or 4 => 3,  // Standard
            5 or 6 => 6,  // Advanced
            _ => 10       // Master
        };

        // Category modifier
        float modifier = category switch
        {
            PhysicalCategory.Aggressive => 1.5f,   // Offensive approaches find breakthroughs
            PhysicalCategory.Tactical => 1.2f,      // Strategic positioning helps
            PhysicalCategory.Defensive => 0.7f,     // Protective, less direct progress
            PhysicalCategory.Evasive => 0.6f,       // Avoidance, least direct
            PhysicalCategory.Endurance => 0.9f,     // Persistent, steady progress
            _ => 1.0f
        };

        return (int)(baseProgress * modifier);
    }

    /// <summary>
    /// Get automatic Danger risk based on depth and approach
    /// Reckless approaches increase danger
    /// Methodical approaches reduce it
    /// </summary>
    public static int GetDangerFromProperties(int depth, Approach approach)
    {
        int baseDanger = depth switch
        {
            1 or 2 => 0,  // Foundation safe
            3 or 4 => 1,  // Standard slight risk
            5 or 6 => 2,  // Advanced moderate risk
            _ => 3        // Master high risk
        };

        // Approach modifier
        int modifier = approach switch
        {
            Approach.Methodical => -1,   // Reduces risk
            Approach.Standard => 0,       // No change
            Approach.Aggressive => +1,    // Increases risk
            Approach.Reckless => +2,      // Major risk increase
            _ => 0
        };

        return Math.Max(0, baseDanger + modifier);
    }
}
