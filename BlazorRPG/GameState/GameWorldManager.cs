public class GameWorldManager
{
    private bool _useMemory;
    private bool _processStateChanges;
    private GameWorld gameState;
    private Player playerState;
    private WorldState worldState;
    private EncounterSystem encounterSystem;
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
    public GameWorldManager(GameWorld gameState, EncounterSystem encounterSystem,
                       PersistentChangeProcessor evolutionSystem, LocationSystem locationSystem,
                       MessageSystem messageSystem, ActionFactory actionFactory, ActionRepository actionRepository,
                       LocationRepository locationRepository, TravelManager travelManager,
                       ActionGenerator actionGenerator, PlayerProgression playerProgression,
                       ActionProcessor actionProcessor, ContentLoader contentLoader, IConfiguration configuration)
    {
        this.gameState = gameState;
        this.playerState = gameState.Player;
        this.worldState = gameState.WorldState;
        this.encounterSystem = encounterSystem;
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
        _useMemory = configuration.GetValue<bool>("useMemory");
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
    }

    public async Task StartGame()
    {
        ProcessPlayerArchetype();
        playerState.HealFully();

        gameState.TimeManager.SetNewTime(TimeManager.TimeDayStart);

        Location startingLocation = await locationSystem.Initialize();
        worldState.RecordLocationVisit(startingLocation.Id);
        travelManager.StartLocationTravel(startingLocation.Id);

        Location currentLocation = worldState.CurrentLocation;
        if (worldState.CurrentLocationSpot == null && locationSystem.GetLocationSpots(currentLocation.Id).Any() == true)
        {
            Console.WriteLine("Current location spot is null despite spots existing - manually setting");
            gameState.SetCurrentLocation(startingLocation, locationSystem.GetLocationSpots(currentLocation.Id).First());
        }

        Location? currentLoc = currentLocation;
        Console.WriteLine($"Game started at: {currentLoc?.Id}, Current spot: {worldState.CurrentLocationSpot?.SpotID}");

        await UpdateState();
    }
    
    private void ProcessPlayerArchetype()
    {
        Professions archetype = playerState.Archetype;
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
        playerState.RefreshCard(card);
    }

    private async Task<List<UserActionOption>> CreateUserActionsForLocationSpot(Location location, LocationSpot locationSpot, List<LocationAction> locationActions)
    {
        string? currentLocation = worldState.CurrentLocation?.Id;
        if (string.IsNullOrWhiteSpace(currentLocation)) return new List<UserActionOption>();

        List<UserActionOption> options = new List<UserActionOption>();
        foreach (LocationAction locationAction in locationActions)
        {
            UserActionOption commission =
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

            bool requirementsMet = actionProcessor.CanExecute(commission.locationAction);

            commission = commission with { IsDisabled = !requirementsMet };
            options.Add(commission);
        }

        return options;
    }

    private async Task<List<LocationAction>> CreateCommissions(Location location, LocationSpot locationSpot)
    {
        List<LocationAction> commissionImplementations = new List<LocationAction>();
        List<CommissionDefinition> locationSpotCommissions = actionRepository.GetCommissionsForSpot(locationSpot.SpotID);
        for (int i = 0; i < locationSpotCommissions.Count; i++)
        {
            CommissionDefinition commissionTemplate = locationSpotCommissions[i];
            if (commissionTemplate == null)
            {
                string commissionId =
                    await actionGenerator.GenerateCommission(
                    commissionTemplate.Name,
                    location.Id,
                    locationSpot.SpotID
                    );

                commissionTemplate = actionRepository.GetCommission(commissionTemplate.Id);
            }

            LocationAction commissionImplementation = actionFactory.CreateActionFromCommission(commissionTemplate);
            commissionImplementations.Add(commissionImplementation);
        }

        return commissionImplementations;
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
        Player player = gameState.Player;

        Location location = locationSystem.GetLocation(action.LocationId);
        LocationSpot locationSpot = location.LocationSpots.FirstOrDefault(spot => spot.LocationId == action.LocationSpot);

        await OnPlayerSelectsAction(action, player, locationSpot);
    }

    private async Task OnPlayerSelectsAction(UserActionOption action, Player player, LocationSpot? locationSpot)
    {
        // Set current action in game state
        gameState.ActionStateTracker.SetCurrentUserAction(action);

        // Use our action classification system to determine execution path
        LocationAction locationAction = action.locationAction;
        ActionExecutionTypes executionType = GetExecutionType(locationAction);

        SkillCard card = action.SelectedCard;
        if (card != null)
        {
            playerState.ExhaustCard(card);
        }

        switch (executionType)
        {
            case ActionExecutionTypes.Encounter:
                EncounterManager encounterManager =
                    await RunEncounter(
                        locationAction.ActionId,
                        locationAction.Commission,
                        action.ApproachId,
                        locationAction);

                gameState.ActionStateTracker.SetActiveEncounter(encounterManager);

                break;

            case ActionExecutionTypes.Instant:
            default:
                await ProcessActionCompletion(locationAction);
                break;
        }
    }


    private async Task<EncounterManager> RunEncounter(
        string id,
        CommissionDefinition commission,
        string approachId,
        LocationAction locationAction)
    {
        Location location = worldState.CurrentLocation;
        string locationId = location.Id;
        string locationName = location.Name;

        // Get time of day
        string timeOfDay = GetTimeOfDay(worldState.CurrentTimeHours);

        // Find characters at this location
        List<NPC> presentCharacters = worldState.GetCharacters()
            .Where(c =>
            {
                return c.Location == locationId;
            })
            .ToList();

        // Find available opportunities
        List<Opportunity> opportunities = worldState.GetOpportunities()
            .Where(o =>
            {
                return o.Location == locationId && o.Status == "Available";
            })
            .ToList();

        // Get player's history with this location
        List<string> previousInteractions = new();

        LocationSpot? locationSpot = locationSystem.GetLocationSpot(
            location.Id, worldState.CurrentLocationSpot.SpotID);

        // Create initial context with our new value system
        int playerLevel = playerState.Level;

        ApproachDefinition? approach = commission.Approaches.Where(a => a.Id == approachId).FirstOrDefault();
        SkillCategories SkillCategory = approach.RequiredCardType;

        EncounterContext context = new EncounterContext()
        {
            LocationAction = locationAction,
            SkillCategory = SkillCategory,
            LocationName = location,
            LocationSpotName = locationSpot,
        };

        EncounterManager encounterManager = await encounterSystem.GenerateEncounter(
            id,
            commission,
            approach,
            location,
            locationSpot,
            worldState,
            playerState,
            locationAction);

        List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(encounterManager.EncounterResult);
        gameState.ActionStateTracker.SetEncounterChoiceOptions(choiceOptions);

        return encounterManager;
    }

    public async Task<EncounterResult> RunEncounter(LocationSpot spot, LocationAction action, Player player)
    {
        // 1. Initialize encounter context
        EncounterContext context = actionProcessor.InitializeEncounter(spot, action, player);
        EncounterState state = CreateEncounterState(context, player);

        // 2. Run encounter beats
        while (!state.IsEncounterComplete && state.FocusPoints > 0)
        {
            // Generate AI choices
            AIPrompt prompt = promptBuilder.BuildBeatPrompt(context, state);
            AIResponse aiResponse = await aiService.GetResponse(prompt);

            // Present choices to player
            List<ValidatedChoice> choices = responseProcessor.ProcessAIResponse(aiResponse, state);
            PlayerChoiceSelection selection = await uiService.PresentChoices(choices);

            // Resolve choice
            BeatOutcome outcome = choiceResolver.ResolveChoice(selection, state);

            // Update state
            stateManager.ProcessBeatOutcome(outcome, state);

            // Update UI
            uiService.UpdateEncounterDisplay(state, outcome);
        }

        // 3. Process conclusion
        EncounterConclusion conclusion = await ProcessConclusion(state, context);
        ApplyPersistentChanges(conclusion);

        return new EncounterResult
        {
            Success = DetermineSuccess(state),
            PersistentChanges = conclusion.ApprovedChanges
        };
    }


    public async Task ProcessActionCompletion(LocationAction action)
    {
        gameState.ActionStateTracker.CompleteAction();

        await HandlePlayerMoving(action);

        actionProcessor.ProcessAction(action);

        await UpdateState();
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

    public async Task InitiateTravelToLocation(string locationName)
    {
        Location loc = locationRepository.GetLocationByName(locationName);

        // Create travel action using travelManager
        LocationAction travelAction = travelManager.StartLocationTravel(loc.Id, TravelMethods.Walking);

        // Create option with consistent structure
        UserActionOption travelOption = new UserActionOption(
            "Travel to " + locationName, false, travelAction,
            worldState.CurrentLocation.Id, worldState.CurrentLocationSpot.SpotID,
            null, worldState.CurrentLocation.Difficulty, null, null);

        // Use unified action execution
        await ExecuteAction(travelOption);
    }


    public async Task<EncounterResult> ExecuteEncounterChoice(UserEncounterChoiceOption choiceOption)
    {
        Location location = locationSystem.GetLocation(choiceOption.LocationName);

        string? currentLocation = worldState.CurrentLocation?.Id;
        if (string.IsNullOrWhiteSpace(currentLocation)) return null;

        EncounterResult encounterResult = await encounterSystem.ExecuteChoice(
            choiceOption.AIResponse,
            choiceOption.Choice);

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
        if (!IsGameOver(gameState.Player))
        {
            gameState.ActionStateTracker.EncounterResult = encounterResult;

            List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(encounterResult);
            gameState.ActionStateTracker.SetEncounterChoiceOptions(choiceOptions);
        }
        else
        {
            GameOver();
            return;
        }
    }

    private void GameOver()
    {
        gameState.ActionStateTracker.CompleteAction();
    }

    private bool IsGameOver(Player playerState)
    {
        return false;
    }

    /// <summary>
    /// ONLY Encounter related outcome that is NOT ALSO included in basic actions
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public async Task ProcessEncounterNarrativeEnding(EncounterResult result)
    {
        worldState.MarkEncounterCompleted(result.locationAction.ActionId);

        gameState.ActionStateTracker.EncounterResult = result;

        AIResponse AIResponse = result.AIResponse;
        string narrative = AIResponse?.BeatNarration;
        string outcome = AIResponse?.Outcome.ToString();

        if (_processStateChanges)
        {
            await ProcessPostEncounterEvolution(result, narrative, outcome);
        }
    }

    private async Task ProcessPostEncounterEvolution(EncounterResult result, string narrative, string outcome)
    {
        Location currentLocation = locationRepository.GetLocationById(result.EncounterContext.LocationName);
        if (_useMemory)
        {
            await CreateMemoryRecord(result);
        }

        // Prepare the input
        PostEncounterEvolutionInput input = evolutionSystem.PreparePostEncounterEvolutionInput(narrative, outcome);

        // Process world evolution
        PostEncounterEvolutionResult evolutionResponse = await evolutionSystem.ProcessEncounterOutcome(result.EncounterContext, input, result);

        // Store the evolution response in the result
        result.PostEncounterEvolution = evolutionResponse;

        // Update world state
        await evolutionSystem.IntegrateEncounterOutcome(evolutionResponse, worldState, playerState);
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
        List<EncounterChoice> choices = encounterSystem.GetChoices();
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
        EncounterManager encounter = gameState.ActionStateTracker.GetCurrentEncounter();
        ChoiceProjection choiceProjection = encounterSystem.GetChoiceProjection(encounter, choiceOption.Choice);
        return choiceProjection;
    }

    public async Task MoveToLocationSpot(string locationSpotName)
    {
        Location location = gameState.WorldState.CurrentLocation;

        LocationSpot locationSpot = locationSystem.GetLocationSpot(location.Id, locationSpotName);
        gameState.SetCurrentLocation(location, locationSpot);

        await UpdateState();
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
                return x != gameState.WorldState.CurrentLocation;
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
        EncounterManager encounterManager = encounterSystem.GetCurrentEncounter();
        List<UserEncounterChoiceOption> userEncounterChoiceOptions = gameState.ActionStateTracker.UserEncounterChoiceOptions;

        if (encounterManager == null)
        {
            EncounterViewModel encounterViewModel = new();
            encounterViewModel.CurrentEncounter = encounterManager;
            encounterViewModel.CurrentChoices = userEncounterChoiceOptions;
            encounterViewModel.ChoiceSetName = "Current Situation";
        }
        else
        {
            EncounterState state = encounterManager.state;

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

    public ActionExecutionTypes GetExecutionType(LocationAction action)
    {
        if (action.RequiredCardType == ActionExecutionTypes.Encounter)
        {
            return ActionExecutionTypes.Encounter;
        }
        // Instant action
        return ActionExecutionTypes.Instant;
    }

    public async Task UpdateState()
    {
        gameState.ActionStateTracker.ClearCurrentUserAction();
        actionProcessor.UpdateState();

        Location location = worldState.CurrentLocation;
        LocationSpot locationSpot = worldState.CurrentLocationSpot;

        List<LocationAction> locationActions = await CreateActions(location, locationSpot);
        List<LocationAction> commissionImplementations = await CreateCommissions(location, locationSpot);
        locationActions.AddRange(commissionImplementations);

        List<UserActionOption> locationSpotActionOptions =
            await CreateUserActionsForLocationSpot(
                location,
                locationSpot,
                locationActions);

        gameState.ActionStateTracker.SetLocationSpotActions(locationSpotActionOptions);
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

        if (gameState.Player.CurrentActionPoints() == 0)
        {
            actionProcessor.ProcessTurnChange();
        }

        UpdateCommissions(gameState);
        await UpdateState();
    }

    public void UpdateCommissions(GameWorld gameState)
    {
        List<CommissionDefinition> expiredCommissions = new List<CommissionDefinition>();

        foreach (CommissionDefinition commission in gameState.WorldState.ActiveCommissions)
        {
            // Reduce days remaining
            commission.ExpirationDays--;

            // Check if expired
            if (commission.ExpirationDays <= 0)
            {
                expiredCommissions.Add(commission);
            }
        }

        // Remove expired commissions
        foreach (CommissionDefinition expired in expiredCommissions)
        {
            gameState.WorldState.ActiveCommissions.Remove(expired);
            // Optionally: Add to failed commissions list
            gameState.WorldState.FailedCommissions.Add(expired);

            // Add message
            // messageSystem.AddSystemMessage($"Commission expired: {expired.Name}");
        }
    }

    private void SaveGame()
    {
        try
        {
            contentLoader.SaveGame(gameState);
            messageSystem.AddSystemMessage("Game saved successfully");
        }
        catch (Exception ex)
        {
            messageSystem.AddSystemMessage($"Failed to save game: {ex.Message}");
            Console.WriteLine($"Error saving game: {ex}");
        }
    }

}
