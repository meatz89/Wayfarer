using System.Text;

public class GameManager
{
    private const int hoursToAdvanceForActions = 2;

    public GameState gameState;
    private GameRules currentRules;

    public NarrativeSystem NarrativeSystem { get; }
    public LocationSystem LocationSystem { get; }
    public ActionValidator ActionValidator { get; }
    public ActionSystem ContextEngine { get; }
    public QuestSystem QuestSystem { get; }
    public ItemSystem ItemSystem { get; }
    public MessageSystem MessageSystem { get; }

    public GameManager(
        GameState gameState,
        NarrativeSystem narrativeSystem,
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

        this.NarrativeSystem = narrativeSystem;
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
        gameState.Actions.SetLocationSpotActions(new List<UserActionOption>());
        gameState.Actions.SetCharacterActions(new List<UserActionOption>());
        gameState.Actions.SetQuestActions(new List<UserActionOption>());

        CreateGlobalActions();
        OnPlayerEnterLocation(gameState.World.CurrentLocation);
        //CreateCharacterActions();
        CreateQuestActions();
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

        foreach (LocationSpot locationSpot in locationSpots)
        {
            locationSpot.Actions = new List<ActionImplementation>();
        }

        foreach (ActionTemplate template in allActionTemplates)
        {
            if (template.AvailabilityConditions.All(c => MeetsCondition(c, location.Properties)))
            {
                ActionImplementation actionImplementation = template.CreateActionImplementation();

                // Get the corresponding LocationSpot based on the action type
                LocationSpot? matchingSpot = locationSpots.FirstOrDefault(s => s.ActionType == actionImplementation.ActionType);

                // If a matching spot is found, add the action to it
                if (matchingSpot != null)
                {
                    matchingSpot.AddAction(actionImplementation);
                }
                else
                {
                    // Optional: Handle cases where no matching spot is found (log an error, use a default spot, etc.)
                    Console.WriteLine($"Warning: No LocationSpot found for ActionType '{actionImplementation.ActionType}' at location '{location.LocationName}'.");
                }
            }
        }

        List<UserActionOption> options = new List<UserActionOption>();
        foreach(LocationSpot locationSpot in locationSpots)
        {
            foreach (ActionImplementation action in locationSpot.Actions)
            {
                UserActionOption userActionOption = new UserActionOption()
                {
                    ActionImplementation = action,
                    Description = action.Name,
                    IsDisabled = false,
                    Location = locationSpot.LocationName,
                    LocationSpot = locationSpot.Name
                };
                options.Add(userActionOption);
            }
        }
        gameState.Actions.SetLocationSpotActions(options);
    }

    private bool MeetsCondition(LocationPropertyCondition condition, LocationProperties properties)
    {
        object actualValue = properties.GetProperty(condition.PropertyType);
        return actualValue.Equals(condition.ExpectedValue);
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

            UserActionOption ua = new UserActionOption
            {
                ActionImplementation = questAction,
                Description = questAction.Name,
                Index = actionIndex++,
                IsDisabled = false,
                Location = step.Location,
                Character = step.Character
            };
            userActions.Add(ua);
        }

        gameState.Actions.AddQuestActions(userActions);
    }


    private void UpdateActiveQuests()
    {
        List<Quest> quests = QuestSystem.GetAvailableQuests();
        gameState.Actions.ActiveQuests = quests;
    }

    public void SetNarrativeChoices(Narrative narrative)
    {
        NarrativeStage stage = NarrativeSystem.GetCurrentStage(narrative);
        List<NarrativeChoice> choices = NarrativeSystem.GetCurrentStageChoices(narrative);

        List<UserNarrativeChoiceOption> choiceOptions = new List<UserNarrativeChoiceOption>();
        foreach (NarrativeChoice choice in choices)
        {
            UserNarrativeChoiceOption option = new UserNarrativeChoiceOption()
            {
                Index = choice.Index,
                Description = choice.Description,
                Location = narrative.LocationName,
                Character = narrative.NarrativeCharacter,
                Narrative = narrative,
                NarrativeStage = stage,
                NarrativeChoice = choice
            };

            choiceOptions.Add(option);
        }

        gameState.Actions.SetNarrativeChoiceOptions(choiceOptions);
    }

    public void ExecuteNarrativeChoice(UserNarrativeChoiceOption choiceOption)
    {
        Narrative narrative = choiceOption.Narrative;
        NarrativeChoice choice = choiceOption.NarrativeChoice;

        NarrativeSystem.ExecuteChoice(narrative, choice);
        ProceedNarrative(narrative);
    }

    private void InitializeNarrative(Narrative narrative)
    {
        bool hasNextStage = NarrativeSystem.GetNextStage(narrative);
        if (hasNextStage)
        {
            SetNarrativeChoices(narrative);
        }
        else
        {
            gameState.Actions.CompleteActiveNarrative();
        }
    }

    private void ProceedNarrative(Narrative narrative)
    {
        bool hasNextStage = NarrativeSystem.GetNextStage(narrative);
        if (hasNextStage)
        {
            SetNarrativeChoices(narrative);
        }
        else
        {
            gameState.Actions.CompleteActiveNarrative();
        }
    }

    public ActionResult ExecuteBasicAction(UserActionOption action, ActionImplementation basicAction)
    {
        gameState.Actions.SetCurrentUserAction(action);

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

        Narrative narrative = NarrativeSystem.GetAvailableNarrative(modifiedAction.ActionType, location);
        if (narrative != null)
        {
            NarrativeSystem.SetActiveNarrative(narrative);
            InitializeNarrative(narrative);
        }

        bool stillAlive = AdvanceTime(1); // Normal time advance

        return ActionResult.Success("Action success!", allMessages);
    }

    public ActionResult MoveToLocation(LocationNames locationName)
    {
        Location location = LocationSystem.GetLocation(locationName);
        gameState.World.SetNewLocation(location);
        UpdateState();

        ActionResult actionResult = ActionResult.Success($"Moved to {locationName}.", new ActionResultMessages());

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
        List<LocationNames> loc = LocationSystem.GetLocationConnections(gameState.World.CurrentLocation.LocationName);
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
            UserLocationSpotOption locationSpotOption = new UserLocationSpotOption()
            {
                Index = i + 1,
                Location = location.LocationName,
                LocationSpot = locationSpot.Name
            };

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

            UserLocationTravelOption travel = new UserLocationTravelOption()
            {
                Index = i + 1,
                Location = location
            };

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