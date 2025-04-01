/// <summary>
/// Represents a location in the game where encounters take place
/// </summary>
public class EncounterInfo
{
    public string Name { get; set; }
    public string LocationSpotName { get; set; }
    public List<FocusTags> PressureReducingFocuses { get; } = new List<FocusTags>();
    public List<FocusTags> MomentumReducingFocuses { get; } = new List<FocusTags>();
    public List<ApproachTags> MomentumBoostApproaches { get; } = new List<ApproachTags>();
    public List<ApproachTags> DangerousApproaches { get; } = new List<ApproachTags>();
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
    public EncounterTypes Type { get; set; }

    public EncounterInfo(
        string locationName,
        string locationSpot,
        List<ApproachTags> favoreApproaches,
        List<ApproachTags> dangerousApproaches,
        List<FocusTags> PressureReducingFocuses,
        List<FocusTags> MomentumReducingFocuses,
        int duration,
        int maxPressure,
        int partialThreshold,
        int standardThreshold,
        int exceptionalThreshold,
        HostilityLevels hostility,
        EncounterTypes style)
    {
        this.Name = locationName;
        this.LocationSpotName = locationSpot;

        this.MomentumBoostApproaches = favoreApproaches;
        this.DangerousApproaches = dangerousApproaches;
        this.PressureReducingFocuses = PressureReducingFocuses;
        this.MomentumReducingFocuses = MomentumReducingFocuses;

        this.PartialThreshold = partialThreshold;
        this.StandardThreshold = standardThreshold;
        this.ExceptionalThreshold = exceptionalThreshold;

        this.TurnDuration = duration;
        this.Hostility = hostility;
        this.Type = style;
        this.MaxPressure = maxPressure;
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
