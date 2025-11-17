using System;
using System.Collections.Generic;

/// <summary>
/// Coordinate pair for hex validation - replaces tuple
/// </summary>
internal struct CoordinatePair : IEquatable<CoordinatePair>
{
    public int Q { get; }
    public int R { get; }

    public CoordinatePair(int q, int r)
    {
        Q = q;
        R = r;
    }

    public bool Equals(CoordinatePair other)
    {
        return Q == other.Q && R == other.R;
    }

    public override bool Equals(object obj)
    {
        return obj is CoordinatePair pair && Equals(pair);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Q, R);
    }
}

/// <summary>
/// Parses hex map data from JSON DTOs to domain entities.
/// Validates hex coordinates, terrain types, and optional location references.
/// Part of hex-based travel system spatial scaffolding.
/// </summary>
public static class HexParser
{
    /// <summary>
    /// Parse HexMapDTO to HexMap domain entity
    /// Validates all hexes, builds coordinate lookup, validates terrain types
    /// </summary>
    public static HexMap ParseHexMap(HexMapDTO dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "HexMapDTO cannot be null");

        if (dto.hexes == null || dto.hexes.Count == 0)
            throw new InvalidDataException("HexMap must contain at least one hex");

        HexMap hexMap = new HexMap
        {
            Width = dto.width,
            Height = dto.height,
            Origin = new AxialCoordinates(dto.originQ, dto.originR)
        };

        // Parse each hex
        foreach (HexDTO hexDto in dto.hexes)
        {
            Hex hex = ParseHex(hexDto);
            hexMap.Hexes.Add(hex);
        }

        // Validate hex grid integrity before building lookup
        ValidateDuplicateCoordinates(hexMap);
        ValidateDuplicateLocationIds(hexMap);

        // Build coordinate lookup dictionary
        hexMap.BuildLookup();

