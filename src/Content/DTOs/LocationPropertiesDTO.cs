/// <summary>
/// DTO for the properties object structure in Venue Locations JSON.
/// Maps time periods to property lists.
/// </summary>
public class LocationPropertiesDTO
{
    public List<string> Base { get; set; } = new List<string>();

    public List<string> All { get; set; } = new List<string>();

    public List<string> Morning { get; set; } = new List<string>();

    public List<string> Midday { get; set; } = new List<string>();

    public List<string> Afternoon { get; set; } = new List<string>();

    public List<string> Evening { get; set; } = new List<string>();

    public List<string> Night { get; set; } = new List<string>();

    public List<string> Dawn { get; set; } = new List<string>();
}
