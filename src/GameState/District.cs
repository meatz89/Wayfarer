using System.Collections.Generic;

public class District
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // District knows its parent Region
    public string RegionId { get; set; }

    // Districts contain multiple locations
    public List<string> LocationIds { get; set; } = new List<string>();

    // District-level properties
    public string DistrictType { get; set; } // e.g., "Trade Quarter", "Noble Quarter", "Industrial Zone"
    public int DangerLevel { get; set; } // 1-5 scale
    public List<string> Characteristics { get; set; } = new List<string>(); // e.g., "Well-patrolled", "Crowded", "Quiet"
}