/// <summary>
/// Generates procedural venues with hex allocation and spatial placement.
/// Enables unlimited world expansion while maintaining geographic coherence.
/// </summary>
public class VenueGeneratorService
{
    private readonly HexSynchronizationService _hexSyncService;
    private readonly Random _random = new Random();

    public VenueGeneratorService(HexSynchronizationService hexSyncService)
    {
        _hexSyncService = hexSyncService;
    }

    /// <summary>
    /// Generate venue from template with hex allocation.
    /// Finds unoccupied hex cluster, creates venue entity, marks hexes as allocated.
    /// </summary>
    public Venue GenerateVenue(VenueTemplate template, SceneSpawnContext context, GameWorld gameWorld)
    {
        // 1. Resolve district from template or context
        string districtId = template.District;
        if (string.IsNullOrEmpty(districtId))
        {
            if (context.CurrentLocation?.Venue != null)
            {
                districtId = context.CurrentLocation.Venue.District ?? "wilderness";
            }
            else
            {
                districtId = "wilderness";
            }
        }

        // 2. Find unoccupied hex cluster based on allocation strategy
        AxialCoordinates centerHex = FindUnoccupiedCluster(template.HexAllocation, gameWorld);

        // 3. Generate unique venue ID
        string venueId = $"generated_venue_{Guid.NewGuid().ToString().Substring(0, 8)}";

        // 4. Replace placeholders in name/description
        string venueName = ReplacePlaceholders(template.NamePattern, context, districtId);
        string venueDescription = ReplacePlaceholders(template.DescriptionPattern, context, districtId);

        // 5. Create Venue entity
        Venue venue = new Venue(venueId, venueName)
        {
            Description = venueDescription,
            Type = template.Type,
            Tier = template.Tier,
            District = districtId,
            MaxLocations = template.MaxLocations,
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
    /// </summary>
    private AxialCoordinates FindUnoccupiedCluster(HexAllocationStrategy strategy, GameWorld gameWorld)
    {
        int searchRadius = 1;
        int maxRadius = 50;

        while (searchRadius <= maxRadius)
        {
            List<Hex> candidateHexes = gameWorld.WorldHexGrid.Hexes
                .Where(h => CalculateDistance(h.Coordinates, gameWorld.WorldHexGrid.Origin) == searchRadius)
                .ToList();

            ShuffleList(candidateHexes);

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
            "Could not find unoccupied hex cluster for venue generation. " +
            "World may be fully populated or search radius exceeded."
        );
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Check if hex cluster is unoccupied (no locations present).
    /// </summary>
    private bool IsClusterUnoccupied(AxialCoordinates center, HexAllocationStrategy strategy, GameWorld gameWorld)
    {
        if (strategy == HexAllocationStrategy.SingleHex)
        {
            // Check only center hex
            Hex centerHex = gameWorld.WorldHexGrid.GetHex(center.Q, center.R);
            return centerHex != null && string.IsNullOrEmpty(centerHex.LocationId);
        }
        else // ClusterOf7
        {
            // Check center + 6 neighbors
            Hex centerHex = gameWorld.WorldHexGrid.GetHex(center.Q, center.R);
            if (centerHex == null || !string.IsNullOrEmpty(centerHex.LocationId))
            {
                return false;
            }

            // Check all 6 neighbors
            AxialCoordinates[] neighbors = center.GetNeighbors();
            foreach (AxialCoordinates neighbor in neighbors)
            {
                Hex neighborHex = gameWorld.WorldHexGrid.GetHex(neighbor.Q, neighbor.R);
                if (neighborHex == null || !string.IsNullOrEmpty(neighborHex.LocationId))
                {
                    return false;
                }
            }

            return true;
        }
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
