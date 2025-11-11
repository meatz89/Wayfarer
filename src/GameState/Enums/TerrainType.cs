/// <summary>
/// Terrain types for hex grid tiles
/// Determines movement cost, transport compatibility, scene spawning, and visual representation
/// </summary>
public enum TerrainType
{
    /// <summary>
    /// Open grassland - fast movement, low danger, common
    /// Movement cost: 1.0x base
    /// Compatible: All transport types
    /// Scene themes: Pastoral, travelers, trade
    /// </summary>
    Plains,

    /// <summary>
    /// Paved or dirt road - fastest movement, safest, connects venues
    /// Movement cost: 0.8x base
    /// Compatible: All transport types
    /// Scene themes: Trade, encounters, law enforcement
    /// </summary>
    Road,

    /// <summary>
    /// Dense woodland - slower movement, moderate danger, ambush risk
    /// Movement cost: 1.5x base
    /// Compatible: Walking, Horseback (limited), Cart (difficult)
    /// Scene themes: Bandits, wildlife, foraging
    /// </summary>
    Forest,

    /// <summary>
    /// Rocky peaks - very slow movement, high danger, climbing required
    /// Movement cost: 2.0x base
    /// Compatible: Walking only
    /// Scene themes: Climbing hazards, isolation, harsh weather
    /// </summary>
    Mountains,

    /// <summary>
    /// Wetlands - very slow movement, disease risk, poor footing
    /// Movement cost: 2.5x base
    /// Compatible: Walking only (careful traversal)
    /// Scene themes: Getting lost, disease, hostile terrain
    /// </summary>
    Swamp,

    /// <summary>
    /// Rivers, lakes, ocean - requires boat transport
    /// Movement cost: 0.9x base (with boat)
    /// Compatible: Boat only
    /// Scene themes: Weather, drowning, piracy
    /// </summary>
    Water,

    /// <summary>
    /// Cliff faces, chasms, deep water - cannot traverse
    /// Movement cost: Infinite (blocks pathing)
    /// Compatible: None
    /// Scene themes: None (impassable)
    /// </summary>
    Impassable
}
