using System.Text;

public class GameManager
{
    private const int hoursToAdvanceForActions = 2;

    public GameState gameState;
    private GameRules currentRules;

    public NarrativeSystem NarrativeSystem { get; }
    public LocationSystem LocationSystem { get; }
    public CharacterSystem CharacterSystem { get; }
    public ActionValidator ActionValidator { get; }
    public KnowledgeSystem KnowledgeSystem { get; }
    public CharacterRelationshipSystem RelationshipSystem { get; }
    public TimeSystem TimeSystem { get; }
    public InformationSystem InformationSystem { get; }
    public LocationAccess LocationAccess { get; }
    public ContextEngine ContextEngine { get; }
    public QuestSystem QuestSystem { get; }
    public MessageSystem MessageSystem { get; }

    public GameManager(
        GameState gameState,
        NarrativeSystem narrativeSystem,
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        ActionValidator actionValidator,
        KnowledgeSystem knowledgeSystem,
        CharacterRelationshipSystem relationshipSystem,
        TimeSystem timeSystem,
        InformationSystem informationSystem,
        LocationAccess locationAccess,
        ContextEngine contextEngine,
        QuestSystem questSystem,
        MessageSystem messageSystem
        )
    {
        this.gameState = gameState;
        this.currentRules = GameRules.StandardRuleset;

        this.NarrativeSystem = narrativeSystem;
        this.LocationSystem = locationSystem;
        this.CharacterSystem = characterSystem;
        this.ActionValidator = actionValidator;
        this.KnowledgeSystem = knowledgeSystem;
        this.RelationshipSystem = relationshipSystem;
        this.TimeSystem = timeSystem;
        this.InformationSystem = informationSystem;
        this.LocationAccess = locationAccess;
        this.ContextEngine = contextEngine;
        this.QuestSystem = questSystem;
        this.MessageSystem = messageSystem;
    }

    public void StartGame()
    {
        UpdateState();
    }

    public void UpdateState()
    {
        UpdateActiveQuests();

        UpdateLocationTravelOptions();
        UpdateLocationSpotOptions();
        UpdateAvailableActions();
    }

    public void UpdateAvailableActions()
    {
        gameState.SetLocationSpotActions(new List<UserActionOption>());
        gameState.SetCharacterActions(new List<UserActionOption>());
        gameState.SetQuestActions(new List<UserActionOption>());

        CreateGlobalActions();
        CreateLocationSpotActions();
        CreateCharacterActions();
        CreateQuestActions();
    }

    public void CreateGlobalActions()
    {
        List<UserActionOption> userActions = new List<UserActionOption>();
        int actionIndex = 1;

        UserActionOption ua = new UserActionOption
        {
            BasicAction = new BasicAction() { ActionType = BasicActionTypes.Wait },
            Description = "Wait",
            Index = actionIndex++,
            IsDisabled = false
        };
        userActions.Add(ua);

        gameState.SetGlobalActions(userActions);
    }

    private void CreateLocationSpotActions()
    {
        foreach (Location location in LocationSystem.GetLocations())
        {
            foreach (LocationSpot locationSpot in location.Spots)
            {
                List<UserActionOption> locationSpotActions = new();
                BasicAction locationSpotAction = locationSpot.LocationSpotAction;

                // If no time slots specified, action is always enabled
                // Otherwise check if current time is in valid slots
                bool isDisabled = locationSpotAction.TimeSlots.Count > 0 &&
                    !locationSpotAction.TimeSlots.Contains(gameState.CurrentTimeSlot);

                UserActionOption ua = new UserActionOption
                {
                    BasicAction = locationSpotAction,
                    Description = locationSpotAction.Name,
                    IsDisabled = isDisabled,
                    Location = location.Name,
                    LocationSpot = locationSpot.Name
                };
                locationSpotActions.Add(ua);
                gameState.AddLocationSpotActions(locationSpotActions);
            }
        }
    }

