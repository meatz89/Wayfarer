/// <summary>
/// Data Transfer Object for deserializing individual hex tile data from JSON.
/// Maps hex position, terrain, danger level, and optional location placement.
/// Part of hex-based travel system spatial scaffolding.
/// </summary>
public class HexDTO
{
/// <summary>
/// Q coordinate (column) in axial coordinate system
/// </summary>
public int q { get; set; }

/// <summary>
/// R coordinate (row) in axial coordinate system
/// </summary>
public int r { get; set; }

/// <summary>
/// Terrain type as string (e.g., "Plains", "Forest", "Mountains")
/// Parsed to TerrainType enum by HexParser
/// </summary>
public string terrain { get; set; }

/// <summary>
/// Danger level (0-10) affecting encounter spawn rates
/// 0 = Safe (towns, roads)
/// 5 = Moderate (forests, hills)
/// 10 = Deadly (monster lairs, war zones)
/// </summary>
public int dangerLevel { get; set; }

/// <summary>
/// Optional location ID occupying this hex
/// Null if hex is wilderness (no location)
/// </summary>
public string locationId { get; set; }

/// <summary>
/// Whether this hex has been discovered by player (fog of war)
/// Defaults to false
/// </summary>
public bool isDiscovered { get; set; }
}
