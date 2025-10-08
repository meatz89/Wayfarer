using System.Collections.Generic;

public class LocationSpot
{
    public string Id { get; set; }
    public string Name { get; set; }
    // Description removed - generated from SpotPropertyType combinations
    public string LocationId { get; set; }

    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton

    public Dictionary<TimeBlocks, List<SpotPropertyType>> TimeSpecificProperties { get; set; } = new Dictionary<TimeBlocks, List<SpotPropertyType>>();

    public List<TimeBlocks> CurrentTimeBlocks { get; set; } = new List<TimeBlocks>();
    public string InitialState { get; set; }
    public bool PlayerKnowledge { get; set; }

    public AccessRequirement AccessRequirement { get; set; }

    public List<ChallengeGoal> Goals { get; set; } = new List<ChallengeGoal>();
    public List<SpotPropertyType> SpotProperties { get; set; } = new List<SpotPropertyType>();
    public List<string> Properties => SpotProperties.Select(p => p.ToString()).ToList();

    public int FlowModifier { get; set; } = 0;
    public int Tier { get; set; } = 1;

    public string LocationTypeString { get; set; } // Mechanical property for display type (e.g., "Tavern", "Crossroads")
    public List<string> MorningProperties { get; set; } = new List<string>();
    public List<string> AfternoonProperties { get; set; } = new List<string>();
    public List<string> EveningProperties { get; set; } = new List<string>();
    public List<string> NightProperties { get; set; } = new List<string>();
    public int TravelTimeSegments { get; set; }
    public string TravelDescription { get; set; }
    public int Difficulty { get; set; }
    public LocationTypes LocationType { get; set; } = LocationTypes.Connective;
    public List<ServiceTypes> AvailableServices { get; set; } = new List<ServiceTypes>();
    public bool HasBeenVisited { get; set; }
    public int VisitCount { get; set; }
    public List<NPC> NPCsPresent { get; set; } = new List<NPC>();

    public Dictionary<TimeBlocks, List<Professions>> AvailableProfessionsByTime { get; set; } = new Dictionary<TimeBlocks, List<Professions>>();
    public Dictionary<TimeBlocks, List<string>> AvailableActions { get; private set; }
    public Dictionary<TimeBlocks, string> TimeSpecificDescription { get; private set; }
    public List<string> ConnectedLocationIds { get; internal set; }
    public List<Item> MarketItems { get; internal set; }
    public List<RestOption> RestOptions { get; internal set; }

    public int Familiarity { get; set; } = 0;
    public int MaxFamiliarity { get; set; } = 3;
    public int HighestObservationCompleted { get; set; } = 0;
    public List<ObservationReward> ObservationRewards { get; set; } = new List<ObservationReward>();
    public List<WorkAction> AvailableWork { get; set; } = new List<WorkAction>();

    public int Exposure { get; set; } = 0;

    public InvestigationDiscipline InvestigationProfile { get; set; } = InvestigationDiscipline.Research;


    public LocationSpot(string id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Get active properties for the current time, combining base and time-specific
    /// </summary>
    public List<SpotPropertyType> GetActiveProperties(TimeBlocks currentTime)
    {
        List<SpotPropertyType> activeProperties = new List<SpotPropertyType>(SpotProperties);

        if (TimeSpecificProperties.ContainsKey(currentTime))
        {
            activeProperties.AddRange(TimeSpecificProperties[currentTime]);
        }

        return activeProperties;
    }

    /// <summary>
    /// Calculate the flow modifier for conversations at this spot
    /// Based on spot properties and the NPC's personality
    /// </summary>
    public int CalculateFlowModifier(PersonalityType npcPersonality, TimeBlocks currentTime)
    {
        int modifier = FlowModifier; // Base modifier from spot

        // Get all active properties (base + time-specific)
        List<SpotPropertyType> activeProperties = new List<SpotPropertyType>(SpotProperties);
        if (TimeSpecificProperties.ContainsKey(currentTime))
        {
            activeProperties.AddRange(TimeSpecificProperties[currentTime]);
        }

        // Apply property-based modifiers
        foreach (SpotPropertyType property in activeProperties)
        {
            switch (property)
            {
                case SpotPropertyType.Private:
                    modifier += 2;
                    break;
                case SpotPropertyType.Discrete:
                    modifier += 1;
                    break;
                case SpotPropertyType.Exposed:
                    modifier -= 1;
                    break;
                case SpotPropertyType.Quiet:
                    if (npcPersonality == PersonalityType.DEVOTED || npcPersonality == PersonalityType.CUNNING)
                        modifier += 1;
                    break;
                case SpotPropertyType.Loud:
                    if (npcPersonality == PersonalityType.MERCANTILE)
                        modifier += 1;
                    else
                        modifier -= 1;
                    break;
                case SpotPropertyType.NobleFavored:
                    if (npcPersonality == PersonalityType.PROUD)
                        modifier += 1;
                    break;
                case SpotPropertyType.CommonerHaunt:
                    if (npcPersonality == PersonalityType.STEADFAST)
                        modifier += 1;
                    break;
                case SpotPropertyType.MerchantHub:
                    if (npcPersonality == PersonalityType.MERCANTILE)
                        modifier += 1;
                    break;
                case SpotPropertyType.SacredGround:
                    if (npcPersonality == PersonalityType.DEVOTED)
                        modifier += 1;
                    break;
            }
        }

        return modifier;
    }
}