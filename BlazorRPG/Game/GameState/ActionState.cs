public class ActionState
{
    public UserActionOption CurrentUserAction { get; private set; }

    public List<UserActionOption> GlobalActions { get; private set; } = new();
    public List<UserActionOption> LocationSpotActions { get; private set; } = new();
    public List<UserActionOption> CharacterActions { get; private set; } = new();
    public List<UserActionOption> QuestActions { get; private set; } = new();

    public ActionResult LastActionResult { get; private set; }
    public ActionResultMessages LastActionResultMessages { get; private set; }

    public List<Quest> ActiveQuests { get; set; }

    public Encounter CurrentEncounter { get; private set; }
    public List<UserEncounterChoiceOption> EncounterChoiceOptions { get; private set; } = new();

    public void SetActiveEncounter(Encounter encounter)
    {
        this.CurrentEncounter = encounter;
    }

    public void SetEncounterChoiceOptions(List<UserEncounterChoiceOption> choiceOptions)
    {
        this.EncounterChoiceOptions = choiceOptions;
    }

    public void CompleteActiveEncounter()
    {
        this.CurrentEncounter = null;
        this.EncounterChoiceOptions = new();
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

}