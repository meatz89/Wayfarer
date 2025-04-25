public class TravelManager
{
    public GameState gameState { get; }
    public WorldState worldState { get; }
    public LocationSystem LocationSystem { get; }
    public ActionRepository ActionRepository { get; }
    public ActionFactory ActionFactory { get; }

    public TravelManager(
        GameState gameState,
        LocationSystem locationSystem,
        ActionRepository actionRepository,
        ActionFactory actionFactory
        )
    {
        this.gameState = gameState;
        this.worldState = gameState.WorldState;
        this.LocationSystem = locationSystem;
        this.ActionRepository = actionRepository;
        this.ActionFactory = actionFactory;
    }

    public ActionImplementation TravelToLocation(string travelLocation, TravelMethods travelMethod = TravelMethods.Walking)
    {
        Location currentLocation = worldState.CurrentLocation;

        if (currentLocation == null)
        {
            EndLocationTravel(travelLocation);
            return null;
        }
        else
        {
            int travelMinutes = CalculateTravelTime(currentLocation, travelLocation, travelMethod);
            int travelHours = (int)Math.Ceiling(travelMinutes / 60.0);

            ConsumeTravelResources(travelMinutes, travelMethod);

            ActionDefinition travelTemplate = GetTravelTemplate(travelLocation, string.Empty);
            ActionImplementation travelAction = ActionFactory.CreateActionFromTemplate(travelTemplate, currentLocation.Name, string.Empty);

            return travelAction;
        }
    }

    public void EndLocationTravel(string travelLocation)
    {
        Location targetLocation = LocationSystem.GetLocation(travelLocation);

        List<LocationSpot> spots = LocationSystem.GetLocationSpots(targetLocation.Name);
        LocationSpot firstSpot = spots.FirstOrDefault();

        worldState.SetCurrentLocation(targetLocation, firstSpot!);

        string? currentLocation = worldState.CurrentLocation?.Name;

        bool isFirstVisit = worldState.IsFirstVisit(targetLocation.Name);
        if (isFirstVisit)
        {
            worldState.RecordLocationVisit(targetLocation.Name);
            if (targetLocation != null)
            {
                ApplyDiscoveryBonus(targetLocation);
            }
        }
    }

    private ActionDefinition GetTravelTemplate(string location, string locationSpot)
    {
        ActionDefinition travelTemplate = new ActionDefinition("travel", "travel")
        {
            Goal = "Travel safely to your destination",
            MoveToLocation = location,
            MoveToLocationSpot = locationSpot,
            EnergyCost = 1,
        };

        return travelTemplate;
    }

    private void ApplyDiscoveryBonus(Location location)
    {
        // Only apply bonus if location has values set
        if (location.DiscoveryBonusXP > 0 || location.DiscoveryBonusCoins > 0)
        {
            // Award XP
            int xpBonus = location.DiscoveryBonusXP;
            gameState.PlayerState.AddExperiencePoints(xpBonus);

            // Award coins
            int coinBonus = location.DiscoveryBonusCoins;
            if (coinBonus > 0)
            {
                gameState.PlayerState.AddCoins(coinBonus);
            }
        }
    }

    private void ConsumeTravelResources(int travelMinutes, TravelMethods travelMethod)
    {
        // Calculate energy cost based on travel time and location depth
        string currentLocation = worldState.CurrentLocation?.Name;
        if (string.IsNullOrWhiteSpace(currentLocation)) return;

        int currentDepth = worldState.GetLocationDepth(currentLocation);

        // Base energy cost (1 energy per 30 minutes)
        int baseCost = Math.Max(1, travelMinutes / 30);

        // Depth scaling (higher depths cost more)
        double depthMultiplier = 1.0 + (currentDepth * 0.1);

        // Method modifier (different travel methods have different efficiency)
        double methodMultiplier = travelMethod switch
        {
            TravelMethods.Walking => 1.0,
            TravelMethods.Horseback => 0.7,
            TravelMethods.Carriage => 0.5,
            _ => 1.0
        };

        // Calculate final energy cost
        int energyCost = (int)Math.Ceiling(baseCost * depthMultiplier * methodMultiplier);
        energyCost = Math.Max(1, energyCost); // Always cost at least 1 energy

        // Apply energy cost
        gameState.PlayerState.ModifyEnergy(-energyCost);

        // Potentially apply food cost for longer journeys
        if (travelMinutes > 60) // Over an hour
        {
            int foodCost = Math.Max(1, travelMinutes / 120); // 1 food per 2 hours
        }
    }

    public int CalculateTravelTime(Location startLocation, string endLocationId, TravelMethods travelMethod)
    {
        WorldState worldState = gameState.WorldState;

        // Get base travel time between locations
        int baseTravelMinutes = 0;

        if (worldState.GetLocations().Any(x =>
        {
            return x.Name == endLocationId;
        }))
        {
            Location location = worldState.GetLocation(endLocationId);
            baseTravelMinutes = location.TravelTimeMinutes;
        }

        // Apply travel method modifier
        double modifier = GetTravelMethodSpeedModifier(travelMethod);

        // Calculate final travel time
        int travelMinutes = (int)(baseTravelMinutes / modifier);

        return travelMinutes;
    }

    public double GetTravelMethodSpeedModifier(TravelMethods travelMethod)
    {
        switch (travelMethod)
        {
            case TravelMethods.Walking: return 1.0;
            default: return 1.0;
        }
    }
}

