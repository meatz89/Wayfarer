using System;

/// <summary>
/// Mental Card Effect Catalog - Automatic effect derivation from categorical properties
/// Parallel to CardEffectCatalog for Conversation system
/// Reduces manual JSON specification by deriving costs/effects from Type + Depth + ClueType
/// </summary>
public static class MentalCardEffectCatalog
{
    /// <summary>
    /// Get automatic AttentionCost based on card depth
    /// Foundation (1-2): Cost 0 (generates Attention)
    /// Standard (3-4): Cost 2
    /// Advanced (5-6): Cost 4
    /// Master (7+): Cost 6
    /// </summary>
    public static int GetAttentionCostFromDepth(int depth)
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
    /// Get automatic Progress effect based on depth and mental category
    /// Category determines approach and progress generation
    /// </summary>
    public static int GetProgressFromProperties(int depth, MentalCategory category)
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
            MentalCategory.Analytical => 1.5f,      // Systematic analysis finds more progress
            MentalCategory.Physical => 1.0f,         // Standard hands-on examination
            MentalCategory.Observational => 0.8f,    // Perception-based, less direct progress
            MentalCategory.Social => 0.7f,           // Interpersonal, indirect progress
            MentalCategory.Synthesis => 1.2f,        // Pattern recognition, good progress
            _ => 1.0f
        };

        return (int)(baseProgress * modifier);
    }

    /// <summary>
    /// Get automatic Exposure risk based on depth and method
    /// Reckless methods increase exposure risk
    /// Careful methods reduce it
    /// </summary>
    public static int GetExposureFromProperties(int depth, Method method)
    {
        int baseExposure = depth switch
        {
            1 or 2 => 0,  // Foundation safe
            3 or 4 => 1,  // Standard slight risk
            5 or 6 => 2,  // Advanced moderate risk
            _ => 3        // Master high risk
        };

        // Method modifier
        int modifier = method switch
        {
            Method.Careful => -1,    // Reduces risk
            Method.Standard => 0,    // No change
            Method.Bold => +1,       // Increases risk
            Method.Reckless => +2,   // Major risk increase
            _ => 0
        };

        return Math.Max(0, baseExposure + modifier);
    }

    // ==================== STRATEGIC RESOURCE COSTS ====================
    // Parallel to effect derivation - costs derived from categorical properties
    // NO Tags, NO string matching, NO hardcoding

    /// <summary>
    /// Get stamina cost from categorical properties
    /// ExertionLevel is primary driver, Method/Depth provide modifiers
    /// PARSER-BASED ARCHITECTURE: Costs calculated once at parse time
    /// </summary>
    public static int GetStaminaCost(Method method, int depth, ExertionLevel exertion)
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

        // Method modifier - Careful conserves stamina, Reckless burns it
        int methodModifier = method switch
        {
            Method.Careful => -1,    // Careful approach reduces exertion
            Method.Standard => 0,
            Method.Bold => 0,
            Method.Reckless => 1,    // Reckless burns stamina
            _ => 0
        };

        // Depth scaling - higher complexity requires more stamina
        int depthScaling = depth / 3;

        return Math.Max(0, baseCost + methodModifier + depthScaling);
    }

    /// <summary>
    /// Get health cost from categorical properties
    /// Direct health costs are RARE - most health loss from Exposure threshold
    /// Only Reckless + Dangerous combinations cause direct health loss
    /// </summary>
    public static int GetHealthCost(Method method, RiskLevel risk, int depth)
    {
        // Reckless investigation of dangerous situations causes direct health loss
        if (method == Method.Reckless && risk == RiskLevel.Dangerous)
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
    /// Only specific categories/contexts cost coins (bribes, payments, etc.)
    /// Most mental cards cost 0 coins
    /// </summary>
    public static int GetCoinCost(MentalCategory category, int depth)
    {
        // Social investigations might require bribes/payments at high depth
        return category switch
        {
            MentalCategory.Social when depth >= 5 => 1,
            _ => 0
        };
    }
}