        return hexMap;
    }

    /// <summary>
    /// Parse individual HexDTO to Hex entity
    /// Validates coordinates, terrain type, and danger level
    /// </summary>
    private static Hex ParseHex(HexDTO dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "HexDTO cannot be null");

        if (string.IsNullOrEmpty(dto.terrain))
            throw new InvalidDataException($"Hex at ({dto.q}, {dto.r}) missing required 'terrain' field");

        // Parse terrain type
        if (!Enum.TryParse<TerrainType>(dto.terrain, ignoreCase: true, out TerrainType terrainType))
        {
            throw new InvalidDataException(
                $"Hex at ({dto.q}, {dto.r}) has invalid terrain type '{dto.terrain}'. " +
                $"Valid values: Plains, Road, Forest, Mountains, Swamp, Water, Impassable");
        }

        // Validate danger level range
        if (dto.dangerLevel < 0 || dto.dangerLevel > 10)
        {
            throw new InvalidDataException(
                $"Hex at ({dto.q}, {dto.r}) has invalid danger level {dto.dangerLevel}. " +
                $"Danger level must be between 0 (safe) and 10 (deadly)");
        }

        Hex hex = new Hex
        {
            Coordinates = new AxialCoordinates(dto.q, dto.r),
            Terrain = terrainType,
            DangerLevel = dto.dangerLevel,
            LocationId = dto.locationId, // Optional - null if wilderness hex
            IsDiscovered = dto.isDiscovered
        };

        return hex;
    }

    /// <summary>
    /// Update Location entities with hex position references after hex grid is loaded
    /// CRITICAL: This must be called AFTER hex grid is loaded but BEFORE locations are used
    /// Syncs Location.HexPosition (source of truth) with Hex.LocationId (derived lookup)
    /// </summary>
    public static void SyncLocationHexPositions(HexMap hexMap, List<Location> locations)
    {
        if (hexMap == null)
            throw new ArgumentNullException(nameof(hexMap), "HexMap cannot be null");

        if (locations == null)
            throw new ArgumentNullException(nameof(locations), "Locations list cannot be null");

        // For each hex that has a location, find the location entity and set its HexPosition
        // NOTE: Location.Id removed per ADR-007 (categorical architecture)
        // Hex-to-Location syncing now happens via HexPosition matching (reverse lookup)
        int syncedCount = 0;
        foreach (Hex hex in hexMap.Hexes)
        {
            if (string.IsNullOrEmpty(hex.LocationId))
                continue; // Wilderness hex, skip

            // Find location by HexPosition (source of truth per ADR-007)
            // Locations are matched by coordinates, not by ID
            Location location = locations.FirstOrDefault(l =>
                l.HexPosition != null &&
                l.HexPosition.Q == hex.Coordinates.Q &&
                l.HexPosition.R == hex.Coordinates.R);

            if (location == null)
            {
                // Location not found at these coordinates
                // This means hex_grid.json references a location that doesn't exist
                // Log warning but don't throw - allow procedural generation
                continue;
            }

            // HexPosition already set during location creation
            // Verify it matches
            if (location.HexPosition.Q != hex.Coordinates.Q || location.HexPosition.R != hex.Coordinates.R)
            {
                location.HexPosition = hex.Coordinates;
            }
            syncedCount++;
            Console.WriteLine($"[HexSync] ✅ Synced location at hex position ({hex.Coordinates.Q}, {hex.Coordinates.R})");
        }

        Console.WriteLine($"[HexSync] Synced {syncedCount} locations to hex positions");

        // Validate all locations have hex positions after sync
        ValidateAllLocationsHaveHexPositions(locations);
    }

    /// <summary>
    /// Ensure hex grid completeness - create hexes for positioned locations that don't have them
    /// INVERSE of SyncLocationHexPositions: Location.HexPosition (source of truth) → Hex (derived index)
    /// Maintains invariant: "Every positioned location has a corresponding hex in the grid"
    /// Called after loading dynamic content where locations exist before hexes
    /// </summary>
    public static void EnsureHexGridCompleteness(HexMap hexMap, List<Location> locations)
    {
        if (hexMap == null)
            throw new ArgumentNullException(nameof(hexMap), "HexMap cannot be null");

        if (locations == null)
            throw new ArgumentNullException(nameof(locations), "Locations list cannot be null");

        int hexesCreated = 0;

        // Find positioned locations and ensure hexes reference them correctly
        foreach (Location location in locations)
        {
            if (!location.HexPosition.HasValue)
                continue; // Skip locations without positions (intra-venue only)

            // Get hex at these coordinates
            Hex existingHex = hexMap.GetHex(location.HexPosition.Value);

            if (existingHex != null)
            {
                // Hex exists - update its LocationId if it's null or different
                if (string.IsNullOrEmpty(existingHex.LocationId) || existingHex.LocationId != location.Id)
                {
                    Console.WriteLine($"[HexGridCompleteness] Updating hex at ({location.HexPosition.Value.Q}, {location.HexPosition.Value.R}): LocationId '{existingHex.LocationId}' → '{location.Id}'");
                    existingHex.LocationId = location.Id;
                    hexesCreated++;
                }
            }
            else
            {
                // Hex doesn't exist - create new hex for this location
                Hex hex = new Hex
                {
                    Coordinates = location.HexPosition.Value,
                    LocationId = location.Id,
                    Terrain = TerrainType.Road, // Default safe terrain for scene-generated locations (within settlements)
                    DangerLevel = 0, // Default safe for dependent locations
                    IsDiscovered = true // Player knows about scene-generated content
                };

                hexMap.Hexes.Add(hex);
                hexesCreated++;

                Console.WriteLine($"[HexGridCompleteness] ✅ Created hex at ({hex.Coordinates.Q}, {hex.Coordinates.R}) for location '{location.Id}'");
            }
        }

        // Rebuild lookup dictionary if any hexes were added
        if (hexesCreated > 0)
        {
            hexMap.BuildLookup();
            Console.WriteLine($"[HexGridCompleteness] Added {hexesCreated} hexes to grid, rebuilt lookup dictionary");
        }
    }

    /// <summary>
    /// Validate no duplicate hex coordinates exist in hex map
    /// CRITICAL: Prevents coordinate lookup dictionary corruption
    /// Called after parsing all hexes, before BuildLookup()
    /// </summary>
    private static void ValidateDuplicateCoordinates(HexMap hexMap)
    {
        List<CoordinatePair> seenCoordinates = new List<CoordinatePair>();

        for (int i = 0; i < hexMap.Hexes.Count; i++)
        {
            Hex hex = hexMap.Hexes[i];
            CoordinatePair coords = new CoordinatePair(hex.Coordinates.Q, hex.Coordinates.R);

            if (seenCoordinates.Any(c => c.Equals(coords)))
            {
                throw new InvalidDataException(
                    $"Duplicate hex coordinate ({hex.Coordinates.Q}, {hex.Coordinates.R}) found in hex_grid.json. " +
                    $"Each hex position must be unique. Check hexes array for duplicate Q/R values.");
            }

            seenCoordinates.Add(coords);
        }
    }

    /// <summary>
    /// Validate no duplicate locationIds exist across hexes
    /// CRITICAL: Prevents same location appearing on multiple hexes (impossible spatial state)
    /// Called after coordinate validation, before BuildLookup()
    /// </summary>
    private static void ValidateDuplicateLocationIds(HexMap hexMap)
    {
        List<LocationHexMapping> locationMappings = new List<LocationHexMapping>();

        foreach (Hex hex in hexMap.Hexes)
        {
            if (string.IsNullOrEmpty(hex.LocationId))
                continue; // Wilderness hex, skip

            LocationHexMapping existing = locationMappings.FirstOrDefault(m => m.LocationId == hex.LocationId);
            if (existing != null)
            {
                throw new InvalidDataException(
                    $"Location '{hex.LocationId}' referenced by multiple hexes: " +
                    $"({existing.Coordinates.Q}, {existing.Coordinates.R}) and ({hex.Coordinates.Q}, {hex.Coordinates.R}). " +
                    $"Each location must occupy exactly one hex position.");
            }

            locationMappings.Add(new LocationHexMapping
            {
                LocationId = hex.LocationId,
                Coordinates = hex.Coordinates
            });
        }
    }

    /// <summary>
    /// Helper class for location-hex validation mapping
    /// </summary>
    private class LocationHexMapping
    {
        public string LocationId { get; set; }
        public AxialCoordinates Coordinates { get; set; }
    }

    /// <summary>
    /// Validate all locations have hex positions after sync
    /// CRITICAL: Ensures travel system can reach all locations
    /// Called after SyncLocationHexPositions assigns all positions
    /// </summary>
    private static void ValidateAllLocationsHaveHexPositions(List<Location> locations)
    {
        List<string> missingPositions = new List<string>();

        foreach (Location location in locations)
        {
            if (!location.HexPosition.HasValue)
            {
                missingPositions.Add(location.Id);
            }
        }

        if (missingPositions.Count > 0)
        {
            throw new InvalidDataException(
                $"Locations without hex positions: {string.Join(", ", missingPositions)}. " +
                $"Every location must have a hex position defined in 02_hex_grid.json " +
                $"or generated at scene finalization for dependent locations.");
        }
    }
}
