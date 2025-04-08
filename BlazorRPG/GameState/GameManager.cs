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
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
        _useMemory = configuration.GetValue<bool>("useMemory");
    }

    public async Task StartGame()
    {
        Location startingLocation = await LocationSystem.Initialize(GameRules.StandardRuleset.StartingLocation);
        LocationSystem.SetCurrentLocation(startingLocation);

        TravelManager.TravelToLocation(gameState.PlayerState.StartingLocation, TravelMethods.Walking);

        // Verify current location spot was set
        if (worldState.CurrentLocationSpot == null && worldState.CurrentLocation?.LocationSpots?.Any() == true)
        {
            Console.WriteLine("Current location spot is null despite spots existing - manually setting");
            worldState.SetCurrentLocationSpot(worldState.CurrentLocation.LocationSpots.First());
        }

        UpdateState();

        // Debug info - print current state
        Location? currentLoc = worldState.CurrentLocation;
        Console.WriteLine($"Game started at: {currentLoc?.Name}, Current spot: {worldState.CurrentLocationSpot?.Name}");
    }

    public void InitiateTravelToLocation(string locationName)
    {
        ActionImplementation travelAction = TravelManager.TravelToLocation(locationName, TravelMethods.Walking);

        // Create option
        UserActionOption travelOption = new UserActionOption(
            0, "Travel to " + locationName, false, travelAction,
            worldState.CurrentLocation.Name, worldState.CurrentLocationSpot.Name,
            null, worldState.CurrentLocation.Difficulty);

        // Execute to start travel encounter
        ExecuteBasicAction(travelOption);
    }

    private async Task CreateActionsForLocationSpot(
        List<UserActionOption> options,
        Location location,
        LocationSpot locationSpot)
    {
        List<string> locationSpotActions = locationSpot.ActionTemplates.ToList();
        foreach (string actionName in locationSpotActions)
        {
            SpotAction actionTemplate = ActionRepository.GetAction(actionName);
            if(actionTemplate == null)
            {
                string actionTemplateName = 
                    await ActionGenerator.GenerateActionAndEncounter(
                    actionName,
                    locationSpot.Name,
                    location.Name);

                actionTemplate = ActionRepository.GetAction(actionTemplateName);
            }

            EncounterTemplate encounterTemplate = ActionRepository.GetEncounterTemplate(actionTemplate.EncounterTemplateName);
            if (encounterTemplate == null)
            {
                string encounterTemplateName = await ActionGenerator.CreateEncounterForAction(actionTemplate);
                encounterTemplate = ActionRepository.GetEncounterTemplate(encounterTemplateName);
            }

            ActionImplementation actionImplementation = ActionFactory.CreateActionFromTemplate(actionTemplate, encounterTemplate);

            UserActionOption userActionOption =
                new UserActionOption(
                    default,
                    actionImplementation.Name.ToString(),
                    false,
                    actionImplementation,
                    locationSpot.LocationName,
                    locationSpot.Name,
                    default,
                    location.Difficulty);

            options.Add(userActionOption);
        }
    }

    public void ExecuteBasicAction(UserActionOption action)
    {
        actionImplementation = action.ActionImplementation;
        locationSpot = action.LocationSpot;

        location = LocationSystem.GetLocation(action.Location);
        gameState.Actions.SetCurrentUserAction(action);

        bool startEncounter = actionImplementation.ActionType == ActionTypes.Encounter;
        if (!startEncounter)
        {
            ApplyActionOutcomes(actionImplementation);
        }
        else
        {
            gameState.Actions.IsActiveEncounter = true;
        }
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
        gameState.Actions.SetEncounterChoiceOptions(choiceOptions);

        EncounterManager encounterManager = GetEncounter();
        return encounterManager;
    }

    public EncounterManager GetEncounter()
    {
        return gameState.Actions.GetCurrentEncounter();
    }

    public async Task EndEncounter(EncounterManager encounter)
    {
        ActionImplementation actionImplementation = encounter.ActionImplementation;
        ApplyActionOutcomes(actionImplementation);
        gameState.Actions.CompleteActiveEncounter();

    }

    public async Task<EncounterResult> ExecuteEncounterChoice(UserEncounterChoiceOption choiceOption)
    {
        EncounterResult currentResult = EncounterSystem.CurrentResult;

        EncounterManager encounter = choiceOption.encounter;
        Location location = LocationSystem.GetLocation(choiceOption.LocationName);

        // Execute the choice
        EncounterResult encounterResult = await EncounterSystem.ExecuteChoice(
            encounter,
            currentResult.NarrativeResult,
            choiceOption.Choice);

        EncounterResults currentEncounterResult = encounterResult.EncounterResults;
        if (currentEncounterResult == EncounterResults.Ongoing)
        {
            // Encounter is Ongoing - unchanged
            if (IsGameOver(gameState.PlayerState))
            {
                gameState.Actions.CompleteActiveEncounter();
                return encounterResult;
            }

            gameState.Actions.EncounterResult = encounterResult;

            List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(encounterResult.Encounter);
            gameState.Actions.SetEncounterChoiceOptions(choiceOptions);
        }
        else
        {
            await OnEncounterCompleted(encounterResult);
        }

        return encounterResult;
    }

    public async Task OnEncounterCompleted(EncounterResult result)
    {
        // Encounter is Over
        gameState.Actions.EncounterResult = result;

        // If this was a travel encounter that completed successfully
        bool wasTravelEncounter = gameState.PendingTravel != null && gameState.PendingTravel.IsTravelPending;
        if (wasTravelEncounter)
        {
            await OnLocationArrival(gameState.PendingTravel.TravelDestination);
            gameState.PendingTravel.Clear();
        }
        else
        {
            // Process world changes from encounter
            await ProcessEncounterOutcome(result);
        }
    }

    public async Task ProcessEncounterOutcome(EncounterResult result)
    {
        NarrativeResult narrativeResult = result.NarrativeResult;
        string narrative = narrativeResult.SceneNarrative;
        string outcome = narrativeResult.Outcome.ToString();

        // Generate a unique encounter ID based on the context
        string encounterId = GenerateEncounterId(result);

        // MARK ENCOUNTER AS COMPLETED - This was missing
        worldState.MarkEncounterCompleted(encounterId);

        if (_processStateChanges)
        {
            // If not a travel encounter, evolve the current location
            if (result.Encounter.ActionImplementation.BasicActionType != BasicActionTypes.Travel)
            {
                Location currentLocation = worldState.GetLocation(result.Encounter.encounterInfo.LocationName);
                if (_useMemory)
                {
                    await CreateMemoryRecord(result);
                }

                // Prepare the input
                PostEncounterEvolutionInput input = evolutionSystem.PreparePostEncounterEvolutionInput(narrative, outcome);

                // Process world evolution
                PostEncounterEvolutionResult evolutionResponse = await evolutionSystem.ProcessEncounterOutcome(result.NarrativeContext, input, result);

                // Store the evolution response in the result
                result.PostEncounterEvolution = evolutionResponse;

                // Update world state
                await evolutionSystem.IntegrateEncounterOutcome(evolutionResponse, worldState, LocationSystem, playerState);
            }
        }
    }

    private async Task OnLocationArrival(string locationName)
    {
        // Check if this was a travel encounter
        bool wasTravelEncounter = gameState.PendingTravel?.IsTravelPending ?? false;

        Location travelOrigin = gameState.PendingTravel?.TravelOrigin;
        string locationToPopulate = gameState.PendingTravel?.TravelDestination ?? "";

        Location location = gameState.WorldState.GetLocation(locationName);

        // Check if this is the first visit
        bool knownLocation = location.PlayerKnowledge;
        if (!knownLocation)
        {
            Location originLocation = worldState.GetLocation(travelOrigin.Name);
            int newLocationDepth = originLocation.Depth + 1;

            location =
                await locationCreationSystem.PopulateLocation(
                locationToPopulate,
                wasTravelEncounter,
                travelOrigin.Name,
                newLocationDepth);
        }

        // Start with first spot
        if (location.LocationSpots.Count > 0)
        {
            worldState.CurrentLocationSpot = location.LocationSpots[0];
        }

        List<UserActionOption> options = new List<UserActionOption>();
        if (location == null) return;

        // Ensure we have a current location spot
        if (gameState.WorldState.CurrentLocationSpot == null && location.LocationSpots?.Any() == true)
        {
            Console.WriteLine($"Setting location spot to {location.LocationSpots.First().Name} in OnPlayerEnterLocation");
            gameState.WorldState.SetCurrentLocationSpot(location.LocationSpots.First());
        }

        List<LocationSpot> locationSpots = location.LocationSpots;
        Console.WriteLine($"Location {location.Name} has {locationSpots?.Count ?? 0} spots");

        foreach (LocationSpot locationSpot in locationSpots)
        {
            await CreateActionsForLocationSpot(options, this.location, locationSpot);
        }

        gameState.Actions.SetLocationSpotActions(options);

        if (gameState.PendingTravel.IsTravelPending)
        {
            gameState.PendingTravel.Clear();
        }

        // Set as current location
        LocationSystem.SetCurrentLocation(location);
    }

    private async Task CreateMemoryRecord(EncounterResult encounterResult)
    {
        string oldMemory = await MemoryFileAccess.ReadFromMemoryFile();

        // Create memory entry
        MemoryConsolidationInput memoryInput = new MemoryConsolidationInput { OldMemory = oldMemory };
        string memoryEntry = await evolutionSystem.ConsolidateMemory(encounterResult.NarrativeContext, memoryInput);

        string location = encounterResult.Encounter.GetNarrativeContext().LocationName;
        string locationSpot = encounterResult.Encounter.GetNarrativeContext().locationSpotName;
        string action = encounterResult.Encounter.ActionImplementation.Name;
        string goal = encounterResult.Encounter.ActionImplementation.Goal;

        string title = $"{location} - {locationSpot}, {action} - {goal}" + Environment.NewLine;

        string memoryEntryToWrite = title + memoryEntry;

        await MemoryFileAccess.WriteToMemoryFile(memoryEntryToWrite);
    }

    public List<UserEncounterChoiceOption> GetUserEncounterChoiceOptions(EncounterManager encounter)
    {
        NarrativeResult narrativeResult = EncounterSystem.CurrentResult.NarrativeResult;
        List<IChoice> choices = EncounterSystem.GetChoices();
        List<UserEncounterChoiceOption> choiceOptions = new List<UserEncounterChoiceOption>();

        int i = 0;
        foreach (IChoice choice in choices)
        {
            i++;

            string locationName = encounter.encounterState.Location.LocationName;

            UserEncounterChoiceOption option = new UserEncounterChoiceOption(
                i,
                choice.ToString(),
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

    public void UpdateAvailableActions()
    {
        gameState.Actions.SetCharacterActions(new List<UserActionOption>());
        gameState.Actions.SetQuestActions(new List<UserActionOption>());

        CreateGlobalActions();
    }

    public void MoveToLocationSpot(string location, string locationSpotName)
    {
        LocationSpot locationSpot = LocationSystem.GetLocationSpotForLocation(location, locationSpotName);
        gameState.WorldState.SetCurrentLocationSpot(locationSpot);
        UpdateState();
    }

    private bool IsGameOver(PlayerState player)
    {
        bool canPayPhysical = player.Energy > 0 || player.Health > 1;
        bool canPayFocus = player.Concentration > 0 || player.Concentration > 1;
        bool canPaySocial = player.Confidence > 0 || player.Confidence > 1;

        bool isGameOver = !(canPayPhysical || canPayFocus || canPaySocial);
        return isGameOver;
    }

    public void CreateGlobalActions()
    {
        List<UserActionOption> userActions = new List<UserActionOption>();
        int actionIndex = 1;

        gameState.Actions.SetGlobalActions(userActions);
    }

    public void ApplyActionOutcomes(ActionImplementation action)
    {
        foreach (Outcome energyCost in action.EnergyCosts)
        {
            energyCost.Apply(gameState.PlayerState);
            MessageSystem.AddOutcome(energyCost);
        }

        foreach (Outcome cost in action.Costs)
        {
            cost.Apply(gameState.PlayerState);
            MessageSystem.AddOutcome(cost);
        }

        foreach (Outcome reward in action.Rewards)
        {
            reward.Apply(gameState.PlayerState);
            MessageSystem.AddOutcome(reward);
        }
    }

    public List<Location> GetPlayerKnownLocations()
    {
        List<Location> playerKnownLocations = new List<Location>();

        foreach (Location location in LocationSystem.GetAllLocations())
        {
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
        // Check if the action has been completed and is non-repeatable
        if (!action.ActionImplementation.IsRepeatable)
        {
            string encounterId = GenerateActionEncounterId(action);
            if (gameState.WorldState.IsEncounterCompleted(encounterId))
            {
                return false; // Already completed this non-repeatable action
            }
        }

        // Continue with existing requirement checks
        return action.ActionImplementation.CanExecute(gameState);
    }

    private string GenerateActionEncounterId(UserActionOption action)
    {
        return $"{action.Location}_{action.LocationSpot}_{action.ActionImplementation.Name}";
    }

    public void UpdateLocationSpotOptions()
    {
        Location location = gameState.WorldState.CurrentLocation;
        List<LocationSpot> locationSpots = LocationSystem.GetLocationSpots(location);

        List<UserLocationSpotOption> userLocationSpotOption = new List<UserLocationSpotOption>();

        for (int i = 0; i < locationSpots.Count; i++)
        {
            LocationSpot locationSpot = locationSpots[i];
            UserLocationSpotOption locationSpotOption = new UserLocationSpotOption(i + 1, location.Name, locationSpot.Name);

            userLocationSpotOption.Add(locationSpotOption);
        }

        gameState.WorldState.SetCurrentLocationSpotOptions(userLocationSpotOption);
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

    public void UpdateState()
    {
        gameState.Actions.ClearCurrentUserAction();
        UpdateLocationSpotOptions();
        UpdateAvailableActions();
    }

    public EncounterViewModel? GetEncounterViewModel()
    {
        EncounterResult encounterResult = EncounterSystem.CurrentResult;

        EncounterViewModel model = new EncounterViewModel();
        EncounterManager encounterManager = GetEncounter();

        List<UserEncounterChoiceOption> userEncounterChoiceOptions = EncounterSystem.GetUserEncounterChoiceOptions();

        EncounterState state = encounterManager.encounterState;

        model.CurrentEncounter = encounterManager;
        model.CurrentChoices = userEncounterChoiceOptions;
        model.ChoiceSetName = "Current Situation";
        model.State = state;
        model.EncounterResult = encounterResult;

        return model;
    }


    private string GenerateEncounterId(EncounterResult result)
    {
        // Create a unique ID based on location, spot, and action
        NarrativeContext context = result.NarrativeContext;
        return $"{context.LocationName}_{context.locationSpotName}_{context.ActionImplementation.Name}";
    }

}
