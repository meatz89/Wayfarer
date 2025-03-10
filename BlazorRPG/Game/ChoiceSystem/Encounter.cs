using BlazorRPG.Game.EncounterManager;

/// <summary>
/// Manages the state and progression of an Encounter
/// </summary>
public class Encounter
{
    public NarrativePhases NarrativePhase { get; internal set; }

    public EncounterContext EncounterContext = new EncounterContext();
    public EncounterHistory History = new EncounterHistory();

    public Encounter(
        EncounterContext context,
        Dictionary<ApproachTypes, int> initialApproachTypesDict = null,
        Dictionary<FocusTypes, int> initialFocusTypesDict = null)
    {
        EncounterContext = context;
    }
}