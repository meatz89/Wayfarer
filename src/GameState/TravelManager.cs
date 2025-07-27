using Wayfarer.GameState.Constants;

public class TravelManager
{
    private readonly GameWorld _gameWorld;
    private readonly ITimeManager _timeManager;
    private readonly TransportCompatibilityValidator _transportValidator;
    private readonly RouteRepository _routeRepository;
    private readonly AccessRequirementChecker _accessChecker;
    private readonly FlagService _flagService;
    public LocationSystem LocationSystem { get; }
    public LocationRepository LocationRepository { get; }
    public ItemRepository ItemRepository { get; }

    public TravelManager(
        GameWorld gameWorld,
        LocationSystem locationSystem,
        LocationRepository locationRepository,
        ItemRepository itemRepository,
        TransportCompatibilityValidator transportValidator,
        RouteRepository routeRepository,
        AccessRequirementChecker accessChecker,
        ITimeManager timeManager,
        FlagService flagService
        )
    {
        _gameWorld = gameWorld;
        _timeManager = timeManager;
        _transportValidator = transportValidator;
        _routeRepository = routeRepository;
        _accessChecker = accessChecker;
        _flagService = flagService;
        this.LocationSystem = locationSystem;
        this.LocationRepository = locationRepository;
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

        // Check if player has enough stamina for cheapest route
        Player player = _gameWorld.GetPlayer();
        if (player.Stamina < 2) // Base travel stamina cost
        {
            return false; // Not enough stamina for any travel
        }

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

    public void TravelToLocation(RouteOption selectedRoute)
    {
        // Calculate actual costs with weather and weight modifications
        int adjustedStaminaCost = CalculateStaminaCost(selectedRoute);
        int adjustedCoinCost = CalculateCoinCost(selectedRoute);

        // Deduct costs
        _gameWorld.GetPlayer().ModifyCoins(-adjustedCoinCost);
        _gameWorld.GetPlayer().SpendStamina(adjustedStaminaCost);

        // Record route usage for discovery mechanics
        // Route usage counting removed - violates NO USAGE COUNTERS principle
        // Routes are discovered through NPC relationships and token spending

        // Advance time
        _timeManager.AdvanceTime(selectedRoute.TravelTimeHours);

        // Update location
        Location targetLocation = LocationSystem.GetLocation(selectedRoute.Destination);

        List<LocationSpot> spots = LocationSystem.GetLocationSpots(targetLocation.Id);
        LocationSpot locSpot = null;
        if (spots.Count > 0)
        {
            locSpot = spots[0];
        }

        LocationRepository.SetCurrentLocation(targetLocation, locSpot);

        string? currentLocation = LocationRepository.GetCurrentLocation()?.Id;

        bool isFirstVisit = LocationRepository.IsFirstVisit(targetLocation.Id);
        if (isFirstVisit)
        {
            LocationRepository.RecordLocationVisit(targetLocation.Id);
            // Discovery bonuses removed - new locations provide natural opportunities through different markets
        }

        // Set tutorial flags for movement tracking
        if (!_flagService.HasFlag(FlagService.TUTORIAL_FIRST_MOVEMENT))
        {
            _flagService.SetFlag(FlagService.TUTORIAL_FIRST_MOVEMENT);
        }

        // Check if arriving at docks for tutorial
        if (targetLocation.Id == "millbrook_docks" && !_flagService.HasFlag("tutorial_docks_visited"))
        {
            _flagService.SetFlag("tutorial_docks_visited");
        }
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

            // Check departure times and weather-dependent boat schedules
            if (route.DepartureTime != null && route.DepartureTime != _timeManager.GetCurrentTimeBlock())
                continue;

            // Check weather-dependent boat schedules
            if (route.Method == TravelMethods.Boat && !IsBoatScheduleAvailable(route))
                continue;

            // Check if route is temporarily blocked
            if (_routeRepository.IsRouteBlocked(route.Id))
                continue;

            // Weather no longer blocks routes - it affects efficiency instead

            // Check if route is accessible using logical blocking system
            RouteAccessResult accessResult = route.CheckRouteAccess(ItemRepository, _gameWorld.GetPlayer(), _routeRepository.GetCurrentWeather());
            if (!accessResult.IsAllowed)
                continue;

            // Check additional access requirements (token/equipment based)
            if (route.AccessRequirement != null)
            {
                AccessCheckResult requirementCheck = _accessChecker.CheckRouteAccess(route);
                if (!requirementCheck.IsAllowed)
                    continue;
            }

            availableRoutes.Add(route);
        }

        return availableRoutes;
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

        // Add coin weight
        totalWeight += _gameWorld.GetPlayer().Coins / GameConstants.Inventory.COINS_PER_WEIGHT_UNIT;

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
            _ when totalWeight <= GameConstants.LoadWeight.LIGHT_LOAD_MAX => "Light load",
            _ when totalWeight <= GameConstants.LoadWeight.MEDIUM_LOAD_MAX => "Medium load (+1 stamina cost)",
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
    /// Get token-based access information for UI display
    /// </summary>
    public AccessCheckResult GetTokenAccessInfo(RouteOption route)
    {
        if (route.AccessRequirement == null)
            return AccessCheckResult.Allowed();
        
        return _accessChecker.CheckRouteAccess(route);
    }

    /// <summary>
    /// Check if boat schedule is available based on weather conditions and river state
    /// </summary>
    private bool IsBoatScheduleAvailable(RouteOption route)
    {
        // Boats can't operate in poor weather conditions
        WeatherCondition currentWeather = _routeRepository.GetCurrentWeather();

        // Block boat schedules during dangerous weather
        if (currentWeather == WeatherCondition.Rain || currentWeather == WeatherCondition.Snow)
        {
            return false;
        }

        // Boats need specific departure times (no always-available boat routes)
        if (route.DepartureTime == null)
        {
            return false;
        }

        // Additional river condition logic could be added here
        // For now, boats depend on weather and scheduled departure times
        return true;
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

    // REMOVED: RecordRouteUsage violated NO USAGE COUNTERS principle
    // Routes are discovered through NPC relationships and token spending, not usage counting

    // REMOVED: CheckForRouteUnlocks violated NO USAGE COUNTERS principle
    // Routes are discovered through RouteDiscoveryManager and NPC relationships


}


