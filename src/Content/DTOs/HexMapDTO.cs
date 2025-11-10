/// <summary>
/// Data Transfer Object for deserializing hex map world grid from JSON.
/// Contains all hexes in the world with map dimensions and origin.
/// Part of hex-based travel system spatial scaffolding.
/// Maps to the structure in hex_grid.json (root-level content file).
/// </summary>
public class HexMapDTO
{
/// <summary>
/// All hexes in the world grid
/// Each hex has coordinates, terrain, danger level, and optional location
/// </summary>
public List<HexDTO> hexes { get; set; } = new List<HexDTO>();

/// <summary>
/// Map width (Q axis range) for bounds checking and UI rendering
/// </summary>
public int width { get; set; }

/// <summary>
/// Map height (R axis range) for bounds checking and UI rendering
/// </summary>
public int height { get; set; }

/// <summary>
/// Origin Q coordinate (typically 0 for centered maps)
/// </summary>
public int originQ { get; set; }

/// <summary>
/// Origin R coordinate (typically 0 for centered maps)
/// </summary>
public int originR { get; set; }
}
