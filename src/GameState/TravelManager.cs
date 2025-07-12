using Wayfarer.Game.MainSystem;

public class TravelManager
{
    private readonly GameWorld _gameWorld;
    private readonly TimeManager _timeManager;
    private readonly ContractProgressionService _contractProgressionService;
    private readonly TransportCompatibilityValidator _transportValidator;
    private readonly RouteRepository _routeRepository;
    public LocationSystem LocationSystem { get; }
    public ActionRepository ActionRepository { get; }
    public LocationRepository LocationRepository { get; }
    public ActionFactory ActionFactory { get; }
    public ItemRepository ItemRepository { get; }

    public TravelManager(
        GameWorld gameWorld,
        LocationSystem locationSystem,
        ActionRepository actionRepository,
        LocationRepository locationRepository,
        ActionFactory actionFactory,
        ItemRepository itemRepository,
        ContractProgressionService contractProgressionService,
        TransportCompatibilityValidator transportValidator,
        RouteRepository routeRepository
        )
    {
        _gameWorld = gameWorld;
        _timeManager = gameWorld.TimeManager;
        _contractProgressionService = contractProgressionService;
        _transportValidator = transportValidator;
        _routeRepository = routeRepository;
        this.LocationSystem = locationSystem;
        this.ActionRepository = actionRepository;
        this.LocationRepository = locationRepository;
        this.ActionFactory = actionFactory;
        ItemRepository = itemRepository;
    }

    public bool CanTravelTo(string locationId)
    {
        Location destination = LocationRepository.GetLocation(locationId);
        Location currentLocation = LocationRepository.GetCurrentLocation();

        if (destination == null || currentLocation == null)
            return false;

        // Check if any route exists and is available
        List<RouteOption> routes = GetAvailableRoutes(currentLocation.Id, destination.Id);
        return routes.Any();
    }

    public RouteOption StartLocationTravel(string locationId, TravelMethods method = TravelMethods.Walking)
    {
        Location destination = LocationRepository.GetLocation(locationId);
        Location currentLocation = LocationRepository.GetCurrentLocation();

        // Find the appropriate route

        List<RouteOption> routes = GetAvailableRoutes(currentLocation.Id, destination.Id);
        RouteOption? selectedRoute = routes.FirstOrDefault(r => r.Method == method);

        if (selectedRoute == null) return null;

        return selectedRoute;
    }

    public void TravelToLocation(string travelLocation, string locationSpotName, RouteOption selectedRoute)
    {
        // Calculate actual costs with weather and weight modifications
        int adjustedStaminaCost = CalculateStaminaCost(selectedRoute);
        int adjustedCoinCost = CalculateCoinCost(selectedRoute);

        // Deduct costs
        _gameWorld.GetPlayer().ModifyCoins(-adjustedCoinCost);
        _gameWorld.GetPlayer().SpendStamina(adjustedStaminaCost);

        // Record route usage for discovery mechanics
        RecordRouteUsage(selectedRoute.Id);

        // Advance time
        AdvanceTimeBlocks(selectedRoute.TimeBlockCost);

        // Update location
        Location targetLocation = LocationSystem.GetLocation(travelLocation);

        List<LocationSpot> spots = LocationSystem.GetLocationSpots(targetLocation.Id);
        LocationSpot? locSpot = spots.FirstOrDefault((Func<LocationSpot, bool>)(ls =>
        {
            return ls.SpotID == locationSpotName;
        }));

        LocationRepository.SetCurrentLocation(targetLocation, locSpot);

        // Check for contract progression based on arrival at destination
        _contractProgressionService.CheckTravelProgression(targetLocation.Id, _gameWorld.GetPlayer());

        string? currentLocation = LocationRepository.GetCurrentLocation()?.Id;

        bool isFirstVisit = LocationRepository.IsFirstVisit(targetLocation.Id);
        if (isFirstVisit)
        {
            LocationRepository.RecordLocationVisit(targetLocation.Id);
            // Discovery bonuses removed - new locations provide natural opportunities through different markets
        }
    }

