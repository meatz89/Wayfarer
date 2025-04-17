using System.Data;
using System.Text;

public class GameManager
{
    public PlayerState playerState => gameState.PlayerState;
    public WorldState worldState => gameState.WorldState;

    public GameState gameState;
    public PostEncounterEvolutionSystem evolutionSystem { get; }
    public LocationCreationSystem locationCreationSystem { get; }
    public LocationSystem LocationSystem { get; }
    public ItemSystem ItemSystem { get; }
    public NarrativeService NarrativeService { get; }
    public MessageSystem MessageSystem { get; }
    public ActionFactory ActionFactory { get; }
    public ActionRepository ActionRepository { get; }
    public TravelManager TravelManager { get; }
    public ActionGenerator ActionGenerator { get; }
    public CharacterSystem CharacterSystem { get; }
    public OpportunitySystem OpportunitySystem { get; }
    public PlayerProgression PlayerProgression { get; }
    public EncounterSystem EncounterSystem { get; }
    public TutorialManager TutorialManager { get; }

    private bool _useMemory = false;
    private bool _processStateChanges = false;

    public GameManager(
        GameState gameState,
        PostEncounterEvolutionSystem postEncounterEvolutionService,
        LocationCreationSystem locationCreationSystem,
        EncounterSystem EncounterSystem,
        LocationSystem locationSystem,
        ItemSystem itemSystem,
        NarrativeService narrativeService,
        MessageSystem messageSystem,
        ActionFactory actionFactory,
        ActionRepository actionRepository,
        TravelManager travelManager,
        ActionGenerator actionGenerator,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        PlayerProgression playerProgression,
        TutorialManager TutorialManager,
        IConfiguration configuration
        )
    {
        this.gameState = gameState;
        this.EncounterSystem = EncounterSystem;
        this.LocationSystem = locationSystem;
        this.ItemSystem = itemSystem;
        this.NarrativeService = narrativeService;
        this.MessageSystem = messageSystem;
        evolutionSystem = postEncounterEvolutionService;
        this.locationCreationSystem = locationCreationSystem;
        ActionFactory = actionFactory;
        ActionRepository = actionRepository;
        TravelManager = travelManager;
        ActionGenerator = actionGenerator;
        CharacterSystem = characterSystem;
        OpportunitySystem = opportunitySystem;
        PlayerProgression = playerProgression;
        this.TutorialManager = TutorialManager;
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
        _useMemory = configuration.GetValue<bool>("useMemory");
    }

    public async Task StartGame()
    {
        Location startingLocation = await LocationSystem.Initialize(GameRules.StandardRuleset.StartingLocation);
        string startingLocationName = gameState.PlayerState.StartingLocation;

        worldState.RecordLocationVisit(startingLocationName);
        TravelManager.TravelToLocation(startingLocationName, TravelMethods.Walking);
        await OnLocationArrival(startingLocationName);

        if (worldState.CurrentLocationSpot == null && worldState.CurrentLocation?.LocationSpots?.Any() == true)
        {
            Console.WriteLine("Current location spot is null despite spots existing - manually setting");
            worldState.SetCurrentLocationSpot(worldState.CurrentLocation.LocationSpots.First());
        }

        await UpdateState();

        if (gameState.GameMode == Modes.Tutorial)
        {
            InitializeTutorial();
        }

        Location? currentLoc = worldState.CurrentLocation;
        Console.WriteLine($"Game started at: {currentLoc?.Name}, Current spot: {worldState.CurrentLocationSpot?.Name}");
    }

    private void InitializeTutorial()
    {
        // Set initial time (8:00 AM)
        worldState.CurrentTimeInHours = 8;
        worldState.DetermineCurrentTimeWindow(1); // Morning

        playerState.Food = 0;
        playerState.MedicinalHerbs = 0;
        playerState.Energy = (int)(playerState.MaxEnergy * 0.8);

        // Initialize the TutorialManager
        TutorialManager.Initialize();
    }

