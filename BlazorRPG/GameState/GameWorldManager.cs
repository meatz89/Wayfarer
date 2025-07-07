public class GameWorldManager
{
    private bool _useMemory;
    private bool _processStateChanges;
    private GameWorld GameWorld;
    private Player player;
    private WorldState worldState;

    private ActionRepository actionRepository;
    private ItemRepository itemRepository;

    private EncounterFactory encounterFactory;
    private MessageSystem messageSystem;
    private ActionGenerator actionGenerator;
    private ActionFactory actionFactory;
    private ActionProcessor actionProcessor;
    private LocationSystem locationSystem;
    private LocationRepository locationRepository;
    private TravelManager travelManager;
    private ContentLoader contentLoader;
    private List<Contract> availableContracts = new List<Contract>();
    
    private bool isAiAvailable = true;

    private readonly ILogger<GameWorldManager> logger;

    public GameWorldManager(GameWorld gameWorld, 
                       ItemRepository itemRepository, EncounterFactory encounterSystem,
                       PersistentChangeProcessor evolutionSystem, LocationSystem locationSystem,
                       MessageSystem messageSystem, ActionFactory actionFactory, ActionRepository actionRepository,
                       LocationRepository locationRepository, TravelManager travelManager,
                       ActionGenerator actionGenerator, PlayerProgression playerProgression,
                       ActionProcessor actionProcessor, ContentLoader contentLoader,
                       ChoiceProjectionService choiceProjectionService,
                       IConfiguration configuration, ILogger<GameWorldManager> logger)
    {
        this.GameWorld = gameWorld;
        this.itemRepository = itemRepository;
        this.player = gameWorld.GetPlayer();
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

        GameWorld.TimeManager.SetNewTime(TimeManager.TimeDayStart);

        Location startingLocation = await locationSystem.Initialize();
        worldState.RecordLocationVisit(startingLocation.Id);
        travelManager.StartLocationTravel(startingLocation.Id);
        GameWorld.SetCurrentLocation(startingLocation);

        Location currentLocation = worldState.CurrentLocation;
        if (worldState.CurrentLocationSpot == null && locationSystem.GetLocationSpots(currentLocation.Id).Any() == true)
        {
            Console.WriteLine("Current location spot is null despite spots existing - manually setting");
            GameWorld.SetCurrentLocation(startingLocation, locationSystem.GetLocationSpots(currentLocation.Id).First());
        }

        Location? currentLoc = currentLocation;
        Console.WriteLine($"Game started at: {currentLoc?.Id}, Current spot: {worldState.CurrentLocationSpot?.SpotID}");

        await UpdateGameWorld();
    }

    private async Task UpdateGameWorld()
    {
        GameWorld.ActionStateTracker.ClearCurrentUserAction();
        actionProcessor.UpdateState();

        UpdateAvailableContracts();

        Location location = worldState.CurrentLocation;
        LocationSpot locationSpot = worldState.CurrentLocationSpot;

        List<LocationAction> locationActions = await CreateActions(location, locationSpot);
        List<LocationAction> contractImplementations = await CreateContracts(location, locationSpot);
        locationActions.AddRange(contractImplementations);

        List<UserActionOption> locationSpotActionOptions =
            await CreateUserActionsForLocationSpot(
                location,
                locationSpot,
                locationActions);

        GameWorld.ActionStateTracker.SetLocationSpotActions(locationSpotActionOptions);
    }

    public async Task NextEncounterBeat()
    {
        if(!isAiAvailable)
        {
            return;
        }

        EncounterManager currentEncounterManager = GameWorld.ActionStateTracker.CurrentEncounterManager;
        isAiAvailable = await currentEncounterManager.ProcessNextBeat();
    }

    public async Task<EncounterManager> StartEncounter(string approachId)
    {
        LocationAction locationAction = GameWorld.ActionStateTracker.CurrentAction.locationAction;

        Location location = worldState.CurrentLocation;
        string locationId = location.Id;
        string locationName = location.Name;

        string TimeBlocks = GetTimeBlocks(worldState.CurrentTimeHours);

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
            GameWorld = GameWorld,

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
        GameWorld.ActionStateTracker.SetActiveEncounter(encounterManager);

        // Initialize the encounter
        await encounterManager.InitializeEncounter();
        return encounterManager;
    }

    public async Task<BeatOutcome> ProcessPlayerChoice(PlayerChoiceSelection playerChoice)
    {
        Location location = worldState.CurrentLocation;
        string? currentLocation = worldState.CurrentLocation?.Id;

        if (string.IsNullOrWhiteSpace(currentLocation)) return null;

        BeatOutcome beatOutcome = await GameWorld.ActionStateTracker.CurrentEncounterManager
            .ProcessPlayerChoice(playerChoice);

        return beatOutcome;
    }


    public void ProcessEndEncounter(EncounterResult result)
    {
        worldState.MarkEncounterCompleted(result.locationAction.ActionId);
        GameWorld.ActionStateTracker.CurrentEncounterResult = result;

        AIResponse AIResponse = result.AIResponse;
        string narrative = AIResponse?.BeatNarration;
    }

    private int CalculateFocusPoints(int complexity)
    {
        return 10; // For simplicity, using complexity directly as focus points
    }

    private async Task<List<UserActionOption>> CreateUserActionsForLocationSpot(Location location, LocationSpot locationSpot, List<LocationAction> locationActions)
    {
        string? currentLocation = worldState.CurrentLocation?.Id;
        if (string.IsNullOrWhiteSpace(currentLocation)) return new List<UserActionOption>();

        List<UserActionOption> options = new List<UserActionOption>();
        foreach (LocationAction locationAction in locationActions)
        {
            UserActionOption contract =
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

            bool requirementsMet = actionProcessor.CanExecute(contract.locationAction);

            contract = contract with { IsDisabled = !requirementsMet };
            options.Add(contract);
        }

        return options;
    }

    private async Task<List<LocationAction>> CreateContracts(Location location, LocationSpot locationSpot)
    {
        List<LocationAction> contractImplementations = new List<LocationAction>();
        List<ContractDefinition> locationSpotContracts = actionRepository.GetContractsForSpot(locationSpot.SpotID);
        for (int i = 0; i < locationSpotContracts.Count; i++)
        {
            ContractDefinition contractTemplate = locationSpotContracts[i];
            if (contractTemplate == null)
            {
                string contractId =
                    await actionGenerator.GenerateOpportunity(
                    contractTemplate.Name,
                    location.Id,
                    locationSpot.SpotID
                    );

                contractTemplate = actionRepository.GetOpportunity(contractTemplate.Id);
            }

            LocationAction contractImplementation = actionFactory.CreateActionFromOpportunity(contractTemplate);
            contractImplementations.Add(contractImplementation);
        }

        return contractImplementations;
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
        Player player = GameWorld.GetPlayer();
        Location location = locationSystem.GetLocation(action.LocationId);
        LocationSpot locationSpot = location.AvailableSpots.FirstOrDefault(spot => spot.LocationId == action.LocationSpot);

        // Set current action in game state
        GameWorld.ActionStateTracker.SetCurrentUserAction(action);

        // Use our action classification system to determine execution path
        LocationAction locationAction = action.locationAction;
        ActionExecutionTypes executionType = ActionExecutionTypes.Encounter;

        SkillCard card = action.SelectedCard;
        if (card != null)
        {
            this.player.ExhaustCard(card);
        }

        executionType = ActionExecutionTypes.Instant;

        switch (executionType)
        {
            case ActionExecutionTypes.Instant:
                await ProcessActionCompletion();
                break;

            case ActionExecutionTypes.Encounter:
                string approachId = action.ApproachId;
                await StartEncounter(approachId);
                break;

            default:
                await ProcessActionCompletion();
                break;
        }
    }

    public async Task ProcessActionCompletion()
    {
        LocationAction action = GameWorld.ActionStateTracker.CurrentAction.locationAction;
        GameWorld.ActionStateTracker.EndEncounter();

        await HandlePlayerMoving(action);
        actionProcessor.ProcessAction(action);

        await UpdateGameWorld();
    }

    private async Task HandlePlayerMoving(LocationAction locationAction)
    {
        string location = locationAction.DestinationLocation;
        string locationSpot = locationAction.DestinationLocationSpot;

        string fromLocationId = GameWorld.CurrentLocation.Id;
        RouteOption routeOption = travelManager.GetAvailableRoutes(fromLocationId, location).FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(location))
        {
            travelManager.TravelToLocation(location, locationSpot, routeOption);
        }
    }

    public int CalculateTotalWeight()
    {
        int totalWeight = 0;

        // Calculate item weight
        foreach (string itemName in player.Inventory.ItemSlots)
        {
            if (itemName != null)
            {
                Item item = itemRepository.GetItemByName(itemName);
                if (item != null)
                {
                    totalWeight += item.Weight;
                }
            }
        }

        // Add coin weight (10 coins = 1 weight)
        totalWeight += player.Coins / 10;

        return totalWeight;
    }

    public async Task Travel(RouteOption route)
    {
        // Check if player can travel this route
        Player player = GameWorld.GetPlayer();
        int totalWeight = CalculateTotalWeight();
        if (!route.CanTravel(itemRepository, player, totalWeight))
            return;

        // Apply costs
        int timeCost = route.GetActualTimeCost();
        int staminaCost = travelManager.CalculateStaminaCost(route);

        player.SpendStamina(staminaCost);
        player.ModifyCoins(route.BaseCoinCost);
        GameWorld.AdvanceTime(timeCost);

        int seed = GameWorld.CurrentDay + player.GetHashCode();
        EncounterContext encounterContext = route.GetEncounter(seed);

        if (encounterContext != null)
        {
            // Start a travel encounter
            // This would use the encounter system
        }
        else
        {
            // Arrived safely
            GameWorld.SetCurrentLocation(route.Destination);
            await UpdateGameWorld();
        }
    }

    public ChoiceProjection GetChoicePreview(EncounterChoice choiceOption)
    {
        EncounterManager encounterManager = GameWorld.ActionStateTracker.CurrentEncounterManager;
        ChoiceProjection choiceProjection = GameWorld.ActionStateTracker.CurrentEncounterManager
            .GetChoiceProjection(choiceOption);

        return choiceProjection;
    }

    public async Task MoveToLocationSpot(string locationSpotName)
    {
        Location location = GameWorld.WorldState.CurrentLocation;

        LocationSpot locationSpot = locationSystem.GetLocationSpot(location.Id, locationSpotName);
        GameWorld.SetCurrentLocation(location, locationSpot);

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

    public string GetTimeBlocks(int totalHours)
    {
        if (totalHours >= 5 && totalHours < 12) return "Morning";
        if (totalHours >= 12 && totalHours < 17) return "Afternoon";
        if (totalHours >= 17 && totalHours < 21) return "Evening";
        return "Night";
    }

    public void UpdateAvailableContracts()
    {
        foreach (Contract contract in GameWorld.AllContracts)
        {
            if (contract.IsAvailable(GameWorld.CurrentDay, GameWorld.CurrentTimeBlock))
            {
                if (!availableContracts.Contains(contract))
                {
                    availableContracts.Add(contract);
                }
            }
            else
            {
                availableContracts.Remove(contract);
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

        if (GameWorld.GetPlayer().CurrentActionPoints() == 0)
        {
            actionProcessor.ProcessTurnChange();
        }

        UpdateContracts(GameWorld);
        await UpdateGameWorld();
    }

    public void UpdateContracts(GameWorld gameWorld)
    {
        List<ContractDefinition> expiredContracts = new List<ContractDefinition>();

        foreach (ContractDefinition contract in gameWorld.WorldState.ActiveContracts)
        {
            // Reduce days remaining
            contract.ExpirationDays--;

            // Check if expired
            if (contract.ExpirationDays <= 0)
            {
                expiredContracts.Add(contract);
            }
        }

        // Remove expired Contracts
        foreach (ContractDefinition expired in expiredContracts)
        {
            gameWorld.WorldState.ActiveContracts.Remove(expired);
            // Optionally: Add to failed Contracts list
            gameWorld.WorldState.FailedContracts.Add(expired);

            // Add message
            // messageSystem.AddSystemMessage($"Opportunity expired: {expired.Name}");
        }
    }

    public bool CanTravelTo(string locationId)
    {
        return travelManager.CanTravelTo(locationId);
    }

    public async Task RefreshCard(SkillCard card)
    {
        player.RefreshCard(card);
    }

    public GameWorldSnapshot GetGameSnapshot()
    {
        return new GameWorldSnapshot(GameWorld);
    }

    private void SaveGame()
    {
        try
        {
            contentLoader.SaveGame(GameWorld);
            messageSystem.AddSystemMessage("Game saved successfully");
        }
        catch (Exception ex)
        {
            messageSystem.AddSystemMessage($"Failed to save game: {ex.Message}");
            Console.WriteLine($"Error saving game: {ex}");
        }
    }

    private void GameOver()
    {
        GameWorld.ActionStateTracker.EndEncounter();
    }

    private bool IsGameOver(Player player)
    {
        return false;
    }
}
