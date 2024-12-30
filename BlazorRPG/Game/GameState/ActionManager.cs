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

    public ActionResult ExecuteBasicAction(BasicAction action)
    {
        var currentLocation = gameState.CurrentLocation;

        SystemActionResult result = ExecuteAction(action);
        if (!result.IsSuccess)
        {
            return ActionResult.Failure("Not Possible");
        }

        gameState.ApplyAllChanges();
        ActionResultMessages allMessages = gameState.GetAndClearChanges();

        return ActionResult.Success("Action success!", allMessages);
    }

    public SystemActionResult ExecuteAction(BasicAction basicAction)
    {
        foreach (IOutcome outcome in basicAction.Outcomes)
        {
            ApplyOutcome(outcome);
        }

        AdvanceTime();
        return SystemActionResult.Success("Action completed successfully");
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

        AdvanceTime();
        return ActionResult.Success("Action success!", allMessages);
    }

    public void AdvanceTime()
    {
        gameState.AdvanceTime(1);
    }

    private void ApplyOutcome(IOutcome outcome)
    {
        switch (outcome)
        {
            case CoinsOutcome coinsOutcome:
                gameState.AddCoinsChange(coinsOutcome);
                break;
            case FoodOutcome foodOutcome:
                gameState.AddFoodChange(foodOutcome);
                break;
            case HealthOutcome healthOutcome:
                gameState.AddHealthChange(healthOutcome);
                break;
            case PhysicalEnergyOutcome physicalEnergyOutcome:
                gameState.AddPhysicalEnergyChange(physicalEnergyOutcome);
                break;
            case FocusEnergyOutcome focusEnergyOutcome:
                gameState.AddFocusEnergyChange(focusEnergyOutcome);
                break;
            case SocialEnergyOutcome socialEnergyOutcome:
                gameState.AddSocialEnergyChange(socialEnergyOutcome);
                break;
            case SkillLevelOutcome skillLevelOutcome:
                gameState.AddSkillLevelChange(skillLevelOutcome);
                break;
            case ItemOutcome itemOutcome:
                gameState.AddItemChange(itemOutcome);
                break;
        }
    }

    public bool HasNarrative(BasicAction action)
    {
        Narrative narrative = NarrativeSystem.GetNarrativeFor(action.ActionType);
        bool hasNarrative = narrative != null;
        return false;
    }

    public bool StartNarrativeFor(BasicAction action)
    {
        Narrative narrative = NarrativeSystem.GetNarrativeFor(action.ActionType);
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

