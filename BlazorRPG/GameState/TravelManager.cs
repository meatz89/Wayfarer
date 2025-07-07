public class TravelManager
{
    public GameWorld gameWorld { get; }
    public WorldState worldState { get; }
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
        this.gameWorld = gameWorld;
        this.worldState = gameWorld.WorldState;
        this.LocationSystem = locationSystem;
        this.ActionRepository = actionRepository;
        this.LocationRepository = locationRepository;
        this.ActionFactory = actionFactory;
        ItemRepository = itemRepository;
    }

    public bool CanTravelTo(string locationId)
    {
        Location destination = LocationRepository.GetLocation(locationId);
        Location currentLocation = gameWorld.WorldState.CurrentLocation;

        // Check if any route exists and is available
        List<RouteOption> routes = GetAvailableRoutes(currentLocation.Id, destination.Id);
        return routes.Any(r => r.CanTravel(ItemRepository, gameWorld.GetPlayer()));
    }

    public RouteOption StartLocationTravel(string locationId, TravelMethods method = TravelMethods.Walking)
    {
        Location destination = LocationRepository.GetLocation(locationId);
        Location currentLocation = gameWorld.WorldState.CurrentLocation;

        // Find the appropriate route

        List<RouteOption> routes = GetAvailableRoutes(currentLocation.Id, destination.Id);
        RouteOption? selectedRoute = routes.FirstOrDefault(r => r.Method == method);
        
        if (selectedRoute == null) return null;

        return selectedRoute;
    }
    
    public void TravelToLocation(string travelLocation, string locationSpotName, RouteOption selectedRoute)
    {
        int totalWeight = CalculateCurrentWeight(gameWorld);
        int adjustedStaminaCost = selectedRoute.BaseStaminaCost;

        // Apply weight penalties
        if (totalWeight >= 4 && totalWeight <= 6)
        {
            adjustedStaminaCost += 1;
        }
        else if (totalWeight >= 7)
        {
            adjustedStaminaCost += 2;
        }

        // Deduct costs
        gameWorld.PlayerCoins -= selectedRoute.BaseCoinCost;
        gameWorld.PlayerStamina -= adjustedStaminaCost;

        // Advance time
        AdvanceTimeBlocks(selectedRoute.TimeBlockCost);

        // Update location
        Location targetLocation = LocationSystem.GetLocation(travelLocation);

        List<LocationSpot> spots = LocationSystem.GetLocationSpots(targetLocation.Id);
        LocationSpot? locSpot = spots.FirstOrDefault((Func<LocationSpot, bool>)(ls =>
        {
            return ls.SpotID == locationSpotName;
        }));

        gameWorld.SetCurrentLocation(targetLocation, locSpot);

        string? currentLocation = worldState.CurrentLocation?.Id;

        bool isFirstVisit = worldState.IsFirstVisit(targetLocation.Id);
        if (isFirstVisit)
        {
            worldState.RecordLocationVisit(targetLocation.Id);
            if (targetLocation != null)
            {
                ApplyDiscoveryBonus(targetLocation);
            }
        }
    }

    private void AdvanceTimeBlocks(int timeBlockCost)
    {
        gameWorld.AdvanceTime(timeBlockCost);
    }

    private void ApplyDiscoveryBonus(Location location)
    {
        // Only apply bonus if location has values set
        if (location.DiscoveryBonusXP > 0 || location.DiscoveryBonusCoins > 0)
        {
            // Award XP
            int xpBonus = location.DiscoveryBonusXP;
            gameWorld.GetPlayer().AddExperiencePoints(xpBonus);

            // Award coins
            int coinBonus = location.DiscoveryBonusCoins;
            if (coinBonus > 0)
            {
                gameWorld.GetPlayer().AddCoins(coinBonus);
            }
        }
    }

    public List<RouteOption> GetAvailableRoutes(string fromLocationId, string toLocationId)
    {
        List<RouteOption> availableRoutes = new List<RouteOption>();
        Location fromLocation = LocationRepository.GetLocation(fromLocationId);

        // Find connection to destination
        LocationConnection connection = fromLocation.Connections.Find(c => c.DestinationLocationId == toLocationId);
        if (connection == null) return availableRoutes;

        foreach (RouteOption route in connection.RouteOptions)
        {
            // Check if route is discovered
            if (!route.IsDiscovered)
                continue;

            // Check departure times
            if (route.DepartureTime != null && route.DepartureTime > gameWorld.CurrentTimeBlock)
                continue;

            // Check if player has required items for this route type
            bool hasRequiredItems = true;
            foreach (string requiredRouteType in route.RequiredRouteTypes)
            {
                // Check if any inventory item enables this route type
                bool routeTypeEnabled = false;
                foreach (string itemName in gameWorld.PlayerInventory.ItemSlots)
                {
                    if (itemName != null)
                    {
                        Item item = ItemRepository.GetItemByName(itemName);
                        if (item != null && item.EnabledRouteTypes.Contains(requiredRouteType))
                        {
                            routeTypeEnabled = true;
                            break;
                        }
                    }
                }

                if (!routeTypeEnabled)
                {
                    hasRequiredItems = false;
                    break;
                }
            }

            if (hasRequiredItems)
            {
                availableRoutes.Add(route);
            }
        }

        return availableRoutes;
    }

    public int CalculateCurrentWeight(GameWorld gameWorld)
    {
        int totalWeight = 0;

        // Calculate item weight
        foreach (string itemName in gameWorld.PlayerInventory.ItemSlots)
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
        totalWeight += gameWorld.PlayerCoins / 10;

        return totalWeight;
    }


    public int CalculateStaminaCost(RouteOption route)
    {
        int totalWeight = CalculateCurrentWeight(gameWorld);
        int adjustedStaminaCost = route.BaseStaminaCost;

        // Apply weight penalties
        if (totalWeight >= 4 && totalWeight <= 6)
        {
            adjustedStaminaCost += 1;
        }
        else if (totalWeight >= 7)
        {
            adjustedStaminaCost += 2;
        }

        return adjustedStaminaCost;
    }

    public bool CanTravel(RouteOption route)
    {
        bool canTravel = gameWorld.PlayerCoins >= route.BaseCoinCost &&
               gameWorld.PlayerStamina >= CalculateStaminaCost(route);

        return canTravel;
    }

}