    public async Task<ActionImplementation> ExecuteAction(UserActionOption action)
    {
        // Store action context for current execution
        ActionImplementation actionImplementation = action.ActionImplementation;
        var location = LocationSystem.GetLocation(action.Location);
        var locationSpot = action.LocationSpot;

        // Set current action in game state
        gameState.ActionStateTracker.SetCurrentUserAction(action);

        // Use our action classification system to determine execution path
        ActionExecutionType executionType = GetExecutionType(action.ActionImplementation);

        switch (executionType)
        {
            case ActionExecutionType.Encounter:
                EncounterManager encounterManager = await PrepareEncounter(actionImplementation);
                gameState.ActionStateTracker.SetActiveEncounter(encounterManager);

                break;

            case ActionExecutionType.Basic:
            default:
                await ProcessActionCompletion(actionImplementation);

                await UpdateState();
                break;
        }

        return actionImplementation;
    }

    public async Task ProcessActionCompletion(ActionImplementation actionImplementation)
    {
        ApplyActionOutcomes(actionImplementation);
        gameState.ActionStateTracker.CompleteAction();

        string location = actionImplementation.DestinationLocation;
        if (!string.IsNullOrWhiteSpace(location))
        {
            await OnLocationArrival(location);
        }
        string locationSpot = actionImplementation.DestinationLocationSpot;
        if (!string.IsNullOrWhiteSpace(locationSpot))
        {
            await MoveToLocationSpot(locationSpot);
        }

        UpdateTime(actionImplementation.TimeCostHours);
        await UpdateState();
    }

    private void UpdateTime(int timeCostHours)
    {
        int hours = timeCostHours;
        for (int i = 0; i < hours; i++)
        {
            gameState.TimeManager.AdvanceTime(1);
        }

        Location currentLocation = worldState.CurrentLocation;
        currentLocation.OnTimeChanged(worldState.WorldTime);
    }

    public void CreateGlobalActions()
    {
        List<UserActionOption> userActions = new List<UserActionOption>();

        // Only add food consumption if player has food
        if (gameState.PlayerState.Food > 0)
        {
            ActionTemplate foodActionTemplate = ActionRepository.GetAction(ActionNames.ConsumeFood.ToString());
            if (foodActionTemplate != null)
            {
                ActionImplementation consumeFoodAction = ActionFactory.CreateActionFromTemplate(foodActionTemplate);

                // Check if requirements are met
                bool requirementsMet = consumeFoodAction.CanExecute(gameState);

                UserActionOption consumeFoodOption = new UserActionOption(
                    consumeFoodAction.ActionId.ToString(),
                    consumeFoodAction.Name.ToString(),
                    !requirementsMet, // Disabled if requirements aren't met
                    consumeFoodAction,
                    gameState.WorldState.CurrentLocation?.Name ?? "Global",
                    gameState.WorldState.CurrentLocationSpot?.Name ?? "Global",
                    null,
                    0,
                    string.Empty);

                userActions.Add(consumeFoodOption);
            }
        }

        // Only add medicinal herbs consumption if player has herbs
        if (gameState.PlayerState.MedicinalHerbs > 0)
        {
            ActionTemplate herbsActionTemplate = ActionRepository.GetAction(ActionNames.ConsumeMedicinalHerbs.ToString());
            if (herbsActionTemplate != null)
            {
                ActionImplementation consumeHerbsAction = ActionFactory.CreateActionFromTemplate(herbsActionTemplate);

                // Check if requirements are met
                bool requirementsMet = consumeHerbsAction.CanExecute(gameState);

                UserActionOption consumeHerbsOption = new UserActionOption(
                    consumeHerbsAction.ActionId.ToString(),
                    consumeHerbsAction.Name.ToString(),
                    !requirementsMet, // Disabled if requirements aren't met
                    consumeHerbsAction,
                    gameState.WorldState.CurrentLocation?.Name ?? "Global",
                    gameState.WorldState.CurrentLocationSpot?.Name ?? "Global",
                    null,
                    0,
                    string.Empty);

                userActions.Add(consumeHerbsOption);
            }
        }

        gameState.ActionStateTracker.SetGlobalActions(userActions);
    }
    
