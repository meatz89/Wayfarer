public class Location
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string VenueId { get; set; }
    public Venue Venue { get; internal set; }

    // HEX-BASED TRAVEL SYSTEM: Location is THE primary spatial entity
    /// <summary>
    /// Hex grid position of this location (nullable only during initialization - ALL locations MUST have positions after parsing)
    /// VENUE = 7-hex cluster (center + 6 adjacent hexes). Each location occupies ONE hex in the cluster.
    /// Same-venue movement = moving between ADJACENT hexes in the same cluster (instant/free BECAUSE adjacent).
    /// Cross-venue travel = moving between non-adjacent hexes (requires routes, costs resources).
    /// Source of truth for location spatial positioning - ALL routes connect locations via hex paths.
    /// HIGHLANDER: Location.HexPosition is source of truth, Hex.LocationId is derived lookup.
    /// </summary>
    public AxialCoordinates? HexPosition { get; set; }

    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton

    public List<TimeSpecificProperty> TimeSpecificProperties { get; set; } = new List<TimeSpecificProperty>();

    public List<TimeBlocks> CurrentTimeBlocks { get; set; } = new List<TimeBlocks>();
    public string InitialState { get; set; }
    // Knowledge system eliminated - Understanding resource replaces Knowledge tokens

    // Active situation IDs for this location (Mental/Physical challenges)
    // References situations in GameWorld.Situations (single source of truth)
    public List<string> ActiveSituationIds { get; set; } = new List<string>();

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
    public List<string> DomainTags { get; set; } = new List<string>();
    public LocationTypes LocationType { get; set; } = LocationTypes.Crossroads;
    public bool IsStartingLocation { get; set; } = false;

    /// <summary>
    /// Lock state for location access control
    /// Used by multi-situation scene pattern: locked locations require items to access
    /// Scene rewards can modify this state (unlock), cleanup restores original state (relock)
    /// Tutorial: Upper floor locked until player has room_key, relocked on departure
    /// HIGHLANDER: Location entity owns its lock state, no duplicate tracking
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Provenance tracking: which scene created this location (if any)
    /// null = location from base game content (not dynamically created)
    /// non-null = location created by scene during gameplay (dependent resource)
    /// </summary>
    public SceneProvenance Provenance { get; set; } = null;

    public string? Description { get; internal set; }

    public Location(string id, string name)
    {
        Id = id;
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