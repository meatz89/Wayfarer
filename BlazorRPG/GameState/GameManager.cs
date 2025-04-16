using System.Data;
using System.Text;

public partial class GameManager
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

    public ActionImplementation actionImplementation;
    public Location location;
    public string locationSpot;

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

        // Tutorial mode initialization
        if (gameState.GameMode == Modes.Tutorial)
        {
            InitializeTutorial();
        }

        Location? currentLoc = worldState.CurrentLocation;
        Console.WriteLine($"Game started at: {currentLoc?.Name}, Current spot: {worldState.CurrentLocationSpot?.Name}");
    }

    private void InitializeTutorial()
    {
        // Set initial tutorial state
        gameState.TutorialState.SetFlag("TutorialStarted");

        // Set initial time (8:00 AM)
        worldState.CurrentTimeInHours = 8;
        worldState.DetermineCurrentTimeWindow(1); // Morning

        // Ensure player has 0 Food and 0 Herbs to start
        playerState.Food = 0;
        playerState.MedicinalHerbs = 0;

        // Set starting energy at 80% (player will need to learn about resources)
        playerState.Energy = (int)(playerState.MaxEnergy * 0.8);
    }

    public async Task ExecuteAction(UserActionOption action)
    {
        // Store action context for current execution
        actionImplementation = action.ActionImplementation;
        location = LocationSystem.GetLocation(action.Location);
        locationSpot = action.LocationSpot;

        // Set current action in game state
        gameState.ActionStateTracker.SetCurrentUserAction(action);

        // Tutorial progression tracking
        UpdateTutorialStateForAction(action);

        // Use our action classification system to determine execution path
        ActionExecutionType executionType = GetExecutionType(action.ActionImplementation);

        switch (executionType)
        {
            case ActionExecutionType.Encounter:
                gameState.ActionStateTracker.SetActiveEncounter();
                break;

            case ActionExecutionType.Travel:
                await ProcessBasicAction(action);

                // Travel actions should always set PendingTravel
                if (gameState.PendingTravel.IsTravelPending)
                {
                    await CompleteTravel(gameState.PendingTravel.TravelDestination);
                }
                else
                {
                    Console.WriteLine("Warning: Travel action did not set pending travel");
                }

                await UpdateState();
                break;

            case ActionExecutionType.Basic:
            default:
                await ProcessBasicAction(action);

                // Handle any spot changes
                if (gameState.PendingTravel.IsTravelPending)
                {
                    await CompleteTravel(gameState.PendingTravel.TravelDestination);
                }

                await UpdateState();
                break;
        }
    }

    private void UpdateTutorialStateForAction(UserActionOption action)
    {
        if (gameState.GameMode != Modes.Tutorial)
            return;

        string actionId = action.ActionId;
        string locationSpot = action.LocationSpot;

        // Track location visits
        if (locationSpot == "Forest Stream" && !gameState.TutorialState.CheckFlag("VisitedStream"))
        {
            gameState.TutorialState.SetFlag("VisitedStream");
            gameState.TutorialState.SetFlag("FoundStream");
        }
        else if (locationSpot == "High Ground" && !gameState.TutorialState.CheckFlag("VisitedHighGround"))
        {
            gameState.TutorialState.SetFlag("VisitedHighGround");
        }

        // Track specific actions
        switch (actionId)
        {
            case "SearchSurroundings":
                gameState.TutorialState.SetFlag("ExploredSurroundings");
                break;

            case "GatherHerbs":
                // Flag will be set after encounter completion
                break;

            case "ForageForFood":
                // Flag will be set after encounter completion
                break;

            case "ConsumeFood":
                gameState.TutorialState.SetFlag("UsedFood");
                break;

            case "ConsumeMedicinalHerbs":
                gameState.TutorialState.SetFlag("UsedHerbs");
                break;

            case "FindPathOut":
                // Flag will be set after encounter completion
                break;
        }
    }

    private async Task ProcessBasicAction(UserActionOption action)
    {
        // Apply the action's outcomes
        ApplyActionOutcomes(action.ActionImplementation);

        // Apply time cost
        int hours = action.ActionImplementation.TimeCostHours;
        for (int i = 0; i < hours; i++)
        {
            // Apply time advancement one hour at a time
            gameState.TimeManager.AdvanceTime(1);

            // Signal the location to update any time-dependent properties
            await OnTimeChanged(worldState.WorldTime);

            // Apply effects for each hour that passes
            ApplyHourlyEffects();
        }
    }

    private async Task OnTimeChanged(TimeWindows newTime)
    {
        Location currentLocation = worldState.CurrentLocation;

        // Check for time-specific events
        currentLocation.OnTimeChanged(newTime);

        // Update available actions to reflect time-based availability
        await UpdateState();
    }


    public void CreateGlobalActions()
    {
        List<UserActionOption> userActions = new List<UserActionOption>();

        // Only add food consumption if player has food
        if (gameState.PlayerState.Food > 0)
        {
            SpotAction foodActionTemplate = ActionRepository.GetAction(ActionNames.ConsumeFood.ToString());
            if (foodActionTemplate != null)
            {
                ActionImplementation consumeFoodAction = ActionFactory.CreateActionFromTemplate(foodActionTemplate, null);

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
            SpotAction herbsActionTemplate = ActionRepository.GetAction(ActionNames.ConsumeMedicinalHerbs.ToString());
            if (herbsActionTemplate != null)
            {
                ActionImplementation consumeHerbsAction = ActionFactory.CreateActionFromTemplate(herbsActionTemplate, null);

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

    private void ApplyHourlyEffects()
    {
        // Skip energy drain for rest actions
        if (gameState.ActionStateTracker.CurrentAction?.ActionImplementation.BasicActionType == BasicActionTypes.Rest)
            return;

        // Different drain rates based on time of day
        int drainAmount = gameState.WorldState.WorldTime switch
        {
            TimeWindows.Night => 2,     // Higher drain at night if not resting
            TimeWindows.Morning => 1,   // Lower drain in morning (fresh)
            TimeWindows.Afternoon => 2, // Normal drain
            TimeWindows.Evening => 3,   // Higher drain (tired)
            _ => 2
        };

        // Location safety modifier
        if (gameState.WorldState.CurrentLocation?.LocationType == LocationTypes.Hub)
        {
            drainAmount = Math.Max(1, drainAmount - 1); // Reduce drain in safe locations
        }

        // Apply energy drain
        playerState.Energy = Math.Max(0, playerState.Energy - drainAmount);
    }

    private async Task CompleteTravel(string destination)
    {
        await OnLocationArrival(destination);
        gameState.PendingTravel.Clear();
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

    public async Task ReturnToLocationAfterEncounterEnd(EncounterManager encounter)
    {
        // Apply the encounter's outcomes
        ApplyActionOutcomes(encounter.ActionImplementation);

        // Complete the encounter
        gameState.ActionStateTracker.CompleteActiveEncounter();

        // Handle travel if this was a travel encounter
        if (gameState.PendingTravel.IsTravelPending)
        {
            await CompleteTravel(gameState.PendingTravel.TravelDestination);
        }
        await UpdateState();
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
            SpotAction actionTemplate = ActionRepository.GetAction(locationSpotAction);
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

            EncounterTemplate encounterTemplate = ActionRepository
                .GetEncounterTemplate(
                    actionTemplate.EncounterId);

            if (encounterTemplate == null)
            {
                var actionId =
                    await ActionGenerator.CreateEncounterForAction(
                        actionTemplate.ActionId,
                        actionTemplate,
                        worldStateInput);

                encounterTemplate = ActionRepository.GetEncounterTemplate(locationSpotAction);
            }

            ActionImplementation actionImplementation = ActionFactory.CreateActionFromTemplate(actionTemplate, encounterTemplate);

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

    public async Task<EncounterContext> PrepareEncounter()
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

        // Create encounter context
        EncounterContext encounterContextNew = new EncounterContext
        {
            Location = location,
            PresentCharacters = presentCharacters,
            AvailableOpportunities = opportunities,
            TimeOfDay = timeOfDay,
            CurrentEnvironmentalProperties = new List<IEnvironmentalProperty>(),
            Player = new PlayerSummary(),
            PreviousInteractions = previousInteractions
        };

        EncounterManager encounterContext = await GenerateEncounter(
            actionImplementation,
            location,
            gameState.PlayerState,
            locationSpot);

        return encounterContextNew;
    }

    public async Task<EncounterManager> GenerateEncounter(ActionImplementation actionImplementation, Location location, PlayerState playerState, string locationSpotName)
    {
        LocationSpot? locationSpot = LocationSystem.GetLocationSpotForLocation(location.Name, locationSpotName);

        // Create initial context with our new value system
        int playerLevel = playerState.Level;

        EncounterContext context = new EncounterContext()
        {
            ActionImplementation = actionImplementation,
            BasicActionType = actionImplementation.BasicActionType,
            Location = location,
            LocationSpot = locationSpot,
        };

        EncounterResult encounterResult = await EncounterSystem
            .GenerateEncounter(location, locationSpot.Name, context, worldState, playerState, actionImplementation);

        List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(encounterResult.Encounter);
        gameState.ActionStateTracker.SetEncounterChoiceOptions(choiceOptions);

        EncounterManager encounterManager = EncounterSystem.GetCurrentEncounter();
        return encounterManager;
    }

    public async Task<EncounterResult> ExecuteEncounterChoice(UserEncounterChoiceOption choiceOption)
    {
        EncounterResult currentResult = EncounterSystem.failureResult;

        EncounterManager encounter = choiceOption.encounter;
        Location location = LocationSystem.GetLocation(choiceOption.LocationName);

        var currentLocation = worldState.CurrentLocation?.Name;
        if (string.IsNullOrWhiteSpace(currentLocation)) return null;

        WorldStateInput worldStateInput = await CreateWorldStateInput(currentLocation);

        // Execute the choice
        EncounterResult encounterResult = await EncounterSystem.ExecuteChoice(
            encounter,
            currentResult.NarrativeResult,
            choiceOption.Choice,
            worldStateInput);

        EncounterResults currentEncounterResult = encounterResult.EncounterResults;
        if (currentEncounterResult == EncounterResults.Ongoing)
        {
            ProcessOngoingEncounter(encounterResult);
        }
        else
        {
            await OnEncounterCompleted(encounterResult);
        }

        return encounterResult;
    }

    private async Task OnEncounterCompleted(EncounterResult encounterResult)
    {
        await ProcessEncounterOutcome(encounterResult);

        if (gameState.GameMode == Modes.Tutorial)
        {
            UpdateTutorialStateForEncounterOutcome(encounterResult);
        }

        bool wasTravelEncounter = gameState.PendingTravel != null && gameState.PendingTravel.IsTravelPending;
        if (wasTravelEncounter)
        {
            await OnLocationArrival(gameState.PendingTravel.TravelDestination);
            gameState.PendingTravel.Clear();
        }
    }

    // Update tutorial state based on encounter outcomes
    private void UpdateTutorialStateForEncounterOutcome(EncounterResult result)
    {
        string actionId = result.Encounter.ActionImplementation.ActionId;
        EncounterResults outcome = result.EncounterResults;

        // Only track successful encounters
        if (outcome != EncounterResults.EncounterSuccess)
            return;

        switch (actionId)
        {
            case "GatherHerbs":
                if (playerState.MedicinalHerbs > 0)
                    gameState.TutorialState.SetFlag("GatheredHerbs");
                break;

            case "ForageForFood":
                if (playerState.Food > 0)
                    gameState.TutorialState.SetFlag("GatheredFood");
                break;

            case "FindPathOut":
                gameState.TutorialState.SetFlag("FoundPathOut");
                break;

            case "SearchSurroundings":
                // After successful exploration, unlock the path to the stream
                gameState.TutorialState.SetFlag("FoundStream");
                break;
        }
    }

    private void ProcessOngoingEncounter(EncounterResult encounterResult)
    {
        if (!IsGameOver(gameState.PlayerState))
        {
            gameState.ActionStateTracker.EncounterResult = encounterResult;
            List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(encounterResult.Encounter);
            gameState.ActionStateTracker.SetEncounterChoiceOptions(choiceOptions);
        }
        else
        {
            gameState.ActionStateTracker.CompleteActiveEncounter();
            return;
        }
    }

    private async Task OnLocationArrival(string targetLocation)
    {
        Location travelOrigin = gameState.PendingTravel?.TravelOrigin;
        string locationToPopulate = gameState.PendingTravel?.TravelDestination ?? "";

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
            Location originLocation = worldState.GetLocation(travelOrigin.Name);
            int newLocationDepth = originLocation.Depth + 1;

            travelLocation =
                await locationCreationSystem.PopulateLocation(
                locationToPopulate,
                travelOrigin.Name,
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

    public async Task ProcessEncounterOutcome(EncounterResult result)
    {
        gameState.ActionStateTracker.EncounterResult = result;

        NarrativeResult narrativeResult = result.NarrativeResult;
        string narrative = narrativeResult.SceneNarrative;
        string outcome = narrativeResult.Outcome.ToString();

        string encounterId = result.Encounter.ActionImplementation.ActionId;

        worldState.MarkEncounterCompleted(encounterId);

        if (result.EncounterResults == EncounterResults.EncounterSuccess)
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

            xpAward += result.Encounter.encounterInfo.EncounterDifficulty * 5;
            PlayerProgression.AddExperience(xpAward);

            // Add message about XP gain
            MessageSystem.AddSystemMessage($"Gained {xpAward} experience points");
        }

        if (_processStateChanges)
        {
            // If not a travel encounter, evolve the current location
            BasicActionTypes basicActionType = result.Encounter.ActionImplementation.BasicActionType;
            if (basicActionType != BasicActionTypes.Travel)
            {
                Location currentLocation = worldState.GetLocation(result.Encounter.encounterInfo.LocationName);
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
        }

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

        string location = encounterResult.Encounter.GetNarrativeContext().LocationName;
        string locationSpot = encounterResult.Encounter.GetNarrativeContext().locationSpotName;
        string actionName = encounterResult.Encounter.ActionImplementation.Name;
        string goal = encounterResult.Encounter.ActionImplementation.Goal;

        string title = $"{location} - {locationSpot}, {actionName} - {goal}" + Environment.NewLine;

        string memoryEntryToWrite = title + memoryEntry;

        await MemoryFileAccess.WriteToMemoryFile(memoryEntryToWrite);
    }

    public List<UserEncounterChoiceOption> GetUserEncounterChoiceOptions(EncounterManager encounter)
    {
        NarrativeResult narrativeResult = EncounterSystem.failureResult.NarrativeResult;
        List<ChoiceCard> choices = EncounterSystem.GetChoices();
        List<UserEncounterChoiceOption> choiceOptions = new List<UserEncounterChoiceOption>();

        int i = 0;
        foreach (ChoiceCard choice in choices)
        {
            i++;

            string locationName = encounter.EncounterState.EncounterInfo.LocationName;

            UserEncounterChoiceOption option = new UserEncounterChoiceOption(
                i,
                choice.GetDetails(),
                "Narrative",
                locationName,
                "locationSpotName",
                encounter,
                narrativeResult,
                choice);

            choiceOptions.Add(option);
        }

        return choiceOptions;
    }

    public ChoiceProjection GetChoicePreview(UserEncounterChoiceOption choiceOption)
    {
        EncounterManager encounter = choiceOption.encounter;
        Location location = LocationSystem.GetLocation(choiceOption.LocationName);

        // Execute the choice
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
        List<Location> playerKnownLocations = new List<Location>();
        foreach (Location location in LocationSystem.GetAllLocations())
        {
            if (gameState.PlayerState.KnownLocations.Contains(location.Name))
                playerKnownLocations.Add(location);
        }
        return playerKnownLocations;
    }

    public List<string> GetConnectedLocations()
    {
        List<string> loc =
            LocationSystem.GetAllLocations()
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

    public async Task UpdateState()
    {
        gameState.ActionStateTracker.ClearCurrentUserAction();

        // Create global actions first
        CreateGlobalActions();

        // Then update other available actions
        await CreateLocationActions(worldState.CurrentLocation, worldState.CurrentLocationSpot);
    }

    private async Task CreateLocationActions(Location currentLocation, LocationSpot locationSpot)
    {
        List<LocationSpot> locationSpots = currentLocation.LocationSpots;
        Console.WriteLine($"Location {currentLocation.Name} has {locationSpots?.Count ?? 0} spots");

        if (gameState.PendingTravel!.IsTravelPending)
        {
            gameState.PendingTravel.Clear();
        }

        // Set as current location
        worldState.SetCurrentLocation(currentLocation);

        var locationSpotActionOptions = await CreateActionsForLocationSpot(currentLocation, locationSpot);
        gameState.ActionStateTracker.SetLocationSpotActions(locationSpotActionOptions);
    }

    public EncounterViewModel? GetEncounterViewModel()
    {
        EncounterResult encounterResult = EncounterSystem.failureResult;

        EncounterViewModel model = new EncounterViewModel();
        EncounterManager encounterManager = EncounterSystem.GetCurrentEncounter();

        List<UserEncounterChoiceOption> userEncounterChoiceOptions = EncounterSystem.GetUserEncounterChoiceOptions();

        EncounterState state = encounterManager.EncounterState;

        model.CurrentEncounter = encounterManager;
        model.CurrentChoices = userEncounterChoiceOptions;
        model.ChoiceSetName = "Current Situation";
        model.State = state;
        model.EncounterResult = encounterResult;

        return model;
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
        SpotAction actionTemplate = ActionRepository.GetAction(actionName);
        if (actionTemplate == null)
        {
            Console.WriteLine($"Action not found: {actionName}");
            return;
        }

        // Check requirements
        ActionImplementation action = ActionFactory.CreateActionFromTemplate(
            actionTemplate,
            actionTemplate.ActionType == ActionTypes.Encounter ?
                ActionRepository.GetEncounterTemplate(actionTemplate.EncounterId) : null);

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

    public static ActionExecutionType GetExecutionType(ActionImplementation action)
    {
        // Inter-location travel (true travel between different locations)
        if (action.BasicActionType == BasicActionTypes.Travel &&
            !string.IsNullOrEmpty(action.DestinationLocation) &&
            action.DestinationLocation != action.CurrentLocation &&
            !IsLocationSpot(action.DestinationLocation)) // Check if destination is a location, not a spot
        {
            return ActionExecutionType.Travel;
        }

        // Travel to spot within same location (behaves like a basic action)
        if (action.BasicActionType == BasicActionTypes.Travel &&
            !string.IsNullOrEmpty(action.DestinationLocation) &&
            IsLocationSpot(action.DestinationLocation)) // Check if destination is a spot
        {
            return ActionExecutionType.Basic;
        }

        // Regular encounter
        if (action.ActionType == ActionTypes.Encounter)
        {
            return ActionExecutionType.Encounter;
        }

        // Basic action
        return ActionExecutionType.Basic;
    }

    private static bool IsLocationSpot(string name)
    {
        // Implement logic to determine if this is a location spot name
        // Could be based on naming convention or checking the actual spots list
        return name.Contains(" - ") || name.Contains("Spot"); // Example logic
    }
}
  
public enum ActionExecutionType
{
    Basic,          // Immediate effect, no encounter, repeatable
    Encounter,      // Multi-turn strategic challenge, non-repeatable once completed
    Travel          // Movement between locations or spots
}
