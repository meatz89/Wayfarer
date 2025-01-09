using System.Text;

public class GameManager
{
    private const int hoursToAdvanceForActions = 2;

    public GameState gameState;
    private GameRules currentRules;

    public EncounterSystem EncounterSystem { get; }
    public LocationSystem LocationSystem { get; }
    public ActionValidator ActionValidator { get; }
    public ActionSystem ContextEngine { get; }
    public QuestSystem QuestSystem { get; }
    public ItemSystem ItemSystem { get; }
    public MessageSystem MessageSystem { get; }

    public GameManager(
        GameState gameState,
        EncounterSystem encounterSystem,
        LocationSystem locationSystem,
        ActionValidator actionValidator,
        ActionSystem actionSystem,
        QuestSystem questSystem,
        ItemSystem itemSystem,
        MessageSystem messageSystem
        )
    {
        this.gameState = gameState;
        this.currentRules = GameRules.StandardRuleset;

        this.EncounterSystem = encounterSystem;
        this.LocationSystem = locationSystem;
        this.ActionValidator = actionValidator;
        this.ContextEngine = actionSystem;
        this.QuestSystem = questSystem;
        this.ItemSystem = itemSystem;
        this.MessageSystem = messageSystem;
    }

    public void StartGame()
    {
        UpdateState();

        Item item = ItemSystem.GetItemFromName(ItemNames.CharmingPendant);
        if (item != null) gameState.Player.Equipment.SetMainHand(item);
    }

    public void UpdateState()
    {
        gameState.Actions.ClearCurrentUserAction();
        UpdateActiveQuests();
        UpdateLocationTravelOptions();
        UpdateLocationSpotOptions();
        UpdateAvailableActions();
    }

    public void UpdateAvailableActions()
    {
        gameState.Actions.SetCharacterActions(new List<UserActionOption>());
        gameState.Actions.SetQuestActions(new List<UserActionOption>());

        CreateGlobalActions();
        //CreateCharacterActions();
        //CreateQuestActions();
    }

    public void CreateGlobalActions()
    {
        List<UserActionOption> userActions = new List<UserActionOption>();
        int actionIndex = 1;

        gameState.Actions.SetGlobalActions(userActions);
    }

    private void OnPlayerEnterLocation(Location location)
    {
        List<ActionTemplate> allActionTemplates = ActionContent.LoadActionTemplates();
        List<LocationSpot> locationSpots = location.LocationSpots;
        ActionAvailabilityService availabilityService = new ActionAvailabilityService();

        // Clear existing actions in each LocationSpot
        foreach (LocationSpot spot in locationSpots)
        {
            spot.Actions.Clear();
        }

        foreach (ActionTemplate template in allActionTemplates)
        {
            // Use ActionAvailabilityService to check availability
            if (availabilityService.IsActionAvailable(template, location.LocationProperties))
            {
                // Create ActionImplementation using the factory
                ActionImplementation actionImplementation = ActionFactory.CreateAction(template, location);

                // Find a matching LocationSpot based on ActionType
                LocationSpot matchingSpot = null;
                foreach (LocationSpot spot in locationSpots)
                {
                    if (spot.ActionType == actionImplementation.ActionType)
                    {
                        matchingSpot = spot;
                        break;
                    }
                }

                // Add the action to the LocationSpot (if found)
                if (matchingSpot != null)
                {
                    matchingSpot.AddAction(actionImplementation);
                }
                else
                {
                    Console.WriteLine($"Warning: No LocationSpot found for ActionType '{actionImplementation.ActionType}' at location '{location.LocationName}'.");
                }
            }
        }


        List<UserActionOption> options = new List<UserActionOption>();
        foreach (LocationSpot locationSpot in locationSpots)
        {
            foreach (ActionImplementation action in locationSpot.Actions)
            {
                UserActionOption userActionOption = new UserActionOption(default, action.Name, false, action, locationSpot.LocationName, locationSpot.Name, default);
                options.Add(userActionOption);
            }
        }
        gameState.Actions.SetLocationSpotActions(options);
    }

