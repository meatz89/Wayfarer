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

    public bool CanExecuteChoice(NarrativeStage currentNarrativeStage, int choice)
    {
        bool result = NarrativeSystem.CanExecute(currentNarrativeStage, choice);
        return result;
    }

    public ActionResult MakeChoiceForNarrative(Narrative currentNarrative, NarrativeStage narrativeStage, int choice)
    {
        SystemActionResult result = NarrativeSystem.ExecuteChoice(currentNarrative, narrativeStage, choice);
        if (!result.IsSuccess)
        {
            return ActionResult.Failure(result.Message);
        }

        gameState.ApplyAllChanges();
        ActionResultMessages allMessages = gameState.GetAndClearChanges();

        return ActionResult.Success("Action success!", allMessages);
    }
}

