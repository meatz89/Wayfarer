public class ActionManager
{
    private GameState gameState;
    public NarrativeSystem NarrativeSystem { get; private set; }

    public ActionManager(GameState gameState, NarrativeSystem NarrativeSystem)
    {
        this.gameState = gameState;
        this.NarrativeSystem = NarrativeSystem;
    }

    public bool HasNarrative(BasicActionTypes actionType)
    {
        Narrative narrative = NarrativeSystem.GetNarrativeFor(actionType);
        return narrative != null;
    }

    public bool StartNarrativeFor(BasicActionTypes actionType)
    {
        Narrative narrative = NarrativeSystem.GetNarrativeFor(actionType);
        if (narrative == null)
        {
            throw new Exception("No Narrative Found");
        }

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

    public ActionResult TravelTo(LocationNames locationName)
    {
        Location location = FindLocation(locationName);

        gameState.CurrentLocation = location.Name;
        return ActionResult.Success($"Moved to {location.Name}.", new ActionResultMessages());
    }

    private Location FindLocation(LocationNames locationName)
    {
        return gameState.Locations.Where(x => x.Name == locationName).FirstOrDefault();
    }

}

