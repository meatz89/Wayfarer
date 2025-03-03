public class ActionState
{
    public UserActionOption CurrentUserAction { get; set; }
    public List<UserActionOption> GlobalActions { get; set; } = new();
    public List<UserActionOption> LocationSpotActions { get; set; } = new();
    public List<UserActionOption> CharacterActions { get; set; } = new();
    public List<UserActionOption> QuestActions { get; set; } = new();
    public ActionResult LastActionResult { get; set; }
    public ActionResultMessages LastActionResultMessages { get; set; }

    public List<Quest> ActiveQuests { get; set; }

    public Encounter CurrentEncounter { get; set; }
    public List<UserEncounterChoiceOption> CurrentChoiceOptions { get; set; }
    public EncounterResult EncounterResult { get; set; }

    public Encounter GetCurrentEncounter()
    {
        return this.CurrentEncounter;
    }

    public void SetEncounterChoiceOptions(List<UserEncounterChoiceOption> options)
    {
        CurrentChoiceOptions = options;
    }

    public void CompleteActiveEncounter()
    {
        SetActiveEncounter(null);
        EncounterResult = null;
    }

    public void SetActiveEncounter(Encounter encounter)
    {
        CurrentEncounter = encounter;
        EncounterResult = new EncounterResult()
        { Encounter = encounter, EncounterResults = EncounterResults.Ongoing };

        CurrentChoiceOptions = new();
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