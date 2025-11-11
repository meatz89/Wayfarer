/// <summary>
/// Transport types for hex grid traversal
/// Determines which terrain can be crossed and movement speed modifiers
/// Pathfinding respects transport compatibility with terrain
/// </summary>
public enum TransportType
{
    /// <summary>
    /// On foot - universal but slow
    /// Terrain compatibility: All except Water, Impassable
    /// Speed modifier: 1.0x base
    /// Resource costs: High stamina drain
    /// Use case: Default travel, mountainous regions
    /// </summary>
    Walking,

    /// <summary>
    /// Horse-drawn cart - faster on roads, struggles offroad
    /// Terrain compatibility: Road, Plains, Forest (limited)
    /// Speed modifier: 1.3x on Road/Plains, 0.7x on Forest
    /// Resource costs: Moderate stamina, coin upkeep
    /// Use case: Trade routes, flat terrain
    /// </summary>
    Cart,

    /// <summary>
    /// Riding horse - fast and versatile
    /// Terrain compatibility: Road, Plains, Forest
    /// Speed modifier: 1.5x on Road/Plains, 1.0x on Forest
    /// Resource costs: High coin upkeep, moderate stamina
    /// Use case: Fast travel, varied terrain
    /// </summary>
    Horseback,

    /// <summary>
    /// Watercraft - fast on water, useless on land
    /// Terrain compatibility: Water only
    /// Speed modifier: 1.4x on Water
    /// Resource costs: Low stamina (sailing), moderate coin
    /// Use case: River/coastal routes
    /// </summary>
    Boat
}
