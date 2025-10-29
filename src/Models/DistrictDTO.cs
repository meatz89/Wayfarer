/// <summary>
/// DTO for District data from JSON packages
/// </summary>
public class DistrictDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // District knows its parent Region
    public string RegionId { get; set; }

    // Districts contain multiple locations
    public List<string> VenueIds { get; set; } = new List<string>();

    // District-level properties
    public string DistrictType { get; set; }
    public int DangerLevel { get; set; }
    public List<string> Characteristics { get; set; } = new List<string>();
}