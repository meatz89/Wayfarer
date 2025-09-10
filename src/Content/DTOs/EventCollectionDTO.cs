using System.Collections.Generic;

/// <summary>
/// DTO for event collections used in Event-type route segments
/// </summary>
public class EventCollectionDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string NarrativeText { get; set; }
    public List<PathCardDTO> ResponseCards { get; set; } = new List<PathCardDTO>();
}