public class GameWorldManager
{
    private bool _useMemory;
    private bool _processStateChanges;
    private GameWorld gameWorld;
    private Player player;
    private WorldState worldState;
    private EncounterFactory encounterFactory;
    private MessageSystem messageSystem;
    private ActionGenerator actionGenerator;
    private ActionFactory actionFactory;
    private ActionRepository actionRepository;
    private ActionProcessor actionProcessor;
    private LocationSystem locationSystem;
    private LocationRepository locationRepository;
    private TravelManager travelManager;
    private ContentLoader contentLoader;
    private List<Opportunity> availableOpportunities = new List<Opportunity>();
    private readonly ILogger<GameWorldManager> logger;

    public GameWorldManager(GameWorld gameWorld, EncounterFactory encounterSystem,
                       PersistentChangeProcessor evolutionSystem, LocationSystem locationSystem,
                       MessageSystem messageSystem, ActionFactory actionFactory, ActionRepository actionRepository,
                       LocationRepository locationRepository, TravelManager travelManager,
                       ActionGenerator actionGenerator, PlayerProgression playerProgression,
                       ActionProcessor actionProcessor, ContentLoader contentLoader, 
                       ChoiceProjectionService choiceProjectionService,
                       IConfiguration configuration, ILogger<GameWorldManager> logger)
    {
        this.gameWorld = gameWorld;
        this.player = gameWorld.Player;
        this.worldState = gameWorld.WorldState;
        this.encounterFactory = encounterSystem;
        this.locationSystem = locationSystem;
        this.messageSystem = messageSystem;
        this.actionFactory = actionFactory;
        this.actionRepository = actionRepository;
        this.locationRepository = locationRepository;
        this.travelManager = travelManager;
        this.actionGenerator = actionGenerator;
        this.actionProcessor = actionProcessor;
        this.contentLoader = contentLoader;
        this.logger = logger;
        _useMemory = configuration.GetValue<bool>("useMemory");
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
    }

    public async Task StartGame()
    {
        player.HealFully();

        gameWorld.TimeManager.SetNewTime(TimeManager.TimeDayStart);

        Location startingLocation = await locationSystem.Initialize();
        worldState.RecordLocationVisit(startingLocation.Id);
        travelManager.StartLocationTravel(startingLocation.Id);

        Location currentLocation = worldState.CurrentLocation;
        if (worldState.CurrentLocationSpot == null && locationSystem.GetLocationSpots(currentLocation.Id).Any() == true)
        {
            Console.WriteLine("Current location spot is null despite spots existing - manually setting");
            gameWorld.SetCurrentLocation(startingLocation, locationSystem.GetLocationSpots(currentLocation.Id).First());
        }

        Location? currentLoc = currentLocation;
        Console.WriteLine($"Game started at: {currentLoc?.Id}, Current spot: {worldState.CurrentLocationSpot?.SpotID}");

        await UpdateGameWorld();
    }

    public async Task UpdateGameWorld()
    {
        // Process any pending AI responses
        ProcessPendingAIResponses();

        // Process streaming content updates
        UpdateStreamingContent();

        // Check for encounter completion
        CheckEncounterState();
        
        await UpdateState();
    }

    private void ProcessPendingAIResponses()
    {
    }

    private void UpdateStreamingContent()
    {
    }

    private async Task UpdateState()
    {
        gameWorld.ActionStateTracker.ClearCurrentUserAction();
        actionProcessor.UpdateState();

        UpdateAvailableOpportunities();

        Location location = worldState.CurrentLocation;
        LocationSpot locationSpot = worldState.CurrentLocationSpot;

        List<LocationAction> locationActions = await CreateActions(location, locationSpot);
        List<LocationAction> opportunityImplementations = await CreateOpportunities(location, locationSpot);
        locationActions.AddRange(opportunityImplementations);

        List<UserActionOption> locationSpotActionOptions =
            await CreateUserActionsForLocationSpot(
                location,
                locationSpot,
                locationActions);

        gameWorld.ActionStateTracker.SetLocationSpotActions(locationSpotActionOptions);
    }

