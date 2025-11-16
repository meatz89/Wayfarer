public class Region
{
    // HIGHLANDER: NO Id property - Region identified by object reference
    public string Name { get; set; }
    public string Description { get; set; }

    // HIGHLANDER: Object references ONLY, no DistrictIds
    // Regions contain multiple districts
    public List<District> Districts { get; set; } = new List<District>();

    // Region tier (1-3: personal/local/regional scope)
    // Tier 1: Starting region (personal stakes)
    // Tier 2: Adjacent regions (local expansion)
    // Tier 3: Distant regions (regional scope, maximum breadth)
    public int Tier { get; set; } = 1;

    // Region-level properties
    public string Government { get; set; } // e.g., "City Council", "Noble House", "Merchant Guild"
    public string Culture { get; set; } // e.g., "Trade-focused", "Traditional", "Cosmopolitan"
    public int Population { get; set; } // Rough population size
    public List<string> MajorExports { get; set; } = new List<string>();
    public List<string> MajorImports { get; set; } = new List<string>();
}