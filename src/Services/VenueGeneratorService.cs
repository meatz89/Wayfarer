/// <summary>
/// Generates procedural venues with hex allocation and spatial placement.
/// Enables unlimited world expansion while maintaining geographic coherence.
/// </summary>
public class VenueGeneratorService
{
    public VenueGeneratorService()
    {
    }

    /// <summary>
    /// Place all authored venues procedurally using deterministic algorithm.
    /// Called ONCE after all packages loaded, BEFORE location placement.
    /// HIGHLANDER: Same spatial logic as runtime venue generation, deterministic variant.
    /// </summary>
    public void PlaceAuthoredVenues(List<Venue> authoredVenues, GameWorld gameWorld)
    {
        Console.WriteLine($"[VenuePlacement] Placing {authoredVenues.Count} authored venues procedurally");

        // Track allocated hexes to maintain venue separation BEFORE locations are placed
        HashSet<AxialCoordinates> allocatedHexes = new HashSet<AxialCoordinates>();

        foreach (Venue venue in authoredVenues)
        {
            // Find unoccupied cluster (DETERMINISTIC variant - no shuffle)
            // Pass allocated hexes to avoid overlaps
            AxialCoordinates centerHex = FindUnoccupiedClusterForAuthoredVenues(
                venue.HexAllocation,
                gameWorld,
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
        GameWorld gameWorld,
        HashSet<AxialCoordinates> allocatedHexes)
    {
        int searchRadius = 1;
        int maxRadius = 50;

        while (searchRadius <= maxRadius)
        {
            List<Hex> candidateHexes = gameWorld.WorldHexGrid.Hexes
                .Where(h => CalculateDistance(h.Coordinates, gameWorld.WorldHexGrid.Origin) == searchRadius)
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
        HashSet<AxialCoordinates> allocatedHexes)
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
    /// </summary>
    public Venue GenerateVenue(VenueTemplate template, SceneSpawnContext context, GameWorld gameWorld)
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
        District district = gameWorld.Districts.FirstOrDefault(d => d.Name == districtName);

        // 2. Find unoccupied hex cluster based on allocation strategy (NON-deterministic for runtime variety)
        AxialCoordinates centerHex = FindUnoccupiedCluster(template.HexAllocation, gameWorld, deterministic: false);

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
            Tier = template.Tier,
            District = district, // Object reference
            MaxLocations = template.MaxLocations,
            CenterHex = centerHex,
            IsSkeleton = false
        };

        // 6. Add to GameWorld.Venues
        gameWorld.Venues.Add(venue);

        // 7. Mark hexes as part of this venue (for routing/travel rules)
        // Note: Individual locations will set their own HexPosition and sync with hex grid
        // Venue itself has no position - it's a logical container

        return venue;
    }

    /// <summary>
    /// Find unoccupied hex cluster for venue placement.
    /// Returns center hex coordinates for the venue.
    /// HIGHLANDER: Single algorithm for both authored (deterministic) and runtime (randomized) venues.
    /// </summary>
    private AxialCoordinates FindUnoccupiedCluster(HexAllocationStrategy strategy, GameWorld gameWorld, bool deterministic = false)
    {
        int searchRadius = 1;
        int maxRadius = 50;

        while (searchRadius <= maxRadius)
        {
            List<Hex> candidateHexes = gameWorld.WorldHexGrid.Hexes
                .Where(h => CalculateDistance(h.Coordinates, gameWorld.WorldHexGrid.Origin) == searchRadius)
                .ToList();

            // HIGHLANDER: Conditional shuffle based on deterministic flag
            // Authored venues: deterministic=true → No shuffle, use hex grid natural order
            // Runtime venues: deterministic=false → Shuffle for procedural variety
            if (!deterministic)
            {
                ShuffleList(candidateHexes);
            }
            // Else: Use natural hex grid order (deterministic placement for authored venues)

            foreach (Hex candidate in candidateHexes)
            {
                if (IsClusterUnoccupied(candidate.Coordinates, strategy, gameWorld))
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

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Shared.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Check if hex cluster is unoccupied (no locations present) AND separated from other venues.
    /// Enforces venue separation: minimum 1-hex gap between venues (non-adjacency rule).
    /// </summary>
    private bool IsClusterUnoccupied(AxialCoordinates center, HexAllocationStrategy strategy, GameWorld gameWorld)
    {
        if (strategy == HexAllocationStrategy.SingleHex)
        {
            Hex centerHex = gameWorld.WorldHexGrid.GetHex(center.Q, center.R);
            // HIGHLANDER: Query Location.HexPosition (source of truth) instead of derived lookup
            if (centerHex == null || gameWorld.GetLocationsAtHex(center.Q, center.R).Count > 0)
            {
                return false;
            }

            return IsHexSeparatedFromVenues(center, gameWorld);
        }
        else // ClusterOf7
        {
            Hex centerHex = gameWorld.WorldHexGrid.GetHex(center.Q, center.R);
            // HIGHLANDER: Query Location.HexPosition (source of truth) instead of derived lookup
            if (centerHex == null || gameWorld.GetLocationsAtHex(center.Q, center.R).Count > 0)
            {
                return false;
            }

            AxialCoordinates[] neighbors = center.GetNeighbors();
            foreach (AxialCoordinates neighbor in neighbors)
            {
                Hex neighborHex = gameWorld.WorldHexGrid.GetHex(neighbor.Q, neighbor.R);
                // HIGHLANDER: Query Location.HexPosition (source of truth) instead of derived lookup
                if (neighborHex == null || gameWorld.GetLocationsAtHex(neighbor.Q, neighbor.R).Count > 0)
                {
                    return false;
                }
            }

            if (!IsHexSeparatedFromVenues(center, gameWorld))
            {
                return false;
            }

            foreach (AxialCoordinates neighbor in neighbors)
            {
                if (!IsHexSeparatedFromVenues(neighbor, gameWorld))
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
    private bool IsHexSeparatedFromVenues(AxialCoordinates hexCoords, GameWorld gameWorld)
    {
        AxialCoordinates[] neighbors = hexCoords.GetNeighbors();

        foreach (AxialCoordinates neighborCoords in neighbors)
        {
            Hex neighborHex = gameWorld.WorldHexGrid.GetHex(neighborCoords.Q, neighborCoords.R);
            if (neighborHex == null)
            {
                continue;
            }

            // HIGHLANDER: Query Location.HexPosition (source of truth) instead of derived lookup
            List<Location> locationsAtHex = gameWorld.GetLocationsAtHex(neighborCoords.Q, neighborCoords.R);
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
