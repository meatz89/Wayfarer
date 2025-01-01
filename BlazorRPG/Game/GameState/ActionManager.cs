public class ActionManager
{
    private const int hoursToAdvanceForActions = 2;

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
        UpdateLocationSpotOptions();
        UpdateAvailableActions();
    }

    public void UpdateAvailableActions()
    {
        GameState.SetLocationActions(new List<UserActionOption>());
        GameState.SetLocationSpotActions(new List<UserActionOption>());

        CreateGlobalActions();

        foreach (Location location in LocationSystem.GetLocations())
        {
            List<BasicAction> locationActions = LocationSystem.GetActionsForLocation(location.Name);

            List<UserActionOption> userActions = new List<UserActionOption>();
            int actionIndex = 1;

            foreach (BasicAction ga in locationActions)
            {
                // If no time slots specified, action is always enabled
                // Otherwise check if current time is in valid slots
                bool isDisabled = ga.TimeSlots.Count > 0 &&
                    !ga.TimeSlots.Contains(GameState.CurrentTimeSlot);

                UserActionOption ua = new UserActionOption
                {
                    BasicAction = ga,
                    Description = ga.Name,
                    Index = actionIndex++,
                    IsDisabled = isDisabled,
                    Location = location.Name
                };
                userActions.Add(ua);
            }

            GameState.AddLocationActions(userActions);

            foreach (LocationSpot locationSpot in location.Spots)
            {
                List<UserActionOption> locationSpotActions = new();
                BasicAction locationSpotAction = locationSpot.LocationSpotAction;

                // If no time slots specified, action is always enabled
                // Otherwise check if current time is in valid slots
                bool isDisabled = locationSpotAction.TimeSlots.Count > 0 &&
                    !locationSpotAction.TimeSlots.Contains(GameState.CurrentTimeSlot);

                UserActionOption ua = new UserActionOption
                {
                    BasicAction = locationSpotAction,
                    Description = locationSpotAction.Name,
                    Index = actionIndex++,
                    IsDisabled = isDisabled,
                    Location = location.Name,
                    LocationSpot = locationSpot.Name
                };
                locationSpotActions.Add(ua);
                GameState.AddLocationSpotActions(locationSpotActions);
            }
        }

        //List<BasicAction> character = GetCharacterActions(character.Name);
        //CreateCharacterActions(character);
    }

    public void CreateGlobalActions()
    {
        List<UserActionOption> userActions = new List<UserActionOption>();
        int actionIndex = 1;

        UserActionOption ua = new UserActionOption
        {
            BasicAction = new BasicAction() { Id = BasicActionTypes.Wait },
            Description = "Wait",
            Index = actionIndex++,
            IsDisabled = false
        };
        userActions.Add(ua);

        GameState.SetGlobalActions(userActions);
    }

    public void CreateCharacterActions(List<BasicAction> BasicActions, LocationNames locationName, CharacterNames characterName)
    {
        List<UserActionOption> userActions = new List<UserActionOption>();
        int actionIndex = 1;

        foreach (BasicAction ga in BasicActions)
        {
            // If no time slots specified, action is always enabled
            // Otherwise check if current time is in valid slots
            bool isDisabled = ga.TimeSlots.Count > 0 &&
                !ga.TimeSlots.Contains(GameState.CurrentTimeSlot);

            UserActionOption ua = new UserActionOption
            {
                BasicAction = ga,
                Description = ga.Name,
                Index = actionIndex++,
                IsDisabled = isDisabled,
                Location = locationName,
                Character = characterName
            };
            userActions.Add(ua);
        }

        GameState.SetLocationActions(userActions);
    }

    //public List<BasicAction> GetCharacterActions()
    //{
    //    List<BasicAction> actions = new List<BasicAction>();
    //    LocationNames currentLocation = GameState.CurrentLocation.Name;

    //    CharacterNames? character = CharacterSystem.GetCharacterAtLocation(currentLocation);
    //    if (!character.HasValue) return actions;

    //    List<BasicAction> characterActions = CharacterSystem.GetActionsForCharacter(character.Value);

    //    if (characterActions.Count > 0)
    //    {
    //        foreach (BasicAction locationAction in characterActions)
    //        {
    //            actions.Add(new BasicAction()
    //            {
    //                Action = locationAction,
    //                Description = $"[{locationAction.Id.ToString()}] {locationAction.Name}"
    //            });
    //        }
    //    }
    //    return actions;
    //}

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
        var location = LocationSystem.GetLocation(locationName);
        GameState.SetNewLocation(location);

        UpdateTavelOptions();
        UpdateAvailableActions();

        ActionResult actionResult = ActionResult.Success($"Moved to {locationName}.", new ActionResultMessages());

        return actionResult;
    }

    public void MoveToLocationSpot(LocationNames location, LocationSpotTypes locationSpotName)
    {
        var locationSpot = LocationSystem.GetLocationSpotForLocation(location, locationSpotName);
        GameState.SetNewLocationSpot(locationSpot);

        UpdateTavelOptions();
        UpdateAvailableActions();
    }

    public bool AdvanceTime()
    {
        bool stillAlive = GameState.AdvanceTime(hoursToAdvanceForActions);

        UpdateTavelOptions();
        UpdateAvailableActions();

        return stillAlive;
    }


    public bool HasNarrative(BasicAction action)
    {
        Narrative narrative = NarrativeSystem.GetNarrativeFor(action.Id);
        bool hasNarrative = narrative != null;
        return false;
    }

    public bool StartNarrativeFor(BasicAction action)
    {
        Narrative narrative = NarrativeSystem.GetNarrativeFor(action.Id);
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

    public List<LocationNames> GetConnectedLocations()
    {
        List<LocationNames> loc = LocationSystem.GetLocationConnections(GameState.CurrentLocation.Name);
        return loc;
    }

    public List<Location> GetAllLocations()
    {
        List<Location> loc = LocationSystem.GetLocations();
        return loc;
    }


    public bool CanTravelTo(LocationNames locationName)
    {
        List<LocationNames> locs = GetConnectedLocations();
        return locs.Contains(locationName);
    }

    public bool CanMoveToSpot(LocationSpotTypes locationName)
    {
        return true;
    }

    public bool AreRequirementsMet(UserActionOption action)
    {
        var Player = GameState.Player;

        return action.BasicAction.Requirements.All(requirement => requirement switch
        {
            PhysicalEnergyRequirement r => Player.PhysicalEnergy >= r.Amount,
            FocusEnergyRequirement r => Player.FocusEnergy >= r.Amount,
            SocialEnergyRequirement r => Player.SocialEnergy >= r.Amount,
            InventorySlotsRequirement r => Player.Inventory.GetEmptySlots() >= r.Count,
            HealthRequirement r => Player.Health >= r.Amount,
            CoinsRequirement r => Player.Coins >= r.Amount,
            FoodRequirement r => Player.Inventory.GetItemCount(ResourceTypes.Food) >= r.Amount,
            SkillLevelRequirement r => Player.Skills.ContainsKey(r.SkillType) && Player.Skills[r.SkillType] >= r.Amount,
            ItemRequirement r => Player.Inventory.GetItemCount(r.ResourceType) >= r.Count,
            _ => false
        });
    }


    private void UpdateLocationSpotOptions()
    {
        Location location = GameState.CurrentLocation;
        List<LocationSpot> locationSpots = LocationSystem.GetLocationSpots(location);

        List<UserLocationSpotOption> userLocationSpotOption = new List<UserLocationSpotOption>();

        for (int i = 0; i < locationSpots.Count; i++)
        {
            LocationSpot locationSpot = locationSpots[i];
            UserLocationSpotOption locationSpotOption = new UserLocationSpotOption()
            {
                Index = i + 1,
                Location = location.Name,
                LocationSpot = locationSpot.Name
            };

            userLocationSpotOption.Add(locationSpotOption);
        }

        GameState.SetCurrentLocationSpotOptions(userLocationSpotOption);
    }

    public void UpdateTavelOptions()
    {
        List<LocationNames> connectedLocations = GetConnectedLocations();

        List<UserLocationTravelOption> userTravelOptions = new List<UserLocationTravelOption>();

        for (int i = 0; i < connectedLocations.Count; i++)
        {
            LocationNames location = connectedLocations[i];

            UserLocationTravelOption travel = new UserLocationTravelOption()
            {
                Index = i + 1,
                Location = location
            };

            userTravelOptions.Add(travel);
        }

        GameState.SetCurrentTravelOptions(userTravelOptions);
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
        if (requirement is InventorySlotsRequirement inventorySlot)
        {
            return GameState.Player.Inventory.GetEmptySlots() >= inventorySlot.Count;
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
}
