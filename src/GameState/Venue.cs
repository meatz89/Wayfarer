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

    public List<string> LocationIds { get; set; } = new List<string>();

    // HEX-BASED TRAVEL SYSTEM: Venue is ONLY a wrapper for travel cost rules
    // Venue has NO spatial position - Locations are the spatial entities
    // Same Venue = instant free travel between locations
    // Different Venue = Route required with hex path, costs, scenes

    // GENERATION BUDGET SYSTEM: Bounded infinity for procedural venues
    // Prevents unlimited expansion while enabling procedural variety
    /// <summary>
    /// Maximum number of locations that can be dynamically generated in this venue.
    /// Small venues: 5-10 (intimate, constrained)
    /// Large venues: 50-100 (expansive, variety)
    /// Wilderness: int.MaxValue (unlimited)
    /// </summary>
    public int MaxGeneratedLocations { get; set; } = 20;

    /// <summary>
    /// Count of dynamically generated locations currently in this venue.
    /// Incremented when dynamic location added, NOT decremented on cleanup (orphaned locations become permanent).
    /// </summary>
    public int GeneratedLocationCount { get; set; } = 0;

    /// <summary>
    /// Check if venue can generate more locations within budget.
    /// </summary>
    public bool CanGenerateMoreLocations()
    {
        return GeneratedLocationCount < MaxGeneratedLocations;
    }

    /// <summary>
    /// Increment generated location count after adding dynamic location.
    /// Called by VenueGeneratorService and DependentResourceOrchestrationService.
    /// </summary>
    public void IncrementGeneratedCount()
    {
        GeneratedLocationCount++;
    }

    public Venue(string id, string name)
    {
        Id = id;
        Name = name;
    }

}
