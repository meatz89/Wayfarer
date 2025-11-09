using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// World hex grid - spatial scaffolding for procedural generation
/// Stores all hexes in world with fast coordinate-based lookup
/// Provides spatial queries (neighbors, distance, regions)
///
/// HIGHLANDER: Single source of truth for hex spatial data
/// GameWorld.HexMap owns all Hex entities
/// </summary>
public class HexMap
{
/// <summary>
/// All hexes in world (flat list for iteration)
/// </summary>
public List<Hex> Hexes { get; set; } = new List<Hex>();

/// <summary>
/// Fast coordinate lookup (O(1) access by Q, R coordinates)
/// Built from Hexes list during initialization
/// </summary>
private Dictionary<(int q, int r), Hex> _hexLookup = new Dictionary<(int q, int r), Hex>();

/// <summary>
/// Map dimensions (for bounds checking and UI rendering)
/// Width = Q axis range
/// Height = R axis range
/// </summary>
public int Width { get; set; }
public int Height { get; set; }

/// <summary>
/// Map origin coordinates (typically 0, 0 for centered maps)
/// </summary>
public AxialCoordinates Origin { get; set; }

/// <summary>
/// Build fast lookup dictionary from Hexes list
/// Must be called after Hexes populated during parsing
/// </summary>
public void BuildLookup()
{
    _hexLookup.Clear();
    foreach (Hex hex in Hexes)
    {
        _hexLookup[(hex.Coordinates.Q, hex.Coordinates.R)] = hex;
    }
}

/// <summary>
/// Get hex at specific coordinates (O(1) lookup)
/// Returns null if coordinates out of bounds or hex doesn't exist
/// </summary>
public Hex GetHex(int q, int r)
{
    _hexLookup.TryGetValue((q, r), out Hex hex);
    return hex;
}

/// <summary>
/// Get hex at specific coordinates (O(1) lookup)
/// </summary>
public Hex GetHex(AxialCoordinates coordinates)
{
    return GetHex(coordinates.Q, coordinates.R);
}

/// <summary>
/// Check if hex exists at coordinates
/// </summary>
public bool HasHex(int q, int r)
{
    return _hexLookup.ContainsKey((q, r));
}

/// <summary>
/// Check if hex exists at coordinates
/// </summary>
public bool HasHex(AxialCoordinates coordinates)
{
    return HasHex(coordinates.Q, coordinates.R);
}

/// <summary>
/// Get all valid neighbors of hex (hexes that exist in grid)
/// Returns 0-6 neighbors (fewer if at map edge)
/// </summary>
public List<Hex> GetNeighbors(Hex hex)
{
    AxialCoordinates[] neighborCoords = hex.Coordinates.GetNeighbors();
    List<Hex> neighbors = new List<Hex>(6);

    foreach (AxialCoordinates coords in neighborCoords)
    {
        Hex neighbor = GetHex(coords);
        if (neighbor != null)
        {
            neighbors.Add(neighbor);
        }
    }

    return neighbors;
}

/// <summary>
/// Get all valid neighbors of coordinates
/// </summary>
public List<Hex> GetNeighbors(AxialCoordinates coordinates)
{
    Hex hex = GetHex(coordinates);
    return hex != null ? GetNeighbors(hex) : new List<Hex>();
}

/// <summary>
/// Get all hexes within radius of center hex (circular region)
/// Includes center hex
/// </summary>
public List<Hex> GetHexesInRadius(Hex center, int radius)
{
    List<Hex> hexesInRadius = new List<Hex>();

    // Iterate over diamond-shaped region containing circle
    for (int q = center.Coordinates.Q - radius; q <= center.Coordinates.Q + radius; q++)
    {
        for (int r = center.Coordinates.R - radius; r <= center.Coordinates.R + radius; r++)
        {
            AxialCoordinates coords = new AxialCoordinates(q, r);

            // Check if within radius using hex distance
            if (coords.DistanceTo(center.Coordinates) <= radius)
            {
                Hex hex = GetHex(coords);
                if (hex != null)
                {
                    hexesInRadius.Add(hex);
                }
            }
        }
    }

    return hexesInRadius;
}

/// <summary>
/// Get all hexes within radius of coordinates
/// </summary>
public List<Hex> GetHexesInRadius(AxialCoordinates coordinates, int radius)
{
    Hex center = GetHex(coordinates);
    return center != null ? GetHexesInRadius(center, radius) : new List<Hex>();
}

/// <summary>
/// Get hex occupied by specific location
/// Returns null if location has no hex position
/// </summary>
public Hex GetHexForLocation(string locationId)
{
    return Hexes.FirstOrDefault(h => h.LocationId == locationId);
}

/// <summary>
/// Get all hexes with locations (occupied hexes)
/// Excludes wilderness hexes
/// </summary>
public List<Hex> GetOccupiedHexes()
{
    return Hexes.Where(h => h.LocationId != null).ToList();
}

/// <summary>
/// Calculate distance between two hexes
/// </summary>
public int GetDistance(Hex hex1, Hex hex2)
{
    return hex1.Coordinates.DistanceTo(hex2.Coordinates);
}

/// <summary>
/// Calculate distance between two coordinates
/// </summary>
public int GetDistance(AxialCoordinates coords1, AxialCoordinates coords2)
{
    return coords1.DistanceTo(coords2);
}

/// <summary>
/// Get all hexes matching terrain type (for scene template filtering)
/// </summary>
public List<Hex> GetHexesByTerrain(TerrainType terrain)
{
    return Hexes.Where(h => h.Terrain == terrain).ToList();
}

/// <summary>
/// Get all hexes within danger level range (for encounter spawning)
/// </summary>
public List<Hex> GetHexesByDangerRange(int minDanger, int maxDanger)
{
    return Hexes.Where(h => h.DangerLevel >= minDanger && h.DangerLevel <= maxDanger).ToList();
}

/// <summary>
/// Get all discovered hexes (for fog of war UI)
/// </summary>
public List<Hex> GetDiscoveredHexes()
{
    return Hexes.Where(h => h.IsDiscovered).ToList();
}

/// <summary>
/// Mark hex as discovered (fog of war progression)
/// </summary>
public void DiscoverHex(AxialCoordinates coordinates)
{
    Hex hex = GetHex(coordinates);
    if (hex != null)
    {
        hex.IsDiscovered = true;
    }
}

/// <summary>
/// Mark all hexes in radius as discovered (scouting mechanics)
/// </summary>
public void DiscoverHexesInRadius(AxialCoordinates center, int radius)
{
    List<Hex> hexes = GetHexesInRadius(center, radius);
    foreach (Hex hex in hexes)
    {
        hex.IsDiscovered = true;
    }
}
}
