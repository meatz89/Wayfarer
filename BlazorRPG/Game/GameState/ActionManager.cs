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

    public ActionResult ExecuteBasicAction(BasicAction basicAction)
    {
        foreach (IRequirement requirement in basicAction.Requirements)
        {
            bool hasRequirement = CheckRequirement(requirement);
            if (!hasRequirement)
            {
                ActionResult actionResultFail = ActionResult.Failure("Requirement not met");
                gameState.SetLastActionResult(actionResultFail);

                return actionResultFail;
            }
        }

        foreach (IOutcome outcome in basicAction.Outcomes)
        {
            ApplyOutcome(outcome);
        }

        gameState.ApplyAllChanges();
        ActionResultMessages allMessages = gameState.GetAndClearChanges();

        AdvanceTime();

        ActionResult actionResult = ActionResult.Success("Action success!", allMessages);
        gameState.SetLastActionResult(actionResult);

        return actionResult;
    }

    public ActionResult MakeChoiceForNarrative(Narrative currentNarrative, NarrativeStage narrativeStage, int choice)
    {
        var outcomes = NarrativeSystem.GetChoiceOutcomes(currentNarrative, narrativeStage, choice);
        if (outcomes == null)
        {
            ActionResult actionResultFail = ActionResult.Failure("No Success");
            gameState.SetLastActionResult(actionResultFail);

            return actionResultFail;
        }
        else
        {
            foreach (IOutcome outcome in outcomes)
            {
                ApplyOutcome(outcome);
            }
        }

        gameState.ApplyAllChanges();
        ActionResultMessages allMessages = gameState.GetAndClearChanges();

        AdvanceTime();
        
        ActionResult actionResult = ActionResult.Success("Action success!", allMessages);
        gameState.SetLastActionResult(actionResult);

        return actionResult;
    }

    public ActionResult ExecuteTravelAction(LocationNames locationName)
    {
        Location location = FindLocation(locationName);

        gameState.SetCurrentLocation(location.Name);
        ActionResult actionResult = ActionResult.Success($"Moved to {location.Name}.", new ActionResultMessages());

        return actionResult;
    }

    public void AdvanceTime()
    {
        gameState.AdvanceTime(1);
    }

    private bool CheckRequirement(IRequirement requirement)
    {
        if (requirement is CoinsRequirement money)
        {
            return gameState.PlayerInfo.Coins >= money.Amount;
        }
        if (requirement is FoodRequirement food)
        {
            return gameState.PlayerInfo.Health >= food.Amount;
        }
        if (requirement is HealthRequirement health)
        {
            return gameState.PlayerInfo.Health >= health.Amount;
        }
        if (requirement is PhysicalEnergyRequirement physicalEnergy)
        {
            return gameState.PlayerInfo.PhysicalEnergy >= physicalEnergy.Amount;
        }
        if (requirement is FocusEnergyRequirement focusEnergy)
        {
            return gameState.PlayerInfo.FocusEnergy >= focusEnergy.Amount;
        }
        if (requirement is SocialEnergyRequirement socialEnergy)
        {
            return gameState.PlayerInfo.SocialEnergy >= socialEnergy.Amount;
        }
        if (requirement is SkillLevelRequirement skillLevel)
        {
            return gameState.PlayerInfo.Skills[skillLevel.SkillType] >= skillLevel.Amount;
        }
        if (requirement is ItemRequirement item)
        {
            return false;
        }
        return false;
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

    private Location FindLocation(LocationNames locationName)
    {
        return gameState.Locations.Where(x => x.Name == locationName).FirstOrDefault();
    }
}

