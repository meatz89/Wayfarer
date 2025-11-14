/// <summary>
/// Population density and activity level of location
/// Orthogonal categorical dimension for entity resolution
/// Determines NPC availability and social dynamics
/// </summary>
public enum LocationActivity
{
    /// <summary>
    /// Isolated, peaceful, few people present
    /// Minimal social interaction, contemplative atmosphere
    /// Example: Remote shrine, abandoned building, secluded grove, private study
    /// </summary>
    Quiet,

    /// <summary>
    /// Normal traffic and activity for location type
    /// Typical social density, standard interaction opportunities
    /// Example: Residential street, small tavern, workshop, library
    /// </summary>
    Moderate,

    /// <summary>
    /// Crowded, active, many people present
    /// High social density, bustling atmosphere, constant activity
    /// Example: Market at peak hours, festival square, busy port, popular tavern
    /// </summary>
    Busy
}
