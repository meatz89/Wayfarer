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
}
