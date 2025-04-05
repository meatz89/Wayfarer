using System.Text;

public class GameManager
{
    public PlayerState playerState => gameState.PlayerState;
    public WorldState worldState => gameState.WorldState;

    public GameState gameState;

    public LocationSystem LocationSystem { get; }
    public ItemSystem ItemSystem { get; }
    public NarrativeService NarrativeService { get; }
    public MessageSystem MessageSystem { get; }
    public PostEncounterEvolutionSystem evolutionService { get; }
    public ActionFactory ActionFactory { get; }
    public ActionRepository ActionRepository { get; }
    public TravelManager TravelManager { get; }
    public ActionGenerator ActionGenerator { get; }
    public EncounterSystem EncounterSystem { get; }
    public EncounterResult currentResult { get; set; }

    public ActionImplementation actionImplementation;
    public Location location;
    public string locationSpot;

    private bool _useMemory = false;
    private bool _processStateChanges = false;

    public GameManager(
        GameState gameState,
        EncounterSystem EncounterSystem,
        LocationSystem locationSystem,
        ItemSystem itemSystem,
        NarrativeService narrativeService,
        MessageSystem messageSystem,
        PostEncounterEvolutionSystem postEncounterEvolutionService,
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
        evolutionService = postEncounterEvolutionService;
        ActionFactory = actionFactory;
        ActionRepository = actionRepository;
        TravelManager = travelManager;
        ActionGenerator = actionGenerator;
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
        _useMemory = configuration.GetValue<bool>("useMemory");
    }

    public async Task StartGame()
    {
        await TravelToLocation(gameState.PlayerState.StartingLocation);

        // Verify current location spot was set
        if (worldState.CurrentLocationSpot == null && worldState.CurrentLocation?.Spots?.Any() == true)
        {
            Console.WriteLine("Current location spot is null despite spots existing - manually setting");
            worldState.SetCurrentLocationSpot(worldState.CurrentLocation.Spots.First());
        }

        UpdateState();

        // Debug info - print current state
        var currentLoc = worldState.CurrentLocation;
        Console.WriteLine($"Game started at: {currentLoc?.Name}, Current spot: {worldState.CurrentLocationSpot?.Name}");
    }

    public void InitiateTravelToLocation(string locationName)
    {
        // Store the travel destination
        gameState.PendingTravel.Destination = locationName;
        gameState.PendingTravel.TravelMethod = TravelMethods.Walking;

        // Get the travel action template
        SpotAction travelTemplate = ActionRepository.GetAction("Travel");
        if (travelTemplate == null)
        {
            travelTemplate = new SpotAction
            {
                Name = "Travel",
                EncounterTemplateName = "Travel",
                ActionType = ActionTypes.Encounter,
                BasicActionType = BasicActionTypes.Travel,
                Goal = "Travel safely to your destination",
                IsRepeatable = true,
                Costs = new()
                {
                    new EnergyOutcome(-1)
                    {
                        Amount = -1
                    }
                },
            };
        }

        // Create travel action
        EncounterTemplate travelEncounter = ActionRepository.GetEncounterTemplate("Travel");
        ActionImplementation travelAction = ActionFactory.CreateActionFromTemplate(travelTemplate, travelEncounter);

        // Create option
        UserActionOption travelOption = new UserActionOption(
            0, "Travel to " + locationName, false, travelAction,
            worldState.CurrentLocation.Name, worldState.CurrentLocationSpot.Name,
            null, worldState.CurrentLocation.Difficulty);

        // Execute to start travel encounter
        ExecuteBasicAction(travelOption);
    }

    internal async Task TravelToLocation(string name)
    {
        TravelManager.TravelToLocation(name, TravelMethods.Walking);

        UpdateState();

        await OnPlayerEnterLocation(gameState.WorldState.CurrentLocation);
    }