    public async Task NextEncounterBeat()
    {
        EncounterManager currentEncounterManager = gameWorld.ActionStateTracker.CurrentEncounterManager;
        await currentEncounterManager.ProcessNextBeat();
    }
    
    public async Task<EncounterManager> StartEncounter(string approachId)
    {
        LocationAction locationAction = gameWorld.ActionStateTracker.CurrentAction.locationAction;

        Location location = worldState.CurrentLocation;
        string locationId = location.Id;
        string locationName = location.Name;

        string timeOfDay = GetTimeOfDay(worldState.CurrentTimeHours);

        LocationSpot? locationSpot = locationSystem.GetLocationSpot(
            location.Id, worldState.CurrentLocationSpot.SpotID);

        int playerLevel = player.Level;

        ApproachDefinition? approach = locationAction.Approaches.Where(a => a.Id == approachId).FirstOrDefault();
        SkillCategories SkillCategory = approach?.RequiredCardType ?? SkillCategories.None;

        EncounterContext context = new EncounterContext()
        {
            LocationAction = locationAction,
            SkillCategory = SkillCategory,
            LocationName = location.Name,
            LocationSpotName = locationSpot.Name,
            LocationProperties = locationSpot.GetCurrentProperties(),
            DangerLevel = 1,

            Player = player,
            GameWorld = gameWorld,

            ActionName = locationAction.Name,
            ActionApproach = approach,
            ObjectiveDescription = locationAction.ObjectiveDescription,
            StartingFocusPoints = CalculateFocusPoints(locationAction.Complexity),

            PlayerAllCards = player.GetAllAvailableCards(),
            PlayerSkillCards = player.GetCardsOfType(locationAction.RequiredCardType),

            TargetNPC = locationSpot.PrimaryNPC,
        };

        logger.LogInformation("StartEncounterContext called for encounter at location: {LocationId}", location?.Id);
        EncounterManager encounterManager = await encounterFactory.GenerateEncounter(
            context,
            player,
            locationAction);
        
        // Store reference in GameWorld
        gameWorld.ActionStateTracker.SetActiveEncounter(encounterManager);

        // Initialize the encounter
        await encounterManager.InitializeEncounter();
        return encounterManager;
    }

    public async Task<BeatOutcome> ProcessPlayerChoice(PlayerChoiceSelection playerChoice)
    {
        Location location = worldState.CurrentLocation;
        string? currentLocation = worldState.CurrentLocation?.Id;

        if (string.IsNullOrWhiteSpace(currentLocation)) return null;

        BeatOutcome beatOutcome = await gameWorld.ActionStateTracker.CurrentEncounterManager
            .ProcessPlayerChoice(playerChoice.Choice);

        //ProcessOngoingEncounter(beatOutcome);
        //ProcessEndEncounter(beatOutcome);

        return beatOutcome;
    }


    private void ProcessOngoingEncounter(BeatOutcome encounterResult)
    {
        if (!IsGameOver(gameWorld.Player))
        {
            //gameWorld.ActionStateTracker.CurrentEncounterResult = encounterResult;

            //List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(encounterResult);
            //gameWorld.ActionStateTracker.SetEncounterChoiceOptions(choiceOptions);
        }
        else
        {
            GameOver();
            return;
        }
    }

    public void ProcessEndEncounter(EncounterResult result)
    {
        worldState.MarkEncounterCompleted(result.locationAction.ActionId);
        gameWorld.ActionStateTracker.CurrentEncounterResult = result;

        AIResponse AIResponse = result.AIResponse;
        string narrative = AIResponse?.BeatNarration;
    }

    private int CalculateFocusPoints(int complexity)
    {
        return 10; // For simplicity, using complexity directly as focus points
    }

    private void CheckEncounterState()
    {
        if (gameWorld.ActionStateTracker.CurrentEncounterManager != null && gameWorld.ActionStateTracker.CurrentEncounterManager.IsEncounterComplete)
        {
            // Handle encounter completion
            if (!gameWorld.StreamingContentState.IsStreaming)
            {
                // Only end encounter after streaming is complete
                gameWorld.EndEncounter();
            }
        }
    }

