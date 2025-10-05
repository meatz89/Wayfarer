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

    // ==================== STRATEGIC RESOURCE COSTS ====================
    // Parallel to effect derivation - costs derived from categorical properties
    // NO Tags, NO string matching, NO hardcoding

    /// <summary>
    /// Get stamina cost from categorical properties
    /// ExertionLevel is primary driver, Approach/Depth provide modifiers
    /// PARSER-BASED ARCHITECTURE: Costs calculated once at parse time
    /// </summary>
    public static int GetStaminaCost(Approach approach, int depth, ExertionLevel exertion)
    {
        int baseCost = exertion switch
        {
            ExertionLevel.Minimal => 0,
            ExertionLevel.Light => 1,
            ExertionLevel.Moderate => 2,
            ExertionLevel.Heavy => 3,
            ExertionLevel.Extreme => 5,
            _ => 1
        };

        // Approach modifier - Methodical conserves stamina, Reckless burns it
        int approachModifier = approach switch
        {
            Approach.Methodical => -1,   // Methodical approach reduces exertion
            Approach.Standard => 0,
            Approach.Aggressive => 0,
            Approach.Reckless => 1,      // Reckless burns stamina
            _ => 0
        };

        // Depth scaling - higher complexity requires more stamina
        int depthScaling = depth / 3;

        return Math.Max(0, baseCost + approachModifier + depthScaling);
    }

    /// <summary>
    /// Get health cost from categorical properties
    /// Direct health costs are RARE - most health loss from Danger threshold
    /// Only Reckless + Dangerous combinations cause direct health loss
    /// </summary>
    public static int GetHealthCost(Approach approach, RiskLevel risk, int depth)
    {
        // Reckless physical actions in dangerous situations cause direct health loss
        if (approach == Approach.Reckless && risk == RiskLevel.Dangerous)
        {
            return 1 + (depth / 4); // Scales slowly with depth
        }

        // Very high risk at high depth
        if (risk == RiskLevel.Dangerous && depth >= 6)
        {
            return 1;
        }

        return 0; // Default: no direct health cost
    }

    /// <summary>
    /// Get coin cost from categorical properties
    /// Only specific categories/contexts cost coins (equipment usage, etc.)
    /// Most physical cards cost 0 coins
    /// </summary>
    public static int GetCoinCost(PhysicalCategory category, int depth)
    {
        // Tactical approaches might require special equipment/resources at high depth
        return category switch
        {
            PhysicalCategory.Tactical when depth >= 5 => 1,
            _ => 0
        };
    }
}
