/// <summary>
/// Functional/narrative role of a location in gameplay.
/// This is the primary categorical property for Location entities.
/// Describes what PURPOSE the location serves in the game world.
/// </summary>
public enum LocationRole
{
    Generic,
    Hub,           // Central gathering point, social nexus
    Connective,    // Passage between areas, transitional space
    Landmark,      // Notable/memorable place, navigation reference
    Hazard,        // Dangerous area, environmental threat
    Rest,          // Place for sleeping/recovery, private space
    Other
}
