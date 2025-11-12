public class Venue
{
    public string Id { get; set; }
    public string Name { get; set; } // Changed from private set for procedural generation
    public string Description { get; set; }

    // Hierarchical organization - Venue only knows its District
    public string District { get; set; } // e.g., "Lower Wards"

    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton

    // Tier system (1-5) for difficulty/content progression
    public VenueType Type { get; set; } = VenueType.Wilderness;  // Strongly-typed venue category (replaces LocationTypeString)
    public int Tier { get; set; } = 1;

    // SPATIAL: Center hex position for venue cluster
    // Set during venue generation, used for placing first location
    // Subsequent locations placed adjacent to existing locations (organic growth)
    public AxialCoordinates? CenterHex { get; set; }

    // BIDIRECTIONAL RELATIONSHIP: Venue ↔ Location
    // Venue.LocationIds maintained by GameWorld.AddOrUpdateLocation
    // Location.Venue maintained by LocationParser.ConvertDTOToLocation
    // CRITICAL: Capacity budget depends on LocationIds.Count being accurate
    public List<string> LocationIds { get; set; } = new List<string>();

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
    /// Budget is derived (LocationIds.Count) not tracked.
    /// </summary>
    public int MaxLocations { get; set; } = 20;

    /// <summary>
    /// Check if venue can accept more locations within capacity budget.
    /// Counts ALL locations (authored + generated) against MaxLocations.
    /// </summary>
    public bool CanAddMoreLocations()
    {
        return LocationIds.Count < MaxLocations;
    }

    public Venue(string id, string name)
    {
        Id = id;
        Name = name;
    }

}