    public void CreateQuestActions()
    {
        List<UserActionOption> userActions = new List<UserActionOption>();

        List<Quest> quests = QuestSystem.GetAvailableQuests();
        foreach (Quest quest in quests)
        {
            QuestStep step = quest.GetCurrentStep();

            ActionImplementation questAction = step.QuestAction;
            int actionIndex = 1;

            UserActionOption ua = new UserActionOption(actionIndex++, questAction.Name, false, questAction, step.Location, default, step.Character);
            userActions.Add(ua);
        }

        gameState.Actions.AddQuestActions(userActions);
    }


    private void UpdateActiveQuests()
    {
        List<Quest> quests = QuestSystem.GetAvailableQuests();
        gameState.Actions.ActiveQuests = quests;
    }

    public void SetEncounterChoices(Encounter encounter, LocationNames location)
    {
        string locationSpot = "LocationSpot";

        EncounterStage stage = EncounterSystem.GetCurrentStage(encounter);
        List<EncounterChoice> choices = stage.Choices;

        List<UserEncounterChoiceOption> choiceOptions = new List<UserEncounterChoiceOption>();
        foreach (EncounterChoice choice in choices)
        {
            UserEncounterChoiceOption option = new UserEncounterChoiceOption(
                choice.Index, choice.Description, location,
                encounter, stage, choice);

            choiceOptions.Add(option);
        }

        gameState.Actions.SetEncounterChoiceOptions(choiceOptions);
    }

    public Encounter GenerateEncounter(BasicActionTypes action, Location location, PlayerState playerState)
    {
        // Create initial context
        EncounterContext context = new(
            action,
            location.LocationType,
            location.LocationArchetype,
            gameState.World.CurrentTimeSlot,
            location.LocationProperties,
            playerState,
            new EncounterStateValues(
                outcome: 5 + (playerState.Level - location.DifficultyLevel),
                insight: 0,
                resonance: 5,
                pressure: 0
            ),
            1,
            location.DifficultyLevel
        );

        // Generate encounter
        Encounter encounter = EncounterSystem.GenerateEncounter(context);
        if (encounter == null) return null;

        // Setup initial choices
        SetEncounterChoices(encounter, location.LocationName);

        return encounter;
    }

    public void ExecuteEncounterChoice(UserEncounterChoiceOption choiceOption)
    {
        Encounter encounter = choiceOption.Encounter;
        EncounterChoice choice = choiceOption.EncounterChoice;

        Location location = LocationSystem.GetLocation(choiceOption.LocationName);

        // Get preview before execution
        string preview = EncounterSystem.GetChoicePreview(choice);
        MessageSystem.AddSystemMessage($"Executing choice: {preview}");

        // Execute the choice
        EncounterSystem.ExecuteChoice(encounter, choice, location.LocationProperties);

        // Check if we should proceed to next stage
        ProceedEncounter(encounter, location.LocationName);
    }

    private void ProceedEncounter(Encounter encounter, LocationNames locationName)
    {
        bool hasNextStage = EncounterSystem.GetNextStage(encounter);
        if (hasNextStage)
        {
            SetEncounterChoices(encounter, locationName);
        }
        else
        {
            gameState.Actions.CompleteActiveEncounter();
        }
    }

    public ActionResult ExecuteBasicAction(UserActionOption action, ActionImplementation basicAction)
    {
        Location location = LocationSystem.GetLocation(action.Location);

        gameState.Actions.SetCurrentUserAction(action);

        if (!ContextEngine.CanExecuteInContext(basicAction))
            return ActionResult.Failure("Current context prevents this action");

        Encounter encounter = GenerateEncounter(basicAction.ActionType, location, gameState.Player);
        if (encounter != null)
        {
            // Set as active encounter
            EncounterSystem.SetActiveEncounter(encounter);
            return ActionResult.Success("Action started!", new ActionResultMessages());
        }

        return GenerateNormalAction(action, basicAction);
    }