    private async Task<List<UserActionOption>> CreateUserActionsForLocationSpot(Location location, LocationSpot locationSpot, List<LocationAction> locationActions)
    {
        string? currentLocation = worldState.CurrentLocation?.Id;
        if (string.IsNullOrWhiteSpace(currentLocation)) return new List<UserActionOption>();

        List<UserActionOption> options = new List<UserActionOption>();
        foreach (LocationAction locationAction in locationActions)
        {
            UserActionOption opportunity =
                    new UserActionOption(
                        locationAction.Name,
                        locationSpot.IsClosed,
                        locationAction,
                        locationSpot.LocationId,
                        locationSpot.SpotID,
                        default,
                        location.Difficulty,
                        string.Empty,
                        null);

            bool requirementsMet = actionProcessor.CanExecute(opportunity.locationAction);

            opportunity = opportunity with { IsDisabled = !requirementsMet };
            options.Add(opportunity);
        }

        return options;
    }

    private async Task<List<LocationAction>> CreateOpportunities(Location location, LocationSpot locationSpot)
    {
        List<LocationAction> opportunityImplementations = new List<LocationAction>();
        List<OpportunityDefinition> locationSpotOpportunities = actionRepository.GetOpportunitiesForSpot(locationSpot.SpotID);
        for (int i = 0; i < locationSpotOpportunities.Count; i++)
        {
            OpportunityDefinition opportunityTemplate = locationSpotOpportunities[i];
            if (opportunityTemplate == null)
            {
                string opportunityId =
                    await actionGenerator.GenerateOpportunity(
                    opportunityTemplate.Name,
                    location.Id,
                    locationSpot.SpotID
                    );

                opportunityTemplate = actionRepository.GetOpportunity(opportunityTemplate.Id);
            }

            LocationAction opportunityImplementation = actionFactory.CreateActionFromOpportunity(opportunityTemplate);
            opportunityImplementations.Add(opportunityImplementation);
        }

        return opportunityImplementations;
    }

    private async Task<List<LocationAction>> CreateActions(Location location, LocationSpot locationSpot)
    {
        List<LocationAction> locationActions = new List<LocationAction>();
        List<ActionDefinition> locationSpotActions = actionRepository.GetActionsForSpot(locationSpot.SpotID);
        for (int i = 0; i < locationSpotActions.Count; i++)
        {
            ActionDefinition actionTemplate = locationSpotActions[i];
            if (actionTemplate == null)
            {
                string actionId =
                    await actionGenerator.GenerateAction(
                    actionTemplate.Name,
                    location.Id,
                    locationSpot.SpotID
                    );

                actionTemplate = actionRepository.GetAction(actionTemplate.Id);
            }

            LocationAction locationAction = actionFactory.CreateActionFromTemplate(
                    actionTemplate,
                    location.Id,
                    locationSpot.SpotID,
                    ActionExecutionTypes.Instant);

            locationActions.Add(locationAction);
        }

        return locationActions;
    }

    public async Task OnPlayerSelectsAction(UserActionOption action)
    {
        Player player = gameWorld.Player;
        Location location = locationSystem.GetLocation(action.LocationId);
        LocationSpot locationSpot = location.AvailableSpots.FirstOrDefault(spot => spot.LocationId == action.LocationSpot);

        // Set current action in game state
        gameWorld.ActionStateTracker.SetCurrentUserAction(action);

        // Use our action classification system to determine execution path
        LocationAction locationAction = action.locationAction;
        ActionExecutionTypes executionType = ActionExecutionTypes.Encounter;

        SkillCard card = action.SelectedCard;
        if (card != null)
        {
            this.player.ExhaustCard(card);
        }

        switch (executionType)
        {
            case ActionExecutionTypes.Encounter:
                string approachId = action.ApproachId;
                await StartEncounter(approachId);

                break;

            case ActionExecutionTypes.Instant:
            default:
                await ProcessActionCompletion();
                break;
        }
    }

    public async Task ProcessActionCompletion()
    {
        var action = gameWorld.ActionStateTracker.CurrentAction.locationAction;
        gameWorld.ActionStateTracker.EndEncounter();

        await HandlePlayerMoving(action);
        actionProcessor.ProcessAction(action);

        await UpdateGameWorld();
    }

