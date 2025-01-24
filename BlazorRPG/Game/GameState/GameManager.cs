using System.Text;
public class GameManager
{
    public GameState gameState;
    private GameRules currentRules;

    public EncounterSystem EncounterSystem { get; }
    public LocationSystem LocationSystem { get; }
    public ActionValidator ActionValidator { get; }
    public ActionSystem ContextEngine { get; }
    public QuestSystem QuestSystem { get; }
    public ItemSystem ItemSystem { get; }
    public MessageSystem MessageSystem { get; }
    public NarrativeSystem NarrativeSystem { get; }
    public JournalSystem JournalSystem { get; }
    public PlayerStatusSystem PlayerStatusSystem { get; }

    public GameManager(
        GameState gameState,
        EncounterSystem encounterSystem,
        LocationSystem locationSystem,
        ActionValidator actionValidator,
        ActionSystem actionSystem,
        QuestSystem questSystem,
        ItemSystem itemSystem,
        MessageSystem messageSystem,
        NarrativeSystem narrativeSystem,
        JournalSystem journalSystem,
        PlayerStatusSystem playerStatusSystem
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
        this.NarrativeSystem = narrativeSystem;
        this.JournalSystem = journalSystem;
        this.PlayerStatusSystem = playerStatusSystem;
    }

    public void StartGame()
    {
        TravelToLocation(gameState.Player.StartingLocation);
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

    private void OnPlayerEnterLocation(Location location)
    {
        List<ActionTemplate> allActionTemplates = LocationActionsContent.LoadLocationActionTemplates();

        List<LocationSpot> locationSpots = location.LocationSpots;
        PopulateLocationSpotActions(location, allActionTemplates, locationSpots);

        List<UserActionOption> options = GetUserActionOptions(locationSpots);
        gameState.Actions.SetLocationSpotActions(options);
    }

    private static List<UserActionOption> GetUserActionOptions(List<LocationSpot> locationSpots)
    {
        List<UserActionOption> options = new List<UserActionOption>();
        foreach (LocationSpot locationSpot in locationSpots)
        {
            CreateActionsForLocationSpot(options, locationSpot);
        }

        return options;
    }

    private static void CreateActionsForLocationSpot(List<UserActionOption> options, LocationSpot locationSpot)
    {
        List<ActionImplementation> locationSpotActions = locationSpot.Actions;
        foreach (ActionImplementation actionImplementation in locationSpotActions)
        {
            UserActionOption userActionOption =
                new UserActionOption(
                    default,
                    actionImplementation.Name,
                    false,
                    actionImplementation,
                    locationSpot.LocationName,
                    locationSpot.Name,
                    default);

            options.Add(userActionOption);
        }
    }

    public string GetLocationNarrative(LocationNames locationName)
    {
        return NarrativeSystem.GetLocationNarrative(locationName);
    }

    public void UpdateAvailableActions()
    {
        gameState.Actions.SetCharacterActions(new List<UserActionOption>());
        gameState.Actions.SetQuestActions(new List<UserActionOption>());

        CreateGlobalActions();
        //CreateCharacterActions();
        //CreateQuestActions();
    }

    public EncounterResult ExecuteEncounterChoice(UserEncounterChoiceOption choiceOption)
    {
        Encounter encounter = choiceOption.Encounter;
        EncounterChoice choice = choiceOption.EncounterChoice;

        Location location = LocationSystem.GetLocation(choiceOption.LocationName);
        LocationSpot locationSpot = LocationSystem.GetLocationSpotForLocation(choiceOption.LocationName, choiceOption.locationSpotName);

        // Execute the choice
        EncounterResult encounterResult = EncounterSystem.ExecuteChoice(encounter, choice, locationSpot.SpotProperties);
        gameState.Actions.LastEncounterResult = encounterResult;

        if (encounterResult.encounterResults != EncounterResults.Ongoing)
        {
            gameState.Actions.CompleteActiveEncounter();
        }
        else
        {
            if (IsGameOver(gameState.Player))
            {
                gameState.Actions.CompleteActiveEncounter();

                return new EncounterResult()
                {
                    encounter = encounter,
                    encounterResults = EncounterResults.GameOver,
                    EncounterEndMessage = "Game Over"
                };
            }
            ProceedEncounter(encounter);
        }

        return encounterResult;
    }

    private bool IsGameOver(PlayerState player)
    {
        bool canPayPhysical = player.PhysicalEnergy > 0 || player.Health > 1;
        bool canPayFocus = player.FocusEnergy > 0 || player.Concentration > 1;
        bool canPaySocial = player.SocialEnergy > 0 || player.Reputation > 1;

        bool isGameOver = !(canPayPhysical || canPayFocus || canPaySocial);
        return isGameOver;
    }

    private void ProceedEncounter(Encounter encounter)
    {
        encounter.AdvanceStage();
        SetEncounterChoices(encounter);
    }

    public void CreateGlobalActions()
    {
        List<UserActionOption> userActions = new List<UserActionOption>();
        int actionIndex = 1;

        gameState.Actions.SetGlobalActions(userActions);
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

            UserActionOption ua = new UserActionOption(
                actionIndex++, 
                questAction.Name, 
                false, 
                questAction, 
                step.Location, 
                default, 
                step.Character);

            userActions.Add(ua);
        }

        gameState.Actions.AddQuestActions(userActions);
    }


    private void UpdateActiveQuests()
    {
        List<Quest> quests = QuestSystem.GetAvailableQuests();
        gameState.Actions.ActiveQuests = quests;
    }

