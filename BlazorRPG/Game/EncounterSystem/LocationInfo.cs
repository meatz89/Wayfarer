/// <summary>
/// Represents a location in the game where encounters take place
/// </summary>
public class LocationInfo
{
    public string Name { get; }
    public List<ApproachTags> FavoredApproaches { get; } = new List<ApproachTags>();
    public List<ApproachTags> DisfavoredApproaches { get; } = new List<ApproachTags>();
    public List<IEncounterTag> AvailableTags { get; } = new List<IEncounterTag>();

    // Success thresholds
    public int PartialThreshold { get; }
    public int StandardThreshold { get; }
    public int ExceptionalThreshold { get; }

    // Encounter duration in turns
    public int TurnDuration { get; }

    // How hostile is this location (affects tag balance)
    public enum HostilityLevels { Friendly, Neutral, Hostile }
    public HostilityLevels Hostility { get; }

    // Presentation style for this location
    public PresentationStyles Style { get; }

    public LocationTypes LocationType { get; set; } // Industrial/Commercial/etc
    public LocationNames LocationName { get; set; }
    public List<LocationNames> TravelConnections { get; set; }
    public List<LocationSpot> LocationSpots { get; set; } // Action groupings
    public int Difficulty { get; set; }
    public LocationArchetypes LocationArchetype { get; }
    public CrowdDensity CrowdDensity { get; }
    public OpportunityTypes Opportunity { get; }
    public ItemTypes ResourceType { get; }
    public bool PlayerKnowledge { get; }
    public EncounterTypes EncounterType { get; internal set; }

    public LocationInfo(
        string name,
        List<ApproachTags> favoreApproaches,
        List<ApproachTags> disfavoredApproaches,
        int duration,
        int partialThreshold,
        int standardThreshold,
        int exceptionalThreshold,
        HostilityLevels hostility,
        PresentationStyles style)
    {
        Name = name;
        FavoredApproaches = favoreApproaches;
        DisfavoredApproaches = disfavoredApproaches;

        PartialThreshold = partialThreshold;
        StandardThreshold = standardThreshold;
        ExceptionalThreshold = exceptionalThreshold;

        TurnDuration = duration;
        Hostility = hostility;
        Style = style;
    }

    public void AddTag(NarrativeTag narrativeTag)
    {
        AvailableTags.Add(narrativeTag);

    }
    public void AddTag(StrategicTag strategicTag)
    {
        AvailableTags.Add(strategicTag);
    }

    internal void SetDifficulty(int difficulty)
    {
        this.Difficulty = difficulty;
    }

    public int GetEnvironmentalPressure(int turnNumber)
    {
        // Difficulty is the base pressure added each turn
        int pressureFromDifficulty = Difficulty;

        // Additional pressure from hostility (could be used for escalating difficulty)
        int pressureFromHostility = 0;
        switch (Hostility)
        {
            case HostilityLevels.Hostile:
                pressureFromHostility = 1; // +1 additional pressure in hostile locations
                break;
            case HostilityLevels.Neutral:
                pressureFromHostility = turnNumber > 3 ? 1 : 0; // +1 additional pressure after turn 3
                break;
            default:
                pressureFromHostility = 0;
                break;
        }

        return pressureFromDifficulty + pressureFromHostility;
    }

}
