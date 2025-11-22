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

    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton

    public List<TimeSpecificProperty> TimeSpecificProperties { get; set; } = new List<TimeSpecificProperty>();

    public List<TimeBlocks> CurrentTimeBlocks { get; set; } = new List<TimeBlocks>();
    public string InitialState { get; set; }
    // Knowledge system eliminated - Understanding resource replaces Knowledge tokens

    // NOTE: ActiveSituationIds DELETED - situations embedded in scenes
    // Query GameWorld.Scenes.SelectMany(s => s.Situations).Where(sit => sit.Location == this)

    // NOTE: SceneIds removed - OLD equipment-based Scene system deleted
    // NEW Scene-Situation architecture: Query GameWorld.Scenes by PlacementType/PlacementId
    public List<LocationPropertyType> LocationProperties { get; set; } = new List<LocationPropertyType>();
    public List<string> Properties => LocationProperties.Select(p => p.ToString()).ToList();

    public int FlowModifier { get; set; } = 0;
    public int Tier { get; set; } = 1;

    // DELETED: Legacy time properties (use TimeSpecificProperties dictionary instead)
    // MorningProperties, AfternoonProperties, EveningProperties, NightProperties

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

    public int Familiarity { get; set; } = 0;
    public int MaxFamiliarity { get; set; } = 3;
    public int HighestObservationCompleted { get; set; } = 0;
    // ObservationRewards system eliminated - replaced by transparent resource competition
    public List<WorkAction> AvailableWork { get; set; } = new List<WorkAction>();

    // Localized mastery - InvestigationCubes reduce Mental Exposure at THIS location only
    // 0-10 scale: 0 cubes = full exposure, 10 cubes = mastery (no exposure)
    public int InvestigationCubes { get; set; } = 0;

    // Gameplay properties moved from Location
    public ObligationDiscipline ObligationProfile { get; set; } = ObligationDiscipline.Research;
    public List<string> DomainTags { get; set; } = new List<string>(); // DEPRECATED: Used only for DEPENDENT_LOCATION marker system
    public LocationTypes LocationType { get; set; } = LocationTypes.Crossroads;
    public bool IsStartingLocation { get; set; } = false;

    // Orthogonal Categorical Dimensions (Entity Resolution)
    // These dimensions compose to create location archetypes
    // Example: SemiPublic + Safe + Moderate + Dwelling = Inn common room
    public LocationPrivacy Privacy { get; set; } = LocationPrivacy.Public;
    public LocationSafety Safety { get; set; } = LocationSafety.Neutral;
    public LocationActivity Activity { get; set; } = LocationActivity.Moderate;
    public LocationPurpose Purpose { get; set; } = LocationPurpose.Transit;

    /// <summary>
    /// Provenance tracking: which scene created this location (if any)
    /// null = location from base game content (not dynamically created)
    /// non-null = location created by scene during gameplay (dependent resource)
    /// </summary>
    public SceneProvenance Provenance { get; set; } = null;

    public string? Description { get; internal set; }

    // ADR-007: Constructor uses Name only (natural key, no Id parameter)
    public Location(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Get active properties for the current time, combining base and time-specific
    /// </summary>
    public List<LocationPropertyType> GetActiveProperties(TimeBlocks currentTime)
    {
        List<LocationPropertyType> activeProperties = new List<LocationPropertyType>(LocationProperties);

        if (TimeSpecificProperties.Any(t => t.TimeBlock == currentTime))
        {
            activeProperties.AddRange(TimeSpecificProperties.First(t => t.TimeBlock == currentTime).Properties);
        }

        return activeProperties;
    }

    /// <summary>
    /// Calculate the flow modifier for conversations at this location
    /// Based on location properties and the NPC's personality
    /// </summary>
    public int CalculateFlowModifier(PersonalityType npcPersonality, TimeBlocks currentTime)
    {
        int modifier = FlowModifier; // Base modifier from location

        // Get all active properties (base + time-specific)
        List<LocationPropertyType> activeProperties = new List<LocationPropertyType>(LocationProperties);
        if (TimeSpecificProperties.Any(t => t.TimeBlock == currentTime))
        {
            activeProperties.AddRange(TimeSpecificProperties.First(t => t.TimeBlock == currentTime).Properties);
        }

        // Apply property-based modifiers
        foreach (LocationPropertyType property in activeProperties)
        {
            switch (property)
            {
                case LocationPropertyType.Private:
                    modifier += 2;
                    break;
                case LocationPropertyType.Discrete:
                    modifier += 1;
                    break;
                case LocationPropertyType.Exposed:
                    modifier -= 1;
                    break;
                case LocationPropertyType.Quiet:
                    if (npcPersonality == PersonalityType.DEVOTED || npcPersonality == PersonalityType.CUNNING)
                        modifier += 1;
                    break;
                case LocationPropertyType.Loud:
                    if (npcPersonality == PersonalityType.MERCANTILE)
                        modifier += 1;
                    else
                        modifier -= 1;
                    break;
                case LocationPropertyType.NobleFavored:
                    if (npcPersonality == PersonalityType.PROUD)
                        modifier += 1;
                    break;
                case LocationPropertyType.CommonerHaunt:
                    if (npcPersonality == PersonalityType.STEADFAST)
                        modifier += 1;
                    break;
                case LocationPropertyType.MerchantHub:
                    if (npcPersonality == PersonalityType.MERCANTILE)
                        modifier += 1;
                    break;
                case LocationPropertyType.SacredGround:
                    if (npcPersonality == PersonalityType.DEVOTED)
                        modifier += 1;
                    break;
            }
        }

        return modifier;
    }

}