    private ActionResult GenerateNormalAction(UserActionOption action, ActionImplementation basicAction)
    {
        // 1. Check context requirements
        if (!ContextEngine.CanExecuteInContext(basicAction))
            return ActionResult.Failure("Current context prevents this action");

        // 2. Update quest progress if applicable
        QuestSystem.ProcessAction(basicAction);

        // 3. Apply character relationship effects
        //CharacterSystem.ProcessActionImpact(basicAction);

        // 4. Execute outcomes and check if day change is needed
        ActionImplementation modifiedAction = ContextEngine.ProcessActionOutcome(basicAction);

        ActionResultMessages allMessages = MessageSystem.GetAndClearChanges();
        gameState.Actions.SetLastActionResultMessages(allMessages);

        LocationNames location = action.Location;

        bool stillAlive = AdvanceTime(1); // Normal time advance

        return ActionResult.Success("Action success!", allMessages);
    }

    public ActionResult TravelToLocation(LocationNames locationName)
    {
        Location location = LocationSystem.GetLocation(locationName);
        gameState.World.SetNewLocation(location);
        UpdateState();

        ActionResult actionResult = ActionResult.Success($"Moved to {locationName}.", new ActionResultMessages());
        OnPlayerEnterLocation(gameState.World.CurrentLocation);

        return actionResult;
    }

    public void MoveToLocationSpot(LocationNames location, string locationSpotName)
    {
        LocationSpot locationSpot = LocationSystem.GetLocationSpotForLocation(location, locationSpotName);
        gameState.World.SetNewLocationSpot(locationSpot);
        UpdateState();
    }

    public List<LocationNames> GetConnectedLocations()
    {
        List<LocationNames> loc = LocationSystem.GetLocationResonances(gameState.World.CurrentLocation.LocationName);
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

    public bool CanMoveToSpot(string locationSpotName)
    {
        return true;
    }

    public bool AreRequirementsMet(UserActionOption action)
    {
        PlayerState Player = gameState.Player;
        return action.ActionImplementation.CanExecute(Player);
    }

    public void UpdateLocationSpotOptions()
    {
        Location location = gameState.World.CurrentLocation;
        List<LocationSpot> locationSpots = LocationSystem.GetLocationSpots(location);

        List<UserLocationSpotOption> userLocationSpotOption = new List<UserLocationSpotOption>();

        for (int i = 0; i < locationSpots.Count; i++)
        {
            LocationSpot locationSpot = locationSpots[i];
            UserLocationSpotOption locationSpotOption = new UserLocationSpotOption(i + 1, location.LocationName, locationSpot.Name);

            userLocationSpotOption.Add(locationSpotOption);
        }

        gameState.World.SetCurrentLocationSpotOptions(userLocationSpotOption);
    }

    public void UpdateLocationTravelOptions()
    {
        List<LocationNames> connectedLocations = GetConnectedLocations();

        List<UserLocationTravelOption> userTravelOptions = new List<UserLocationTravelOption>();

        for (int i = 0; i < connectedLocations.Count; i++)
        {
            LocationNames location = connectedLocations[i];

            UserLocationTravelOption travel = new UserLocationTravelOption(i + 1, location);

            userTravelOptions.Add(travel);
        }

        gameState.World.SetCurrentTravelOptions(userTravelOptions);
    }


    public void AdvanceTime()
    {
        AdvanceTime(1);
    }

    public void AdvanceTimeTo(int hours)
    {
        int timeToMidnight = 24 - gameState.World.CurrentTimeInHours;
        int timeToAdvance = timeToMidnight + hours;
        AdvanceTime(timeToAdvance);
    }

    public bool AdvanceTime(int inHours)
    {
        bool daySkip = false;
        // Advance the current time
        if (gameState.World.CurrentTimeInHours + inHours > 24)
        {
            daySkip = true;
        }

        gameState.World.CurrentTimeInHours = (gameState.World.CurrentTimeInHours + inHours) % 24;

        const int timeWindowsPerDay = 4;
        const int hoursPerTimeWindow = 6;
        int timeSlot = (gameState.World.CurrentTimeInHours / hoursPerTimeWindow) % timeWindowsPerDay;

        gameState.World.DetermineCurrentTimeSlot(timeSlot);

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
        PlayerState Player = gameState.Player;

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

        if (!hasFood) gameState.Player.ChangeHealth(currentRules.NoFoodEffectOnHealth);

        return Player.Health > Player.MinHealth;
    }

}