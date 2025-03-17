/// <summary>
/// Represents a location in the game where encounters take place
/// </summary>
public class LocationEncounterInfo
{
    public LocationNames LocationName { get; set; }
    public List<EncounterStateTags> FavoredApproaches { get; } = new List<EncounterStateTags>();
    public List<EncounterStateTags> DisfavoredApproaches { get; } = new List<EncounterStateTags>();
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
    public int Difficulty { get; set; }

    // Presentation style for this location
    public EncounterTypes EncounterType { get; set; }

    public LocationEncounterInfo(
        LocationNames locationName,
        List<EncounterStateTags> favoreApproaches,
        List<EncounterStateTags> disfavoredApproaches,
        int duration,
        int partialThreshold,
        int standardThreshold,
        int exceptionalThreshold,
        HostilityLevels hostility,
        EncounterTypes style)
    {
        LocationName = locationName;
        FavoredApproaches = favoreApproaches;
        DisfavoredApproaches = disfavoredApproaches;

        PartialThreshold = partialThreshold;
        StandardThreshold = standardThreshold;
        ExceptionalThreshold = exceptionalThreshold;

        TurnDuration = duration;
        Hostility = hostility;
        EncounterType = style;
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
