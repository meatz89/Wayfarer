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
    public string DistrictId { get; set; }
    public EnvironmentalPropertiesDTO EnvironmentalProperties { get; set; }
    public List<string> DomainTags { get; set; } = new List<string>();
    public List<string> LocationSpots { get; set; } = new List<string>();
    public List<string> ConnectedTo { get; set; } = new List<string>();
    public string SocialExpectation { get; set; }
    public string AccessLevel { get; set; }
    public List<string> RequiredSocialClasses { get; set; } = new List<string>();
    public Dictionary<string, List<string>> AvailableProfessionsByTime { get; set; } = new Dictionary<string, List<string>>();
    public AccessRequirementDTO AccessRequirement { get; set; }
    public int Tier { get; set; }

    // Mechanical properties to replace hardcoded location checks
    public string LocationType { get; set; } // e.g., "Tavern", "Crossroads", "Elite Quarter"
    public bool IsStartingLocation { get; set; }

    // Investigation Profile - Primary discipline for Mental challenges (for specialist bonuses)
    public string InvestigationProfile { get; set; } = "Research";

    // Work System - Available work actions at this location
    public List<WorkActionDTO> AvailableWork { get; set; } = new List<WorkActionDTO>();
}

/// <summary>
/// Data Transfer Object for work actions available at locations
/// </summary>
public class WorkActionDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; } // Standard, Enhanced, or Service
    public int BaseCoins { get; set; }
    public string LocationId { get; set; }
    public string SpotId { get; set; }
    public int? RequiredTokens { get; set; }
    public string RequiredTokenType { get; set; }
    public string RequiredPermit { get; set; }
    public int? HungerReduction { get; set; }
    public int? HealthRestore { get; set; }
    public string GrantedItem { get; set; }
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