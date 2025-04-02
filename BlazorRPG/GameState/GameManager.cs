using System.Text;
public class GameManager
{
    public PlayerState playerState => gameState.PlayerState;
    public WorldState worldState => gameState.WorldState;

    public GameState gameState;

    private GameRules currentRules;
    public LocationSystem LocationSystem { get; }
    public ItemSystem ItemSystem { get; }
    public NarrativeService NarrativeService { get; }
    public MessageSystem MessageSystem { get; }
    public WorldEvolutionService evolutionService { get; }
    public ActionFactory ActionFactory { get; }
    public ActionRepository ActionRepository { get; }
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
        WorldEvolutionService worldEvolutionService,
        ActionFactory actionFactory,
        ActionRepository actionRepository,
        IConfiguration configuration
        )
    {
        this.gameState = gameState;
        this.currentRules = GameRules.StandardRuleset;

        this.EncounterSystem = EncounterSystem;
        this.LocationSystem = locationSystem;
        this.ItemSystem = itemSystem;
        this.NarrativeService = narrativeService;
        this.MessageSystem = messageSystem;
        evolutionService = worldEvolutionService;
        ActionFactory = actionFactory;
        ActionRepository = actionRepository;
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
        _useMemory = configuration.GetValue<bool>("useMemory");
    }

    public async Task StartGame()
    {
        await TravelToLocation(gameState.PlayerState.StartingLocation);
        UpdateState();

        Item item = ItemSystem.GetItemFromName(ItemNames.CharmingPendant);
        if (item != null) gameState.PlayerState.Equipment.SetMainHand(item);
    }

    public async Task TravelToLocation(string locationName, TravelMethods travelMethod = TravelMethods.Walking)
    {
        if (worldState.CurrentLocation != null)
        {
            string startLocationId = worldState.CurrentLocation.Name;

            // Calculate travel time
            int travelMinutes = CalculateTravelTime(startLocationId, locationName, travelMethod);

            // Advance game time
            worldState.CurrentTimeMinutes += travelMinutes;

            // Consume resources
            ConsumeTravelResources(travelMinutes, travelMethod);

            // Determine if travel encounter occurs
            if (ShouldGenerateTravelEncounter(startLocationId, locationName, travelMethod))
            {
                await GenerateTravelEncounter(startLocationId, locationName, travelMethod);
            }
        }

        // Update player location
        if (!LocationSystem.IsInitialized) await InitializeLocationSystem();

        Location location = LocationSystem.GetLocation(locationName);
        if (location == null)
            location = LocationSystem.GetAllLocations().FirstOrDefault();
        gameState.WorldState.SetCurrentLocation(location);

        LocationSystem.SetCurrentLocation(location.Name);

        UpdateState();
        OnPlayerEnterLocation(gameState.WorldState.CurrentLocation);
    }

    internal async Task InitializeLocationSystem()
    {
        if (!LocationSystem.IsInitialized) await LocationSystem.Initialize();
    }

    public async Task<bool> ExecuteBasicAction(UserActionOption action)
    {
        actionImplementation = action.ActionImplementation;
        locationSpot = action.LocationSpot;

        location = LocationSystem.GetLocation(action.Location);
        gameState.Actions.SetCurrentUserAction(action);

        bool startEncounter = actionImplementation.IsEncounterAction;
        if (startEncounter)
        {
            return true;
        }
        else
        {
            ApplyActionOutcomes(actionImplementation);
            return false;
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
        List<string> previousInteractions = GetLocationInteractionHistory(locationId);

        // Create encounter context
        EncounterContext encounterContextNew = new EncounterContext
        {
            Location = location,
            PresentCharacters = presentCharacters,
            AvailableOpportunities = opportunities,
            TimeOfDay = timeOfDay,
            CurrentEnvironmentalProperties = new List<IEnvironmentalProperty>(),
            Player = CreatePlayerSummary(),
            PreviousInteractions = previousInteractions
        };

        EncounterManager encounterContextOld = await GenerateEncounter(
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
            ActionType = actionImplementation.ActionType,
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
    }

    private List<string> GetLocationInteractionHistory(string locationId)
    {
        return new List<string>();
    }

    private PlayerSummary CreatePlayerSummary()
    {
        return new PlayerSummary();
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
        if (currentEncounterResult != EncounterResults.Ongoing)
        {
            // Encounter is Over
            gameState.Actions.EncounterResult = currentResult;
            await ProcessEncounterOutcome(currentResult);
        }
        else
        {
            // Encounter is Ongoing
            if (IsGameOver(gameState.PlayerState))
            {
                gameState.Actions.CompleteActiveEncounter();
                return currentResult;
            }

            gameState.Actions.EncounterResult = currentResult;

            List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(currentResult.Encounter);
            gameState.Actions.SetEncounterChoiceOptions(choiceOptions);
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

    private void OnPlayerEnterLocation(Location location)
    {
        List<UserActionOption> options = new List<UserActionOption>();
        if (location == null) return;

        List<LocationSpot> locationSpots = location.Spots;

        foreach (LocationSpot locationSpot in locationSpots)
        {
            CreateActionsForLocationSpot(options, location, locationSpot);
        }

        gameState.Actions.SetLocationSpotActions(options);
    }

    private void CreateActionsForLocationSpot(
        List<UserActionOption> options,
        Location location,
        LocationSpot locationSpot)
    {
        List<string> locationSpotActions = locationSpot.ActionTemplates.ToList();
        foreach (string locationSpotAction in locationSpotActions)
        {
            ActionTemplate actionTemplate = ActionRepository.GetAction(locationSpotAction);
            ActionImplementation actionImplementation = ActionFactory.CreateActionFromTemplate(actionTemplate);

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

    public void UpdateAvailableActions()
    {
        gameState.Actions.SetCharacterActions(new List<UserActionOption>());
        gameState.Actions.SetQuestActions(new List<UserActionOption>());

        CreateGlobalActions();
        //CreateCharacterActions();
        //CreateQuestActions();
    }

    public void MoveToLocationSpot(string location, string locationSpotName)
    {
        LocationSpot locationSpot = LocationSystem.GetLocationSpotForLocation(location, locationSpotName);
        gameState.WorldState.SetNewLocationSpot(locationSpot);
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

    public bool CanTravelTo(string locationName)
    {
        List<string> locs = GetConnectedLocations();
        bool canTravel = locs.Contains(locationName);

        int cost = GetTravelCostForLocation(locationName);

        return canTravel;
    }

    private int GetTravelCostForLocation(string locationName)
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

    public void UpdateLocationTravelOptions()
    {
        List<string> connectedLocations = GetConnectedLocations();

        List<UserLocationTravelOption> userTravelOptions = new List<UserLocationTravelOption>();

        for (int i = 0; i < connectedLocations.Count; i++)
        {
            string location = connectedLocations[i];

            UserLocationTravelOption travel = new UserLocationTravelOption(i + 1, location);

            userTravelOptions.Add(travel);
        }

        gameState.WorldState.SetCurrentTravelOptions(userTravelOptions);
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


    public int CalculateTravelTime(string startLocationId, string endLocationId, TravelMethods travelMethod)
    {
        // Get base travel time between locations
        int baseTravelMinutes = 0;

        if (worldState.GetLocations().Any(x => x.Name == endLocationId))
        {
            Location location = worldState.GetLocation(endLocationId);
            baseTravelMinutes = location.TravelTimeMinutes;
        }

        // Apply travel method modifier
        double modifier = GetTravelMethodSpeedModifier(travelMethod);

        // Calculate final travel time
        int travelMinutes = (int)(baseTravelMinutes / modifier);

        return travelMinutes;
    }

    public double GetTravelMethodSpeedModifier(TravelMethods travelMethod)
    {
        switch (travelMethod)
        {
            case TravelMethods.Walking: return 1.0;
            default: return 1.0;
        }
    }

    private string GetRelationshipStatus(int value)
    {
        if (value <= -75) return "Nemesis";
        if (value <= -50) return "Enemy";
        if (value <= -25) return "Hostile";
        if (value <= -10) return "Distrusting";
        if (value < 10) return "Neutral";
        if (value < 25) return "Acquaintance";
        if (value < 50) return "Friend";
        if (value < 75) return "Trusted Friend";
        return "Ally";
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

    public List<InteractionOption> GetSpotInteractions(string locationId, string spotId)
    {
        Location location = worldState.GetLocation(locationId);
        LocationSpot? spot = location.Spots.FirstOrDefault(s => s.Name == spotId);

        if (spot == null)
            return new List<InteractionOption>();

        List<InteractionOption> interactions = new List<InteractionOption>();

        // Add character interactions
        foreach (string characterId in spot.ResidentCharacterIds)
        {
            Character character = worldState.GetCharacter(characterId);
            interactions.Add(new InteractionOption
            {
                Type = "Character",
                Name = $"Speak with {character.Name}",
                Description = $"Initiate a conversation with {character.Name}, {character.Role}",
                TargetId = characterId
            });
        }

        // Add opportunity interactions
        foreach (string opportunityId in spot.AssociatedOpportunityIds)
        {
            Opportunity opportunity = worldState.GetOpportunity(opportunityId);

            if (opportunity.Status == "Available" || opportunity.Status == "In Progress")
            {
                interactions.Add(new InteractionOption
                {
                    Type = opportunity.Type,
                    Name = opportunity.Name,
                    Description = opportunity.Description,
                    TargetId = opportunityId
                });
            }
        }

        // Add spot-specific interactions
        if (spot.InteractionType == "Feature")
        {
            interactions.Add(new InteractionOption
            {
                Type = "Examine",
                Name = $"Examine {spot.Name}",
                Description = spot.InteractionDescription,
                TargetId = spotId
            });
        }

        return interactions;
    }

    public void UpdateState()
    {
        gameState.Actions.ClearCurrentUserAction();
        UpdateLocationTravelOptions();
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


    public void SetCurrentLocation(string locationName)
    {
        LocationSystem.SetCurrentLocation(locationName);
    }

    public async Task ProcessEncounterOutcome(EncounterResult encounterResult)
    {
        NarrativeResult narrativeResult = encounterResult.NarrativeResult;

        string narrative = narrativeResult.SceneNarrative;
        string outcome = narrativeResult.Outcome.ToString();
        if (_processStateChanges)
        {
            // Prepare the input
            WorldEvolutionInput input = PrepareWorldEvolutionInput(narrative, outcome);

            // Process world evolution
            WorldEvolutionResponse evolutionResponse = await evolutionService.ProcessWorldEvolution(encounterResult.NarrativeContext, input);

            // Update world state
            evolutionService.IntegrateWorldEvolution(evolutionResponse, worldState, LocationSystem, playerState);

        }
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
    }


    private WorldEvolutionInput PrepareWorldEvolutionInput(string encounterNarrative, string encounterOutcome)
    {
        return new WorldEvolutionInput
        {
            EncounterNarrative = encounterNarrative,
            CharacterBackground = "The player is a former soldier turned merchant seeking new opportunities.",  // Get from player state
            CurrentLocation = worldState.CurrentLocation.Name ?? "Unknown",
            KnownLocations = string.Join(", ", worldState.GetLocations().Select(l => l.Name)),
            KnownCharacters = string.Join(", ", worldState.GetCharacters().Select(c => c.Name)),
            ActiveOpportunities = string.Join(", ", worldState.GetOpportunities().Select(o => o.Name)),
            EncounterOutcome = encounterOutcome
        };
    }


    private async Task GenerateTravelEncounter(string startLocationId, string locationName, TravelMethods travelMethod = TravelMethods.Walking)
    {
    }

    private bool ShouldGenerateTravelEncounter(string startLocationId, string locationName, TravelMethods travelMethod = TravelMethods.Walking)
    {
        return false;
    }

    private void ConsumeTravelResources(int travelMinutes, TravelMethods travelMethod)
    {
        return;
    }

}