    public void CreateCharacterActions()
    {
        List<UserActionOption> userActions = new List<UserActionOption>();

        foreach (Location location in LocationSystem.GetLocations())
        {
            foreach (LocationSpot locationSpot in location.Spots)
            {
                foreach (BasicAction ga in locationSpot.CharacterActions)
                {
                    int actionIndex = 1;

                    bool isDisabled = ga.TimeSlots.Count > 0 &&
                        !ga.TimeSlots.Contains(gameState.CurrentTimeSlot);

                    LocationSpotNames locationSpotName = locationSpot.Name;
                    LocationNames name = location.Name;
                    UserActionOption ua = new UserActionOption
                    {
                        BasicAction = ga,
                        Description = ga.Name,
                        Index = actionIndex++,
                        IsDisabled = isDisabled,
                        Location = name,
                        LocationSpot = locationSpotName,
                        Character = locationSpot.Character
                    };
                    userActions.Add(ua);
                }
            }
        }

        gameState.AddCharacterActions(userActions);
    }

    public void CreateQuestActions()
    {
        List<UserActionOption> userActions = new List<UserActionOption>();

        List<Quest> quests = QuestSystem.GetAvailableQuests();
        foreach (Quest quest in quests)
        {
            QuestStep step = quest.GetCurrentStep();

            BasicAction questAction = step.QuestAction;
            int actionIndex = 1;

            UserActionOption ua = new UserActionOption
            {
                BasicAction = questAction,
                Description = questAction.Name,
                Index = actionIndex++,
                IsDisabled = false,
                Location = step.Location,
                LocationSpot = step.LocationSpot,
                Character = step.Character
            };
            userActions.Add(ua);
        }

        gameState.AddQuestActions(userActions);
    }

    private bool IsActionLocation(LocationNames location, LocationSpotNames locationSpot)
    {
        return location != gameState.CurrentLocation.Name || locationSpot != gameState.CurrentLocationSpot.Name;
    }

    private void UpdateActiveQuests()
    {
        List<Quest> quests = QuestSystem.GetAvailableQuests();
        gameState.ActiveQuests = quests;
    }

    public ActionResult ExecuteBasicAction(BasicAction basicAction)
    {
        // 1. Check context requirements
        if (!ContextEngine.CanExecuteInContext(basicAction))
            return ActionResult.Failure("Current context prevents this action");

        // 2. Update quest progress if applicable
        QuestSystem.ProcessAction(basicAction);

        // 3. Apply character relationship effects
        CharacterSystem.ProcessActionImpact(basicAction);

        // 4. Execute core action logic
        // ... existing execution code

        // 5. Update contexts based on results
        ContextEngine.ProcessActionOutcome(basicAction);

        ActionResultMessages allMessages = MessageSystem.GetAndClearChanges();

        bool stillAlive = AdvanceTime(1);
        if (!stillAlive) return ActionResult.Failure("you died");

        ActionResult actionResult = ActionResult.Success("Action success!", allMessages);
        gameState.SetLastActionResult(actionResult);

        return actionResult;
    }

    //public ActionResult MakeChoiceForNarrative(Narrative currentNarrative, NarrativeStage narrativeStage, int choice)
    //{
    //    List<IOutcome> outcomes = NarrativeSystem.GetChoiceOutcomes(currentNarrative, narrativeStage, choice);
    //    if (outcomes == null)
    //    {
    //        ActionResult actionResultFail = ActionResult.Failure("No Success");
    //        GameState.SetLastActionResult(actionResultFail);

    //        return actionResultFail;
    //    }
    //    else
    //    {
    //        foreach (IOutcome outcome in outcomes)
    //        {
    //            ApplyOutcome(outcome);
    //        }
    //    }

    //    GameState.ApplyAllChanges();
    //    ActionResultMessages allMessages = GameState.GetAndClearChanges();

    //    bool stillAlive = AdvanceTime();
    //    if (!stillAlive) return ActionResult.Failure("you died");

    //    ActionResult actionResult = ActionResult.Success("Action success!", allMessages);
    //    GameState.SetLastActionResult(actionResult);

    //    return actionResult;
    //}

    public ActionResult MoveToLocation(LocationNames locationName)
    {
        Location location = LocationSystem.GetLocation(locationName);
        gameState.SetNewLocation(location);
        UpdateState();

        ActionResult actionResult = ActionResult.Success($"Moved to {locationName}.", new ActionResultMessages());

        return actionResult;
    }