    public async Task InitiateTravelToLocation(string locationName)
    {
        // Create travel action using TravelManager
        ActionImplementation travelAction = TravelManager.TravelToLocation(locationName, TravelMethods.Walking);

        // Create option with consistent structure
        UserActionOption travelOption = new UserActionOption(
            "Travel", "Travel to " + locationName, false, travelAction,
            worldState.CurrentLocation.Name, worldState.CurrentLocationSpot.Name,
            null, worldState.CurrentLocation.Difficulty, null);

        // Use unified action execution
        await ExecuteAction(travelOption);
    }

    public void ApplyActionOutcomes(ActionImplementation action)
    {
        foreach (Outcome energyCost in action.EnergyCosts)
        {
            energyCost.Apply(gameState);
            MessageSystem.AddOutcome(energyCost);
        }

        foreach (Outcome cost in action.Costs)
        {
            cost.Apply(gameState);
            MessageSystem.AddOutcome(cost);
        }

        foreach (Outcome reward in action.Rewards)
        {
            reward.Apply(gameState);
            MessageSystem.AddOutcome(reward);
        }
    }

    private async Task<List<UserActionOption>> CreateActionsForLocationSpot
        ( Location location, LocationSpot locationSpot)
    {
        var currentLocation = worldState.CurrentLocation?.Name;
        if (string.IsNullOrWhiteSpace(currentLocation)) return new List<UserActionOption>();

        WorldStateInput worldStateInput = await CreateWorldStateInput(currentLocation);

        List<string> locationSpotActions = locationSpot.ActionIds.ToList();

        List<UserActionOption> options = new List<UserActionOption>();

        foreach (string locationSpotAction in locationSpotActions)
        {
            ActionTemplate actionTemplate = ActionRepository.GetAction(locationSpotAction);
            if (actionTemplate == null)
            {
                var actionId =
                    await ActionGenerator.GenerateActionAndEncounter(
                    worldStateInput,
                    actionTemplate.ActionId,
                    locationSpotAction,
                    locationSpot.Name,
                    location.Name);

                actionTemplate = ActionRepository.GetAction(locationSpotAction);
            }

            ActionImplementation actionImplementation = ActionFactory.CreateActionFromTemplate(actionTemplate);

            UserActionOption userActionOption =
                new UserActionOption(
                    actionImplementation.ActionId.ToString(),
                    actionImplementation.Name.ToString(),
                    false,
                    actionImplementation,
                    locationSpot.LocationName,
                    locationSpot.Name,
                    default,
                    location.Difficulty,
                    string.Empty);

            if (AreRequirementsMet(userActionOption))
            {
               options.Add(userActionOption);
            }
        }

        return options;
    }

    private async Task<EncounterManager> PrepareEncounter(ActionImplementation actionImplementation)
    {
        Location location = worldState.CurrentLocation;
        string locationId = location.Name;
        string locationName = location.Name;

        // Get time of day
        string timeOfDay = GetTimeOfDay(worldState.CurrentTimeMinutes);

        // Find characters at this location
        List<Character> presentCharacters = worldState.GetCharacters()
            .Where(c => c.Location == locationId)
            .ToList();

        // Find available opportunities
        List<Opportunity> opportunities = worldState.GetOpportunities()
            .Where(o => o.Location == locationId && o.Status == "Available")
            .ToList();

        // Get player's history with this location
        List<string> previousInteractions = new();

        LocationSpot? locationSpot = LocationSystem.GetLocationSpotForLocation(
            location.Name, worldState.CurrentLocationSpot.Name);

        // Create initial context with our new value system
        int playerLevel = playerState.Level;

        EncounterContext context = new EncounterContext()
        {
            ActionImplementation = actionImplementation,
            BasicActionType = actionImplementation.BasicActionType,
            Location = location,
            LocationSpot = locationSpot,
        };

        string id = actionImplementation.ActionId;

        EncounterManager encounterManager = await EncounterSystem
            .GenerateEncounter(id, location, locationSpot.Name, context, worldState, playerState, actionImplementation);

        List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(encounterManager.EncounterResult);
        gameState.ActionStateTracker.SetEncounterChoiceOptions(choiceOptions);

        return encounterManager;
    }