    private async Task HandlePlayerMoving(LocationAction locationAction)
    {
        string location = locationAction.DestinationLocation;
        string locationSpot = locationAction.DestinationLocationSpot;

        if (!string.IsNullOrWhiteSpace(location))
        {
            travelManager.EndLocationTravel(location, locationSpot);
        }
    }

    public async Task Travel(string targetLocation)
    {
        Location location = locationRepository.GetLocationByName(targetLocation);
        TravelRoute route = travelManager.StartLocationTravel(location.Id, TravelMethods.Walking);

        await Travel(route);
    }

    public async Task Travel(TravelRoute route)
    {
        // Check if player can travel this route
        if (!route.CanTravel(gameWorld.Player))
            return;

        // Apply costs
        int timeCost = route.GetActualTimeCost();
        int energyCost = route.GetActualEnergyCost();

        gameWorld.Player.SpendEnergy(energyCost);
        GameWorld.AdvanceTime(TimeSpan.FromHours(timeCost));

        // Increase knowledge of this route
        route.IncreaseKnowledge();

        // Check for encounters
        int seed = GameWorld.CurrentDay + gameWorld.Player.GetHashCode();
        EncounterContext encounterContext = route.GetEncounter(seed);

        if (encounterContext != null)
        {
            // Start a travel encounter
        }
        else
        {
            // Arrived safely
            gameWorld.SetCurrentLocation(route.Destination);
            await UpdateGameWorld();
        }
    }

    public List<UserEncounterChoiceOption> GetUserEncounterChoiceOptions(EncounterResult encounterResult)
    {
        AIResponse AIResponse = encounterResult.AIResponse;
        List<EncounterChoice> choices = gameWorld.ActionStateTracker.CurrentEncounterManager.Choices;
        List<UserEncounterChoiceOption> choiceOptions = new List<UserEncounterChoiceOption>();

        int i = 0;
        foreach (EncounterChoice choice in choices)
        {
            i++;
            EncounterContext EncounterContext = encounterResult.EncounterContext;
            UserEncounterChoiceOption option = new UserEncounterChoiceOption(
                i,
                choice.ChoiceID,
                choice.NarrativeText,
                EncounterContext.LocationName,
                "locationSpotName",
                encounterResult,
                AIResponse,
                choice);

            choiceOptions.Add(option);
        }

        return choiceOptions;
    }

    public ChoiceProjection GetChoicePreview(UserEncounterChoiceOption choiceOption)
    {
        Location location = locationSystem.GetLocation(choiceOption.LocationName);

        // Execute the choice
        EncounterManager encounterManager = gameWorld.ActionStateTracker.CurrentEncounterManager;
        ChoiceProjection choiceProjection = gameWorld.ActionStateTracker.CurrentEncounterManager
            .GetChoiceProjection(choiceOption.Choice);

        return choiceProjection;
    }

    public async Task MoveToLocationSpot(string locationSpotName)
    {
        Location location = gameWorld.WorldState.CurrentLocation;

        LocationSpot locationSpot = locationSystem.GetLocationSpot(location.Id, locationSpotName);
        gameWorld.SetCurrentLocation(location, locationSpot);

        await UpdateGameWorld();
    }

    public List<Location> GetPlayerKnownLocations()
    {
        return locationSystem.GetAllLocations();
    }

    public bool CanMoveToSpot(string locationSpotId)
    {
        Location location = locationRepository.GetCurrentLocation();
        LocationSpot locationSpot = locationRepository.GetSpot(location.Id, locationSpotId);
        bool canMove = !locationSpot.IsClosed;

        return canMove;
    }

    public string GetTimeOfDay(int totalHours)
    {
        if (totalHours >= 5 && totalHours < 12) return "Morning";
        if (totalHours >= 12 && totalHours < 17) return "Afternoon";
        if (totalHours >= 17 && totalHours < 21) return "Evening";
        return "Night";
    }

