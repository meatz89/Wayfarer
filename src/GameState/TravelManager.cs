public class TravelManager
{
    private readonly GameWorld _gameWorld;
    private readonly TimeManager _timeManager;
    // ✅ REMOVED: cached worldState reference - Read from GameWorld when needed
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
        ItemRepository itemRepository
        )
    {
        _gameWorld = gameWorld;
        _timeManager = gameWorld.TimeManager;
        // ✅ REMOVED: cached worldState assignment
        this.LocationSystem = locationSystem;
        this.ActionRepository = actionRepository;
        this.LocationRepository = locationRepository;
        this.ActionFactory = actionFactory;
        ItemRepository = itemRepository;
    }

    public bool CanTravelTo(string locationId)
    {
        Location destination = LocationRepository.GetLocation(locationId);
        Location currentLocation = _gameWorld.CurrentLocation;

        if (destination == null || currentLocation == null)
            return false;

        // Check if any route exists and is available
        List<RouteOption> routes = GetAvailableRoutes(currentLocation.Id, destination.Id);
        return routes.Any();
    }

    public RouteOption StartLocationTravel(string locationId, TravelMethods method = TravelMethods.Walking)
    {
        Location destination = LocationRepository.GetLocation(locationId);
        Location currentLocation = _gameWorld.CurrentLocation;

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

        _gameWorld.WorldState.SetCurrentLocation(targetLocation, locSpot);

        string? currentLocation = _gameWorld.CurrentLocation?.Id;

        bool isFirstVisit = _gameWorld.WorldState.IsFirstVisit(targetLocation.Id);
        if (isFirstVisit)
        {
            _gameWorld.WorldState.RecordLocationVisit(targetLocation.Id);
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
            if (route.DepartureTime != null && route.DepartureTime != _gameWorld.WorldState.CurrentTimeWindow)
                continue;

            // Check if route is temporarily blocked
            if (_gameWorld.WorldState.IsRouteBlocked(route.Id))
                continue;

            // Weather no longer blocks routes - it affects efficiency instead

            // Check if route is accessible using logical blocking system
            RouteAccessResult accessResult = route.CheckRouteAccess(ItemRepository, _gameWorld.GetPlayer(), _gameWorld.CurrentWeather);
            if (accessResult.IsAllowed)
            {
                availableRoutes.Add(route);
            }
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
        WeatherCondition currentWeather = _gameWorld.CurrentWeather;
        int staminaCost = route.CalculateStaminaCost(totalWeight, currentWeather, ItemRepository, _gameWorld.GetPlayer());
        return staminaCost;
    }

    public int CalculateCoinCost(RouteOption route)
    {
        WeatherCondition currentWeather = _gameWorld.CurrentWeather;
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
        return route.CheckRouteAccess(ItemRepository, _gameWorld.GetPlayer(), _gameWorld.CurrentWeather);
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