    public async Task<EncounterResult> ExecuteEncounterChoice(UserEncounterChoiceOption choiceOption)
    {
        Location location = LocationSystem.GetLocation(choiceOption.LocationName);

        var currentLocation = worldState.CurrentLocation?.Name;
        if (string.IsNullOrWhiteSpace(currentLocation)) return null;

        WorldStateInput worldStateInput = await CreateWorldStateInput(currentLocation);

        // Execute the choice
        EncounterResult encounterResult = await EncounterSystem.ExecuteChoice(
            choiceOption.narrativeResult,
            choiceOption.Choice,
            worldStateInput);

        ActionResults currentEncounterResult = encounterResult.ActionResult;
        if (currentEncounterResult == ActionResults.Ongoing)
        {
            ProcessOngoingEncounter(encounterResult);
        }
        else
        {
            await ProcessEncounterOutcome(encounterResult);
        }

        return encounterResult;
    }

    private void ProcessOngoingEncounter(EncounterResult encounterResult)
    {
        if (!IsGameOver(gameState.PlayerState))
        {
            gameState.ActionStateTracker.EncounterResult = encounterResult;

            List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(encounterResult);
            gameState.ActionStateTracker.SetEncounterChoiceOptions(choiceOptions);
        }
        else
        {
            gameState.ActionStateTracker.CompleteAction();
            return;
        }
    }

    private async Task OnLocationArrival(string targetLocation)
    {
        Location travelLocation = gameState.WorldState.GetLocation(targetLocation);

        var currentLocation = worldState.CurrentLocation?.Name;
        if (string.IsNullOrWhiteSpace(currentLocation))
        {
            worldState.SetCurrentLocation(travelLocation);
            currentLocation = worldState.CurrentLocation?.Name;
        }

        WorldStateInput worldStateInput = await CreateWorldStateInput(currentLocation);

        bool isFirstVisit = worldState.IsFirstVisit(travelLocation.Name);
        if (isFirstVisit)
        {
            Location originLocation = worldState.GetLocation(currentLocation);
            int newLocationDepth = originLocation.Depth + 1;

            travelLocation =
                await locationCreationSystem.PopulateLocation(
                targetLocation,
                originLocation.Name,
                newLocationDepth,
                worldStateInput);

            worldState.RecordLocationVisit(travelLocation.Name);
        }

        if (travelLocation.LocationSpots.Count > 0)
        {
            worldState.CurrentLocationSpot = travelLocation.LocationSpots[0];
        }

        if (travelLocation == null) return;

        await UpdateState();
    }

    /// <summary>
    /// ONLY Encounter related outcome that is NOT ALSO included in basic actions
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public async Task ProcessEncounterOutcome(EncounterResult result)
    {
        worldState.MarkEncounterCompleted(result.ActionImplementation.ActionId);

        gameState.ActionStateTracker.EncounterResult = result;

        NarrativeResult narrativeResult = result.NarrativeResult;
        string narrative = narrativeResult.SceneNarrative;
        string outcome = narrativeResult.Outcome.ToString();

        if (result.ActionResult == ActionResults.EncounterSuccess)
        {
            GainExp(result);
        }

        if (_processStateChanges)
        {
            await ProcessPostEncounterEvolution(result, narrative, outcome);
        }
    }

    private void GainExp(EncounterResult result)
    {
        int xpAward;

        switch (result.NarrativeResult.Outcome)
        {
            case EncounterOutcomes.Exceptional:
                xpAward = 50;
                break;
            case EncounterOutcomes.Standard:
                xpAward = 30;
                break;
            case EncounterOutcomes.Partial:
                xpAward = 15;
                break;
            default:
                xpAward = 5; // Even failure gives some XP
                break;
        }

        xpAward += 10;
        PlayerProgression.AddExperience(xpAward);

        // Add message about XP gain
        MessageSystem.AddSystemMessage($"Gained {xpAward} experience points");
    }

    private async Task ProcessPostEncounterEvolution(EncounterResult result, string narrative, string outcome)
    {
        // If not a travel encounter, evolve the current location
        BasicActionTypes basicActionType = result.ActionImplementation.BasicActionType;
         
        Location currentLocation = worldState.GetLocation(result.NarrativeContext.LocationName);
        if (_useMemory)
        {
            await CreateMemoryRecord(result);
        }

        // Prepare the input
        PostEncounterEvolutionInput input = evolutionSystem.PreparePostEncounterEvolutionInput(narrative, outcome);

        WorldStateInput worldStateInput = await CreateWorldStateInput(currentLocation.Name);

        // Process world evolution
        PostEncounterEvolutionResult evolutionResponse = await evolutionSystem.ProcessEncounterOutcome(result.NarrativeContext, input, result, worldStateInput);

        // Store the evolution response in the result
        result.PostEncounterEvolution = evolutionResponse;

        // Update world state
        await evolutionSystem.IntegrateEncounterOutcome(evolutionResponse, worldState, LocationSystem, playerState, worldStateInput);
    }

