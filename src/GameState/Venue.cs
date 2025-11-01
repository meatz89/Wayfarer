using Wayfarer.GameState.Enums;

public class Venue
{
    public string Id { get; set; }
    public string Name { get; private set; }
    public string Description { get; set; }

    // Hierarchical organization - Venue only knows its District
    public string District { get; set; } // e.g., "Lower Wards"

    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton

    // Tier system (1-5) for difficulty/content progression
    public VenueType Type { get; set; } = VenueType.Wilderness;  // Strongly-typed venue category (replaces LocationTypeString)
    public int Tier { get; set; } = 1;

    public List<string> LocationSpotIds { get; set; } = new List<string>();

    // HEX-BASED TRAVEL SYSTEM: Venue is ONLY a wrapper for travel cost rules
    // Venue has NO spatial position - Locations are the spatial entities
    // Same Venue = instant free travel between locations
    // Different Venue = Route required with hex path, costs, scenes

    public Venue(string id, string name)
    {
        Id = id;
        Name = name;
    }

}
