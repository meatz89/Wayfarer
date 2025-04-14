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

        UpdateState();

        Location? currentLoc = worldState.CurrentLocation;
        Console.WriteLine($"Game started at: {currentLoc?.Name}, Current spot: {worldState.CurrentLocationSpot?.Name}");
    }

    public async Task InitiateTravelToLocation(string locationName)
    {
        ActionImplementation travelAction = TravelManager.TravelToLocation(locationName, TravelMethods.Walking);

        // Create option
        UserActionOption travelOption = new UserActionOption(
            0, "Travel", "Travel to " + locationName, false, travelAction,
            worldState.CurrentLocation.Name, worldState.CurrentLocationSpot.Name,
            null, worldState.CurrentLocation.Difficulty);

        await ExecuteBasicAction(travelOption);
    }

    private async Task CreateActionsForLocationSpot(
        List<UserActionOption> options,
        Location location,
        LocationSpot locationSpot)
    {
        WorldStateInput worldStateInput = await CreateWorldStateInput();

        List<string> locationSpotActions = locationSpot.ActionIds.ToList();
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
                    default,
                    actionImplementation.ActionId.ToString(),
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

    public async Task ExecuteBasicAction(UserActionOption action)
    {
        actionImplementation = action.ActionImplementation;
        locationSpot = action.LocationSpot;

        location = LocationSystem.GetLocation(action.Location);
        gameState.Actions.SetCurrentUserAction(action);

        bool startEncounter = actionImplementation.ActionType == ActionTypes.Encounter;
        if (!startEncounter)
        {
            ApplyActionOutcomes(actionImplementation);

            if (gameState.PendingTravel.IsTravelPending)
            {
                await OnLocationArrival(gameState.PendingTravel.TravelDestination);
                gameState.PendingTravel.Clear();
            }
        }
        else
        {
            gameState.Actions.SetActiveEncounter();
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

        EncounterManager encounterManager = gameState.Actions.GetCurrentEncounter();
        return encounterManager;
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

        WorldStateInput worldStateInput = await CreateWorldStateInput();

        // Execute the choice
        EncounterResult encounterResult = await EncounterSystem.ExecuteChoice(
            encounter,
            currentResult.NarrativeResult,
            choiceOption.Choice,
            worldStateInput);

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
        await ProcessEncounterOutcome(result);

        bool wasTravelEncounter = gameState.PendingTravel != null && gameState.PendingTravel.IsTravelPending;
        if (wasTravelEncounter)
        {
            await OnLocationArrival(gameState.PendingTravel.TravelDestination);
            gameState.PendingTravel.Clear();
        }

        await UpdateLocation(gameState.WorldState.CurrentLocation);
    }

    private async Task OnLocationArrival(string targetLocation)
    {
        Location travelOrigin = gameState.PendingTravel?.TravelOrigin;
        string locationToPopulate = gameState.PendingTravel?.TravelDestination ?? "";

        Location travelLocation = gameState.WorldState.GetLocation(targetLocation);

        WorldStateInput worldStateInput = await CreateWorldStateInput();

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
        
        await UpdateLocation(travelLocation);
    }

    private async Task UpdateLocation(Location travelLocation)
    {
        List<UserActionOption> options = new List<UserActionOption>();

        // Ensure we have a current location spot
        if (gameState.WorldState.CurrentLocationSpot == null && travelLocation.LocationSpots?.Any() == true)
        {
            Console.WriteLine($"Setting location spot to {travelLocation.LocationSpots.First().Name} in OnPlayerEnterLocation");
            gameState.WorldState.SetCurrentLocationSpot(travelLocation.LocationSpots.First());
        }

        List<LocationSpot> locationSpots = travelLocation.LocationSpots;
        Console.WriteLine($"Location {travelLocation.Name} has {locationSpots?.Count ?? 0} spots");

        foreach (LocationSpot locationSpot in locationSpots)
        {
            await CreateActionsForLocationSpot(options, travelLocation, locationSpot);
        }

        if (gameState.PendingTravel!.IsTravelPending)
        {
            gameState.PendingTravel.Clear();
        }

        // Set as current location
        worldState.SetCurrentLocation(travelLocation);
        gameState.Actions.SetLocationSpotActions(options);
    }

    public async Task ProcessEncounterOutcome(EncounterResult result)
    {
        gameState.Actions.EncounterResult = result;

        NarrativeResult narrativeResult = result.NarrativeResult;
        string narrative = narrativeResult.SceneNarrative;
        string outcome = narrativeResult.Outcome.ToString();

        // Generate a unique encounter ID based on the context
        string encounterId = result.Encounter.ActionImplementation.ActionId;

        worldState.MarkEncounterCompleted(encounterId);

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

                WorldStateInput worldStateInput = await CreateWorldStateInput();

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
        WorldStateInput worldStateInput = await CreateWorldStateInput();

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
        NarrativeResult narrativeResult = EncounterSystem.CurrentResult.NarrativeResult;
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

    public void UpdateAvailableActions()
    {
        gameState.Actions.SetCharacterActions(new List<UserActionOption>());
        gameState.Actions.SetQuestActions(new List<UserActionOption>());

        CreateGlobalActions();
    }

    public void MoveToLocationSpot(string locationSpotName)
    {
        string locationName = gameState.WorldState.CurrentLocation.Name;

        LocationSpot locationSpot = LocationSystem.GetLocationSpotForLocation(locationName, locationSpotName);
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

    public List<Location> GetPlayerKnownLocations()
    {
        List<Location> playerKnownLocations = new List<Location>();
        foreach (Location location in LocationSystem.GetAllLocations())
        {
            if(gameState.PlayerState.KnownLocations.Contains(location.Name))
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

    public void UpdateState()
    {
        gameState.Actions.ClearCurrentUserAction();
        UpdateAvailableActions();
    }

    public EncounterViewModel? GetEncounterViewModel()
    {
        EncounterResult encounterResult = EncounterSystem.CurrentResult;

        EncounterViewModel model = new EncounterViewModel();
        EncounterManager encounterManager = gameState.Actions.GetCurrentEncounter();

        List<UserEncounterChoiceOption> userEncounterChoiceOptions = EncounterSystem.GetUserEncounterChoiceOptions();

        EncounterState state = encounterManager.EncounterState;

        model.CurrentEncounter = encounterManager;
        model.CurrentChoices = userEncounterChoiceOptions;
        model.ChoiceSetName = "Current Situation";
        model.State = state;
        model.EncounterResult = encounterResult;

        return model;
    }

    private async Task<WorldStateInput> CreateWorldStateInput()
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

            CurrentLocation = worldState.CurrentLocation.Name,
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

    public void ExecuteActionByName(string actionName)
    {
        // Get the action template from repository
        SpotAction actionTemplate = ActionRepository.GetAction(actionName);
        if (actionTemplate == null) return;

        // Create action implementation
        ActionImplementation action = ActionFactory.CreateActionFromTemplate(actionTemplate, null);

        // Apply the outcomes
        ApplyActionOutcomes(action);

        // Update state
        UpdateState();
    }
}