    private async Task CreateMemoryRecord(EncounterResult encounterResult)
    {
        string oldMemory = await MemoryFileAccess.ReadFromMemoryFile();

        // Create memory entry
        MemoryConsolidationInput memoryInput = new MemoryConsolidationInput { OldMemory = oldMemory };

        string currentLocation = worldState.CurrentLocation.Name;
        if (string.IsNullOrWhiteSpace(currentLocation)) return;
        WorldStateInput worldStateInput = await CreateWorldStateInput(currentLocation);

        string memoryEntry = await evolutionSystem.ConsolidateMemory(encounterResult.NarrativeContext, memoryInput, worldStateInput);

        string location = encounterResult.NarrativeContext.LocationName;
        string locationSpot = encounterResult.NarrativeContext.locationSpotName;
        string actionName = encounterResult.ActionImplementation.Name;
        string goal = encounterResult.ActionImplementation.Goal;

        string title = $"{location} - {locationSpot}, {actionName} - {goal}" + Environment.NewLine;

        string memoryEntryToWrite = title + memoryEntry;

        await MemoryFileAccess.WriteToMemoryFile(memoryEntryToWrite);
    }

    public List<UserEncounterChoiceOption> GetUserEncounterChoiceOptions(EncounterResult encounterResult)
    {
        NarrativeResult narrativeResult = encounterResult.NarrativeResult;
        List<ChoiceCard> choices = EncounterSystem.GetChoices();
        List<UserEncounterChoiceOption> choiceOptions = new List<UserEncounterChoiceOption>();

        int i = 0;
        foreach (ChoiceCard choice in choices)
        {
            i++;
            NarrativeContext narrativeContext = encounterResult.NarrativeContext;

            UserEncounterChoiceOption option = new UserEncounterChoiceOption(
                i,
                choice.GetDetails(),
                "Narrative",
                narrativeContext.LocationName,
                "locationSpotName",
                encounterResult,
                narrativeResult,
                choice);

            choiceOptions.Add(option);
        }

        return choiceOptions;
    }

    public ChoiceProjection GetChoicePreview(UserEncounterChoiceOption choiceOption)
    {
        Location location = LocationSystem.GetLocation(choiceOption.LocationName);

        // Execute the choice
        EncounterManager encounter = gameState.ActionStateTracker.GetCurrentEncounter();
        ChoiceProjection choiceProjection = EncounterSystem.GetChoiceProjection(encounter, choiceOption.Choice);
        return choiceProjection;
    }

    public async Task MoveToLocationSpot(string locationSpotName)
    {
        string locationName = gameState.WorldState.CurrentLocation.Name;

        LocationSpot locationSpot = LocationSystem.GetLocationSpotForLocation(locationName, locationSpotName);
        gameState.WorldState.SetCurrentLocationSpot(locationSpot);

        await UpdateState();
    }

    private bool IsGameOver(PlayerState player)
    {
        if (player.Health <= 0) return true;
        if (player.Concentration <= 0) return true;
        if (player.Confidence <= 0) return true;

        return false;
    }

    public List<Location> GetPlayerKnownLocations()
    {
        return LocationSystem.GetKnownLocations();
    }

    public List<string> GetConnectedLocations()
    {
        List<string> loc =
            LocationSystem.GetKnownLocations()
            .Where(x => x != gameState.WorldState.CurrentLocation)
            .Select(x => x.Name)
            .ToList();

        return loc;
    }

    public bool CanTravelTo(string destinationName)
    {
        if (worldState.CurrentLocation == null)
            return false;

        // Check if locations are directly connected
        return worldState.CurrentLocation.ConnectedTo?.Contains(destinationName) ?? false;
    }

