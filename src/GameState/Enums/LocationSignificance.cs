/// <summary>
/// Significance level for location lifecycle management.
/// Determines cleanup eligibility based on emergent gameplay patterns.
/// </summary>
public enum LocationSignificance
{
    /// <summary>
    /// Temporary location - never visited, single scene reference
    /// CLEANUP: Remove when scene completes/expires
    /// </summary>
    Temporary,

    /// <summary>
    /// Persistent location - visited by player OR referenced by multiple scenes
    /// CLEANUP: Promote to permanent (orphan from scene)
    /// </summary>
    Persistent,

    /// <summary>
    /// Critical location - authored content or main story locations
    /// CLEANUP: Never cleanup (permanent world content)
    /// </summary>
    Critical
}
