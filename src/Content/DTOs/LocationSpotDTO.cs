using System.Collections.Generic;
/// <summary>
/// Data Transfer Object for deserializing location spot data from JSON.
/// Maps to the structure in location_spots.json.
/// </summary>
public class LocationSpotDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    // Description removed - generated from SpotProperties
    public string InitialState { get; set; }
    public string LocationId { get; set; }
    public List<string> CurrentTimeBlocks { get; set; } = new List<string>();
    public List<string> DomainTags { get; set; } = new List<string>();
    public List<string> SpotProperties { get; set; } = new List<string>();
    public Dictionary<string, List<string>> TimeSpecificProperties { get; set; } = new Dictionary<string, List<string>>();
    public AccessRequirementDTO AccessRequirement { get; set; }
}