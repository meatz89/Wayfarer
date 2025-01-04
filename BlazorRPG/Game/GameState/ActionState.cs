
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
    
    public Narrative CurrentNarrative { get; private set; }
    public List<UserNarrativeChoiceOption> NarrativeChoiceOptions { get; private set; } = new();

    public void SetActiveNarrative(Narrative narrative)
    {
        this.CurrentNarrative = narrative;
    }

    public void SetNarrativeChoiceOptions(List<UserNarrativeChoiceOption> choiceOptions)
    {
        this.NarrativeChoiceOptions = choiceOptions;
    }

    public void CompleteActiveNarrative()
    {
        this.CurrentNarrative = null;
        this.NarrativeChoiceOptions = new();
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

    public void AddLocationSpotActions(List<UserActionOption> actions)
    {
        LocationSpotActions.AddRange(actions);
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

    public void SetLastActionResult(ActionResult result)
    {
        LastActionResult = result;
    }
}