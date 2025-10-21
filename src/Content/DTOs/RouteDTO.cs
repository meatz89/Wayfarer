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

    // Controls whether a reverse route should be automatically generated
    public bool CreateBidirectional { get; set; } = true;

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
    public string Type { get; set; } = "FixedPath"; // "FixedPath", "Event", or "Encounter"

    // For FixedPath segments: the specific path card collection to use
    public string PathCollectionId { get; set; }

    // For Event segments: the event collection containing events to randomly select from
    public string EventCollectionId { get; set; }

    // For Encounter segments: mandatory obstacle that must be resolved to proceed
    public string MandatoryObstacleId { get; set; }

    // Narrative description of this segment location
    public string NarrativeDescription { get; set; }
}