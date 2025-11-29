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
    /// Get cost adjustment based on emergency severity
    /// More severe emergencies require more resources to handle
    /// Returns flat coin adjustment (additive, not multiplicative)
    /// </summary>
    public static int GetCostAdjustment(EmergencySeverity severity)
    {
        return severity switch
        {
            EmergencySeverity.Minor => 0,     // No adjustment
            EmergencySeverity.Moderate => 5,  // +5 coins
            EmergencySeverity.Urgent => 10,   // +10 coins
            EmergencySeverity.Critical => 20, // +20 coins
            _ => 0
        };
    }

    /// <summary>
    /// Get reward adjustment based on emergency severity
    /// More severe emergencies offer greater rewards
    /// Returns flat coin adjustment (additive, not multiplicative)
    /// </summary>
    public static int GetRewardAdjustment(EmergencySeverity severity)
    {
        return severity switch
        {
            EmergencySeverity.Minor => 0,     // No adjustment
            EmergencySeverity.Moderate => 5,  // +5 coins
            EmergencySeverity.Urgent => 10,   // +10 coins
            EmergencySeverity.Critical => 20, // +20 coins
            _ => 0
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
