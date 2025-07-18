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
    public string Description { get; set; }
    public string InitialState { get; set; }
    public string LocationId { get; set; }
    public List<string> CurrentTimeBlocks { get; set; } = new List<string>();
    public List<string> DomainTags { get; set; } = new List<string>();
    public int CurrentLevel { get; set; } = 1;
    public int CurrentXP { get; set; } = 0;
    public int XPToNextLevel { get; set; } = 100;
}