    private void AdvanceTimeBlocks(int timeBlockCost)
    {
        // Consume time blocks through TimeManager
        _timeManager.ConsumeTimeBlock(timeBlockCost);
    }

    // Discovery bonuses removed - emergent gameplay provides rewards through:
    // - New market prices for arbitrage
    // - Different contract opportunities
    // - Unique items available at new locations

    public List<RouteOption> GetAvailableRoutes(string fromLocationId, string toLocationId)
    {
        List<RouteOption> availableRoutes = new List<RouteOption>();
        Location fromLocation = LocationRepository.GetLocation(fromLocationId);

        // If location doesn't exist, return empty list
        if (fromLocation == null) return availableRoutes;

        // Find connection to destination
        LocationConnection connection = fromLocation.Connections.Find(c => c.DestinationLocationId == toLocationId);
        if (connection == null) return availableRoutes;

        foreach (RouteOption route in connection.RouteOptions)
        {
            // Check if route is discovered
            if (!route.IsDiscovered)
                continue;

            // Check departure times
            if (route.DepartureTime != null && route.DepartureTime != _timeManager.GetCurrentTimeBlock())
                continue;

            // Check if route is temporarily blocked
            if (_routeRepository.IsRouteBlocked(route.Id))
                continue;

            // Weather no longer blocks routes - it affects efficiency instead

            // Check if route is accessible using logical blocking system
            RouteAccessResult accessResult = route.CheckRouteAccess(ItemRepository, _gameWorld.GetPlayer(), _routeRepository.GetCurrentWeather());
            if (accessResult.IsAllowed)
            {
                availableRoutes.Add(route);
            }
        }

        return availableRoutes;
    }

    /// <summary>
    /// Get available transport options for a route with compatibility information
    /// </summary>
    public List<TransportOption> GetAvailableTransportOptions(RouteOption route)
    {
        List<TransportOption> transportOptions = new List<TransportOption>();
        Player player = _gameWorld.GetPlayer();

        // Check each possible transport method
        foreach (TravelMethods transport in Enum.GetValues<TravelMethods>())
        {
            TransportCompatibilityResult compatibility = _transportValidator.CheckFullCompatibility(transport, route, player);
            
            // Check inventory capacity with this transport method
            InventoryCapacityResult inventoryResult = CheckInventoryCapacity(transport);
            
            // Combine transport and inventory compatibility
            bool isFullyCompatible = compatibility.IsCompatible && inventoryResult.CanTravel;
            string blockingReason = compatibility.BlockingReason;
            if (isFullyCompatible && !string.IsNullOrEmpty(inventoryResult.Warning))
            {
                blockingReason = inventoryResult.Warning;
            }
            else if (!inventoryResult.CanTravel)
            {
                blockingReason = inventoryResult.BlockingReason;
                isFullyCompatible = false;
            }
            
            transportOptions.Add(new TransportOption
            {
                Method = transport,
                IsCompatible = isFullyCompatible,
                BlockingReason = blockingReason,
                Warnings = compatibility.Warnings,
                Route = route,
                InventorySlots = GetTransportInventoryBonus(transport)
            });
        }

        return transportOptions;
    }

    /// <summary>
    /// Get all route-transport combinations with compatibility information
    /// </summary>
    public List<TransportOption> GetAvailableRouteTransportCombinations(string fromLocationId, string toLocationId)
    {
        List<TransportOption> allOptions = new List<TransportOption>();
        List<RouteOption> availableRoutes = GetAvailableRoutes(fromLocationId, toLocationId);

        foreach (RouteOption route in availableRoutes)
        {
            List<TransportOption> transportOptions = GetAvailableTransportOptions(route);
            allOptions.AddRange(transportOptions);
        }

        return allOptions;
    }