    public void MoveToLocationSpot(LocationNames location, LocationSpotNames locationSpotName)
    {
        LocationSpot locationSpot = LocationSystem.GetLocationSpotForLocation(location, locationSpotName);
        gameState.SetNewLocationSpot(locationSpot);
        UpdateState();

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

    public List<LocationNames> GetConnectedLocations()
    {
        List<LocationNames> loc = LocationSystem.GetLocationConnections(gameState.CurrentLocation.Name);
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

    public bool CanMoveToSpot(LocationSpotNames locationName)
    {
        return true;
    }

    public bool AreRequirementsMet(UserActionOption action)
    {
        Player Player = gameState.Player;

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

    public void UpdateLocationSpotOptions()
    {
        Location location = gameState.CurrentLocation;
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

        gameState.SetCurrentLocationSpotOptions(userLocationSpotOption);
    }

    public void UpdateLocationTravelOptions()
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

        gameState.SetCurrentTravelOptions(userTravelOptions);
    }


    public void AdvanceTime()
    {
        AdvanceTime(1);
    }

    public void AdvanceTimeTo(int hours)
    {
        int timeToMidnight = 24 - gameState.CurrentTimeInHours;
        int timeToAdvance = timeToMidnight + hours;
        AdvanceTime(timeToAdvance);
    }

    public bool AdvanceTime(int inHours)
    {
        bool daySkip = false;
        // Advance the current time
        if (gameState.CurrentTimeInHours + inHours > 24)
        {
            daySkip = true;
        }

        gameState.CurrentTimeInHours = (gameState.CurrentTimeInHours + inHours) % 24;

        const int timeWindowsPerDay = 4;
        const int hoursPerTimeWindow = 6;
        int timeSlot = (gameState.CurrentTimeInHours / hoursPerTimeWindow) % timeWindowsPerDay;

        gameState.DetermineCurrentTimeSlot(timeSlot);

        if (daySkip)
        {
            bool stillAlive = StartNewDay();
            if (!stillAlive) Environment.Exit(0);
            return stillAlive;
        }

        return true;
    }

    private bool StartNewDay()
    {
        Player Player = gameState.Player;

        // Get quest-modified game rules
        GameRules modifiedRules = QuestSystem.GetModifiedRules(currentRules);

        StringBuilder endDayMessage = new();
        endDayMessage.AppendLine("Night falls...");

        // Show base and modified requirements
        endDayMessage.AppendLine($"You need {currentRules.DailyFoodRequirement} food:");
        endDayMessage.AppendLine($"- Base requirement: {currentRules.DailyFoodRequirement}");
        foreach (IGameStateModifier modifier in QuestSystem.GetActiveModifiers())
        {
            FoodModfier foodModfier = (FoodModfier)modifier;
            endDayMessage.AppendLine($"- {foodModfier.GetSource}: +{foodModfier.AdditionalFood}");
        }

        // Show outcomes
        int food = Player.Inventory.GetItemCount(ResourceTypes.Food);
        if (food >= currentRules.DailyFoodRequirement)
        {
            endDayMessage.AppendLine("You have enough food for everyone.");
        }
        else
        {
            endDayMessage.AppendLine($"You only have {food} food - not enough for everyone!");
        }

        // Show quest condition updates
        foreach (IQuestCondition condition in QuestSystem.GetActiveConditions())
        {
            QuestCondidtion cond = (QuestCondidtion)condition;
            endDayMessage.AppendLine(cond.GetStatusMessage());
        }

        // Display the message through your UI system
        MessageSystem.AddSystemMessage(endDayMessage.ToString());

        int foodNeeded = currentRules.DailyFoodRequirement;
        bool hasFood = food >= foodNeeded;

        food = hasFood ? food - foodNeeded : 0;

        int health = Player.Health;
        int minHealth = Player.MinHealth;
        int noFoodHealthLoss = currentRules.NoFoodEffectOnHealth;
        int noShelterHealthLoss = currentRules.NoShelterEffectOnHealth;

        if (!hasFood) gameState.ChangeHealth(currentRules.NoFoodEffectOnHealth);

        return Player.Health > Player.MinHealth;
    }

}
