public class GameWorldManager
{
    private bool _useMemory;
    private bool _processStateChanges;
    private readonly GameWorld _gameWorld;

    public GameWorld GameWorld
    {
        get
        {
            return _gameWorld;
        }
    }

    private ActionRepository actionRepository;
    private ItemRepository itemRepository;

    private EncounterFactory encounterFactory;
    private ActionFactory actionFactory;
    private ActionProcessor actionProcessor;
    private LocationSystem locationSystem;
    private LocationRepository locationRepository;
    private TravelManager travelManager;
    private MarketManager marketManager;
    private TradeManager tradeManager;
    private ContractSystem contractSystem;
    private RestManager restManager;
    private List<Contract> availableContracts = new List<Contract>();

    private bool isAiAvailable = true;

    private readonly ILogger<GameWorldManager> logger;

    public GameWorldManager(GameWorld gameWorld,
                       ItemRepository itemRepository, EncounterFactory encounterSystem,
                       PersistentChangeProcessor evolutionSystem, LocationSystem locationSystem,
                       ActionFactory actionFactory, ActionRepository actionRepository,
                       LocationRepository locationRepository, TravelManager travelManager,
                       MarketManager marketManager, TradeManager tradeManager,
                       ContractSystem contractSystem, RestManager restManager,
                       PlayerProgression playerProgression, ActionProcessor actionProcessor,
                       ChoiceProjectionService choiceProjectionService,
                       IConfiguration configuration, ILogger<GameWorldManager> logger)
    {
        _gameWorld = gameWorld;
        this.itemRepository = itemRepository;
        this.encounterFactory = encounterSystem;
        this.locationSystem = locationSystem;
        this.actionFactory = actionFactory;
        this.actionRepository = actionRepository;
        this.locationRepository = locationRepository;
        this.travelManager = travelManager;
        this.marketManager = marketManager;
        this.tradeManager = tradeManager;
        this.contractSystem = contractSystem;
        this.restManager = restManager;
        this.actionProcessor = actionProcessor;
        this.logger = logger;
        _useMemory = configuration.GetValue<bool>("useMemory");
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
    }

