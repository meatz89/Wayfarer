public class ActionStateTracker
{
    // Current action information
    public UserActionOption CurrentAction { get; private set; }
    public bool IsActiveEncounter { get; private set; }
    public EncounterResult EncounterResult { get; set; }
    public EncounterManager CurrentEncounter { get; private set; }

    // Action options
    public List<UserActionOption> LocationSpotActions { get; private set; } = new List<UserActionOption>();
    public List<UserActionOption> GlobalActions { get; private set; } = new List<UserActionOption>();
    public List<UserEncounterChoiceOption> UserEncounterChoiceOptions { get; private set; } = new List<UserEncounterChoiceOption>();

    public GameStateMoment PreviousState { get; private set; }

    public void SaveCurrentState(GameState gameState)
    {
        PreviousState = new GameStateMoment(gameState);
    }

    public void SetCurrentUserAction(UserActionOption action)
    {
        CurrentAction = action;
    }

    public void ClearCurrentUserAction()
    {
        CurrentAction = null;
    }

    public void SetActiveEncounter(EncounterManager encounter = null)
    {
        IsActiveEncounter = true;
        CurrentEncounter = encounter;
    }

    public void CompleteAction()
    {
        IsActiveEncounter = false;
        CurrentEncounter = null;
    }

    public EncounterManager GetCurrentEncounter()
    {
        return CurrentEncounter;
    }

    public void SetEncounterChoiceOptions(List<UserEncounterChoiceOption> options)
    {
        UserEncounterChoiceOptions = options;
    }

    public void SetLocationSpotActions(List<UserActionOption> actions)
    {
        LocationSpotActions = actions;
    }

    public void SetGlobalActions(List<UserActionOption> actions)
    {
        GlobalActions = actions;
    }
}
