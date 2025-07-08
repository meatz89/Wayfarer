public class GameWorldManager
{
    private bool _useMemory;
    private bool _processStateChanges;
    private readonly GameWorld _gameWorld;
    
    public GameWorld GameWorld => _gameWorld;

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
    private MarketManager marketManager;
    private TradeManager tradeManager;
    private ContractSystem contractSystem;
    private RestManager restManager;
    private ContentLoader  contentLoader;
    private List<Contract> availableContracts = new List<Contract>();
    
    private bool isAiAvailable = true;

    private readonly ILogger<GameWorldManager> logger;

    public GameWorldManager(GameWorld gameWorld, 
                       ItemRepository itemRepository, EncounterFactory encounterSystem,
                       PersistentChangeProcessor evolutionSystem, LocationSystem locationSystem,
                       MessageSystem messageSystem, ActionFactory actionFactory, ActionRepository actionRepository,
                       LocationRepository locationRepository, TravelManager travelManager,
                       MarketManager marketManager, TradeManager tradeManager, 
                       ContractSystem contractSystem, RestManager restManager,
                       ActionGenerator actionGenerator, PlayerProgression playerProgression,
                       ActionProcessor actionProcessor, ContentLoader  contentLoader,
                       ChoiceProjectionService choiceProjectionService,
                       IConfiguration configuration, ILogger<GameWorldManager> logger)
    {
        _gameWorld = gameWorld;
        this.itemRepository = itemRepository;
        this.encounterFactory = encounterSystem;
        this.locationSystem = locationSystem;
        this.messageSystem = messageSystem;
        this.actionFactory = actionFactory;
        this.actionRepository = actionRepository;
        this.locationRepository = locationRepository;
        this.travelManager = travelManager;
        this.marketManager = marketManager;
        this.tradeManager = tradeManager;
        this.contractSystem = contractSystem;
        this.restManager = restManager;
        this.actionGenerator = actionGenerator;
        this.actionProcessor = actionProcessor;
        this.contentLoader = contentLoader;
        this.logger = logger;
        _useMemory = configuration.GetValue<bool>("useMemory");
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
    }

    public async Task StartGame()
    {
        _gameWorld.GetPlayer().HealFully();

        // TODO: Implement time management through TimeManager

        Location startingLocation = await locationSystem.Initialize();
        _gameWorld.WorldState.RecordLocationVisit(startingLocation.Id);
        travelManager.StartLocationTravel(startingLocation.Id);
        _gameWorld.WorldState.SetCurrentLocation(startingLocation, null);

        Location currentLocation = _gameWorld.WorldState.CurrentLocation;
        if (_gameWorld.WorldState.CurrentLocationSpot == null && locationSystem.GetLocationSpots(currentLocation.Id).Any() == true)
        {
            Console.WriteLine("Current location spot is null despite spots existing - manually setting");
            _gameWorld.WorldState.SetCurrentLocation(startingLocation, locationSystem.GetLocationSpots(currentLocation.Id).First());
        }

        Location? currentLoc = currentLocation;
        Console.WriteLine($"Game started at: {currentLoc?.Id}, Current spot: {_gameWorld.WorldState.CurrentLocationSpot?.SpotID}");

        await Update_gameWorld();
    }

    private async Task Update_gameWorld()
    {
        _gameWorld.ActionStateTracker.ClearCurrentUserAction();
        actionProcessor.UpdateState();

        UpdateAvailableContracts();

        Location location = _gameWorld.WorldState.CurrentLocation;
        LocationSpot locationSpot = _gameWorld.WorldState.CurrentLocationSpot;

        List<LocationAction> locationActions = await CreateActions(location, locationSpot);

        List<UserActionOption> locationSpotActionOptions =
            await CreateUserActionsForLocationSpot(
                location,
                locationSpot,
                locationActions);

        _gameWorld.ActionStateTracker.SetLocationSpotActions(locationSpotActionOptions);
    }

    public async Task NextEncounterBeat()
    {
        if(!isAiAvailable)
        {
            return;
        }

        EncounterManager currentEncounterManager = _gameWorld.ActionStateTracker.CurrentEncounterManager;
        isAiAvailable = await currentEncounterManager.ProcessNextBeat();
    }

    public async Task<EncounterManager> StartEncounter(string approachId)
    {
        LocationAction locationAction = _gameWorld.ActionStateTracker.CurrentAction.locationAction;

        Location location = _gameWorld.WorldState.CurrentLocation;
        string locationId = location.Id;
        string locationName = location.Name;

        string TimeBlocks = GetTimeBlocks(_gameWorld.WorldState.CurrentTimeHours);

        LocationSpot? locationSpot = locationSystem.GetLocationSpot(
            location.Id, _gameWorld.WorldState.CurrentLocationSpot.SpotID);

        int playerLevel = _gameWorld.GetPlayer().Level;

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

            Player = _gameWorld.GetPlayer(),
            GameWorld = _gameWorld,

            ActionName = locationAction.Name,
            ActionApproach = approach,
            ObjectiveDescription = locationAction.ObjectiveDescription,
            StartingFocusPoints = CalculateFocusPoints(locationAction.Complexity),

            PlayerAllCards = _gameWorld.GetPlayer().GetAllAvailableCards(),
            PlayerSkillCards = _gameWorld.GetPlayer().GetCardsOfType(locationAction.RequiredCardType),

            TargetNPC = locationSpot.PrimaryNPC,
        };

        logger.LogInformation("StartEncounterContext called for encounter at location: {LocationId}", location?.Id);
        EncounterManager encounterManager = await encounterFactory.GenerateEncounter(
            context,
            _gameWorld.GetPlayer(),
            locationAction);

        // Store reference in _gameWorld
        _gameWorld.ActionStateTracker.SetActiveEncounter(encounterManager);

        // Initialize the encounter
        await encounterManager.InitializeEncounter();
        return encounterManager;
    }

    public async Task<BeatOutcome> ProcessPlayerChoice(PlayerChoiceSelection playerChoice)
    {
        Location location = _gameWorld.WorldState.CurrentLocation;
        string? currentLocation = _gameWorld.WorldState.CurrentLocation?.Id;

        if (string.IsNullOrWhiteSpace(currentLocation)) return null;

        BeatOutcome beatOutcome = await _gameWorld.ActionStateTracker.CurrentEncounterManager
            .ProcessPlayerChoice(playerChoice);

        return beatOutcome;
    }


    public void ProcessEndEncounter(EncounterResult result)
    {
        _gameWorld.WorldState.MarkEncounterCompleted(result.locationAction.ActionId);
        _gameWorld.ActionStateTracker.CurrentEncounterResult = result;

        AIResponse AIResponse = result.AIResponse;
        string narrative = AIResponse?.BeatNarration;
    }

    private int CalculateFocusPoints(int complexity)
    {
        return 10; // For simplicity, using complexity directly as focus points
    }

    private async Task<List<UserActionOption>> CreateUserActionsForLocationSpot(Location location, LocationSpot locationSpot, List<LocationAction> locationActions)
    {
        string? currentLocation = _gameWorld.WorldState.CurrentLocation?.Id;
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

    private async Task<List<LocationAction>> CreateActions(Location location, LocationSpot locationSpot)
    {
        // Location SPOT IS NULL HERE AFTER GAMESTART???

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
        Player player = _gameWorld.GetPlayer();
        Location location = locationSystem.GetLocation(action.LocationId);
        LocationSpot locationSpot = location.AvailableSpots.FirstOrDefault(spot => spot.LocationId == action.LocationSpot);

        // Set current action in game state
        _gameWorld.ActionStateTracker.SetCurrentUserAction(action);

        // Use our action classification system to determine execution path
        LocationAction locationAction = action.locationAction;
        ActionExecutionTypes executionType = ActionExecutionTypes.Encounter;

        SkillCard card = action.SelectedCard;
        if (card != null)
        {
            _gameWorld.GetPlayer().ExhaustCard(card);
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
        LocationAction action = _gameWorld.ActionStateTracker.CurrentAction.locationAction;
        _gameWorld.ActionStateTracker.EndEncounter();

        await HandlePlayerMoving(action);
        actionProcessor.ProcessAction(action);

        await Update_gameWorld();
    }

    private async Task HandlePlayerMoving(LocationAction locationAction)
    {
        string location = locationAction.DestinationLocation;
        string locationSpot = locationAction.DestinationLocationSpot;

        string fromLocationId = _gameWorld.WorldState.CurrentLocation.Id;
        RouteOption routeOption = travelManager.GetAvailableRoutes(fromLocationId, location).FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(location))
        {
            travelManager.TravelToLocation(location, locationSpot, routeOption);
        }
    }


    public async Task Travel(RouteOption route)
    {
        // Check if player can travel this route
        Player player = _gameWorld.GetPlayer();
        int totalWeight = CalculateTotalWeight();
        if (!route.CanTravel(itemRepository, player, totalWeight))
            return;

        // Apply costs
        int timeCost = route.GetActualTimeCost();
        int staminaCost = travelManager.CalculateStaminaCost(route);

        player.SpendStamina(staminaCost);
        player.ModifyCoins(route.BaseCoinCost);
        // TODO: Implement time advancement through TimeManager

        int seed = _gameWorld.WorldState.CurrentDay + player.GetHashCode();
        EncounterContext encounterContext = route.GetEncounter(seed);

        if (encounterContext != null)
        {
            // Start a travel encounter
            // This would use the encounter system
        }
        else
        {
            // Arrived safely
            var destination = locationRepository.GetLocation(route.Destination);
            _gameWorld.WorldState.SetCurrentLocation(destination, null);
            await Update_gameWorld();
        }
    }

    public ChoiceProjection GetChoicePreview(EncounterChoice choiceOption)
    {
        EncounterManager encounterManager = _gameWorld.ActionStateTracker.CurrentEncounterManager;
        ChoiceProjection choiceProjection = _gameWorld.ActionStateTracker.CurrentEncounterManager
            .GetChoiceProjection(choiceOption);

        return choiceProjection;
    }

    public async Task MoveToLocationSpot(string locationSpotName)
    {
        Location location = _gameWorld.WorldState.CurrentLocation;

        LocationSpot locationSpot = locationSystem.GetLocationSpot(location.Id, locationSpotName);
        _gameWorld.WorldState.SetCurrentLocation(location, locationSpot);

        await Update_gameWorld();
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
        foreach (Contract contract in _gameWorld.WorldState.ActiveContracts)
        {
            if (contract.IsAvailable(_gameWorld.WorldState.CurrentDay, _gameWorld.WorldState.CurrentTimeWindow))
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
            .CreateActionFromTemplate(waitAction, _gameWorld.WorldState.CurrentLocation.Id, spotId, ActionExecutionTypes.Instant);

        return action;
    }

    public async Task StartNewDay()
    {
        //SaveGame();

        if (_gameWorld.GetPlayer().CurrentActionPoints() == 0)
        {
            actionProcessor.ProcessTurnChange();
        }

        UpdateContracts();
        await Update_gameWorld();
    }

    public void UpdateContracts()
    {
        List<Contract> expiredContracts = new List<Contract>();

        // Remove expired Contracts
        foreach (Contract expired in expiredContracts)
        {
            _gameWorld.WorldState.ActiveContracts.Remove(expired);
            _gameWorld.WorldState.FailedContracts.Add(expired);
        }
    }

    public bool CanTravelTo(string locationId)
    {
        return travelManager.CanTravelTo(locationId);
    }

    public async Task RefreshCard(SkillCard card)
    {
        _gameWorld.GetPlayer().RefreshCard(card);
    }

    public GameWorldSnapshot GetGameSnapshot()
    {
        return new GameWorldSnapshot(_gameWorld);
    }

    private void SaveGame()
    {
        try
        {
            contentLoader.SaveGame(_gameWorld);
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
        _gameWorld.ActionStateTracker.EndEncounter();
    }

    private bool IsGameOver(Player player)
    {
        return false;
    }

    // ACTION GATEWAY METHODS
    
    /// <summary>
    /// Execute trading actions (buy/sell items at markets)
    /// </summary>
    public void ExecuteTradeAction(string itemId, string action, string locationId)
    {
        if (action == "buy")
        {
            tradeManager.BuyItem(itemId, locationId);
        }
        else if (action == "sell")
        {
            tradeManager.SellItem(itemId, locationId);
        }
    }

    /// <summary>
    /// Execute travel actions between locations
    /// </summary>
    public void ExecuteTravelAction(string destinationId, string routeId = null)
    {
        travelManager.StartLocationTravel(destinationId);
    }

    /// <summary>
    /// Execute contract-related actions (accept, complete, etc.)
    /// </summary>
    public void ExecuteContractAction(string contractId, string action)
    {
        var contracts = contractSystem.GetActiveContracts();
        var contract = contracts.FirstOrDefault(c => c.Id == contractId);
        
        if (contract != null && action == "complete")
        {
            contractSystem.CompleteContract(contract);
        }
    }

    /// <summary>
    /// Execute rest actions for stamina recovery
    /// </summary>
    public void ExecuteRestAction(string restOptionId)
    {
        var restOptions = restManager.GetAvailableRestOptions();
        var option = restOptions.FirstOrDefault(r => r.Id == restOptionId);
        
        if (option != null)
        {
            restManager.Rest(option);
        }
    }

    /// <summary>
    /// Get market pricing information for UI display
    /// </summary>
    public object GetMarketData(string locationId)
    {
        return marketManager.GetLocationPricing(locationId);
    }

    /// <summary>
    /// Calculate total inventory weight for UI display
    /// </summary>
    public int CalculateTotalWeight()
    {
        var player = _gameWorld.GetPlayer();
        int totalWeight = 0;
        
        // Add item weights
        foreach (var itemName in player.Inventory.ItemSlots)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                var item = itemRepository.GetItem(itemName);
                if (item != null)
                {
                    totalWeight += item.Weight;
                }
            }
        }
        
        // Add coin weight (coins have weight)
        totalWeight += player.Coins / 10;
        
        return totalWeight;
    }
}
