using System.Collections.Generic;

public class Location
{
    public string Id { get; set; }
    public string Name { get; private set; }
    public string Description { get; set; }

    // Hierarchical organization - Location only knows its District
    public string District { get; set; } // e.g., "Lower Wards"

    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton

    // Tier system (1-5) for difficulty/content progression
    public int Tier { get; set; } = 1;

    // REMOVED: TravelHubSpotId - Travel happens directly between spots with Crossroads property

    public List<LocationConnection> Connections { get; set; } = new List<LocationConnection>();
    public List<string> LocationSpotIds { get; set; } = new List<string>();

    // Environmental properties by time window
    public List<string> MorningProperties { get; set; } = new List<string>();
    public List<string> AfternoonProperties { get; set; } = new List<string>();
    public List<string> EveningProperties { get; set; } = new List<string>();
    public List<string> NightProperties { get; set; } = new List<string>();

    // Tag Resonance System
    public List<string> DomainTags { get; set; } = new List<string>();

    public Population? Population { get; set; }
    public Atmosphere? Atmosphere { get; set; }
    public Physical? Physical { get; set; }
    public Illumination? Illumination { get; set; }

    public int TravelTimeSegments { get; set; }
    public string TravelDescription { get; set; }
    public int Difficulty { get; set; }
    public int Depth { get; set; }
    public LocationTypes LocationType { get; set; } = LocationTypes.Connective;
    public string LocationTypeString { get; set; } // Mechanical property for display type (e.g., "Tavern", "Crossroads")
    public bool IsStartingLocation { get; set; } // Mechanical property for starting location preference
    public List<ServiceTypes> AvailableServices { get; set; } = new List<ServiceTypes>();
    public bool HasBeenVisited { get; set; }
    public int VisitCount { get; set; }
    public bool PlayerKnowledge { get; set; }

    // NPCs currently present at this location (populated at runtime)
    public List<NPC> NPCsPresent { get; set; } = new List<NPC>();

    // Categorical Properties for NPC-Location Logical System Interactions
    public Dictionary<TimeBlocks, List<Professions>> AvailableProfessionsByTime { get; set; } = new Dictionary<TimeBlocks, List<Professions>>();

    // Time-based properties
    public Dictionary<TimeBlocks, List<string>> AvailableActions { get; private set; }
    public Dictionary<TimeBlocks, string> TimeSpecificDescription { get; private set; }
    public Dictionary<TimeBlocks, List<ILocationProperty>> TimeProperties { get; private set; }
    public List<string> ConnectedLocationIds { get; internal set; }
    public List<Item> MarketItems { get; internal set; }
    public List<RestOption> RestOptions { get; internal set; }

    // Access Requirements for this location
    public AccessRequirement AccessRequirement { get; set; }

    // Location Familiarity System (Work Packet 1)
    public int Familiarity { get; set; } = 0;
    public int MaxFamiliarity { get; set; } = 3;
    public int HighestObservationCompleted { get; set; } = 0;

    // Observation Rewards System (Work Packet 3)
    public List<ObservationReward> ObservationRewards { get; set; } = new List<ObservationReward>();

    public Location(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public bool IsProfessionAvailable(Professions profession, TimeBlocks currentTime)
    {
        if (!AvailableProfessionsByTime.ContainsKey(currentTime))
            return false;

        return AvailableProfessionsByTime[currentTime].Contains(profession);
    }

    public List<Professions> GetAvailableProfessions(TimeBlocks currentTime)
    {
        return AvailableProfessionsByTime.ContainsKey(currentTime)
            ? AvailableProfessionsByTime[currentTime]
            : new List<Professions>();
    }

}
