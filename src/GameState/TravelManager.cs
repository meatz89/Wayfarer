public class TravelManager
{
    private readonly GameWorld _gameWorld;
    private readonly TimeManager _timeManager;
    private readonly TransportCompatibilityValidator _transportValidator;
    private readonly RouteRepository _routeRepository;
    private readonly AccessRequirementChecker _accessChecker;
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
        TransportCompatibilityValidator transportValidator,
        RouteRepository routeRepository,
        AccessRequirementChecker accessChecker
        )
    {
        _gameWorld = gameWorld;
        _timeManager = gameWorld.TimeManager;
        _transportValidator = transportValidator;
        _routeRepository = routeRepository;
        _accessChecker = accessChecker;
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
        _timeManager.AdvanceTime(selectedRoute.TravelTimeHours);

        // Update location
        Location targetLocation = LocationSystem.GetLocation(travelLocation);

        List<LocationSpot> spots = LocationSystem.GetLocationSpots(targetLocation.Id);
        LocationSpot? locSpot = spots.FirstOrDefault((Func<LocationSpot, bool>)(ls =>
        {
            return ls.SpotID == locationSpotName;
        }));

        LocationRepository.SetCurrentLocation(targetLocation, locSpot);


        string? currentLocation = LocationRepository.GetCurrentLocation()?.Id;

        bool isFirstVisit = LocationRepository.IsFirstVisit(targetLocation.Id);
        if (isFirstVisit)
        {
            LocationRepository.RecordLocationVisit(targetLocation.Id);
            // Discovery bonuses removed - new locations provide natural opportunities through different markets
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
                var requirementCheck = _accessChecker.CheckRouteAccess(route);
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


}


