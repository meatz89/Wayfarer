/// <summary>
/// Primary functional purpose of location
/// Orthogonal categorical dimension for entity resolution
/// Determines what activities naturally occur at location
/// </summary>
public enum LocationPurpose
{
    /// <summary>
    /// Movement, passage, connection between places
    /// Roads, gates, bridges, hallways, docks, transit hubs
    /// Function: Getting from one place to another
    /// Example: Main road, city gate, ferry crossing, mountain pass
    /// </summary>
    Transit,

    /// <summary>
    /// Living, resting, lodging, residential spaces
    /// Homes, inns, shelters, private quarters, sleeping areas
    /// Function: Rest, recovery, personal life
    /// Example: Inn room, private residence, refugee camp, barracks
    /// </summary>
    Dwelling,

    /// <summary>
    /// Trade, work, services, economic activity
    /// Markets, shops, workshops, offices, guild halls
    /// Function: Commerce, crafting, professional services
    /// Example: Market square, smithy, merchant shop, trade guild
    /// </summary>
    Commerce,

    /// <summary>
    /// Governance, authority, official business, sacred rites
    /// Courts, temples, administrative buildings, guard posts
    /// Function: Law, religion, political power, public service
    /// Example: Town hall, temple, courthouse, barracks, noble manor
    /// </summary>
    Civic
}
