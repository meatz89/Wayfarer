using System.Collections.Generic;

public class Location
{
    public string Id { get; set; }
    public string Name { get; private set; }
    public string Description { get; set; }

    // Hierarchical organization - Location only knows its District
    public string District { get; set; } // e.g., "Lower Wards"

    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton

    // Tier system (1-5) for difficulty/content progression
    public int Tier { get; set; } = 1;

    public List<string> LocationSpotIds { get; set; } = new List<string>();
   
    public Location(string id, string name)
    {
        Id = id;
        Name = name;
    }

}
