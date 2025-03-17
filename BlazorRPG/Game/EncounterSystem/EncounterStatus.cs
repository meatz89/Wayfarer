/// <summary>
/// Represents the current status of an encounter
/// </summary>
public class EncounterStatus
{
    public int CurrentTurn { get; }
    public int MaxTurns { get; }
    public int Momentum { get; }
    public int Pressure { get; }
    public Dictionary<EncounterStateTags, int> ApproachTags { get; }
    public Dictionary<FocusTags, int> FocusTags { get; }
    public List<string> ActiveTagNames { get; }

    // Added properties
    public List<IEncounterTag> ActiveTags { get; }
    public LocationInfo Location { get; }
    public LocationTypes LocationType => Location?.LocationType ?? LocationTypes.Neutral;
    public EncounterTypes EncounterType => Location?.EncounterType ?? EncounterTypes.Physical;

    public EncounterStatus(
        int currentTurn,
        int maxTurns,
        int momentum,
        int pressure,
        Dictionary<EncounterStateTags, int> approachTags,
        Dictionary<FocusTags, int> focusTags,
        List<string> activeTagNames)
    {
        CurrentTurn = currentTurn;
        MaxTurns = maxTurns;
        Momentum = momentum;
        Pressure = pressure;
        ApproachTags = approachTags;
        FocusTags = focusTags;
        ActiveTagNames = activeTagNames;

        // Initialize empty collections for the added properties
        ActiveTags = new List<IEncounterTag>();
        Location = null;
    }

}
