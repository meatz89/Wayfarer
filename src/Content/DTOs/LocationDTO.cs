/// <summary>
/// Data Transfer Object for deserializing Venue location data from JSON.
/// SPATIAL ARCHITECTURE: Venue determined by hex position containment (not explicit venueId).
/// Parser validates location hex is within a venue's allocated cluster.
/// EntityResolver.FindOrCreate pattern for categorical location matching (DDR-006).
/// </summary>
public class LocationDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string InitialState { get; set; }

    // HIGHLANDER: NO Q,R coordinates in DTO - hex positions assigned procedurally by LocationPlacementService
    // All locations (authored + generated) placed using single procedural algorithm
    // Placement happens in post-parse initialization phase (PackageLoader.PlaceAllLocations)

    /// <summary>
    /// TEMPORARY: Venue reference by name for backward compatibility.
    /// TODO: Delete after spatial venue assignment implemented (venue determined by hex containment).
    /// </summary>
    public string VenueId { get; set; }

    public List<string> CurrentTimeBlocks { get; set; } = new List<string>();
    public List<string> DomainTags { get; set; } = new List<string>();

    // The JSON has a "properties" object with time-based keys
    public LocationPropertiesDTO Properties { get; set; } = new LocationPropertiesDTO();

    // Additional properties from JSON
    public bool CanInvestigate { get; set; }
    public bool CanWork { get; set; }
    public string WorkType { get; set; }
    public int WorkPay { get; set; }

    // NOTE: Old SceneDTO system deleted - ObservationScene (Mental) and TravelScene (Physical) are SEPARATE systems
    // NEW Scene-Situation architecture uses GameWorld.Scenes with PlacementType/PlacementId filtering

    // Gameplay properties moved from LocationDTO
    public string LocationType { get; set; }
    public bool IsStartingLocation { get; set; }
    public string ObligationProfile { get; set; }
    public Dictionary<string, List<string>> AvailableProfessionsByTime { get; set; } = new Dictionary<string, List<string>>();
    public List<WorkActionDTO> AvailableWork { get; set; } = new List<WorkActionDTO>();

    // Orthogonal Categorical Dimensions (Entity Resolution)
    // String values from JSON parsed to enums by LocationParser
    public string Privacy { get; set; }
    public string Safety { get; set; }
    public string Activity { get; set; }
    public string Purpose { get; set; }
}
