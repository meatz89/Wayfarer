
/// <summary>
/// Manages the state and progression of an Encounter
/// </summary>
public class Encounter
{
    private EncounterState _state;

    private int _currentTurn = 1;
    private List<Choice> _currentChoices;
    public int CurrentTurn => _currentTurn;

    public EncounterState State => _state;
    public IReadOnlyList<Choice> CurrentChoices => _currentChoices.AsReadOnly();
    public EncounterContext EncounterContext = new EncounterContext();
    public EncounterHistory History = new EncounterHistory();

    public Encounter(
        EncounterContext context,
        Dictionary<ApproachTypes, int> initialApproachTypesDict = null,
        Dictionary<FocusTypes, int> initialFocusTypesDict = null)
    {
        EncounterContext = context;
        _state = new EncounterState();

        // Set up initial tags if provided
        if (initialApproachTypesDict != null && initialFocusTypesDict != null)
        {
            _state.SetInitialTags(initialApproachTypesDict, initialFocusTypesDict);
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
        _state = _state.ApplyChoice(choice);

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
        return _currentTurn > EncounterContext.Location.Duration || _state.Pressure >= 10;
    }

    /// <summary>
    /// Get the current outcome of the Encounter
    /// </summary>
    public (bool Succeeded, string Result) GetOutcome()
    {
        return _state.GetOutcome(EncounterContext.Location);
    }
}

