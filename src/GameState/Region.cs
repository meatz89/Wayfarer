public class Region
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Regions contain multiple districts
    public List<string> DistrictIds { get; set; } = new List<string>();

    // Region-level properties
    public string Government { get; set; } // e.g., "City Council", "Noble House", "Merchant Guild"
    public string Culture { get; set; } // e.g., "Trade-focused", "Traditional", "Cosmopolitan"
    public int Population { get; set; } // Rough population size
    public List<string> MajorExports { get; set; } = new List<string>();
    public List<string> MajorImports { get; set; } = new List<string>();
}