    public int CalculateCurrentWeight(GameWorld _gameWorld)
    {
        int totalWeight = 0;

        // Calculate item weight
        foreach (string itemName in _gameWorld.GetPlayer().Inventory.ItemSlots)
        {
            if (itemName != null)
            {
                Item item = ItemRepository.GetItemByName(itemName);
                if (item != null)
                {
                    totalWeight += item.Weight;
                }
            }
        }

        // Add coin weight (10 coins = 1 weight unit)
        totalWeight += _gameWorld.GetPlayer().Coins / 10;

        return totalWeight;
    }

    public int CalculateStaminaCost(RouteOption route)
    {
        int totalWeight = CalculateCurrentWeight(_gameWorld);
        WeatherCondition currentWeather = _routeRepository.GetCurrentWeather();
        int staminaCost = route.CalculateStaminaCost(totalWeight, currentWeather, ItemRepository, _gameWorld.GetPlayer());
        return staminaCost;
    }

    public int CalculateCoinCost(RouteOption route)
    {
        WeatherCondition currentWeather = _routeRepository.GetCurrentWeather();
        return route.CalculateCoinCost(currentWeather);
    }

    // Add a helper method for UI display
    public string GetWeightStatusDescription(int totalWeight)
    {
        return totalWeight switch
        {
            < 4 => "Light load",
            < 7 => "Medium load (+1 stamina cost)",
            _ => "Heavy load (+2 stamina cost)"
        };
    }
    public bool CanTravel(RouteOption route)
    {
        bool canTravel = _gameWorld.GetPlayer().Coins >= CalculateCoinCost(route) &&
               _gameWorld.GetPlayer().Stamina >= CalculateStaminaCost(route);

        return canTravel;
    }

    /// <summary>
    /// Get route access information for UI display
    /// </summary>
    public RouteAccessResult GetRouteAccessInfo(RouteOption route)
    {
        return route.CheckRouteAccess(ItemRepository, _gameWorld.GetPlayer(), _routeRepository.GetCurrentWeather());
    }

    /// <summary>
    /// Check if player's current inventory can be carried with the specified transport method
    /// </summary>
    public InventoryCapacityResult CheckInventoryCapacity(TravelMethods transport)
    {
        Player player = _gameWorld.GetPlayer();
        Inventory inventory = player.Inventory;
        
        int usedSlots = inventory.GetUsedSlots(ItemRepository);
        int maxSlots = inventory.GetMaxSlots(ItemRepository, transport);
        
        bool canTravel = usedSlots <= maxSlots;
        string warning = "";
        string blockingReason = "";
        
        if (!canTravel)
        {
            blockingReason = $"Inventory overloaded: {usedSlots}/{maxSlots} slots used with {transport}";
        }
        else if (usedSlots == maxSlots)
        {
            warning = $"Inventory full: {usedSlots}/{maxSlots} slots used";
        }
        
        return new InventoryCapacityResult
        {
            CanTravel = canTravel,
            UsedSlots = usedSlots,
            MaxSlots = maxSlots,
            Warning = warning,
            BlockingReason = blockingReason,
            Transport = transport
        };
    }
    
    /// <summary>
    /// Get inventory slot bonus provided by transport method
    /// </summary>
    public int GetTransportInventoryBonus(TravelMethods transport)
    {
        return transport switch
        {
            TravelMethods.Cart => 2,      // Cart adds 2 slots but blocks mountain routes
            TravelMethods.Carriage => 1,  // Carriage adds modest storage
            _ => 0                        // Walking, Horseback, Boat use base capacity
        };
    }
    
    /// <summary>
    /// Get current inventory status with transport information
    /// </summary>
    public string GetInventoryStatusDescription(TravelMethods? transport = null)
    {
        Player player = _gameWorld.GetPlayer();
        Inventory inventory = player.Inventory;
        
        int usedSlots = inventory.GetUsedSlots(ItemRepository);
        int maxSlots = inventory.GetMaxSlots(ItemRepository, transport);
        
        string transportInfo = transport.HasValue ? $" with {transport}" : "";
        return $"Inventory: {usedSlots}/{maxSlots} slots used{transportInfo}";
    }

