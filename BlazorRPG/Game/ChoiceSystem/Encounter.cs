/// <summary>
/// Manages the state and progression of an Encounter
/// </summary>
public class Encounter
{
    public NarrativePhases NarrativePhase { get; internal set; }

    public EncounterContext EncounterContext = new EncounterContext();
    public EncounterHistory History = new EncounterHistory();
    public EncounterState State;

    public Encounter(
        EncounterContext context,
        Dictionary<ApproachTypes, int> initialApproachTypesDict = null,
        Dictionary<FocusTypes, int> initialFocusTypesDict = null)
    {
        EncounterContext = context;
        State = new EncounterState();

        // Set some initial tag values
        State.ApproachTypesDic[ApproachTypes.Force] = 1;
        State.ApproachTypesDic[ApproachTypes.Charm] = 2;
        State.FocusTypesDic[FocusTypes.Relationship] = 1;
        State.FocusTypesDic[FocusTypes.Information] = 2;

        // Set up initial tags if provided
        if (initialApproachTypesDict != null && initialFocusTypesDict != null)
        {
            State.SetInitialTags(initialApproachTypesDict, initialFocusTypesDict);
        }
    }

    public void SetChoices(List<Choice> currentChoices)
    {
        State.CurrentChoices = currentChoices;
    }

    /// <summary>
    /// Check if the Encounter is complete
    /// </summary>
    public bool IsComplete()
    {
        // Encounter ends if we reach the duration or hit failure condition
        return State.CurrentTurn > EncounterContext.Location.Duration || State.Pressure >= 10;
    }

    /// <summary>
    /// Get the current outcome of the Encounter
    /// </summary>
    public (bool Succeeded, string Result) GetOutcome()
    {
        return State.GetOutcome(EncounterContext.Location);
    }
}