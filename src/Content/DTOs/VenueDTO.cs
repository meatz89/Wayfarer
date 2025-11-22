/// <summary>
/// Data Transfer Object for deserializing venue data from JSON.
/// SPATIAL ARCHITECTURE: Venue defines hex territory FIRST, locations placed within SECOND.
/// Venue specifies center hex + allocation strategy to claim spatial territory.
/// </summary>
public class VenueDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DistrictId { get; set; }
    public int Tier { get; set; }
    public string LocationType { get; set; }

    /// <summary>
    /// Center hex coordinates for venue spatial cluster.
    /// Required for all authored venues. Parser converts to AxialCoordinates.
    /// </summary>
    public HexCoordinateDTO CenterHex { get; set; }

    /// <summary>
    /// Hex allocation strategy (SingleHex, ClusterOf7).
    /// Defaults to ClusterOf7 if not specified.
    /// </summary>
    public string HexAllocation { get; set; }

    /// <summary>
    /// Maximum locations allowed in this venue (capacity budget).
    /// Defaults to 20 if not specified.
    /// </summary>
    public int? MaxLocations { get; set; }

    public List<string> locations { get; set; } = new List<string>();
}

/// <summary>
/// Hex coordinate pair for JSON deserialization.
/// Parser converts to AxialCoordinates domain type.
/// </summary>
public class HexCoordinateDTO
{
    public int Q { get; set; }
    public int R { get; set; }
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