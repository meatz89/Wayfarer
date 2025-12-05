public class District
{
    public static readonly District NoDistrict = new District() { Name = "No District", Description = "No District" };

    // HIGHLANDER: NO Id property - District identified by object reference
    public string Name { get; set; }
    public string Description { get; set; }

    // HIGHLANDER: Object reference ONLY, no RegionId
    // District knows its parent Region
    public Region Region { get; set; }

    // HIGHLANDER: Object references ONLY, no VenueIds
    // Districts contain multiple locations
    public List<Venue> Venues { get; set; } = new List<Venue>();

    // District-level properties
    public string DistrictType { get; set; } // e.g., "Trade Quarter", "Noble Quarter", "Industrial Zone"
    public int DangerLevel { get; set; } // 1-5 scale
    public List<string> Characteristics { get; set; } = new List<string>(); // e.g., "Well-patrolled", "Crowded", "Quiet"
}