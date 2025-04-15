public class ActionStateTracker
{
    // Current action information
    public UserActionOption CurrentAction { get; private set; }
    public bool IsActiveEncounter { get; private set; }
    public EncounterResult EncounterResult { get; set; }

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
    }

    // Complete active encounter
    public void CompleteActiveEncounter()
    {
        IsActiveEncounter = false;
    }

    public void SetGlobalActions(List<UserActionOption> actions)
    {
        GlobalActions = actions ?? new List<UserActionOption>();
    }

    public bool HasGlobalAction(string actionId)
    {
        return GlobalActions.Any(a => a.ActionId == actionId && !a.IsDisabled);
    }

    public void SetActiveEncounter()
    {
        this.IsActiveEncounter = true;
    }

    public void SetLocationSpotActions(List<UserActionOption> actions)
    {
        LocationSpotActions = actions;
    }

    public void SetEncounterChoiceOptions(List<UserEncounterChoiceOption> choiceOptions)
    {
        this.UserEncounterChoiceOptions = choiceOptions;
    }
}