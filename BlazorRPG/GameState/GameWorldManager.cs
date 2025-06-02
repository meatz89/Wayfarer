public class GameWorldManager
{
    private bool _useMemory;
    private bool _processStateChanges;
    private GameWorld gameWorld;
    private Player player;
    private WorldState worldState;
    private EncounterFactory encounterFactory;
    private PersistentChangeProcessor evolutionSystem;
    private LocationSystem locationSystem;
    private MessageSystem messageSystem;
    private ActionFactory actionFactory;
    private ActionRepository actionRepository;
    private LocationRepository locationRepository;
    private TravelManager travelManager;
    private ActionGenerator actionGenerator;
    private PlayerProgression playerProgression;
    private ActionProcessor actionProcessor;
    private ContentLoader contentLoader;
    private readonly ChoiceProjectionService choiceProjectionService;
    private readonly ILogger<GameWorldManager> logger;
    private List<Opportunity> availableOpportunities = new List<Opportunity>();
    private List<ChoiceTemplate> allTemplates;

    public GameWorldManager(GameWorld gameState, EncounterFactory encounterSystem,
                       PersistentChangeProcessor evolutionSystem, LocationSystem locationSystem,
                       MessageSystem messageSystem, ActionFactory actionFactory, ActionRepository actionRepository,
                       LocationRepository locationRepository, TravelManager travelManager,
                       ActionGenerator actionGenerator, PlayerProgression playerProgression,
                       ActionProcessor actionProcessor, ContentLoader contentLoader, 
                       ChoiceProjectionService choiceProjectionService,
                       IConfiguration configuration, ILogger<GameWorldManager> logger)
    {
        this.gameWorld = gameState;
        this.player = gameState.Player;
        this.worldState = gameState.WorldState;
        this.encounterFactory = encounterSystem;
        this.evolutionSystem = evolutionSystem;
        this.locationSystem = locationSystem;
        this.messageSystem = messageSystem;
        this.actionFactory = actionFactory;
        this.actionRepository = actionRepository;
        this.locationRepository = locationRepository;
        this.travelManager = travelManager;
        this.actionGenerator = actionGenerator;
        this.playerProgression = playerProgression;
        this.actionProcessor = actionProcessor;
        this.contentLoader = contentLoader;
        this.choiceProjectionService = choiceProjectionService;
        this.logger = logger;
        _useMemory = configuration.GetValue<bool>("useMemory");
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
    }

    public async Task StartGame()
    {
        allTemplates = TemplateLibrary.GetAllTemplates();

        ProcessPlayerArchetype();
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

        await UpdateGameState();
    }

    public async Task UpdateGameState()
    {
        // Process any pending AI responses
        ProcessPendingAIResponses();

        // Process streaming content updates
        UpdateStreamingContent();

        // Check for encounter completion
        CheckEncounterState();
        
        await UpdateState();
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

    public async Task<EncounterManager> StartEncounter(
        string approachId,
        LocationAction locationAction)
    {
        Location location = worldState.CurrentLocation;
        string locationId = location.Id;
        string locationName = location.Name;

        string timeOfDay = GetTimeOfDay(worldState.CurrentTimeHours);

        List<NPC> presentCharacters = worldState.GetCharacters()
            .Where(c =>
            {
                return c.Location == locationId;
            })
            .ToList();

        List<Opportunity> opportunities = worldState.GetOpportunities()
            .Where(o =>
            {
                return o.Location == locationId && o.Status == "Available";
            })
            .ToList();

        List<string> previousInteractions = new();

        LocationSpot? locationSpot = locationSystem.GetLocationSpot(
            location.Id, worldState.CurrentLocationSpot.SpotID);

        int playerLevel = player.Level;

        ApproachDefinition? approach = locationAction.Approaches.Where(a => a.Id == approachId).FirstOrDefault();
        SkillCategories SkillCategory = approach.RequiredCardType;

        EncounterContext context = new EncounterContext()
        {
            LocationAction = locationAction,
            SkillCategory = SkillCategory,
            LocationName = location.Name,
            LocationSpotName = locationSpot.Name,

            ActionName = locationAction.Name,
            ObjectiveDescription = locationAction.ObjectiveDescription,
            PlayerSkillCards = player.GetCardsOfType(locationAction.RequiredCardType),
            StartingFocusPoints = CalculateFocusPoints(locationAction.Complexity),
            CurrentNPC = locationSpot.PrimaryNPC,
            LocationProperties = locationSpot.GetCurrentProperties()
        };

        logger.LogInformation("StartEncounterContext called for encounter: {EncounterId}, location: {LocationId}", encounterContext?.Id, location?.Id);
        EncounterManager encounterManager = await encounterFactory.GenerateEncounter(
            context,
            player,
            locationAction);

        gameWorld.ActionStateTracker.SetActiveEncounter(encounterManager);

        await encounterManager.Initialize();
        
    }

    private AIResponse GetInitialResult()
    {
        throw new NotImplementedException();
    }

    private int CalculateFocusPoints(int complexity)
    {
        return 10; // For simplicity, using complexity directly as focus points
    }

    private void CheckEncounterState()
    {
        if (gameWorld.CurrentEncounter != null && gameWorld.CurrentEncounter.IsEncounterComplete)
        {
            // Handle encounter completion
            if (!gameWorld.StreamingContentState.IsStreaming)
            {
                // Only end encounter after streaming is complete
                gameWorld.EndEncounter();
            }
        }
    }

    private void ProcessPlayerArchetype()
    {
        Professions archetype = player.Archetype;
        int XpBonusForArchetype = 300;

        switch (archetype)
        {
            case Professions.Warrior:
                playerProgression.AddSkillExp(SkillTypes.BruteForce, XpBonusForArchetype);
                break;
            default:
                playerProgression.AddSkillExp(SkillTypes.BruteForce, XpBonusForArchetype);
                break;
        }
    }

    public async Task RefreshCard(SkillCard card)
    {
        player.RefreshCard(card);
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

    public async Task ExecuteAction(UserActionOption action)
    {
        LocationAction locationAction = action.locationAction;
        Player player = gameWorld.Player;

        Location location = locationSystem.GetLocation(action.LocationId);
        LocationSpot locationSpot = location.AvailableSpots.FirstOrDefault(spot => spot.LocationId == action.LocationSpot);

        await OnPlayerSelectsAction(action, player, locationSpot);
    }

    private async Task OnPlayerSelectsAction(UserActionOption action, Player player, LocationSpot? locationSpot)
    {
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
                await StartEncounter(action.ApproachId, locationAction);

                break;

            case ActionExecutionTypes.Instant:
            default:
                await ProcessActionCompletion(locationAction);
                break;
        }
    }

    public async Task ProcessActionCompletion(LocationAction action)
    {
        gameWorld.ActionStateTracker.CompleteAction();

        await HandlePlayerMoving(action);

        actionProcessor.ProcessAction(action);

        await UpdateGameState();
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

        UserActionOption travelOption = new UserActionOption(
            "Travel to " + location.Name, false, route,
            worldState.CurrentLocation.Id, worldState.CurrentLocationSpot.SpotID,
            null, worldState.CurrentLocation.Difficulty, null, null);

        Travel(route);

        await ExecuteAction(travelOption);
    }

    public void Travel(TravelRoute route)
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
        TravelEncounterContext encounterContext = route.GetEncounter(seed);

        if (encounterContext != null)
        {
            // Start a travel encounter
            StartEncounter(encounterContext.EncounterTemplates, encounterContext.Description);
        }
        else
        {
            // Arrived safely
            gameWorld.SetCurrentLocation(route.Destination);
            UpdateGameState();
        }
    }


    public async Task<EncounterResult> ProcessPlayerChoice(string choiceId)
    {
        UserEncounterChoiceOption choiceOption = gameWorld.ActionStateTracker.GetEncounterChoiceOption(choiceId);

        Location location = locationSystem.GetLocation(choiceOption.LocationName);

        string? currentLocation = worldState.CurrentLocation?.Id;
        if (string.IsNullOrWhiteSpace(currentLocation)) return null;

        EncounterResult encounterResult = await gameWorld.ActionStateTracker.EncounterManager
            .ProcessPlayerChoice(gameWorld, choiceOption.Choice);

        ActionResults currentEncounterResult = encounterResult.ActionResult;
        if (currentEncounterResult == ActionResults.Ongoing)
        {
            ProcessOngoingEncounter(encounterResult);
        }
        else
        {
            await ProcessEncounterNarrativeEnding(encounterResult);
        }

        return encounterResult;
    }

    private void ProcessOngoingEncounter(EncounterResult encounterResult)
    {
        if (!IsGameOver(gameWorld.Player))
        {
            gameWorld.ActionStateTracker.EncounterResult = encounterResult;

            List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(encounterResult);
            gameWorld.ActionStateTracker.SetEncounterChoiceOptions(choiceOptions);
        }
        else
        {
            GameOver();
            return;
        }
    }

    private void GameOver()
    {
        gameWorld.ActionStateTracker.CompleteAction();
    }

    private bool IsGameOver(Player playerState)
    {
        return false;
    }

    /// <summary>
    /// ONLY EncounterContext related outcome that is NOT ALSO included in basic actions
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public async Task ProcessEncounterNarrativeEnding(EncounterResult result)
    {
        worldState.MarkEncounterCompleted(result.locationAction.ActionId);

        gameWorld.ActionStateTracker.EncounterResult = result;

        AIResponse AIResponse = result.AIResponse;
        string narrative = AIResponse?.BeatNarration;

        if (_processStateChanges)
        {
            //await ProcessPostEncounterEvolution(result, narrative, outcome);
        }
    }

    private async Task CreateMemoryRecord(EncounterResult encounterResult)
    {
        string oldMemory = await MemoryFileAccess.ReadFromMemoryFile();

        // Create memory entry
        MemoryConsolidationInput memoryInput = new MemoryConsolidationInput { OldMemory = oldMemory };

        string currentLocation = worldState.CurrentLocation.Id;
        if (string.IsNullOrWhiteSpace(currentLocation)) return;

        string memoryEntry = await evolutionSystem.ConsolidateMemory(encounterResult.EncounterContext, memoryInput);

        string location = encounterResult.EncounterContext.LocationName;
        string locationSpot = encounterResult.EncounterContext.LocationSpotName;
        string actionName = encounterResult.locationAction.ActionId;
        string description = encounterResult.locationAction.ObjectiveDescription;

        string title = $"{location} - {locationSpot}, {actionName} - {description}" + Environment.NewLine;

        string memoryEntryToWrite = title + memoryEntry;

        await MemoryFileAccess.WriteToMemoryFile(memoryEntryToWrite);
    }

    public List<UserEncounterChoiceOption> GetUserEncounterChoiceOptions(EncounterResult encounterResult)
    {
        AIResponse AIResponse = encounterResult.AIResponse;
        List<EncounterChoice> choices = gameWorld.ActionStateTracker.EncounterManager.GetChoices();
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
        EncounterManager encounterContext = gameWorld.ActionStateTracker.EncounterManager;
        ChoiceProjection choiceProjection = gameWorld.ActionStateTracker.EncounterManager.GetChoiceProjection(encounterContext, choiceOption.Choice);
        return choiceProjection;
    }

    public async Task MoveToLocationSpot(string locationSpotName)
    {
        Location location = gameWorld.WorldState.CurrentLocation;

        LocationSpot locationSpot = locationSystem.GetLocationSpot(location.Id, locationSpotName);
        gameWorld.SetCurrentLocation(location, locationSpot);

        await UpdateGameState();
    }

    public List<Location> GetPlayerKnownLocations()
    {
        return locationSystem.GetAllLocations();
    }

    public List<string> GetConnectedLocations()
    {
        List<string> loc =
            locationSystem.GetAllLocations()
            .Where(x =>
            {
                return x != gameWorld.WorldState.CurrentLocation;
            })
            .Select(x =>
            {
                return x.Id;
            })
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
        EncounterManager encounterManager = gameWorld.ActionStateTracker.EncounterManager;
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
            EncounterState state = encounterManager.state;

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
        await UpdateGameState();
    }

    public void UpdateOpportunities(GameWorld gameState)
    {
        List<OpportunityDefinition> expiredOpportunities = new List<OpportunityDefinition>();

        foreach (OpportunityDefinition opportunity in gameState.WorldState.ActiveOpportunities)
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
            gameState.WorldState.ActiveOpportunities.Remove(expired);
            // Optionally: Add to failed Opportunities list
            gameState.WorldState.FailedOpportunities.Add(expired);

            // Add message
            // messageSystem.AddSystemMessage($"Opportunity expired: {expired.Name}");
        }
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

    private ChoiceTemplate FindTemplateByName(string templateName)
    {
        foreach (ChoiceTemplate template in allTemplates)
        {
            if (template.TemplateName == templateName)
            {
                return template;
            }
        }

        return null;
    }

    private List<FlagStates> DetermineGoalFlags(string encounterType)
    {
        // In a real implementation, this would return different goals based on encounter type
        List<FlagStates> goalFlags = new List<FlagStates>();

        if (encounterType == "SocialIntroduction")
        {
            goalFlags.Add(FlagStates.TrustEstablished);
        }
        else if (encounterType == "Investigation")
        {
            goalFlags.Add(FlagStates.InsightGained);
            goalFlags.Add(FlagStates.SecretRevealed);
        }

        return goalFlags;
    }

    // Public method to get current game state - used by polling UI
    public GameWorldSnapshot GetGameSnapshot()
    {
        return new GameWorldSnapshot(gameWorld);
    }
}