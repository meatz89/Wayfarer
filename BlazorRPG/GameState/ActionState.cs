

public class ActionState
{
    public List<UserEncounterChoiceOption> UserEncounterChoiceOptions;
    public UserActionOption CurrentUserAction { get; set; }
    public List<UserActionOption> GlobalActions { get; set; } = new();
    public List<UserActionOption> LocationSpotActions { get; set; } = new();
    public List<UserActionOption> CharacterActions { get; set; } = new();
    public List<UserActionOption> QuestActions { get; set; } = new();
    public ActionResult LastActionResult { get; set; }
    public ActionResultMessages LastActionResultMessages { get; set; }

    public List<Quest> ActiveQuests { get; set; }

    public EncounterResult EncounterResult { get; set; }
    private EncounterManager CurrentEncounter { get; set; }

    public EncounterManager GetCurrentEncounter()
    {
        return this.CurrentEncounter;
    }

    public void CompleteActiveEncounter()
    {
        SetActiveEncounter(null);
        EncounterResult = null;
    }

    public void SetActiveEncounter(EncounterManager encounter)
    {
        CurrentEncounter = encounter;
        EncounterResult = new EncounterResult()
        { Encounter = encounter, EncounterResults = EncounterResults.Ongoing };
    }

    public void SetLastActionResultMessages(ActionResultMessages allMessages)
    {
        LastActionResultMessages = allMessages;
    }

    public void SetCurrentUserAction(UserActionOption action)
    {
        CurrentUserAction = action;
    }

    public void ClearCurrentUserAction()
    {
        CurrentUserAction = null;
    }

    public void SetGlobalActions(List<UserActionOption> actions)
    {
        GlobalActions = actions;
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