/// <summary>
/// Represents a location in the game where encounters take place
/// </summary>
public class Encounter
{
    public string LocationName { get; set; }
    public string LocationSpotName { get; set; }
    public List<IEncounterTag> AllEncounterTags { get; } = new List<IEncounterTag>();

    // Success thresholds
    public int PartialThreshold { get; }
    public int StandardThreshold { get; }
    public int ExceptionalThreshold { get; }

    // Encounter duration in turns
    public int MaxTurns { get; }
    public int MaxPressure { get; }

    // How hostile is this location (affects tag balance)
    public enum HostilityLevels { Friendly, Neutral, Hostile }
    public HostilityLevels Hostility { get; }
    public int EncounterDifficulty { get; set; }

    // Presentation style for this location
    public EncounterTypes EncounterType { get; set; }

    public Encounter(
        string locationName,
        string locationSpot,
        int duration,
        int maxPressure,
        int partialThreshold,
        int standardThreshold,
        int exceptionalThreshold,
        HostilityLevels hostility,
        EncounterTypes style)
    {
        this.LocationName = locationName;
        this.LocationSpotName = locationSpot;

        this.PartialThreshold = partialThreshold;
        this.StandardThreshold = standardThreshold;
        this.ExceptionalThreshold = exceptionalThreshold;

        this.MaxTurns = duration;
        this.Hostility = hostility;
        this.EncounterType = style;
        this.MaxPressure = maxPressure;
    }

    public void AddTag(NarrativeTag narrativeTag)
    {
        AllEncounterTags.Add(narrativeTag);

    }
    public void AddTag(StrategicTag strategicTag)
    {
        AllEncounterTags.Add(strategicTag);
    }

    public void SetDifficulty(int difficulty)
    {
        this.EncounterDifficulty = difficulty;
    }

    public int GetEnvironmentalPressure(int turnNumber)
    {
        int pressureFromDifficulty = EncounterDifficulty;

        int pressureFromHostility = 0;
        switch (Hostility)
        {
            case HostilityLevels.Hostile:
                pressureFromHostility = 2;
                break;
            case HostilityLevels.Neutral:
                pressureFromHostility = 1;
                break;
            default:
                pressureFromHostility = 0;
                break;
        }

        return pressureFromDifficulty + pressureFromHostility;
    }

}
