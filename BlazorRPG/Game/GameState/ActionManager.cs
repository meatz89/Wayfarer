using System.Diagnostics;

public class ActionManager
{
    public GameState GameState;
    public NarrativeSystem NarrativeSystem { get; private set; }
    public LocationSystem LocationSystem { get; private set; }
    public CharacterSystem CharacterSystem { get; private set; }

    public ActionManager(GameState gameState, NarrativeSystem narrativeSystem, LocationSystem locationSystem, CharacterSystem characterSystem)
    {
        this.GameState = gameState;
        this.NarrativeSystem = narrativeSystem;
        this.LocationSystem = locationSystem;
        this.CharacterSystem = characterSystem;
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
    
        GameState.SetCurrentTravelOptions(userTravelOptions);
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
            // If no time slots specified, action is always enabled
            // Otherwise check if current time is in valid slots
            bool isDisabled = ga.Action.TimeSlots.Count > 0 &&
                !ga.Action.TimeSlots.Contains(GameState.CurrentTimeSlot);

            UserActionOption ua = new UserActionOption
            {
                BasicAction = ga.Action,
                Description = ga.Description,
                Index = actionIndex++,
                IsDisabled = isDisabled
            };
            userActions.Add(ua);
        }

        GameState.SetValidUserActions(userActions);
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
        List<PlayerAction> actions = new List<PlayerAction>();
        LocationNames currentLocation = GameState.CurrentLocation;

        List<BasicAction> locationActions = LocationSystem.GetActionsForLocation(currentLocation);

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
        LocationNames currentLocation = GameState.CurrentLocation;

        CharacterNames? character = CharacterSystem.GetCharacterAtLocation(currentLocation);
        if(!character.HasValue) return actions;

        List<BasicAction> characterActions = CharacterSystem.GetActionsForCharacter(character.Value);

        if (characterActions.Count > 0)
        {
            foreach (BasicAction locationAction in characterActions)
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

    public ActionResult ExecuteBasicAction(BasicAction basicAction)
    {
        foreach (IRequirement requirement in basicAction.Requirements)
        {
            bool hasRequirement = CheckRequirement(requirement);
            if (!hasRequirement)
            {
                ActionResult actionResultFail = ActionResult.Failure("Requirement not met");
                GameState.SetLastActionResult(actionResultFail);

                return actionResultFail;
            }
        }

        foreach (IOutcome outcome in basicAction.Outcomes)
        {
            ApplyOutcome(outcome);
        }

        GameState.ApplyAllChanges();
        ActionResultMessages allMessages = GameState.GetAndClearChanges();

        bool stillAlive = AdvanceTime();
        if (!stillAlive) return ActionResult.Failure("you died");

        ActionResult actionResult = ActionResult.Success("Action success!", allMessages);
        GameState.SetLastActionResult(actionResult);

        return actionResult;
    }

    public ActionResult MakeChoiceForNarrative(Narrative currentNarrative, NarrativeStage narrativeStage, int choice)
    {
        List<IOutcome> outcomes = NarrativeSystem.GetChoiceOutcomes(currentNarrative, narrativeStage, choice);
        if (outcomes == null)
        {
            ActionResult actionResultFail = ActionResult.Failure("No Success");
            GameState.SetLastActionResult(actionResultFail);

            return actionResultFail;
        }
        else
        {
            foreach (IOutcome outcome in outcomes)
            {
                ApplyOutcome(outcome);
            }
        }

        GameState.ApplyAllChanges();
        ActionResultMessages allMessages = GameState.GetAndClearChanges();

        bool stillAlive = AdvanceTime();
        if (!stillAlive) return ActionResult.Failure("you died");

        ActionResult actionResult = ActionResult.Success("Action success!", allMessages);
        GameState.SetLastActionResult(actionResult);

        return actionResult;
    }

    public ActionResult MoveToLocation(LocationNames locationName)
    {
        Location location = FindLocation(locationName);

        GameState.SetNewLocation(location.Name);

        UpdateTavelOptions();
        UpdateAvailableActions();

        ActionResult actionResult = ActionResult.Success($"Moved to {location.Name}.", new ActionResultMessages());

        return actionResult;
    }

    public bool AdvanceTime()
    {
        bool stillAlive = GameState.AdvanceTime(1);

        UpdateTavelOptions();
        UpdateAvailableActions();

        return stillAlive;
    }

    private bool CheckRequirement(IRequirement requirement)
    {
        if (requirement is CoinsRequirement money)
        {
            return GameState.Player.Coins >= money.Amount;
        }
        if (requirement is FoodRequirement food)
        {
            return GameState.Player.Health >= food.Amount;
        }
        if (requirement is HealthRequirement health)
        {
            return GameState.Player.Health >= health.Amount;
        }
        if (requirement is PhysicalEnergyRequirement physicalEnergy)
        {
            return GameState.Player.PhysicalEnergy >= physicalEnergy.Amount;
        }
        if (requirement is FocusEnergyRequirement focusEnergy)
        {
            return GameState.Player.FocusEnergy >= focusEnergy.Amount;
        }
        if (requirement is SocialEnergyRequirement socialEnergy)
        {
            return GameState.Player.SocialEnergy >= socialEnergy.Amount;
        }
        if (requirement is SkillLevelRequirement skillLevel)
        {
            return GameState.Player.Skills[skillLevel.SkillType] >= skillLevel.Amount;
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
                GameState.AddCoinsChange(coinsOutcome);
                break;
            case FoodOutcome foodOutcome:
                GameState.AddFoodChange(foodOutcome);
                break;
            case HealthOutcome healthOutcome:
                GameState.AddHealthChange(healthOutcome);
                break;
            case PhysicalEnergyOutcome physicalEnergyOutcome:
                GameState.AddPhysicalEnergyChange(physicalEnergyOutcome);
                break;
            case FocusEnergyOutcome focusEnergyOutcome:
                GameState.AddFocusEnergyChange(focusEnergyOutcome);
                break;
            case SocialEnergyOutcome socialEnergyOutcome:
                GameState.AddSocialEnergyChange(socialEnergyOutcome);
                break;
            case SkillLevelOutcome skillLevelOutcome:
                GameState.AddSkillLevelChange(skillLevelOutcome);
                break;
            case ItemOutcome itemOutcome:
                GameState.AddItemChange(itemOutcome);
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

        GameState.SetCurrentNarrative(narrative);

        return true;
    }

    public bool CanExecuteChoice(NarrativeStage currentNarrativeStage, int choice)
    {
        bool result = NarrativeSystem.CanExecute(currentNarrativeStage, choice);
        return result;
    }

    private Location FindLocation(LocationNames locationName)
    {
        return GameState.Locations.Where(x => x.Name == locationName).FirstOrDefault();
    }

    public List<LocationNames> GetConnectedLocations()
    {
        Location location = FindLocation(GameState.CurrentLocation);

        List<LocationNames> loc = location.ConnectedLocations;
        return loc;
    }
}