    private async Task OnPlayerEnterLocation(Location location)
    {
        List<UserActionOption> options = new List<UserActionOption>();
        if (location == null) return;

        // Ensure we have a current location spot
        if (gameState.WorldState.CurrentLocationSpot == null && location.Spots?.Any() == true)
        {
            Console.WriteLine($"Setting location spot to {location.Spots.First().Name} in OnPlayerEnterLocation");
            gameState.WorldState.SetCurrentLocationSpot(location.Spots.First());
        }

        List<LocationSpot> locationSpots = location.Spots;
        Console.WriteLine($"Location {location.Name} has {locationSpots?.Count ?? 0} spots");

        foreach (LocationSpot locationSpot in locationSpots)
        {
            await CreateActionsForLocationSpot(options, location, locationSpot);
        }

        gameState.Actions.SetLocationSpotActions(options);
    }

    private async Task CreateActionsForLocationSpot(
        List<UserActionOption> options,
        Location location,
        LocationSpot locationSpot)
    {
        List<string> locationSpotActions = locationSpot.ActionTemplates.ToList();
        foreach (string locationSpotAction in locationSpotActions)
        {
            SpotAction actionTemplate = ActionRepository.GetAction(locationSpotAction);

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

    public async Task<EncounterContext> GenerateEncounter()
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

        UserActionOption action = gameState.Actions.LocationSpotActions.Where(x => x.LocationSpot == locationSpot.Name).First();
        ActionImplementation actionImpl = action.ActionImplementation;

        currentResult = await EncounterSystem
            .GenerateEncounter(location, locationSpot.Name, context, worldState, playerState, actionImpl);

        List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(currentResult.Encounter);
        gameState.Actions.SetEncounterChoiceOptions(choiceOptions);

        EncounterManager encounterManager = GetEncounter();
        return encounterManager;
    }

    public EncounterManager GetEncounter()
    {
        return gameState.Actions.GetCurrentEncounter();
    }

    public void FinishEncounter(EncounterManager encounter)
    {
        ActionImplementation actionImplementation = encounter.ActionImplementation;
        ApplyActionOutcomes(actionImplementation);
        gameState.Actions.CompleteActiveEncounter();

        // Check if there was pending travel and clear it
        if (gameState.PendingTravel.IsTravelPending)
        {
            gameState.PendingTravel.Clear();
        }
    }

    private PostEncounterEvolutionInput PreparePostEncounterEvolutionInput(string encounterNarrative, string encounterOutcome)
    {
        // Get current depth and hub depth
        int currentDepth = worldState.GetLocationDepth(worldState.CurrentLocation?.Name ?? "");

        // Check if this was a travel encounter
        bool wasTravelEncounter = gameState.PendingTravel?.IsTravelPending ?? false;
        string travelDestination = gameState.PendingTravel?.Destination ?? "";

        // Get all locations
        List<Location> allLocations = worldState.GetLocations();

        return new PostEncounterEvolutionInput
        {
            EncounterNarrative = encounterNarrative,
            CharacterBackground = playerState.Archetype.ToString(),
            CurrentLocation = worldState.CurrentLocation?.Name ?? "Unknown",
            KnownLocations = FormatKnownLocations(allLocations),
            KnownCharacters = FormatKnownCharacters(worldState.GetCharacters()),
            ActiveOpportunities = FormatActiveOpportunities(worldState.GetOpportunities()),
            EncounterOutcome = encounterOutcome,

            // Format world context information
            CurrentLocationSpots = FormatLocationSpots(worldState.CurrentLocation),
            AllKnownLocationSpots = FormatAllLocationSpots(allLocations),
            AllExistingActions = FormatExistingActions(allLocations),

            // Add travel-specific information
            WasTravelEncounter = wasTravelEncounter,
            TravelDestination = travelDestination,

            CurrentDepth = currentDepth,
            LastHubDepth = worldState.LastHubDepth,
            Health = playerState.Health,
            MaxHealth = playerState.MaxHealth,
            Energy = playerState.Energy,
            MaxEnergy = playerState.MaxEnergy
        };
    }


    private string FormatKnownLocations(List<Location> locations)
    {
        StringBuilder sb = new StringBuilder();

        if (locations == null || !locations.Any())
            return "None";

        foreach (var loc in locations)
        {
            sb.AppendLine($"- {loc.Name}: {loc.Description} (Depth: {worldState.GetLocationDepth(loc.Name)})");
        }

        return sb.ToString();
    }

    private string FormatKnownCharacters(List<Character> characters)
    {
        StringBuilder sb = new StringBuilder();

        if (characters == null || !characters.Any())
            return "None";

        foreach (var character in characters)
        {
            sb.AppendLine($"- {character.Name}: {character.Role} at {character.Location}");
            if (!string.IsNullOrEmpty(character.Description))
                sb.AppendLine($"  Description: {character.Description}");
        }

        return sb.ToString();
    }

    private string FormatLocationSpots(Location location)
    {
        StringBuilder sb = new StringBuilder();

        if (location == null || location.Spots == null || !location.Spots.Any())
            return "None";

        foreach (var spot in location.Spots)
        {
            sb.AppendLine($"- {spot.Name}: {spot.Description}");
        }

        return sb.ToString();
    }

    private string FormatAllLocationSpots(List<Location> locations)
    {
        StringBuilder sb = new StringBuilder();

        if (locations == null || !locations.Any())
            return "None";

        foreach (var location in locations)
        {
            if (location.Spots == null || !location.Spots.Any())
                continue;

            sb.AppendLine($"## {location.Name} Spots:");
            foreach (var spot in location.Spots)
            {
                sb.AppendLine($"- {spot.Name}: {spot.Description}");
            }
            sb.AppendLine();
        }

        return sb.Length > 0 ? sb.ToString() : "None";
    }

    private string FormatExistingActions(List<Location> locations)
    {
        StringBuilder sb = new StringBuilder();

        if (locations == null || !locations.Any())
            return "None";

        foreach (var location in locations)
        {
            if (location.Spots == null || !location.Spots.Any())
                continue;

            foreach (var spot in location.Spots)
            {
                if (spot.ActionTemplates == null || !spot.ActionTemplates.Any())
                    continue;

                sb.AppendLine($"## Actions at {location.Name} / {spot.Name}:");

                foreach (var actionTemplate in spot.ActionTemplates)
                {
                    var action = ActionRepository.GetAction(actionTemplate);
                    if (action != null)
                    {
                        sb.AppendLine($"- {action.Name}: {action.Goal}");
                    }
                }
                sb.AppendLine();
            }
        }

        return sb.Length > 0 ? sb.ToString() : "None";
    }

    private string FormatActiveOpportunities(List<Opportunity> opportunities)
    {
        StringBuilder sb = new StringBuilder();

        if (opportunities == null || !opportunities.Any())
            return "None";

        foreach (var opportunity in opportunities.Where(o => o.Status == "Available"))
        {
            sb.AppendLine($"- {opportunity.Name}: {opportunity.Description} (at {opportunity.Location})");
        }

        return sb.ToString();
    }

    public async Task<EncounterResult> ExecuteEncounterChoice(UserEncounterChoiceOption choiceOption)
    {
        EncounterManager encounter = choiceOption.encounter;
        Location location = LocationSystem.GetLocation(choiceOption.LocationName);

        // Execute the choice
        currentResult = await EncounterSystem.ExecuteChoice(
            encounter,
            currentResult.NarrativeResult,
            choiceOption.Choice);

        EncounterResults currentEncounterResult = currentResult.EncounterResults;
        if (currentEncounterResult == EncounterResults.Ongoing)
        {
            // Encounter is Ongoing - unchanged
            if (IsGameOver(gameState.PlayerState))
            {
                gameState.Actions.CompleteActiveEncounter();
                return currentResult;
            }

            gameState.Actions.EncounterResult = currentResult;

            List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(currentResult.Encounter);
            gameState.Actions.SetEncounterChoiceOptions(choiceOptions);
        }
        else
        {
            // Encounter is Over
            gameState.Actions.EncounterResult = currentResult;

            // Process world changes from encounter
            Location travelLocation = await ProcessEncounterOutcome(currentResult);

            // Check if this was a travel encounter
            if (gameState.PendingTravel.IsTravelPending &&
                currentResult.EncounterResults == EncounterResults.EncounterSuccess)
            {
                // Use the pending travel destination as the travel location
                travelLocation = LocationSystem.GetLocation(gameState.PendingTravel.Destination);
            }

            currentResult.TravelLocation = travelLocation;
        }

        return currentResult;
    }

    public List<UserEncounterChoiceOption> GetUserEncounterChoiceOptions(EncounterManager encounter)
    {
        List<IChoice> choices = EncounterSystem.GetChoices();
        List<UserEncounterChoiceOption> choiceOptions = new List<UserEncounterChoiceOption>();

        int i = 0;
        foreach (IChoice choice in choices)
        {
            i++;

            string locationName = encounter.encounterState.Location.Name;

            UserEncounterChoiceOption option = new UserEncounterChoiceOption(
                i,
                choice.ToString(),
                "Narrative",
                locationName,
                "locationSpotName",
                encounter,
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
        EncounterViewModel model = new EncounterViewModel();

        EncounterManager encounterManager = GetEncounter();
        List<UserEncounterChoiceOption> userEncounterChoiceOptions = EncounterSystem.GetUserEncounterChoiceOptions();

        EncounterState state = encounterManager.encounterState;
        EncounterResult encounterResult = currentResult;

        model.CurrentEncounter = encounterManager;
        model.CurrentChoices = userEncounterChoiceOptions;
        model.ChoiceSetName = "Current Situation";
        model.State = state;
        model.EncounterResult = encounterResult;

        return model;
    }

    public async Task<Location> ProcessEncounterOutcome(EncounterResult encounterResult)
    {
        NarrativeResult narrativeResult = encounterResult.NarrativeResult;
        string narrative = narrativeResult.SceneNarrative;
        string outcome = narrativeResult.Outcome.ToString();

        // Generate a unique encounter ID based on the context
        string encounterId = GenerateEncounterId(encounterResult);

        // MARK ENCOUNTER AS COMPLETED - This was missing
        worldState.MarkEncounterCompleted(encounterId);
       
        if (_useMemory)
        {
            string oldMemory = await MemoryFileAccess.ReadFromMemoryFile();

            // Create memory entry
            MemoryConsolidationInput memoryInput = new MemoryConsolidationInput { OldMemory = oldMemory };
            string memoryEntry = await evolutionService.ConsolidateMemory(encounterResult.NarrativeContext, memoryInput);

            string location = encounterResult.Encounter.GetNarrativeContext().LocationName;
            string locationSpot = encounterResult.Encounter.GetNarrativeContext().locationSpotName;
            string action = encounterResult.Encounter.ActionImplementation.Name;
            string goal = encounterResult.Encounter.ActionImplementation.Goal;

            string title = $"{location} - {locationSpot}, {action} - {goal}" + Environment.NewLine;

            string memoryEntryToWrite = title + memoryEntry;

            await MemoryFileAccess.WriteToMemoryFile(memoryEntryToWrite);
        }

        if (_processStateChanges)
        {
            // Prepare the input
            PostEncounterEvolutionInput input = PreparePostEncounterEvolutionInput(narrative, outcome);

            // Process world evolution
            EvolutionResult evolutionResponse = await evolutionService.ProcessEncounterOutcome(encounterResult.NarrativeContext, input, encounterResult);

            // Store the evolution response in the result
            encounterResult.PostEncounterEvolution = evolutionResponse;

            // Update world state
            return await evolutionService.IntegrateEncounterOutcome(evolutionResponse, worldState, LocationSystem, playerState);
        }

        return null;
    }

    private string GenerateEncounterId(EncounterResult result)
    {
        // Create a unique ID based on location, spot, and action
        NarrativeContext context = result.NarrativeContext;
        return $"{context.LocationName}_{context.locationSpotName}_{context.ActionImplementation.Name}";
    }

}
