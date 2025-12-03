/// <summary>
/// Discovery trigger types determine WHEN an obligation becomes discoverable
/// Each type has different prerequisite evaluation logic
/// </summary>
public enum DiscoveryTriggerType
{
    /// <summary>
    /// Obligation visible immediately upon entering location
    /// Prerequisites: Player at required location
    /// Example: Broken waterwheel visible in courtyard
    /// </summary>
    ImmediateVisibility,

    /// <summary>
    /// Obligation revealed through examining Venue features
    /// Prerequisites: Player at required location
    /// Example: Hidden damage noticed after thorough exploration
    /// </summary>
    EnvironmentalObservation
}
