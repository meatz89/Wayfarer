/// <summary>
/// Individual hex tile in the world grid
/// Represents a single spatial unit with terrain, danger, and optional venue placement
/// Hexes form the procedural scaffolding for world generation
///
/// HIGHLANDER: Hex stores VenueId (lookup), Venue stores HexPosition (source of truth)
/// </summary>
public class Hex
{
/// <summary>
/// Axial coordinates (Q, R) identifying this hex's position in grid
/// Immutable after creation - hex position never changes
/// </summary>
public AxialCoordinates Coordinates { get; init; }

/// <summary>
/// Terrain type determining movement cost and scene themes
/// Immutable after procedural generation (terrain doesn't change during gameplay)
/// </summary>
public TerrainType Terrain { get; init; }

/// <summary>
/// Danger level (0-10) affecting encounter spawn rate
/// 0 = Safe (towns, roads)
/// 5 = Moderate (forests, hills)
/// 10 = Deadly (monster lairs, war zones)
/// Can change based on world state (completed quests reduce danger)
/// </summary>
public int DangerLevel { get; set; }

/// <summary>
/// Optional location occupying this hex
/// null if hex is wilderness (no location)
/// HIGHLANDER: This is derived lookup, not source of truth
/// Source of truth: Location.HexPosition
/// Locations are THE spatial entities - venues are just abstract wrappers
/// </summary>
public string LocationId { get; set; }

/// <summary>
/// Optional: Hex has been discovered by player (fog of war)
/// false = player hasn't visited or seen this hex yet
/// true = player has traveled through or scouted this hex
/// Affects UI display and available route generation
/// </summary>
public bool IsDiscovered { get; set; }

// Default constructor for entity initialization pattern
public Hex()
{
    DangerLevel = 0;
    IsDiscovered = false;
}
}
