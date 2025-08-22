using System.Collections.Generic;
/// <summary>
/// Data Transfer Object for deserializing location data from JSON.
/// Maps to the structure in locations.json.
/// </summary>
public class LocationDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public EnvironmentalPropertiesDTO EnvironmentalProperties { get; set; }
    public List<string> DomainTags { get; set; } = new List<string>();
    public string TravelHubSpotId { get; set; }
    public List<string> LocationSpots { get; set; } = new List<string>();
    public List<string> ConnectedTo { get; set; } = new List<string>();
    public string SocialExpectation { get; set; }
    public string AccessLevel { get; set; }
    public List<string> RequiredSocialClasses { get; set; } = new List<string>();
    public Dictionary<string, List<string>> AvailableProfessionsByTime { get; set; } = new Dictionary<string, List<string>>();
    public AccessRequirementDTO AccessRequirement { get; set; }
    public int Tier { get; set; } = 1; // Default to tier 1 for backwards compatibility
}

/// <summary>
/// Environmental properties by time of day
/// </summary>
public class EnvironmentalPropertiesDTO
{
    public List<string> Morning { get; set; } = new List<string>();
    public List<string> Afternoon { get; set; } = new List<string>();
    public List<string> Evening { get; set; } = new List<string>();
    public List<string> Night { get; set; } = new List<string>();
}