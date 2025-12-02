/// <summary>
/// Generates procedural venues with hex allocation and spatial placement.
/// Enables unlimited world expansion while maintaining geographic coherence.
/// </summary>
public class VenueGeneratorService
{
    private readonly GameWorld _gameWorld;

    public VenueGeneratorService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Place all authored venues procedurally using deterministic algorithm.
    /// Called ONCE after all packages loaded, BEFORE location placement.
    /// HIGHLANDER: Same spatial logic as runtime venue generation, deterministic variant.
    /// GameWorld accessed via _gameWorld (never passed as parameter).
    /// </summary>
    public void PlaceAuthoredVenues(List<Venue> authoredVenues)
    {
        Console.WriteLine($"[VenuePlacement] Placing {authoredVenues.Count} authored venues procedurally");

        // Track allocated hexes to maintain venue separation BEFORE locations are placed
        List<AxialCoordinates> allocatedHexes = new List<AxialCoordinates>();

        foreach (Venue venue in authoredVenues)
        {
            // Find unoccupied cluster (DETERMINISTIC variant - no shuffle)
            // Pass allocated hexes to avoid overlaps
            AxialCoordinates centerHex = FindUnoccupiedClusterForAuthoredVenues(
                venue.HexAllocation,
                allocatedHexes
            );

            // Assign calculated centerHex to venue
            venue.CenterHex = centerHex;

            // Mark this venue's allocated hexes to prevent overlaps
            if (venue.HexAllocation == HexAllocationStrategy.SingleHex)
            {
                allocatedHexes.Add(centerHex);
            }
            else // ClusterOf7
            {
                allocatedHexes.Add(centerHex); // Center
                foreach (AxialCoordinates neighbor in centerHex.GetNeighbors())
                {
                    allocatedHexes.Add(neighbor); // 6 neighbors
                }
            }

            Console.WriteLine($"[VenuePlacement] Placed venue '{venue.Name}' at ({centerHex.Q}, {centerHex.R})");
        }

        Console.WriteLine($"[VenuePlacement] Completed procedural placement for {authoredVenues.Count} authored venues");
    }

    /// <summary>
    /// Find unoccupied hex cluster for AUTHORED venue placement (before locations exist).
    /// Uses allocated hexes tracking to maintain venue separation.
    /// HIGHLANDER: Deterministic placement for authored venues.
    /// </summary>
    private AxialCoordinates FindUnoccupiedClusterForAuthoredVenues(
        HexAllocationStrategy strategy,
        List<AxialCoordinates> allocatedHexes)
    {
        int searchRadius = 1;
        int maxRadius = 50;

        while (searchRadius <= maxRadius)
        {
            List<Hex> candidateHexes = _gameWorld.WorldHexGrid.Hexes
                .Where(h => CalculateDistance(h.Coordinates, _gameWorld.WorldHexGrid.Origin) == searchRadius)
                .ToList();

            // Deterministic order (no shuffle for authored venues)
            foreach (Hex candidate in candidateHexes)
            {
                if (IsClusterUnoccupiedForAuthoredVenues(candidate.Coordinates, strategy, allocatedHexes))
                {
                    return candidate.Coordinates;
                }
            }

            searchRadius++;
        }

        throw new InvalidOperationException(
            "Could not find unoccupied hex cluster for authored venue placement. " +
            "World may be fully populated or search radius exceeded."
        );
    }

