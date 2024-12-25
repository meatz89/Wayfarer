public class ActionManager
{
    private GameState gameState;
    public NarrativeSystem NarrativeSystem { get; private set; }

    public ActionManager(GameState gameState, NarrativeSystem NarrativeSystem)
    {
        this.gameState = gameState;
        this.NarrativeSystem = NarrativeSystem;
    }

    public bool StartNarrativeFor(BasicActionTypes actionType)
    {
        Narrative narrative = NarrativeSystem.GetNarrativeFor(actionType);
        gameState.SetCurrentNarrative(narrative);

        return true;
    }

    public bool CanExecuteChoice(Narrative currentNarrative, int choice)
    {
        bool result = NarrativeSystem.CanExecute(currentNarrative, choice);
        return result;
    }

    public ActionResult MakeChoiceForNarrative(Narrative currentNarrative, int choice)
    {
        SystemActionResult result = NarrativeSystem.ExecuteChoice(currentNarrative, choice);
        if (!result.IsSuccess)
        {
            return ActionResult.Failure(result.Message);
        }

        gameState.ApplyAllChanges();
        ActionResultMessages allMessages = gameState.GetAndClearChanges();

        return ActionResult.Success("It worked!", allMessages);
    }
}

