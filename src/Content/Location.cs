public class Location
{
    // HIGHLANDER: Name is natural key, NO Id property
    public string Name { get; set; }
    // HIGHLANDER: Object reference ONLY, no VenueId. Private setter enforces single assignment authority.
    public Venue Venue { get; private set; }

    /// <summary>
    /// INTERNAL SETTER: Assign venue during procedural placement.
    /// Called ONLY by LocationPlacementService.PlaceLocationsInVenue().
    /// Private setter prevents backdoor assignments from other code.
    /// </summary>
    internal void AssignVenue(Venue venue)
    {
        Venue = venue;
    }

    /// <summary>
    /// TEMPORARY: Categorical distance hint for pure procedural placement.
    /// Valid values: "start", "near", "medium", "far", "distant".
    /// Flows from JSON → DTO → Parser → PlaceLocation algorithm.
    /// Set during parsing, consumed during placement, can be discarded after.
    /// NOT persisted - purely for initialization phase.
    /// </summary>
    public string DistanceHintForPlacement { get; set; }

    /// <summary>
    /// SCAFFOLDING PROPERTY: Temporary spatial placement constraint for dependent locations.
    /// Set by parser when ProximityConstraintDTO is present, used by LocationPlacementService, then cleared.
    /// Defines WHERE location spawns relative to reference location (SameVenue, AdjacentLocation, etc.).
    /// NOT persisted in game state - purely procedural generation metadata.
    /// </summary>
    public ProximityConstraint? ProximityConstraintForPlacement { get; set; }

    // HEX-BASED TRAVEL SYSTEM: Location is THE primary spatial entity
    /// <summary>
    /// Hex grid position of this location (nullable only during initialization - ALL locations MUST have positions after parsing)
    /// VENUE = 7-hex cluster (center + 6 adjacent hexes). Each location occupies ONE hex in the cluster.
    /// Same-venue movement = moving between ADJACENT hexes in the same cluster (instant/free BECAUSE adjacent).
    /// Cross-venue travel = moving between non-adjacent hexes (requires routes, costs resources).
    /// Source of truth for location spatial positioning - ALL routes connect locations via hex paths.
    /// HIGHLANDER: Location.HexPosition is source of truth, Hex.Location is derived lookup (object reference).
    /// </summary>
    public AxialCoordinates? HexPosition { get; set; }

    // Skeleton tracking - NO DEFAULTS: explicit assignment required
    public bool IsSkeleton { get; set; }
    public string SkeletonSource { get; set; } // What created this skeleton


    /// <summary>
    /// Functional capabilities - what this location CAN DO (not what it IS).
    /// Uses Flags enum for efficient bitwise operations and combination checking.
    /// Separated from categorical dimensions (Privacy/Safety/Activity/Purpose).
    /// NO DEFAULTS: Capabilities.None must be explicitly set if location has no capabilities
    /// </summary>
    public LocationCapability Capabilities { get; set; }

    // NO DEFAULTS: All numeric properties must be explicitly set
    public int FlowModifier { get; set; }
    public int Tier { get; set; }

    public int TravelTimeSegments { get; set; }
    public string TravelDescription { get; set; }
    public int Difficulty { get; set; }
    public bool HasBeenVisited { get; set; }
    public int VisitCount { get; set; }
    public List<NPC> NPCsPresent { get; set; } = new List<NPC>();

    public Dictionary<TimeBlocks, List<Professions>> AvailableProfessionsByTime { get; set; } = new Dictionary<TimeBlocks, List<Professions>>();
    public Dictionary<TimeBlocks, List<string>> AvailableActions { get; private set; }
    public Dictionary<TimeBlocks, string> TimeSpecificDescription { get; private set; }
    public List<string> ConnectedVenueIds { get; internal set; }
    public List<Item> MarketItems { get; internal set; }
    public List<RestOption> RestOptions { get; internal set; }

    // NO DEFAULTS: All progression/mastery properties must be explicitly initialized
    public int Familiarity { get; set; }
    public int MaxFamiliarity { get; set; }
    public int HighestObservationCompleted { get; set; }
    // ObservationRewards system eliminated - replaced by transparent resource competition
    public List<WorkAction> AvailableWork { get; set; } = new List<WorkAction>();

    // Localized mastery - InvestigationCubes reduce Mental Exposure at THIS location only
    // 0-10 scale: 0 cubes = full exposure, 10 cubes = mastery (no exposure)
    // NO DEFAULTS: explicit initialization required
    public int InvestigationCubes { get; set; }

    // Gameplay properties - NO DEFAULTS: must be explicitly set by parser
    public ObligationDiscipline ObligationProfile { get; set; }
    public List<string> DomainTags { get; set; } = new List<string>(); // DEPRECATED: Used only for DEPENDENT_LOCATION marker system
    public LocationTypes LocationType { get; set; }
    public bool IsStartingLocation { get; set; }

    // Orthogonal Categorical Dimensions (Entity Resolution)
    // These dimensions compose to create location archetypes
    // Example: SemiPublic + Safe + Moderate + Dwelling = Inn common room
    // NO DEFAULTS: All categorical dimensions MUST be explicitly set by parser or throw exception
    public LocationPrivacy Privacy { get; set; }
    public LocationSafety Safety { get; set; }
    public LocationActivity Activity { get; set; }
    public LocationPurpose Purpose { get; set; }

    /// <summary>
    /// Explicit discriminator for accessibility model.
    /// CLEAN ARCHITECTURE: Uses explicit enum instead of null-as-domain-meaning.
    /// Defaults to Authored (base game content).
    /// Set to SceneCreated by DependentResourceOrchestrationService when creating dependent locations.
    /// </summary>
    public LocationOrigin Origin { get; set; } = LocationOrigin.Authored;

    /// <summary>
    /// Provenance tracking: forensic metadata about which scene created this location.
    /// Only populated when Origin == SceneCreated.
    /// Contains: Scene reference, creation timestamp (day/timeblock/segment).
    /// Used for: cleanup coordination, resource lifecycle tracking, debug queries.
    /// NOT used for accessibility decisions - use Origin enum instead.
    /// </summary>
    public SceneProvenance Provenance { get; set; } = null;

    public string? Description { get; internal set; }

    // ADR-007: Constructor uses Name only (natural key, no Id parameter)
    public Location(string name)
    {
        Name = name;
    }

}