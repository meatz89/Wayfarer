using System.Collections.Generic;

public class Location
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string VenueId { get; set; }
    public Venue Venue { get; internal set; }

    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton

    public Dictionary<TimeBlocks, List<LocationPropertyType>> TimeSpecificProperties { get; set; } = new Dictionary<TimeBlocks, List<LocationPropertyType>>();

    public List<TimeBlocks> CurrentTimeBlocks { get; set; } = new List<TimeBlocks>();
    public string InitialState { get; set; }
    public bool PlayerKnowledge { get; set; }

    public AccessRequirement AccessRequirement { get; set; }

    public List<Goal> Goals { get; set; } = new List<Goal>();
    public List<LocationPropertyType> LocationProperties { get; set; } = new List<LocationPropertyType>();
    public List<string> Properties => LocationProperties.Select(p => p.ToString()).ToList();

    public int FlowModifier { get; set; } = 0;
    public int Tier { get; set; } = 1;

    // DELETED: Legacy time properties (use TimeSpecificProperties dictionary instead)
    // MorningProperties, AfternoonProperties, EveningProperties, NightProperties

    public int TravelTimeSegments { get; set; }
    public string TravelDescription { get; set; }
    public int Difficulty { get; set; }
    public List<ServiceTypes> AvailableServices { get; set; } = new List<ServiceTypes>();
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
    public List<ObservationReward> ObservationRewards { get; set; } = new List<ObservationReward>();
    public List<WorkAction> AvailableWork { get; set; } = new List<WorkAction>();

    public int Exposure { get; set; } = 0;

    // Gameplay properties moved from Location
    public InvestigationDiscipline InvestigationProfile { get; set; } = InvestigationDiscipline.Research;
    public List<string> DomainTags { get; set; } = new List<string>();
    public LocationTypes LocationType { get; set; } = LocationTypes.Crossroads;
    public bool IsStartingLocation { get; set; } = false;
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

        if (TimeSpecificProperties.ContainsKey(currentTime))
        {
            activeProperties.AddRange(TimeSpecificProperties[currentTime]);
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
        if (TimeSpecificProperties.ContainsKey(currentTime))
        {
            activeProperties.AddRange(TimeSpecificProperties[currentTime]);
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