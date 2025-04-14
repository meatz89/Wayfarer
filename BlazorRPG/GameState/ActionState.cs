public class ActionState
{
    public List<UserEncounterChoiceOption> UserEncounterChoiceOptions;
    public UserActionOption CurrentUserAction { get; set; }
    public List<UserActionOption> GlobalActions { get; set; } = new();
    public List<UserActionOption> LocationSpotActions { get; set; } = new();
    public List<UserActionOption> CharacterActions { get; set; } = new();
    public List<UserActionOption> QuestActions { get; set; } = new();
    public EncounterResult EncounterResult { get; set; }
    private EncounterManager CurrentEncounter { get; set; }
    public bool IsActiveEncounter { get; set; }

    public EncounterManager GetCurrentEncounter()
    {
        return this.CurrentEncounter;
    }

    public void SetGlobalActions(List<UserActionOption> actions)
    {
        GlobalActions = actions ?? new List<UserActionOption>();
    }

    public bool HasGlobalAction(string actionId)
    {
        return GlobalActions.Any(a => a.ActionId == actionId && !a.IsDisabled);
    }

    public void CompleteActiveEncounter()
    {
        IsActiveEncounter = false;
        CurrentEncounter = null;
        EncounterResult = null;
    }

    public void SetActiveEncounter()
    {
        this.IsActiveEncounter = true;
    }

    public void SetActiveEncounter(EncounterManager encounter)
    {
        SetActiveEncounter();
        CurrentEncounter = encounter;
        EncounterResult = new EncounterResult()
        { Encounter = encounter, EncounterResults = EncounterResults.Ongoing };
    }

    public void SetCurrentUserAction(UserActionOption action)
    {
        CurrentUserAction = action;
    }

    public void ClearCurrentUserAction()
    {
        CurrentUserAction = null;
    }

    public void SetLocationSpotActions(List<UserActionOption> actions)
    {
        LocationSpotActions = actions;
    }

    public void SetCharacterActions(List<UserActionOption> actions)
    {
        CharacterActions = actions;
    }

    public void AddCharacterActions(List<UserActionOption> actions)
    {
        CharacterActions.AddRange(actions);
    }

    public void SetQuestActions(List<UserActionOption> actions)
    {
        QuestActions = actions;
    }

    public void AddQuestActions(List<UserActionOption> actions)
    {
        QuestActions.AddRange(actions);
    }

    public void SetEncounterChoiceOptions(List<UserEncounterChoiceOption> choiceOptions)
    {
        this.UserEncounterChoiceOptions = choiceOptions;
    }
}