    public bool CanMoveToSpot(string locationSpotName)
    {
        return true;
    }

    public bool AreRequirementsMet(UserActionOption action)
    {
        // Check time window constraints
        List<TimeWindows> timeWindows = action.ActionImplementation.TimeWindows;
        bool anyTimeWindow = timeWindows != null && timeWindows.Any();
        if (anyTimeWindow && !timeWindows.Contains(gameState.WorldState.WorldTime))
        {
            return false; // Not available during current time window
        }

        // Check if the action has been completed and is non-repeatable
        if (!action.ActionImplementation.IsRepeatable)
        {
            string encounterId = action.ActionId;
            if (gameState.WorldState.IsEncounterCompleted(encounterId))
            {
                return false; // Already completed this non-repeatable action
            }
        }

        // Continue with existing requirement checks
        return action.ActionImplementation.CanExecute(gameState);
    }

    public bool AdvanceTime(int inHours)
    {
        bool daySkip = false;
        // Advance the current time
        if (gameState.WorldState.CurrentTimeInHours + inHours > 24)
        {
            daySkip = true;
        }

        gameState.WorldState.CurrentTimeInHours = (gameState.WorldState.CurrentTimeInHours + inHours) % 24;

        const int timeWindowsPerDay = 4;
        const int hoursPerTimeWindow = 6;
        int timeWindow = (gameState.WorldState.CurrentTimeInHours / hoursPerTimeWindow) % timeWindowsPerDay;

        gameState.WorldState.DetermineCurrentTimeWindow(timeWindow);

        if (daySkip)
        {
        }

        return true;
    }


    public string GetTimeOfDay(int totalMinutes)
    {
        // Calculate hours (24-hour clock)
        int totalHours = (totalMinutes / 60) % 24;

        if (totalHours >= 5 && totalHours < 12) return "Morning";
        if (totalHours >= 12 && totalHours < 17) return "Afternoon";
        if (totalHours >= 17 && totalHours < 21) return "Evening";
        return "Night";
    }

    private async Task CreateLocationActions(Location currentLocation, LocationSpot locationSpot)
    {
        List<LocationSpot> locationSpots = currentLocation.LocationSpots;
        Console.WriteLine($"Location {currentLocation.Name} has {locationSpots?.Count ?? 0} spots");

        worldState.SetCurrentLocation(currentLocation);

        var locationSpotActionOptions = await CreateActionsForLocationSpot(currentLocation, locationSpot);
        gameState.ActionStateTracker.SetLocationSpotActions(locationSpotActionOptions);
    }

    public EncounterViewModel? GetEncounterViewModel()
    {
        EncounterManager encounterManager = EncounterSystem.GetCurrentEncounter();
        List<UserEncounterChoiceOption> userEncounterChoiceOptions = EncounterSystem.GetUserEncounterChoiceOptions();

        if (encounterManager == null)
        {
            EncounterViewModel encounterViewModel = new();
            encounterViewModel.CurrentEncounter = encounterManager;
            encounterViewModel.CurrentChoices = userEncounterChoiceOptions;
            encounterViewModel.ChoiceSetName = "Current Situation";
        }
        else
        {
            EncounterState state = encounterManager.EncounterState;

            EncounterViewModel model = new EncounterViewModel();
            model.CurrentEncounter = encounterManager;
            model.CurrentChoices = userEncounterChoiceOptions;
            model.ChoiceSetName = "Current Situation";
            model.State = state;
            model.EncounterResult = encounterManager.EncounterResult;
            return model;
        }

        return null;
    }

