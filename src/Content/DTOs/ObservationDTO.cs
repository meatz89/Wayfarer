/// <summary>
/// DTO for observation data from JSON packages
/// </summary>
public class ObservationDTO
{
    public string Id { get; set; }
    public string DisplayText { get; set; }
    public string Category { get; set; }
    public int InitiativeCost { get; set; } = 1;
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationId { get; internal set; }
}