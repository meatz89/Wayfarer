/// <summary>
/// Catalog translating categorical emergency properties to mechanical values
/// Pattern: Categorical (fiction) → Mechanical (game design)
/// </summary>
public static class EmergencyCatalog
{
    /// <summary>
    /// Get response window duration based on emergency severity
    /// More severe emergencies demand quicker response
    /// </summary>
    public static int GetResponseWindowSegments(EmergencySeverity severity)
    {
        return severity switch
        {
            EmergencySeverity.Minor => 4,      // Half a day to respond
            EmergencySeverity.Moderate => 2,   // Quarter day window
            EmergencySeverity.Urgent => 1,     // Must respond within segment
            EmergencySeverity.Critical => 0,   // Immediate response required
            _ => throw new InvalidOperationException($"Unknown severity: {severity}")
        };
    }

    /// <summary>
    /// Get base relationship impact for ignoring emergency
    /// More severe emergencies have worse consequences if ignored
    /// </summary>
    public static int GetIgnoreRelationshipPenalty(EmergencySeverity severity)
    {
        return severity switch
        {
            EmergencySeverity.Minor => -5,     // Small reputation hit
            EmergencySeverity.Moderate => -10, // Notable reputation damage
            EmergencySeverity.Urgent => -20,   // Severe reputation loss
            EmergencySeverity.Critical => -30, // Devastating reputation impact
            _ => -10
        };
    }

    /// <summary>
    /// Get cost scaling factor based on emergency severity
    /// More severe emergencies require more resources to handle
    /// </summary>
    public static double GetCostScalingFactor(EmergencySeverity severity)
    {
        return severity switch
        {
            EmergencySeverity.Minor => 1.0,    // Normal costs
            EmergencySeverity.Moderate => 1.5, // 50% higher costs
            EmergencySeverity.Urgent => 2.0,   // Double costs
            EmergencySeverity.Critical => 3.0, // Triple costs
            _ => 1.0
        };
    }

    /// <summary>
    /// Get reward scaling factor based on emergency severity
    /// More severe emergencies offer greater rewards
    /// </summary>
    public static double GetRewardScalingFactor(EmergencySeverity severity)
    {
        return severity switch
        {
            EmergencySeverity.Minor => 1.0,    // Normal rewards
            EmergencySeverity.Moderate => 1.5, // 50% higher rewards
            EmergencySeverity.Urgent => 2.0,   // Double rewards
            EmergencySeverity.Critical => 3.0, // Triple rewards
            _ => 1.0
        };
    }

    /// <summary>
    /// Get default response window when not specified
    /// </summary>
    public static int GetDefaultResponseWindow()
    {
        return 2; // Moderate urgency by default
    }
}

/// <summary>
/// Categorical severity levels for emergencies
/// </summary>
public enum EmergencySeverity
{
    Minor,     // Low stakes, flexible timing
    Moderate,  // Moderate stakes, reasonable window
    Urgent,    // High stakes, tight timing
    Critical   // Critical stakes, immediate response
}
