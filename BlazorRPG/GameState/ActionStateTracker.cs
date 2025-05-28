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

    public LocationAction PreviousAction { get; private set; }

    public void SetCurrentUserAction(UserActionOption currentUserAction)
    {
        PreviousAction = CurrentAction?.locationAction;
        CurrentAction = currentUserAction;
    }

    public void ClearCurrentUserAction()
    {
        PreviousAction = CurrentAction?.locationAction;
        CurrentAction = null;
    }

    public void SetActiveEncounter(EncounterManager encounter)
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
