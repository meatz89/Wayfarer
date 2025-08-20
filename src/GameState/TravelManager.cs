using System.Collections.Generic;
using System.Linq;

public static class TravelTimeMatrix
{
    // Travel times in minutes between locations
    private static readonly Dictionary<(string, string), int> travelTimes = new Dictionary<(string, string), int>
    {
        // Your Room connections (very close to Market Square)
        { ("your_room", "market_square"), 10 },
        { ("market_square", "your_room"), 10 },
        
        // Market Square as central hub
        { ("market_square", "noble_district"), 30 },
        { ("noble_district", "market_square"), 30 },

        { ("market_square", "merchant_row"), 15 },
        { ("merchant_row", "market_square"), 15 },

        { ("market_square", "city_gates"), 30 },
        { ("city_gates", "market_square"), 30 },

        { ("market_square", "riverside"), 45 },
        { ("riverside", "market_square"), 45 },
        
        // Non-hub connections (longer)
        { ("noble_district", "merchant_row"), 45 },
        { ("merchant_row", "noble_district"), 45 },

        { ("noble_district", "city_gates"), 60 },
        { ("city_gates", "noble_district"), 60 },

        { ("noble_district", "riverside"), 60 },
        { ("riverside", "noble_district"), 60 },

        { ("merchant_row", "city_gates"), 30 },
        { ("city_gates", "merchant_row"), 30 },

        { ("merchant_row", "riverside"), 60 },
        { ("riverside", "merchant_row"), 60 },

        { ("city_gates", "riverside"), 30 },
        { ("riverside", "city_gates"), 30 },
        
        // Your Room to other locations (must go through Market Square)
        { ("your_room", "noble_district"), 40 },
        { ("noble_district", "your_room"), 40 },

        { ("your_room", "merchant_row"), 25 },
        { ("merchant_row", "your_room"), 25 },

        { ("your_room", "city_gates"), 40 },
        { ("city_gates", "your_room"), 40 },

        { ("your_room", "riverside"), 55 },
        { ("riverside", "your_room"), 55 },
    };

    public static int GetTravelTime(string fromLocationId, string toLocationId)
    {
        // Same location = no travel time
        if (fromLocationId == toLocationId)
            return 0;

        // Look up travel time
        (string fromLocationId, string toLocationId) key = (fromLocationId, toLocationId);
        if (travelTimes.TryGetValue(key, out int time))
            return time;

        // Default fallback for undefined routes
        return 60;
    }

    public static Dictionary<string, int> GetTravelTimesFrom(string locationId)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();

        foreach (KeyValuePair<(string, string), int> kvp in travelTimes)
        {
            if (kvp.Key.Item1 == locationId)
            {
                result[kvp.Key.Item2] = kvp.Value;
            }
        }

        return result;
    }
}

public class TravelManager
{
    private readonly GameWorld _gameWorld;
    private readonly ITimeManager _timeManager;
    private readonly TransportCompatibilityValidator _transportValidator;
    private readonly RouteRepository _routeRepository;
    private readonly AccessRequirementChecker _accessChecker;
    private readonly FlagService _flagService;
    private readonly TravelEventManager _travelEventManager;
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
        FlagService flagService,
        TravelEventManager travelEventManager
        )
    {
        _gameWorld = gameWorld;
        _timeManager = timeManager;
        _transportValidator = transportValidator;
        _routeRepository = routeRepository;
        _accessChecker = accessChecker;
        _flagService = flagService;
        _travelEventManager = travelEventManager;
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

        // Time advancement handled by GameFacade to ensure letter deadlines are updated

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

        // CRITICAL FIX: If no connection exists, create a basic walking route
        // This ensures players can always travel between locations
        if (connection == null)
        {
            // Check if a travel time exists in the matrix
            int travelTime = TravelTimeMatrix.GetTravelTime(fromLocationId, toLocationId);
            if (travelTime > 0 && travelTime < 100) // Valid travel time exists
            {
                // Create a basic walking route
                RouteOption walkingRoute = new RouteOption
                {
                    Id = $"walk_{fromLocationId}_to_{toLocationId}",
                    Origin = fromLocationId,
                    Destination = toLocationId,
                    Method = TravelMethods.Walking,
                    TravelTimeMinutes = travelTime, // Direct minutes from TravelTimeMatrix
                    BaseStaminaCost = Math.Max(1, travelTime / 30), // 1 stamina per 30 minutes
                    IsDiscovered = true, // All walking routes are discovered
                    Description = "On foot"
                    // Note: CoinCost is calculated, not set directly
                    // Note: TransportNPCId doesn't exist in RouteOption
                };
                availableRoutes.Add(walkingRoute);
                return availableRoutes;
            }
            return availableRoutes;
        }

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
    /// Get travel events for a route based on player familiarity and transport
    /// </summary>
    public TravelEventResult GetTravelEvents(RouteOption route, TravelMethods method)
    {
        // Convert TravelMethods to TransportType
        TransportType transport = method switch
        {
            TravelMethods.Walking => TransportType.Walking,
            TravelMethods.Cart => TransportType.Cart,
            TravelMethods.Carriage => TransportType.Carriage,
            _ => TransportType.Walking
        };
        
        return _travelEventManager.DrawTravelEvents(route, transport);
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


