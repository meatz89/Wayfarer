using System.Collections.Generic;

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
    public string LocationTypeString { get; set; } // Mechanical property for display type (e.g., "Tavern", "Crossroads")
    public int Tier { get; set; } = 1;

    public List<string> LocationSpotIds { get; set; } = new List<string>();

    public Venue(string id, string name)
    {
        Id = id;
        Name = name;
    }

}