    private async Task<WorldStateInput> CreateWorldStateInput(string currentLocation)
    {
        WorldState worldState = gameState.WorldState;
        PlayerState playerState = gameState.PlayerState;

        // Get current depth and hub depth
        int currentDepth = worldState.GetLocationDepth(worldState.CurrentLocation?.Name ?? "");

        // Create context for location generation
        WorldStateInput context = new WorldStateInput
        {
            CharacterArchetype = playerState.Archetype.ToString(),

            Health = playerState.Health,
            MaxHealth = playerState.MaxHealth,
            Concentration = playerState.Concentration,
            MaxConcentration = playerState.MaxConcentration,
            Confidence = playerState.Confidence,
            MaxConfidence = playerState.MaxConfidence,
            Energy = playerState.Energy,
            MaxEnergy = playerState.MaxEnergy,
            Coins = playerState.Coins,

            CurrentLocation = currentLocation,
            LocationSpots = LocationSystem.FormatLocationSpots(worldState.CurrentLocation),
            CurrentSpot = worldState.CurrentLocationSpot.Name,
            LocationDepth = currentDepth,
            ConnectedLocations = LocationSystem.FormatLocations(LocationSystem.GetConnectedLocations()),

            Inventory = FormatPlayerInventory(playerState.Inventory),
            Relationships = playerState.Relationships.ToString(),

            KnownCharacters = CharacterSystem.FormatKnownCharacters(worldState.GetCharacters()),
            ActiveOpportunities = OpportunitySystem.FormatActiveOpportunities(worldState.GetOpportunities()),

            MemorySummary = await MemoryFileAccess.ReadFromMemoryFile(),
        };

        await MemoryFileAccess.WriteToLogFile(context);

        return context;
    }

    private string FormatPlayerInventory(Inventory inventory)
    {
        if (inventory == null || inventory.UsedCapacity == 0)
        {
            return "No significant items";
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Carrying:");

        // Group items by type and count them
        Dictionary<ItemTypes, int> itemCounts = new Dictionary<ItemTypes, int>();
        foreach (ItemTypes itemType in Enum.GetValues(typeof(ItemTypes)))
        {
            if (itemType != ItemTypes.None)
            {
                int count = inventory.GetItemCount(itemType.ToString());
                if (count > 0)
                {
                    itemCounts[itemType] = count;
                }
            }
        }

        // Format items with counts
        foreach (KeyValuePair<ItemTypes, int> item in itemCounts)
        {
            string itemName = GetItemName(item.Key);

            if (item.Value > 1)
            {
                sb.AppendLine($"- {itemName} ({item.Value})");
            }
            else
            {
                sb.AppendLine($"- {itemName}");
            }
        }

        return sb.ToString();
    }

    private string GetItemName(ItemTypes itemType)
    {
        // Convert enum to display name
        return SplitCamelCase(itemType.ToString());
    }

    public static string SplitCamelCase(string str)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            str,
            "([A-Z])",
            " $1",
            System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }

    public async Task ExecuteActionByName(string actionName)
    {
        // Get action template from repository
        ActionTemplate actionTemplate = ActionRepository.GetAction(actionName);
        if (actionTemplate == null)
        {
            Console.WriteLine($"Action not found: {actionName}");
            return;
        }

        // Check requirements
        ActionImplementation action = ActionFactory.CreateActionFromTemplate(actionTemplate);

        if (!action.CanExecute(gameState))
        {
            Console.WriteLine($"Requirements not met for action: {actionName}");
            return;
        }

        // Apply the outcomes directly
        ApplyActionOutcomes(action);

        // Update state
        await UpdateState();

        // Log action execution
        Console.WriteLine($"Executed action by name: {actionName}");
    }

    public ActionExecutionType GetExecutionType(ActionImplementation action)
    {
        if (action.ActionType == ActionTypes.Encounter)
        {
            return ActionExecutionType.Encounter;
        }

        // Basic action
        return ActionExecutionType.Basic;
    }

    public async Task UpdateState()
    {
        gameState.ActionStateTracker.ClearCurrentUserAction();

        CreateGlobalActions();

        await CreateLocationActions(worldState.CurrentLocation, worldState.CurrentLocationSpot);

        if (gameState.GameMode == Modes.Tutorial)
        {
            TutorialManager.CheckTutorialProgress();
        }
    }

    internal EncounterResult GetEncounterResultFor(ActionImplementation actionImplementation)
    {
        return new EncounterResult()
        {
            ActionImplementation = actionImplementation,
            ActionResult = ActionResults.EncounterSuccess,
            EncounterEndMessage = "Success",
            NarrativeResult = null,
            NarrativeContext = null
        };
    }
}
  
public enum ActionExecutionType
{
    Basic,          // Immediate effect, no encounter, repeatable
    Encounter,      // Multi-turn strategic challenge, non-repeatable once completed
}
