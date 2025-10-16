using System.Collections.Generic;
/// <summary>
/// Data Transfer Object for deserializing route data from JSON.
/// Maps to the structure in routes.json.
/// </summary>
public class RouteDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string OriginVenueId { get; set; }
    public string OriginSpotId { get; set; }
    public string DestinationVenueId { get; set; }
    public string DestinationSpotId { get; set; }
    public string Method { get; set; }
    public int BaseCoinCost { get; set; }
    public int BaseStaminaCost { get; set; }
    public int TravelTimeSegments { get; set; }
    public int CoinCost { get; set; }
    public string PermitRequired { get; set; }
    public string DepartureTime { get; set; }
    public List<string> TerrainCategories { get; set; } = new List<string>();
    public int MaxItemCapacity { get; set; }
    public string Description { get; set; }
    public AccessRequirementDTO AccessRequirement { get; set; }

    // Obstacles on this route (bandits, flooding, difficult terrain)
    public List<ObstacleDTO> Obstacles { get; set; } = new List<ObstacleDTO>();

    // Travel path cards system properties
    public int StartingStamina { get; set; } = 3;
    public List<RouteSegmentDTO> Segments { get; set; } = new List<RouteSegmentDTO>();
    public List<string> EncounterDeckIds { get; set; } = new List<string>();

    // Event system properties
    public List<string> EventPool { get; set; } = new List<string>();
}

/// <summary>
/// Data Transfer Object for route segments containing path card options.
/// </summary>
public class RouteSegmentDTO
{
    public int SegmentNumber { get; set; }
    public string Type { get; set; } = "FixedPath"; // "FixedPath" or "Event"

    // For FixedPath segments: the specific path card collection to use
    public string PathCollectionId { get; set; }

    // For Event segments: the event collection containing events to randomly select from
    public string EventCollectionId { get; set; }

    // Core Loop: Path choices within this segment (1-3 paths with different trade-offs)
    public List<RoutePathDTO> AvailablePaths { get; set; } = new List<RoutePathDTO>();

    // Core Loop: Narrative description of this segment location
    public string NarrativeDescription { get; set; }
}

/// <summary>
/// Data Transfer Object for individual path options within a route segment (Core Loop design).
/// Each segment offers 1-3 paths with different time/stamina/obstacle trade-offs.
/// </summary>
public class RoutePathDTO
{
    public string Id { get; set; }
    public int TimeSegments { get; set; }
    public int StaminaCost { get; set; }
    public string OptionalObstacleId { get; set; }
    public string Description { get; set; }
    public int HiddenUntilExploration { get; set; } = 0;
}