    /// <summary>
    /// Record route usage for discovery mechanics
    /// </summary>
    public void RecordRouteUsage(string routeId)
    {
        // Find the route and increment usage count
        foreach (Location location in LocationRepository.GetAllLocations())
        {
            foreach (LocationConnection connection in location.Connections)
            {
                foreach (RouteOption route in connection.RouteOptions)
                {
                    if (route.Id == routeId)
                    {
                        route.UsageCount++;

                        // Check if this unlocks any new routes
                        CheckForRouteUnlocks(route);
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if route usage unlocks new routes
    /// </summary>
    private void CheckForRouteUnlocks(RouteOption usedRoute)
    {
        foreach (Location location in LocationRepository.GetAllLocations())
        {
            foreach (LocationConnection connection in location.Connections)
            {
                foreach (RouteOption route in connection.RouteOptions)
                {
                    if (!route.IsDiscovered && route.UnlockCondition != null)
                    {
                        RouteUnlockCondition condition = route.UnlockCondition;

                        // Check if required route usage count is met
                        if (condition.RequiredRouteUsage == usedRoute.Id &&
                            usedRoute.UsageCount >= condition.RequiredUsageCount)
                        {
                            route.IsDiscovered = true;
                        }
                    }
                }
            }
        }
    }

    // ❌ REMOVED: GetRouteComparisonData method
    // This method violated game design principles by providing automated optimization
    // Players should manually evaluate routes based on their own strategic thinking

    // ❌ REMOVED: GetOptimalRouteRecommendation method
    // This method violated game design principles by solving optimization puzzles for players
    // Players should evaluate routes manually and make their own strategic decisions

    // ❌ REMOVED: All automated optimization helper methods
    // These methods violated game design principles by providing automated analysis
    // Players should manually inspect route properties and make their own decisions
    // 
    // Removed methods:
    // - CalculateArrivalTimeDisplay
    // - GenerateCostBenefitAnalysis  
    // - GenerateRouteRecommendation
    // - FindMostEfficientRoute
    // - FindCheapestRoute
    // - FindLeastStaminaRoute
    // - FindFastestRoute
    // - CalculateRouteEfficiency
    // - GenerateResourceAnalysis
    // - GenerateAlternativeOptions

}

/// <summary>
/// Represents a transport option for a specific route with compatibility information
/// </summary>
public class TransportOption
{
    public TravelMethods Method { get; set; }
    public bool IsCompatible { get; set; }
    public string BlockingReason { get; set; } = "";
    public List<string> Warnings { get; set; } = new List<string>();
    public RouteOption Route { get; set; }
    public int InventorySlots { get; set; } = 0; // Additional inventory slots provided by this transport

    /// <summary>
    /// Get display text for transport compatibility status
    /// </summary>
    public string GetCompatibilityDisplayText()
    {
        if (IsCompatible)
            return "Available";
        else
            return BlockingReason;
    }

    /// <summary>
    /// Get CSS class for UI styling based on compatibility
    /// </summary>
    public string GetCompatibilityCssClass()
    {
        return IsCompatible ? "transport-compatible" : "transport-blocked";
    }
    
    /// <summary>
    /// Get inventory bonus description for UI
    /// </summary>
    public string GetInventoryBonusDescription()
    {
        return InventorySlots > 0 ? $"+{InventorySlots} slots" : "";
    }
}

/// <summary>
/// Result of checking inventory capacity for a transport method
/// </summary>
public class InventoryCapacityResult
{
    public bool CanTravel { get; set; }
    public int UsedSlots { get; set; }
    public int MaxSlots { get; set; }
    public string Warning { get; set; } = "";
    public string BlockingReason { get; set; } = "";
    public TravelMethods Transport { get; set; }
    
    /// <summary>
    /// Get formatted capacity display string
    /// </summary>
    public string GetCapacityDisplayText()
    {
        return $"{UsedSlots}/{MaxSlots} slots";
    }
}

