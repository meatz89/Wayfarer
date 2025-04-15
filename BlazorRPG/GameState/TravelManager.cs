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

        // Set up pending travel
        gameState.PendingTravel = new PendingTravel
        {
            TravelOrigin = currentLocation,
            TravelDestination = travelLocation,
            TravelMethod = travelMethod
        };

        // Calculate travel time
        int travelMinutes = CalculateTravelTime(currentLocation, travelLocation, travelMethod);

        // Advance game time
        worldState.CurrentTimeMinutes += travelMinutes;

        bool isFirstVisit = worldState.IsFirstVisit(travelLocation);

        // Consume resources
        ConsumeTravelResources(travelMinutes, travelMethod);

        // Apply discovery bonus on first visit
        if (isFirstVisit)
        {
            Location targetLocation = LocationSystem.GetLocation(travelLocation);
            if (targetLocation != null)
            {
                ApplyDiscoveryBonus(targetLocation);
            }
        }

        // Create appropriate action based on whether this is a first visit
        SpotAction travelTemplate = isFirstVisit
            ? GetTravelWithEncounterTemplate()
            : GetTravelWithoutEncounterTemplate();

        // Create action implementation
        ActionImplementation travelAction = isFirstVisit
            ? ActionFactory.CreateActionFromTemplate(
                travelTemplate,
                ActionRepository.GetEncounterTemplate(travelTemplate.Name))
            : ActionFactory.CreateActionFromTemplate(travelTemplate);

        return travelAction;
    }

    private SpotAction GetTravelWithoutEncounterTemplate()
    {
        SpotAction travelTemplate = new SpotAction
        {
            Name = "TravelSafe",
            ActionType = ActionTypes.Basic,
            BasicActionType = BasicActionTypes.Travel,
            Goal = "Travel safely to your destination",
            IsRepeatable = true,
            Costs = new()
            {
                new EnergyOutcome(-1)
                {
                    Amount = -1
                }
            },
        };

        return travelTemplate;
    }

    private SpotAction GetTravelWithEncounterTemplate()
    {
        SpotAction travelTemplate = new SpotAction
        {
            Name = "Travel",
            EncounterId = "Travel",
            ActionType = ActionTypes.Encounter,
            BasicActionType = BasicActionTypes.Travel,
            Goal = "Travel dangerously to your destination",
            IsRepeatable = false,
            Costs = new()
            {
                new EnergyOutcome(-1)
                {
                    Amount = -1
                }
            },
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
                                                             // TODO: Implement food consumption when available
        }
    }

    public int CalculateTravelTime(Location startLocation, string endLocationId, TravelMethods travelMethod)
    {
        WorldState worldState = gameState.WorldState;

        // Get base travel time between locations
        int baseTravelMinutes = 0;

        if (worldState.GetLocations().Any(x => x.Name == endLocationId))
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

