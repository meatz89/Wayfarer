using System.Collections.Generic;
/// <summary>
/// Data Transfer Object for deserializing route data from JSON.
/// Maps to the structure in routes.json.
/// </summary>
public class RouteDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string OriginLocationId { get; set; }
    public string OriginSpotId { get; set; }
    public string DestinationLocationId { get; set; }
    public string DestinationSpotId { get; set; }
    public string Method { get; set; }
    public int BaseCoinCost { get; set; }
    public int BaseStaminaCost { get; set; }
    public int TravelTimeMinutes { get; set; }
    public int CoinCost { get; set; }
    public string PermitRequired { get; set; }
    public string DepartureTime { get; set; }
    public bool IsDiscovered { get; set; }
    public List<string> TerrainCategories { get; set; } = new List<string>();
    public int MaxItemCapacity { get; set; }
    public string Description { get; set; }
    public AccessRequirementDTO AccessRequirement { get; set; }
    
    // Travel path cards system properties
    public int StartingStamina { get; set; } = 3;
    public List<RouteSegmentDTO> Segments { get; set; } = new List<RouteSegmentDTO>();
    public List<string> EncounterDeckIds { get; set; } = new List<string>();
}

/// <summary>
/// Data Transfer Object for route segments containing path card options.
/// </summary>
public class RouteSegmentDTO
{
    public int SegmentNumber { get; set; }
    public List<string> PathCardIds { get; set; } = new List<string>();
}