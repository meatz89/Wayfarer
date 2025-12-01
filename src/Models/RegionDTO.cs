/// <summary>
/// DTO for Region data from JSON packages
/// </summary>
public class RegionDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Regions contain multiple districts
    public List<string> DistrictIds { get; set; } = new List<string>();

    // Region-level properties
    public string Government { get; set; }
    public string Culture { get; set; }
    public int Population { get; set; }
    public List<string> MajorExports { get; set; } = new List<string>();
    public List<string> MajorImports { get; set; } = new List<string>();
}