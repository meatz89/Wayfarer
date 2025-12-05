public class Region
{
    public static readonly Region? NoRegion = new Region() { Name = "No Region", Description = "No Region" };

    // HIGHLANDER: NO Id property - Region identified by object reference
    public string Name { get; set; }
    public string Description { get; set; }

    // HIGHLANDER: Object references ONLY, no DistrictIds
    // Regions contain multiple districts
    public List<District> Districts { get; set; } = new List<District>();

    // Region-level properties
    public string Government { get; set; } // e.g., "City Council", "Noble House", "Merchant Guild"
    public string Culture { get; set; } // e.g., "Trade-focused", "Traditional", "Cosmopolitan"
    public int Population { get; set; } // Rough population size
    public List<string> MajorExports { get; set; } = new List<string>();
    public List<string> MajorImports { get; set; } = new List<string>();
}