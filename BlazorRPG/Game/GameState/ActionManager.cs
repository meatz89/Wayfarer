using System.Diagnostics;

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

    public void Initialize()
    {
        UpdateTavelOptions();
        UpdateAvailableActions();
    }

    public void UpdateTavelOptions()
    {
        List<LocationNames> connectedLocations = GetConnectedLocations();

        List<UserTravelOption> userTravelOptions = new List<UserTravelOption>();
        
        for (int i = 0; i < connectedLocations.Count; i++)
        {
            LocationNames location = connectedLocations[i];

            UserTravelOption travel = new UserTravelOption()
            {
                Index = i + 1,
                Location = location
            };

            userTravelOptions.Add(travel);
        }
    
        gameState.SetCurrentTravelOptions(userTravelOptions);
    }

    public void UpdateAvailableActions()
    {
        List<PlayerAction> global = GetGlobalActions();
        List<PlayerAction> location = GetLocationActions();
        List<PlayerAction> character = GetCharacterActions();

        List<PlayerAction> playerActions = new List<PlayerAction>();
        playerActions.AddRange(global);
        playerActions.AddRange(location);
        playerActions.AddRange(character);

        CreateUserActionsFromPlayerActions(playerActions);
    }

    public void CreateUserActionsFromPlayerActions(List<PlayerAction> playerActions)
    {
        List<UserActionOption> userActions = new List<UserActionOption>();
        int actionIndex = 1;

        foreach (PlayerAction ga in playerActions)
        {
            bool isDisabled = ga.Action.TimeSlots.Count > 0 && !ga.Action.TimeSlots.Contains(gameState.CurrentTimeSlot);

            UserActionOption ua = new UserActionOption
            {
                Action = ga.Action,
                Description = ga.Description,
                Index = actionIndex++,
                IsDisabled = isDisabled
            };
            userActions.Add(ua);
        }

        gameState.SetValidUserActions(userActions);
    }

    public List<PlayerAction> GetGlobalActions()
    {
        List<PlayerAction> actions = new List<PlayerAction>();

        actions.Add(new PlayerAction()
        {
            Action = new BasicAction() { ActionType = BasicActionTypes.CheckStatus },
            Description = "(System) Check Status"
        });
        actions.Add(new PlayerAction()
        {
            Action = new BasicAction() { ActionType = BasicActionTypes.Travel },
            Description = "(System) Travel"
        });
        actions.Add(new PlayerAction()
        {
            Action = new BasicAction() { ActionType = BasicActionTypes.Wait },
            Description = "(System) Wait"
        });

        return actions;
    }

    public List<PlayerAction> GetLocationActions()
    {
        LocationNames currentLocation = gameState.CurrentLocation;

        // Add Location Action
        List<BasicAction> locationActions = LocationSystem.GetActionsForLocation(currentLocation);

        List<PlayerAction> actions = new List<PlayerAction>();
        if (locationActions.Count > 0)
        {
            foreach (BasicAction locationAction in locationActions)
            {
                actions.Add(new PlayerAction()
                {
                    Action = locationAction,
                    Description = $"[{locationAction.ActionType.ToString()}] {locationAction.Description}"
                });
            }
        }
        return actions;
    }

    public List<PlayerAction> GetCharacterActions()
    {
        List<PlayerAction> actions = new List<PlayerAction>();
        return actions;
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
        List<IOutcome> outcomes = NarrativeSystem.GetChoiceOutcomes(currentNarrative, narrativeStage, choice);
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

        UpdateTavelOptions();
        UpdateAvailableActions();

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
            return gameState.Player.Coins >= money.Amount;
        }
        if (requirement is FoodRequirement food)
        {
            return gameState.Player.Health >= food.Amount;
        }
        if (requirement is HealthRequirement health)
        {
            return gameState.Player.Health >= health.Amount;
        }
        if (requirement is PhysicalEnergyRequirement physicalEnergy)
        {
            return gameState.Player.PhysicalEnergy >= physicalEnergy.Amount;
        }
        if (requirement is FocusEnergyRequirement focusEnergy)
        {
            return gameState.Player.FocusEnergy >= focusEnergy.Amount;
        }
        if (requirement is SocialEnergyRequirement socialEnergy)
        {
            return gameState.Player.SocialEnergy >= socialEnergy.Amount;
        }
        if (requirement is SkillLevelRequirement skillLevel)
        {
            return gameState.Player.Skills[skillLevel.SkillType] >= skillLevel.Amount;
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

    public List<LocationNames> GetConnectedLocations()
    {
        Location location = FindLocation(gameState.CurrentLocation);

        List<LocationNames> loc = location.ConnectedLocations;
        return loc;
    }
}
