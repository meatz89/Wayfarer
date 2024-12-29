public class ActionManager
{
    private GameState gameState;
    public NarrativeSystem NarrativeSystem { get; private set; }
    public LocationSystem LocationSystem { get; private set; }

    public ActionManager(GameState gameState, NarrativeSystem narrativeSystem, LocationSystem locationSystem)
    {
        this.gameState = gameState;
        this.NarrativeSystem = narrativeSystem;
        this.LocationSystem = locationSystem;
    }

    public ActionResult ExecuteBasicAction(LocationNames currentLocation)
    {
        List<BasicActionTypes> actions = LocationSystem.GetLocationActionsFor(currentLocation);
        var action = actions.FirstOrDefault();

        SystemActionResult result = LocationSystem.ExecuteAction(action);
        if (!result.IsSuccess)
        {
            return ActionResult.Failure("Not Possible");
        }

        gameState.ApplyAllChanges();
        ActionResultMessages allMessages = gameState.GetAndClearChanges();

        return ActionResult.Success("Action success!", allMessages);
    }

    public bool HasNarrative(BasicActionTypes actionType)
    {
        Narrative narrative = NarrativeSystem.GetNarrativeFor(actionType);
        bool hasNarrative = narrative != null;
        return false;
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

