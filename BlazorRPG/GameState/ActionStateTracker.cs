
public class ActionStateTracker
{
    // Current action information
    public UserActionOption CurrentAction { get; private set; }
    public bool IsActiveEncounterContext { get; private set; }
    public EncounterResult EncounterResult { get; set; }
    public EncounterManager CurrentEncounterContext { get; private set; }

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
        IsActiveEncounterContext = true;
        CurrentEncounterContext = encounter;
    }

    public void CompleteAction()
    {
        IsActiveEncounterContext = false;
        CurrentEncounterContext = null;
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

    public UserEncounterChoiceOption GetEncounterChoiceOption(string choiceId)
    {
        return UserEncounterChoiceOptions.FirstOrDefault(o => o.Choice.ChoiceID == choiceId);
    }
}