    public Encounter GenerateEncounter(ActionImplementation actionImplementation, Location location, PlayerState playerState, string locationSpotName)
    {
        List<LocationPropertyChoiceEffect> effects = LocationSystem.GetLocationEffects(location.LocationName, locationSpotName);
        LocationSpot? locationSpot = LocationSystem.GetLocationSpotForLocation(location.LocationName, locationSpotName);

        // Create initial context with our new value system
        int playerLevel = playerState.Level;
        List<PlayerStatusTypes> playerStatusTypes = PlayerStatusSystem.GetActiveStatusList();

        EncounterContext context = new EncounterContext(
            actionImplementation,
            location.LocationName,
            locationSpotName,
            actionImplementation.ActionType,
            location.LocationType,
            location.LocationArchetype,
            gameState.World.CurrentTimeSlot,
            locationSpot.SpotProperties,
            playerState,
            location.DifficultyLevel,
            effects,
            playerLevel,
            playerStatusTypes
        );

        UserActionOption action = gameState.Actions.LocationSpotActions.Where(x => x.LocationSpot == locationSpot.Name).First();
        ActionImplementation actionImpl = action.ActionImplementation;

        return EncounterSystem.GenerateEncounter(context, actionImpl);
    }


    public List<LocationPropertyChoiceEffect> GetLocationEffects(Encounter encounter, EncounterChoice choice)
    {
        LocationNames locationName = encounter.Context.LocationName;
        string locationSpot = encounter.Context.LocationSpotName;

        return LocationSystem.GetLocationEffects(locationName, locationSpot);
    }

    public List<LocationPropertyChoiceEffect> GetLocationEffects(EncounterChoice choice, string locationSpotName)
    {
        Location location = gameState.World.CurrentLocation;
        return LocationSystem.GetLocationEffects(location.LocationName, locationSpotName);
    }

    public void SetEncounterChoices(Encounter encounter)
    {
        List<UserEncounterChoiceOption> choiceOptions =
            EncounterSystem.GetChoices(encounter);

        gameState.Actions.SetEncounterChoiceOptions(choiceOptions);
    }

    public ActionResult ExecuteBasicAction(UserActionOption action)
    {
        Location location = LocationSystem.GetLocation(action.Location);
        gameState.Actions.SetCurrentUserAction(action);

        if (!ContextEngine.CanExecuteInContext(action.ActionImplementation))
            return ActionResult.Failure("Current context prevents this action");

        Encounter encounter = GenerateEncounter(action.ActionImplementation, location, gameState.Player, action.LocationSpot);
        if (encounter != null)
        {
            // Set as active encounter
            EncounterSystem.SetActiveEncounter(encounter);

            // Add this line to populate the initial choices
            SetEncounterChoices(encounter);

            return ActionResult.Success("Action started!", new ActionResultMessages());
        }

        ActionResult actionResult = ExecuteAction(action);
        return actionResult;
    }

    private ActionResult ExecuteAction(UserActionOption action)
    {
        ActionImplementation basicAction = action.ActionImplementation;

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
        if (gameState.World.CurrentLocation == null ||
            gameState.World.CurrentLocation.LocationName != locationName)
        {
            string narrative = GetLocationNarrative(locationName);
            JournalSystem.WriteJourneyEntry(narrative);
        }

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


    public List<Location> GetAllLocations()
    {
        List<Location> loc = LocationSystem.GetLocations();
        return loc;
    }

    public List<LocationNames> GetConnectedLocations()
    {
        List<LocationNames> loc =
            LocationSystem.GetLocations()
            .Where(x => x != gameState.World.CurrentLocation)
            .Select(x => x.LocationName)
            .ToList();

        return loc;
    }

    public bool CanTravelTo(LocationNames locationName)
    {
        List<LocationNames> locs = GetConnectedLocations();
        bool canTravel = locs.Contains(locationName);

        int cost = GetTravelCostForLocation(locationName);

        return canTravel;
    }

    private int GetTravelCostForLocation(LocationNames locationName)
    {
        // Todo: Calculate from range
        return 1;
    }

    public bool CanMoveToSpot(string locationSpotName)
    {
        return true;
    }

    public bool AreRequirementsMet(UserActionOption action)
    {
        return action.ActionImplementation.CanExecute(gameState);
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

        if (!hasFood) gameState.Player.ModifyHealth(currentRules.NoFoodEffectOnHealth);

        return Player.Health > Player.MinHealth;
    }

    private static void PopulateLocationSpotActions(
        Location location,
        List<ActionTemplate> allActionTemplates,
        List<LocationSpot> locationSpots)
    {
        // Clear existing actions in each LocationSpot
        foreach (LocationSpot spot in locationSpots)
        {
            spot.Actions.Clear();
        }

        // For each action template, find matching spots and create actions
        foreach (ActionTemplate actionTemplate in allActionTemplates)
        {
            foreach (LocationSpot locationSpot in locationSpots)
            {
                CreateActionForLocationSpot(location, actionTemplate, locationSpot);
            }
        }
    }

    private static void CreateActionForLocationSpot(Location location, ActionTemplate actionTemplate, LocationSpot locationSpot)
    {
        if (!actionTemplate.IsValidForSpot(location, locationSpot))
        {
            return;
        }

        ActionImplementation actionImplementation = 
            ActionFactory.CreateAction(actionTemplate, location);

        locationSpot.AddAction(actionImplementation);
    }
}