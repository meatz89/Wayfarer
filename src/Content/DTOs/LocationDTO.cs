using System.Text.Json.Serialization;

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

    // HIGHLANDER: NO Q,R coordinates in DTO - hex positions assigned procedurally by LocationPlacementService
    // All locations (authored + generated) placed using single procedural algorithm
    // Placement happens in post-parse initialization phase (PackageLoader.PlaceLocations)

    /// <summary>
    /// PURE PROCEDURAL PLACEMENT: Categorical distance hint for procedural hex positioning.
    /// Valid values: "start" (0-1 hex), "near" (2-5), "medium" (6-12), "far" (13-25), "distant" (26-50).
    /// Algorithm translates hint to radius range, filters venues by distance from player, assigns hex.
    /// NO hardcoded coordinates. NO entity instance IDs. Pure categorical generation.
    /// </summary>
    public string DistanceFromPlayer { get; set; }

    // Additional properties from JSON
    public bool CanInvestigate { get; set; }
    public bool CanWork { get; set; }
    public string WorkType { get; set; }
    public int WorkPay { get; set; }

    // Gameplay properties moved from LocationDTO
    public string Role { get; set; }
    public bool IsStartingLocation { get; set; }
    public string ObligationProfile { get; set; }
    public List<ProfessionsByTimeEntry> AvailableProfessionsByTime { get; set; } = new List<ProfessionsByTimeEntry>();
    public List<WorkActionDTO> AvailableWork { get; set; } = new List<WorkActionDTO>();

    // Orthogonal Categorical Dimensions (Entity Resolution)
    // String values from JSON parsed to enums by LocationParser
    public string Privacy { get; set; }
    public string Safety { get; set; }
    public string Activity { get; set; }
    public string Purpose { get; set; }

    // ORTHOGONAL ENVIRONMENTAL DIMENSIONS (replacing generic capabilities)
    public string Environment { get; set; }  // Indoor, Outdoor, Covered, Underground
    public string Setting { get; set; }  // Urban, Suburban, Rural, Wilderness

    /// <summary>
    /// DEPENDENT LOCATION SPATIAL CONSTRAINT: Categorical proximity to reference location.
    /// Used when this location is generated dynamically (scene activation dependent resource).
    /// Constrains WHERE the location spawns relative to reference (typically activation location).
    /// null = no constraint (standard procedural placement using DistanceFromPlayer).
    /// </summary>
    [JsonPropertyName("proximityConstraint")]
    public ProximityConstraintDTO? ProximityConstraint { get; set; }
}

/// <summary>
/// DTO for ProximityConstraint - defines spatial placement constraints in JSON.
/// Used for dependent locations to maintain verisimilitude ("your room at this inn" must spawn at this inn).
/// </summary>
public class ProximityConstraintDTO
{
    /// <summary>
    /// Categorical proximity relationship: "Anywhere", "SameLocation", "AdjacentLocation", "SameVenue", "SameDistrict", "SameRegion".
    /// </summary>
    [JsonPropertyName("proximity")]
    public string Proximity { get; set; } = "Anywhere";

    /// <summary>
    /// Reference location key: "current" (activation location), "player" (player's location), etc.
    /// </summary>
    [JsonPropertyName("referenceLocation")]
    public string ReferenceLocation { get; set; } = "current";
}