    public async Task StartGame()
    {
        _gameWorld.GetPlayer().HealFully();

        // TODO: Implement time management through gameWorld.TimeManager

        Location startingLocation = await locationSystem.Initialize();
        _gameWorld.WorldState.RecordLocationVisit(startingLocation.Id);
        travelManager.StartLocationTravel(startingLocation.Id);

        // Preserve the location spot that was set by LocationSystem.Initialize()
        LocationSpot currentSpot = _gameWorld.GetPlayer().CurrentLocationSpot;
        _gameWorld.WorldState.SetCurrentLocation(startingLocation, currentSpot);

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
        if (!isAiAvailable)
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
        List<LocationAction> locationActions = new List<LocationAction>();
        List<ActionDefinition> locationSpotActions = actionRepository.GetActionsForSpot(locationSpot.SpotID);

        for (int i = 0; i < locationSpotActions.Count; i++)
        {
            ActionDefinition actionTemplate = locationSpotActions[i];

            // FOR ECONOMIC POC: Only use pre-defined actions from JSON
            // Skip AI generation - use only what's loaded from templates
            if (actionTemplate == null)
            {
                // Skip null actions instead of generating them via AI
                continue;
            }

            // Create action from template (no AI involvement)
            LocationAction locationAction = actionFactory.CreateActionFromTemplate(
                    actionTemplate,
                    location.Id,
                    locationSpot.SpotID,
                    ActionExecutionTypes.Instant); // Economic actions are instant, not encounters

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
        {
            return;
        }

        // Check time block constraints
        if (!GameWorld.TimeManager.ValidateTimeBlockAction(route.TimeBlockCost))
        {
            throw new InvalidOperationException($"Cannot travel: Not enough time blocks remaining. Route requires {route.TimeBlockCost} blocks, but only {gameWorld.TimeManager.RemainingTimeBlocks} available.");
        }

        // Apply costs
        int timeCost = route.GetActualTimeCost();
        int staminaCost = travelManager.CalculateStaminaCost(route);

        player.SpendStamina(staminaCost);
        player.ModifyCoins(route.BaseCoinCost);

        // Consume time blocks
        GameWorld.TimeManager.ConsumeTimeBlock(route.TimeBlockCost);

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
            Location destination = locationRepository.GetLocation(route.Destination);

            // Find the first available location spot for the destination
            LocationSpot? destinationSpot = locationSystem.GetLocationSpots(destination.Id).FirstOrDefault();

            _gameWorld.WorldState.SetCurrentLocation(destination, destinationSpot);

            // Also update the Player's location to keep them in sync
            player.CurrentLocation = destination;
            player.CurrentLocationSpot = destinationSpot;

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
        if (locationSpot == null)
        {
            return false;
        }
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
        foreach (Contract contract in contractSystem.GetActiveContracts())
        {
            if (contract.IsAvailable(_gameWorld.WorldState.CurrentDay, _gameWorld.WorldState.CurrentTimeBlock))
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
        // Check for failed contracts using ContractSystem
        contractSystem.CheckForFailedContracts();
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
        List<Contract> contracts = contractSystem.GetActiveContracts();
        Contract? contract = contracts.FirstOrDefault(c => c.Id == contractId);

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
        List<RestOption> restOptions = restManager.GetAvailableRestOptions();
        RestOption? option = restOptions.FirstOrDefault(r => r.Id == restOptionId);

        if (option != null)
        {
            // Validate time block availability before attempting rest
            if (!gameWorld.TimeManager.ValidateTimeBlockAction(option.TimeBlockCost))
            {
                throw new InvalidOperationException($"Cannot rest: Not enough time blocks remaining. Rest requires {option.TimeBlockCost} blocks, but only {gameWorld.TimeManager.RemainingTimeBlocks} available.");
            }

            restManager.Rest(option);
        }
    }

    /// <summary>
    /// Get market pricing information for UI display
    /// </summary>
    public List<Item> GetMarketData(string locationId)
    {
        return marketManager.GetAvailableItems(locationId);
    }

    /// <summary>
    /// Get available travel routes between locations (UI read-only access)
    /// </summary>
    public List<RouteOption> GetAvailableRoutes(string fromLocationId, string toLocationId)
    {
        return travelManager.GetAvailableRoutes(fromLocationId, toLocationId);
    }

    /// <summary>
    /// Check if player can travel on a specific route (UI validation)
    /// </summary>
    public bool CanTravelRoute(RouteOption route)
    {
        return travelManager.CanTravel(route);
    }

    /// <summary>
    /// Get available market items for UI display
    /// </summary>
    public List<Item> GetAvailableMarketItems(string locationId)
    {
        return marketManager.GetAvailableItems(locationId);
    }

    /// <summary>
    /// Check if player can buy item (UI validation)
    /// </summary>
    public bool CanBuyMarketItem(string itemId, string locationId)
    {
        return marketManager.CanBuyItem(itemId, locationId);
    }


    /// <summary>
    /// Get item price at a specific location
    /// </summary>
    public int GetItemPrice(string locationId, string itemId, bool isBuyPrice)
    {
        return marketManager.GetItemPrice(locationId, itemId, isBuyPrice);
    }

    /// <summary>
    /// Complete a contract action through gateway
    /// </summary>
    public void CompleteContract(Contract contract)
    {
        // Apply contract completion effects
        _gameWorld.GetPlayer().ModifyCoins(contract.Payment);

        // If this contract unlocks others, make them available
        if (contract.UnlocksContractIds.Any())
        {
            foreach (string contractId in contract.UnlocksContractIds)
            {
                // TODO: Add contract repository dependency to GameWorldManager constructor
                // Contract unlockedContract = contractRepository.GetContract(contractId);
                // if (unlockedContract != null)
                // {
                //     unlockedContract.StartDay = _gameWorld.CurrentDay;
                //     unlockedContract.DueDay = _gameWorld.CurrentDay + 5; // Arbitrary due date
                // }
            }
        }

        // If this contract locks others, make them unavailable
        if (contract.LocksContractIds.Any())
        {
            foreach (string contractId in contract.LocksContractIds)
            {
                // TODO: Add contract repository dependency to GameWorldManager constructor
                // Contract lockedContract = contractRepository.GetContract(contractId);
                // if (lockedContract != null)
                // {
                //     lockedContract.IsFailed = true;
                // }
            }
        }

        // Mark contract as completed
        contract.IsCompleted = true;
    }

    /// <summary>
    /// Calculate total inventory weight for UI display
    /// </summary>
    public int CalculateTotalWeight()
    {
        Player player = _gameWorld.GetPlayer();
        int totalWeight = 0;

        // Add item weights
        foreach (string itemName in player.Inventory.ItemSlots)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                Item item = itemRepository.GetItemById(itemName);
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
