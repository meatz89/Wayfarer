/// <summary>
/// Data Transfer Object for deserializing venue data from JSON.
/// Venue is a CONTAINER for locations - has minimal organizational properties only.
/// </summary>
public class VenueDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DistrictId { get; set; }
    public int Tier { get; set; }
    public string LocationType { get; set; }
    public List<string> locations { get; set; } = new List<string>();
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
    public string VenueId { get; set; }
    public string LocationId { get; set; }
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