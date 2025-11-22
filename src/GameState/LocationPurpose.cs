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
    Civic,

    /// <summary>
    /// Defense, security, military installations
    /// Fortresses, guard posts, barracks, watch towers, defensive structures
    /// Function: Protection, combat, law enforcement, military operations
    /// Example: City fortress, guard station, military barracks, watchtower
    /// </summary>
    Defense,

    /// <summary>
    /// Administrative governance and bureaucracy
    /// Official buildings, records halls, tax offices, administrative centers
    /// Function: Governance, paperwork, official records, bureaucratic processes
    /// Example: Administrative building, records office, tax bureau, customs house
    /// </summary>
    Governance,

    /// <summary>
    /// Religious worship and spiritual services
    /// Temples, shrines, chapels, sacred spaces, religious institutions
    /// Function: Prayer, rituals, spiritual guidance, religious ceremonies
    /// Example: Temple, shrine, chapel, monastery, sacred grove
    /// </summary>
    Worship,

    /// <summary>
    /// Education, research, knowledge preservation
    /// Schools, academies, libraries, research institutions
    /// Function: Teaching, learning, scholarship, knowledge sharing
    /// Example: Academy, library, school, research hall, scriptorium
    /// </summary>
    Learning,

    /// <summary>
    /// Entertainment, recreation, cultural performances
    /// Theaters, arenas, performance halls, recreational venues
    /// Function: Entertainment, games, performances, cultural events
    /// Example: Theater, arena, performance hall, festival ground
    /// </summary>
    Entertainment,

    /// <summary>
    /// Generic catch-all purpose - matches any venue type
    /// Flexible locations without specific categorical purpose
    /// Function: Fallback for uncategorized locations, wildcard matching
    /// Example: Generic location, undefined purpose, temporary space
    /// </summary>
    Generic
}
