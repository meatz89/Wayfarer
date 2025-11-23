public class Venue
{
    // HIGHLANDER: NO Id property - Venue identified by Name (natural key)
    public string Name { get; set; } // Changed from private set for procedural generation
    public string Description { get; set; }

    // Hierarchical organization - Venue only knows its District
    // HIGHLANDER: Object reference ONLY, no string District name
    public District District { get; set; }

    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton

    // Tier system (1-5) for difficulty/content progression
    public VenueType Type { get; set; } = VenueType.Wilderness;  // Strongly-typed venue category (replaces LocationTypeString)
    public int Tier { get; set; } = 1;

    // SPATIAL HEX CLUSTER: Venue defines hex territory BEFORE locations placed
    // CenterHex + HexAllocation strategy defines the venue's spatial boundaries
    // All locations must have HexPosition within venue's allocated hex cluster
    public AxialCoordinates CenterHex { get; set; }  // Required - defines venue spatial position
    public HexAllocationStrategy HexAllocation { get; set; } = HexAllocationStrategy.ClusterOf7;

    // UNIDIRECTIONAL RELATIONSHIP: Location → Venue (object reference)
    // HIGHLANDER: Location.Venue is single source of truth, NO reverse cache
    // To find locations in venue: GameWorld.Locations.Where(loc => loc.Venue == this)

    // HEX-BASED TRAVEL SYSTEM: Venue is ONLY a wrapper for travel cost rules
    // Venue has NO spatial position - Locations are the spatial entities
    // Same Venue = instant free travel between locations
    // Different Venue = Route required with hex path, costs, scenes

    // CAPACITY BUDGET SYSTEM: Bounded infinity for all venue locations
    // Prevents unlimited expansion while enabling procedural variety
    // Applies to BOTH authored and generated locations (no distinction after parsing)
    /// <summary>
    /// Maximum total locations allowed in this venue.
    /// Small venues: 5-10 (intimate, constrained)
    /// Large venues: 50-100 (expansive, variety)
    /// Wilderness: int.MaxValue (unlimited)
    /// To check budget: Query GameWorld.Locations.Count(loc => loc.Venue == this)
    /// </summary>
    public int MaxLocations { get; set; } = 20;

    // ADR-007: Constructor uses Name only (natural key, no Id parameter)
    public Venue(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Get all hexes allocated to this venue's spatial cluster.
    /// SPATIAL ARCHITECTURE: Venues claim hex territory BEFORE locations placed.
    /// Locations must have HexPosition within this allocated set.
    /// </summary>
    public List<AxialCoordinates> GetAllocatedHexes()
    {
        List<AxialCoordinates> hexes = new List<AxialCoordinates>();

        if (HexAllocation == HexAllocationStrategy.SingleHex)
        {
            hexes.Add(CenterHex);
        }
        else // ClusterOf7
        {
            hexes.Add(CenterHex);  // Center hex
            hexes.AddRange(CenterHex.GetNeighbors());  // 6 neighboring hexes
        }

        return hexes;
    }

    /// <summary>
    /// Check if a hex position is within this venue's allocated cluster.
    /// Used by LocationParser to validate location hex positions.
    /// </summary>
    public bool ContainsHex(AxialCoordinates hex)
    {
        return GetAllocatedHexes().Contains(hex);
    }

    /// <summary>
    /// Check if venue can accept more locations (capacity budget check).
    /// CATALOGUE PATTERN: Capacity is DERIVED from query, not cached.
    /// Queries GameWorld.Locations to count locations with matching Venue reference.
    /// </summary>
    public bool CanAddLocation(GameWorld world)
    {
        int currentCount = world.Locations.Count(loc => loc.Venue == this);
        return currentCount < MaxLocations;
    }

}
