public class TravelManager
{
    public GameState gameState { get; }
    public WorldState worldState { get; }
    public LocationSystem LocationSystem { get; }
    public ActionRepository ActionRepository { get; }
    public LocationRepository LocationRepository { get; }
    public ActionFactory ActionFactory { get; }

    public TravelManager(
        GameState gameState,
        LocationSystem locationSystem,
        ActionRepository actionRepository,
        LocationRepository locationRepository,
        ActionFactory actionFactory
        )
    {
        this.gameState = gameState;
        this.worldState = gameState.WorldState;
        this.LocationSystem = locationSystem;
        this.ActionRepository = actionRepository;
        LocationRepository = locationRepository;
        this.ActionFactory = actionFactory;
    }

    public ActionImplementation StartLocationTravel(
        string travelLocationId,
        TravelMethods travelMethod = TravelMethods.Walking)
    {
        Location currentLocation = worldState.CurrentLocation;

        List<LocationSpot> locationSpots = LocationRepository
            .GetSpotsForLocation(travelLocationId);

        LocationSpot? locationSpot = locationSpots
            .FirstOrDefault(ls =>
            {
                return !ls.IsClosed;
            });

        if (locationSpot == null)
        {
            locationSpot = locationSpots.FirstOrDefault();
        }

        if (currentLocation == null)
        {
            EndLocationTravel(travelLocationId, locationSpot.Id);
            return null;
        }
        else
        {
            int travelMinutes = CalculateTravelTime(currentLocation, travelLocationId, travelMethod);
            int travelHours = (int)Math.Ceiling(travelMinutes / 60.0);

            ConsumeTravelResources(travelMinutes, travelMethod);

            ActionDefinition travelTemplate =
                GetTravelTemplate(travelLocationId, locationSpot.Id);

            ActionImplementation travelAction =
                ActionFactory.CreateActionFromTemplate(travelTemplate, currentLocation.Id, string.Empty);

            return travelAction;
        }
    }

    public void EndLocationTravel(string travelLocation, string locationSpotName)
    {
        Location targetLocation = LocationSystem.GetLocation(travelLocation);

        List<LocationSpot> spots = LocationSystem.GetLocationSpots(targetLocation.Id);
        LocationSpot? locSpot = spots.FirstOrDefault((Func<LocationSpot, bool>)(ls =>
        {
            return ls.Id == locationSpotName;
        }));

        worldState.SetCurrentLocation(targetLocation, locSpot);

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

    private ActionDefinition GetTravelTemplate(string location, string locationSpotId)
    {
        ActionDefinition travelTemplate = new ActionDefinition("travel", "travel", locationSpotId)
        {
            MoveToLocation = location,
            MoveToLocationSpot = locationSpotId,
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
        string currentLocation = worldState.CurrentLocation?.Id;
        if (string.IsNullOrWhiteSpace(currentLocation)) return;

        // Base energy cost (1 energy per 30 minutes)
        int baseCost = Math.Max(1, travelMinutes / 30);

        // Method modifier (different travel methods have different efficiency)
        double methodMultiplier = travelMethod switch
        {
            TravelMethods.Walking => 1.0,
            TravelMethods.Horseback => 0.7,
            TravelMethods.Carriage => 0.5,
            _ => 1.0
        };

        // Calculate final energy cost
        int energyCost = (int)Math.Ceiling(baseCost * methodMultiplier);
        energyCost = Math.Max(1, energyCost); // Always cost at least 1 energy

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

        if (LocationRepository.GetAllLocations().Any(x =>
        {
            return x.Id == endLocationId;
        }))
        {
            Location location = LocationRepository.GetLocationById(endLocationId);
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