    public EncounterViewModel? GetEncounterViewModel()
    {
        EncounterManager encounterManager = gameWorld.ActionStateTracker.CurrentEncounterManager ;
        List<UserEncounterChoiceOption> userEncounterChoiceOptions = gameWorld.ActionStateTracker.UserEncounterChoiceOptions;

        if (encounterManager == null)
        {
            EncounterViewModel encounterViewModel = new();
            encounterViewModel.CurrentEncounterContext = encounterManager;
            encounterViewModel.CurrentChoices = userEncounterChoiceOptions;
            encounterViewModel.ChoiceSetName = "Current Situation";
        }
        else
        {
            EncounterState state = encounterManager.GetEncounterState();

            EncounterViewModel model = new EncounterViewModel();
            model.CurrentEncounterContext = encounterManager;
            model.CurrentChoices = userEncounterChoiceOptions;
            model.ChoiceSetName = "Current Situation";
            model.State = state;
            model.EncounterResult = encounterManager.EncounterResult;
            return model;
        }

        return null;
    }

    public void UpdateAvailableOpportunities()
    {
        foreach (Opportunity opportunity in GameWorld.AllOpportunities)
        {
            if (opportunity.IsAvailable(GameWorld.CurrentDay, GameWorld.CurrentTimeOfDay))
            {
                if (!availableOpportunities.Contains(opportunity))
                {
                    availableOpportunities.Add(opportunity);
                }
            }
            else
            {
                availableOpportunities.Remove(opportunity);
            }
        }
    }


    public EncounterResult GetEncounterResultFor(LocationAction locationAction)
    {
        return new EncounterResult()
        {
            locationAction = locationAction,
            ActionResult = ActionResults.EncounterSuccess,
            EncounterEndMessage = "Success",
            AIResponse = null,
            EncounterContext = null
        };
    }

    public LocationAction GetWaitAction(string spotId)
    {
        ActionDefinition waitAction = new ActionDefinition("Wait", "Wait", spotId)
        { };

        LocationAction action = actionFactory
            .CreateActionFromTemplate(waitAction, worldState.CurrentLocation.Id, spotId, ActionExecutionTypes.Instant);

        return action;
    }

    public async Task StartNewDay()
    {
        //SaveGame();

        if (gameWorld.Player.CurrentActionPoints() == 0)
        {
            actionProcessor.ProcessTurnChange();
        }

        UpdateOpportunities(gameWorld);
        await UpdateGameWorld();
    }

    public void UpdateOpportunities(GameWorld gameWorld)
    {
        List<OpportunityDefinition> expiredOpportunities = new List<OpportunityDefinition>();

        foreach (OpportunityDefinition opportunity in gameWorld.WorldState.ActiveOpportunities)
        {
            // Reduce days remaining
            opportunity.ExpirationDays--;

            // Check if expired
            if (opportunity.ExpirationDays <= 0)
            {
                expiredOpportunities.Add(opportunity);
            }
        }

        // Remove expired Opportunities
        foreach (OpportunityDefinition expired in expiredOpportunities)
        {
            gameWorld.WorldState.ActiveOpportunities.Remove(expired);
            // Optionally: Add to failed Opportunities list
            gameWorld.WorldState.FailedOpportunities.Add(expired);

            // Add message
            // messageSystem.AddSystemMessage($"Opportunity expired: {expired.Name}");
        }
    }

    public async Task RefreshCard(SkillCard card)
    {
        player.RefreshCard(card);
    }

    private void SaveGame()
    {
        try
        {
            contentLoader.SaveGame(gameWorld);
            messageSystem.AddSystemMessage("Game saved successfully");
        }
        catch (Exception ex)
        {
            messageSystem.AddSystemMessage($"Failed to save game: {ex.Message}");
            Console.WriteLine($"Error saving game: {ex}");
        }
    }

    // Public method to get current game state - used by polling UI
    public GameWorldSnapshot GetGameSnapshot()
    {
        return new GameWorldSnapshot(gameWorld);
    }

    public bool CanTravelTo(string locationId)
    {
        return travelManager.CanTravelTo(locationId);
    }

    private void GameOver()
    {
        gameWorld.ActionStateTracker.EndEncounter();
    }

    private bool IsGameOver(Player player)
    {
        return false;
    }
}
