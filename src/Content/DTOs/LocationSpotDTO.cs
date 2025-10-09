using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Data Transfer Object for deserializing Venue spot data from JSON.
/// Maps to the structure in location_spots.json.
/// </summary>
public class LocationSpotDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string InitialState { get; set; }
    public string VenueId { get; set; }
    public List<string> CurrentTimeBlocks { get; set; } = new List<string>();
    public List<string> DomainTags { get; set; } = new List<string>();

    // The JSON has a "properties" object with time-based keys
    [JsonPropertyName("properties")]
    public SpotPropertiesDTO Properties { get; set; } = new SpotPropertiesDTO();

    // Additional properties from JSON
    public bool CanInvestigate { get; set; }
    public Dictionary<string, int> InvestigationScaling { get; set; } = new Dictionary<string, int>();
    public bool CanWork { get; set; }
    public string WorkType { get; set; }
    public int WorkPay { get; set; }

    public AccessRequirementDTO AccessRequirement { get; set; }

    // Gameplay properties moved from LocationDTO
    public string LocationType { get; set; }
    public bool IsStartingLocation { get; set; }
    public string InvestigationProfile { get; set; }
    public Dictionary<string, List<string>> AvailableProfessionsByTime { get; set; } = new Dictionary<string, List<string>>();
    public List<WorkActionDTO> AvailableWork { get; set; } = new List<WorkActionDTO>();
}