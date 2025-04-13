/// <summary>
/// Represents a location in the game where encounters take place
/// </summary>
public class EncounterInfo
{
    public string LocationName { get; set; }
    public string LocationSpotName { get; set; }
    public List<IEncounterTag> AvailableTags { get; } = new List<IEncounterTag>();

    // Success thresholds
    public int PartialThreshold { get; }
    public int StandardThreshold { get; }
    public int ExceptionalThreshold { get; }

    // Encounter duration in turns
    public int TurnDuration { get; }
    public int MaxPressure { get; }

    // How hostile is this location (affects tag balance)
    public enum HostilityLevels { Friendly, Neutral, Hostile }
    public HostilityLevels Hostility { get; }
    public int Difficulty { get; set; }

    // Presentation style for this location
    public EncounterTypes EncounterType { get; set; }

    public EncounterInfo(
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

        this.TurnDuration = duration;
        this.Hostility = hostility;
        this.EncounterType = style;
        this.MaxPressure = maxPressure;
    }

    public void AddTag(NarrativeTag narrativeTag)
    {
        AvailableTags.Add(narrativeTag);

    }
    public void AddTag(EnvironmentPropertyTag strategicTag)
    {
        AvailableTags.Add(strategicTag);
    }

    public void SetDifficulty(int difficulty)
    {
        this.Difficulty = difficulty;
    }

    public int GetEnvironmentalPressure(int turnNumber)
    {
        int pressureFromDifficulty = Difficulty;

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
