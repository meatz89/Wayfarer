/// <summary>
/// Manages the state and progression of an Encounter
/// </summary>
public class Encounter
{

    private int _currentTurn = 1;
    private List<Choice> _currentChoices;
    public int CurrentTurn => _currentTurn;
    
    public IReadOnlyList<Choice> CurrentChoices => _currentChoices.AsReadOnly();

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
        _currentChoices = currentChoices;
    }

    /// <summary>
    /// Make a choice and advance to the next turn
    /// </summary>
    public EncounterState MakeChoice(Choice choice)
    {
        // Verify the choice is valid
        if (!_currentChoices.Contains(choice))
        {
            throw new ArgumentException("Invalid choice: not in current choice set");
        }

        // Apply the choice effects
        State = State.ApplyChoice(choice);

        // Advance to the next turn
        _currentTurn++;

        return State;
    }

    /// <summary>
    /// Check if the Encounter is complete
    /// </summary>
    public bool IsComplete()
    {
        // Encounter ends if we reach the duration or hit failure condition
        return _currentTurn > EncounterContext.Location.Duration || State.Pressure >= 10;
    }

    /// <summary>
    /// Get the current outcome of the Encounter
    /// </summary>
    public (bool Succeeded, string Result) GetOutcome()
    {
        return State.GetOutcome(EncounterContext.Location);
    }
}

