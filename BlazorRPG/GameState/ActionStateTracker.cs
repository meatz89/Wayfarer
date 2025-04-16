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

    // Set current action
    public void SetCurrentUserAction(UserActionOption action)
    {
        CurrentAction = action;
    }

    // Clear current action
    public void ClearCurrentUserAction()
    {
        CurrentAction = null;
    }

    // Set active encounter
    public void SetActiveEncounter(EncounterManager encounter = null)
    {
        IsActiveEncounter = true;
        CurrentEncounter = encounter;
    }

    // Complete active encounter
    public void CompleteAction()
    {
        IsActiveEncounter = false;
        CurrentEncounter = null;
    }

    // Get current encounter
    public EncounterManager GetCurrentEncounter()
    {
        return CurrentEncounter;
    }

    // Set encounter choice options
    public void SetEncounterChoiceOptions(List<UserEncounterChoiceOption> options)
    {
        UserEncounterChoiceOptions = options;
    }

    // Set location spot actions
    public void SetLocationSpotActions(List<UserActionOption> actions)
    {
        LocationSpotActions = actions;
    }

    // Set global actions
    public void SetGlobalActions(List<UserActionOption> actions)
    {
        GlobalActions = actions;
    }
}