    /// <summary>
    /// Check if hex cluster is unoccupied for AUTHORED venue placement.
    /// Uses allocated hexes tracking (no locations exist yet).
    /// </summary>
    private bool IsClusterUnoccupiedForAuthoredVenues(
        AxialCoordinates center,
        HexAllocationStrategy strategy,
        List<AxialCoordinates> allocatedHexes)
    {
        if (strategy == HexAllocationStrategy.SingleHex)
        {
            // Check if center hex is already allocated
            if (allocatedHexes.Contains(center))
                return false;

            // Check venue separation (1-hex gap)
            foreach (AxialCoordinates neighbor in center.GetNeighbors())
            {
                if (allocatedHexes.Contains(neighbor))
                    return false; // Adjacent to allocated venue
            }

            return true;
        }
        else // ClusterOf7
        {
            // Check if center is allocated
            if (allocatedHexes.Contains(center))
                return false;

            // Check if any of the 6 neighbors are allocated
            foreach (AxialCoordinates neighbor in center.GetNeighbors())
            {
                if (allocatedHexes.Contains(neighbor))
                    return false;
            }

            // Check venue separation (all hexes in cluster must have 1-hex gap from allocated venues)
            foreach (AxialCoordinates neighbor in center.GetNeighbors())
            {
                foreach (AxialCoordinates neighborOfNeighbor in neighbor.GetNeighbors())
                {
                    if (allocatedHexes.Contains(neighborOfNeighbor))
                        return false; // Would be adjacent to allocated venue
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Generate venue from template with hex allocation.
    /// Finds unoccupied hex cluster, creates venue entity, marks hexes as allocated.
    /// GameWorld accessed via _gameWorld (never passed as parameter).
    /// </summary>
    public Venue GenerateVenue(VenueTemplate template, SceneSpawnContext context)
    {
        // 1. Resolve district from template or context
        // HIGHLANDER: Resolve district name to District object
        string districtName = template.District;
        if (string.IsNullOrEmpty(districtName))
        {
            if (context.CurrentLocation?.Venue?.District != null)
            {
                districtName = context.CurrentLocation.Venue.District.Name;
            }
            else
            {
                districtName = "wilderness";
            }
        }

        // Look up District entity (assume it exists, created during world initialization)
        District district = _gameWorld.Districts.FirstOrDefault(d => d.Name == districtName);

        // 2. Find unoccupied hex cluster based on allocation strategy (DETERMINISTIC per architecture rules)
        AxialCoordinates centerHex = FindUnoccupiedCluster(template.HexAllocation);

        // 3. Replace placeholders in name/description
        string venueName = ReplacePlaceholders(template.NamePattern, context, districtName);
        string venueDescription = ReplacePlaceholders(template.DescriptionPattern, context, districtName);

        // 4. Create Venue entity
        // ADR-007: Constructor uses Name only (no Id parameter or generation)
        // HIGHLANDER: Assign District object reference, not string
        Venue venue = new Venue(venueName)
        {
            Description = venueDescription,
            Type = template.Type,
            District = district, // Object reference
            MaxLocations = template.MaxLocations,
            CenterHex = centerHex,
            IsSkeleton = false
        };

        // 6. Add to GameWorld.Venues
        _gameWorld.Venues.Add(venue);

        // 7. Mark hexes as part of this venue (for routing/travel rules)
        // Note: Individual locations will set their own HexPosition and sync with hex grid
        // Venue itself has no position - it's a logical container

        return venue;
    }

    /// <summary>
    /// Find unoccupied hex cluster for venue placement.
    /// Returns center hex coordinates for the venue.
    /// DETERMINISM PRINCIPLE: All venue placement is deterministic for reproducible worlds.
    /// </summary>
    private AxialCoordinates FindUnoccupiedCluster(HexAllocationStrategy strategy)
    {
        int searchRadius = 1;
        int maxRadius = 50;

        while (searchRadius <= maxRadius)
        {
            List<Hex> candidateHexes = _gameWorld.WorldHexGrid.Hexes
                .Where(h => CalculateDistance(h.Coordinates, _gameWorld.WorldHexGrid.Origin) == searchRadius)
                .ToList();

            // DETERMINISM PRINCIPLE: All venue placement is deterministic
            // Natural hex grid order ensures reproducible world generation
            // Variety comes from hex grid shape and venue order, not randomness

            foreach (Hex candidate in candidateHexes)
            {
                if (IsClusterUnoccupied(candidate.Coordinates, strategy))
                {
                    return candidate.Coordinates;
                }
            }

            searchRadius++;
        }

        throw new InvalidOperationException(
            "Could not find unoccupied hex cluster for venue placement. " +
            "World may be fully populated or search radius exceeded."
        );
    }

    /// <summary>
    /// Check if hex cluster is unoccupied (no locations present) AND separated from other venues.
    /// Enforces venue separation: minimum 1-hex gap between venues (non-adjacency rule).
    /// </summary>
    private bool IsClusterUnoccupied(AxialCoordinates center, HexAllocationStrategy strategy)
    {
        if (strategy == HexAllocationStrategy.SingleHex)
        {
            Hex centerHex = _gameWorld.WorldHexGrid.GetHex(center.Q, center.R);
            // HIGHLANDER: Query Location.HexPosition (source of truth) instead of derived lookup
            if (centerHex == null || _gameWorld.GetLocationsAtHex(center.Q, center.R).Count > 0)
            {
                return false;
            }

            return IsHexSeparatedFromVenues(center);
        }
        else // ClusterOf7
        {
            Hex centerHex = _gameWorld.WorldHexGrid.GetHex(center.Q, center.R);
            // HIGHLANDER: Query Location.HexPosition (source of truth) instead of derived lookup
            if (centerHex == null || _gameWorld.GetLocationsAtHex(center.Q, center.R).Count > 0)
            {
                return false;
            }

            AxialCoordinates[] neighbors = center.GetNeighbors();
            foreach (AxialCoordinates neighbor in neighbors)
            {
                Hex neighborHex = _gameWorld.WorldHexGrid.GetHex(neighbor.Q, neighbor.R);
                // HIGHLANDER: Query Location.HexPosition (source of truth) instead of derived lookup
                if (neighborHex == null || _gameWorld.GetLocationsAtHex(neighbor.Q, neighbor.R).Count > 0)
                {
                    return false;
                }
            }

            if (!IsHexSeparatedFromVenues(center))
            {
                return false;
            }

            foreach (AxialCoordinates neighbor in neighbors)
            {
                if (!IsHexSeparatedFromVenues(neighbor))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Check if hex and its neighbors are separated from all existing venues.
    /// Enforces minimum 1-hex gap: no hex in cluster can be adjacent to hex occupied by another venue.
    /// HIGHLANDER: Use Location object references directly
    /// </summary>
    private bool IsHexSeparatedFromVenues(AxialCoordinates hexCoords)
    {
        AxialCoordinates[] neighbors = hexCoords.GetNeighbors();

        foreach (AxialCoordinates neighborCoords in neighbors)
        {
            Hex neighborHex = _gameWorld.WorldHexGrid.GetHex(neighborCoords.Q, neighborCoords.R);
            if (neighborHex == null)
            {
                continue;
            }

            // HIGHLANDER: Query Location.HexPosition (source of truth) instead of derived lookup
            List<Location> locationsAtHex = _gameWorld.GetLocationsAtHex(neighborCoords.Q, neighborCoords.R);
            foreach (Location location in locationsAtHex)
            {
                // Location already exists on this hex - check if it belongs to a venue
                if (location.Venue != null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Calculate hex distance from origin.
    /// </summary>
    private int CalculateDistance(AxialCoordinates a, AxialCoordinates b)
    {
        return a.DistanceTo(b);
    }

    /// <summary>
    /// Replace placeholders in template strings.
    /// </summary>
    private string ReplacePlaceholders(string template, SceneSpawnContext context, string districtId)
    {
        if (string.IsNullOrEmpty(template))
        {
            return "";
        }

        string result = template;

        // District placeholders
        result = result.Replace("{DistrictName}", districtId ?? "unknown");

        // NPC placeholders (if context has NPC)
        if (context.CurrentNPC != null)
        {
            result = result.Replace("{NPCName}", context.CurrentNPC.Name);
        }

        // Location placeholders (if context has location)
        if (context.CurrentLocation != null)
        {
            result = result.Replace("{LocationName}", context.CurrentLocation.Name);
            if (context.CurrentLocation.Venue != null)
            {
                result = result.Replace("{VenueName}", context.CurrentLocation.Venue.Name);
            }
        }

        // Player placeholders
        if (context.Player != null)
        {
            result = result.Replace("{PlayerName}", context.Player.Name ?? "Traveler");
        }

        return result;
    }
}
