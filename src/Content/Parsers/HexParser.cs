using System;
using System.Collections.Generic;

namespace Wayfarer.Content.Parsers
{
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
            foreach (Hex hex in hexMap.Hexes)
            {
                if (string.IsNullOrEmpty(hex.LocationId))
                    continue; // Wilderness hex, skip

                // Find location by ID
                Location location = locations.FirstOrDefault(l => l.Id == hex.LocationId);
                if (location == null)
                {
                    throw new InvalidDataException(
                        $"Hex at ({hex.Coordinates.Q}, {hex.Coordinates.R}) references location '{hex.LocationId}' " +
                        $"which does not exist in GameWorld.Locations. All locationId references in hex_grid.json " +
                        $"must match existing Location IDs.");
                }

                // Set Location.HexPosition (source of truth) from hex coordinates
                location.HexPosition = hex.Coordinates;
            }
        }
    }
}
