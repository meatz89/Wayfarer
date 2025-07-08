public class TravelManager
{
    private readonly GameWorld _gameWorld;
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
        Location currentLocation = _gameWorld.WorldState.CurrentLocation;

        if (destination == null || currentLocation == null)
            return false;

        // Check if any route exists and is available
        List<RouteOption> routes = GetAvailableRoutes(currentLocation.Id, destination.Id);
        return routes.Any();
    }

    public RouteOption StartLocationTravel(string locationId, TravelMethods method = TravelMethods.Walking)
    {
        Location destination = LocationRepository.GetLocation(locationId);
        Location currentLocation = _gameWorld.WorldState.CurrentLocation;

        // Find the appropriate route

        List<RouteOption> routes = GetAvailableRoutes(currentLocation.Id, destination.Id);
        RouteOption? selectedRoute = routes.FirstOrDefault(r => r.Method == method);
        
        if (selectedRoute == null) return null;

        return selectedRoute;
    }
    
    public void TravelToLocation(string travelLocation, string locationSpotName, RouteOption selectedRoute)
    {
        int totalWeight = CalculateCurrentWeight(_gameWorld);
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
        _gameWorld.GetPlayer().ModifyCoins(-selectedRoute.BaseCoinCost);
        _gameWorld.GetPlayer().SpendStamina(adjustedStaminaCost);

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

        string? currentLocation = _gameWorld.WorldState.CurrentLocation?.Id;

        bool isFirstVisit = _gameWorld.WorldState.IsFirstVisit(targetLocation.Id);
        if (isFirstVisit)
        {
            _gameWorld.WorldState.RecordLocationVisit(targetLocation.Id);
            if (targetLocation != null)
            {
                ApplyDiscoveryBonus(targetLocation);
            }
        }
    }

    private void AdvanceTimeBlocks(int timeBlockCost)
    {
        // TODO: Implement time advancement through TimeManager
    }

    private void ApplyDiscoveryBonus(Location location)
    {
        // Only apply bonus if location has values set
        if (location.DiscoveryBonusXP > 0 || location.DiscoveryBonusCoins > 0)
        {
            // Award XP
            int xpBonus = location.DiscoveryBonusXP;
            _gameWorld.GetPlayer().AddExperiencePoints(xpBonus);

            // Award coins
            int coinBonus = location.DiscoveryBonusCoins;
            if (coinBonus > 0)
            {
                _gameWorld.GetPlayer().AddCoins(coinBonus);
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
            if (route.DepartureTime != null && route.DepartureTime > _gameWorld.WorldState.CurrentTimeWindow)
                continue;

            // Check if player has required items for this route type
            bool hasRequiredItems = true;
            foreach (string requiredRouteType in route.RequiredRouteTypes)
            {
                // Check if any inventory item enables this route type
                bool routeTypeEnabled = false;
                foreach (string itemName in _gameWorld.GetPlayer().Inventory.ItemSlots)
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
        int staminaCost = route.CalculateWeightAdjustedStaminaCost(totalWeight);
        return staminaCost;
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
        bool canTravel = _gameWorld.GetPlayer().Coins >= route.BaseCoinCost &&
               _gameWorld.GetPlayer().Stamina >= CalculateStaminaCost(route);

        return canTravel;
    }

    /// <summary>
    /// Gets comprehensive comparison data for all available routes between two locations.
    /// Includes cost-benefit analysis, efficiency scoring, and resource requirements.
    /// </summary>
    public List<RouteComparisonData> GetRouteComparisonData(string fromLocationId, string toLocationId)
    {
        List<RouteComparisonData> comparisonData = new List<RouteComparisonData>();
        List<RouteOption> routes = GetAvailableRoutes(fromLocationId, toLocationId);
        
        if (routes.Count == 0)
        {
            return comparisonData;
        }

        int currentWeight = CalculateCurrentWeight(_gameWorld);
        
        foreach (RouteOption route in routes)
        {
            RouteComparisonData comparison = new RouteComparisonData(route);
            
            // Calculate costs
            comparison.AdjustedStaminaCost = route.CalculateWeightAdjustedStaminaCost(currentWeight);
            comparison.WeightPenalty = comparison.AdjustedStaminaCost - route.BaseStaminaCost;
            comparison.TotalCost = route.BaseCoinCost + comparison.AdjustedStaminaCost;
            
            // Calculate efficiency score (lower is better for costs, higher is better for efficiency)
            double timeEfficiency = 5.0 / Math.Max(route.TimeBlockCost, 1);
            double costEfficiency = 20.0 / Math.Max(comparison.TotalCost, 1);
            comparison.EfficiencyScore = (timeEfficiency + costEfficiency) / 2.0;
            
            // Check affordability
            comparison.CanAfford = _gameWorld.GetPlayer().Coins >= route.BaseCoinCost && 
                                   _gameWorld.GetPlayer().Stamina >= comparison.AdjustedStaminaCost;
            
            // Calculate arrival time
            comparison.ArrivalTime = CalculateArrivalTimeDisplay(_gameWorld.WorldState.CurrentTimeWindow, route.TimeBlockCost);
            
            // Generate cost-benefit analysis
            comparison.CostBenefitAnalysis = GenerateCostBenefitAnalysis(route, comparison, currentWeight);
            
            // Generate resource breakdown
            comparison.ResourceBreakdown = $"Coins: {route.BaseCoinCost}, Stamina: {comparison.AdjustedStaminaCost}, Time: {route.TimeBlockCost} blocks";
            
            // Generate recommendation
            comparison.Recommendation = GenerateRouteRecommendation(route, comparison);
            
            comparisonData.Add(comparison);
        }
        
        // Sort by efficiency score (descending - higher is better)
        comparisonData.Sort((a, b) => b.EfficiencyScore.CompareTo(a.EfficiencyScore));
        
        return comparisonData;
    }

    /// <summary>
    /// Gets an optimal route recommendation based on player resources and optimization strategy.
    /// </summary>
    public RouteRecommendation GetOptimalRouteRecommendation(string fromLocationId, string toLocationId, OptimizationStrategy strategy)
    {
        List<RouteOption> routes = GetAvailableRoutes(fromLocationId, toLocationId);
        
        if (routes.Count == 0)
        {
            return null;
        }

        RouteOption optimalRoute = null;
        string justification = "";
        double efficiencyScore = 0;
        
        switch (strategy)
        {
            case OptimizationStrategy.Efficiency:
                optimalRoute = FindMostEfficientRoute(routes, out efficiencyScore);
                justification = "Recommended for optimal balance of cost, time, and stamina efficiency.";
                break;
                
            case OptimizationStrategy.CheapestCost:
                optimalRoute = FindCheapestRoute(routes);
                justification = "Recommended as the most affordable option that conserves coins.";
                efficiencyScore = CalculateRouteEfficiency(optimalRoute);
                break;
                
            case OptimizationStrategy.LeastStamina:
                optimalRoute = FindLeastStaminaRoute(routes);
                justification = "Recommended to conserve stamina and maintain energy for other activities.";
                efficiencyScore = CalculateRouteEfficiency(optimalRoute);
                break;
                
            case OptimizationStrategy.FastestTime:
                optimalRoute = FindFastestRoute(routes);
                justification = "Recommended for fastest arrival time to maximize remaining day.";
                efficiencyScore = CalculateRouteEfficiency(optimalRoute);
                break;
                
            default:
                optimalRoute = routes.FirstOrDefault();
                justification = "Default route recommendation.";
                efficiencyScore = CalculateRouteEfficiency(optimalRoute);
                break;
        }
        
        if (optimalRoute == null)
        {
            return null;
        }
        
        RouteRecommendation recommendation = new RouteRecommendation(optimalRoute, strategy);
        recommendation.Justification = justification;
        recommendation.EfficiencyScore = efficiencyScore;
        recommendation.ResourceAnalysis = GenerateResourceAnalysis(optimalRoute);
        recommendation.AlternativeOptions = GenerateAlternativeOptions(routes, optimalRoute);
        
        return recommendation;
    }

    /// <summary>
    /// Calculates the arrival time display string for a given route
    /// </summary>
    private string CalculateArrivalTimeDisplay(TimeBlocks currentTime, int timeBlockCost)
    {
        int finalTimeBlockValue = ((int)currentTime + timeBlockCost) % 5;
        int daysLater = ((int)currentTime + timeBlockCost) / 5;

        TimeBlocks arrivalTimeBlock = (TimeBlocks)finalTimeBlockValue;

        if (daysLater == 0)
        {
            return arrivalTimeBlock.ToString();
        }
        else if (daysLater == 1)
        {
            return $"Tomorrow {arrivalTimeBlock}";
        }
        else
        {
            return $"{daysLater} days later, {arrivalTimeBlock}";
        }
    }

    /// <summary>
    /// Generates cost-benefit analysis text for a route
    /// </summary>
    private string GenerateCostBenefitAnalysis(RouteOption route, RouteComparisonData comparison, int currentWeight)
    {
        string analysis = $"{route.Name}: ";
        
        if (route.BaseCoinCost == 0)
        {
            analysis += "Free travel, ";
        }
        else
        {
            analysis += $"{route.BaseCoinCost} coins, ";
        }
        
        if (comparison.WeightPenalty > 0)
        {
            analysis += $"{comparison.AdjustedStaminaCost} stamina (includes +{comparison.WeightPenalty} weight penalty), ";
        }
        else
        {
            analysis += $"{comparison.AdjustedStaminaCost} stamina, ";
        }
        
        analysis += $"{route.TimeBlockCost} time blocks. ";
        
        if (comparison.EfficiencyScore > 3.0)
        {
            analysis += "Excellent efficiency.";
        }
        else if (comparison.EfficiencyScore > 2.0)
        {
            analysis += "Good efficiency.";
        }
        else if (comparison.EfficiencyScore > 1.0)
        {
            analysis += "Moderate efficiency.";
        }
        else
        {
            analysis += "Low efficiency.";
        }
        
        return analysis;
    }

    /// <summary>
    /// Generates a recommendation text for a specific route
    /// </summary>
    private string GenerateRouteRecommendation(RouteOption route, RouteComparisonData comparison)
    {
        if (!comparison.CanAfford)
        {
            return "Cannot afford - insufficient coins or stamina.";
        }
        
        if (comparison.EfficiencyScore > 3.0)
        {
            return "Highly recommended - excellent value.";
        }
        else if (comparison.EfficiencyScore > 2.0)
        {
            return "Recommended - good balance of cost and speed.";
        }
        else if (route.BaseCoinCost == 0)
        {
            return "Budget option - free but slower.";
        }
        else
        {
            return "Consider alternatives if available.";
        }
    }

    /// <summary>
    /// Finds the most efficient route based on overall efficiency score
    /// </summary>
    private RouteOption FindMostEfficientRoute(List<RouteOption> routes, out double bestEfficiency)
    {
        RouteOption bestRoute = null;
        bestEfficiency = 0;
        
        foreach (RouteOption route in routes)
        {
            double efficiency = CalculateRouteEfficiency(route);
            if (efficiency > bestEfficiency && CanTravel(route))
            {
                bestEfficiency = efficiency;
                bestRoute = route;
            }
        }
        
        // If no affordable routes, return the most efficient affordable one
        if (bestRoute == null)
        {
            bestRoute = routes.FirstOrDefault();
            bestEfficiency = CalculateRouteEfficiency(bestRoute);
        }
        
        return bestRoute;
    }

    /// <summary>
    /// Finds the cheapest route by coin cost
    /// </summary>
    private RouteOption FindCheapestRoute(List<RouteOption> routes)
    {
        return routes.Where(r => CanTravel(r))
                    .OrderBy(r => r.BaseCoinCost)
                    .FirstOrDefault() ?? routes.OrderBy(r => r.BaseCoinCost).FirstOrDefault();
    }

    /// <summary>
    /// Finds the route with least stamina cost
    /// </summary>
    private RouteOption FindLeastStaminaRoute(List<RouteOption> routes)
    {
        return routes.Where(r => CanTravel(r))
                    .OrderBy(r => CalculateStaminaCost(r))
                    .FirstOrDefault() ?? routes.OrderBy(r => CalculateStaminaCost(r)).FirstOrDefault();
    }

    /// <summary>
    /// Finds the fastest route by time cost
    /// </summary>
    private RouteOption FindFastestRoute(List<RouteOption> routes)
    {
        return routes.Where(r => CanTravel(r))
                    .OrderBy(r => r.TimeBlockCost)
                    .FirstOrDefault() ?? routes.OrderBy(r => r.TimeBlockCost).FirstOrDefault();
    }

    /// <summary>
    /// Calculates efficiency score for a route
    /// </summary>
    private double CalculateRouteEfficiency(RouteOption route)
    {
        if (route == null) return 0;
        
        int totalCost = route.BaseCoinCost + CalculateStaminaCost(route);
        double timeEfficiency = 5.0 / Math.Max(route.TimeBlockCost, 1);
        double costEfficiency = 20.0 / Math.Max(totalCost, 1);
        return (timeEfficiency + costEfficiency) / 2.0;
    }

    /// <summary>
    /// Generates resource analysis for a route recommendation
    /// </summary>
    private string GenerateResourceAnalysis(RouteOption route)
    {
        if (route == null) return "";
        
        int staminaCost = CalculateStaminaCost(route);
        return $"This route requires {route.BaseCoinCost} coins and {staminaCost} stamina, " +
               $"taking {route.TimeBlockCost} time blocks to complete.";
    }

    /// <summary>
    /// Generates alternative options text for a route recommendation
    /// </summary>
    private string GenerateAlternativeOptions(List<RouteOption> allRoutes, RouteOption selectedRoute)
    {
        List<RouteOption> alternatives = allRoutes.Where(r => r != selectedRoute).ToList();
        
        if (alternatives.Count == 0)
        {
            return "No alternative routes available.";
        }
        
        if (alternatives.Count == 1)
        {
            RouteOption alt = alternatives[0];
            return $"Alternative: {alt.Name} - {alt.BaseCoinCost} coins, {CalculateStaminaCost(alt)} stamina, {alt.TimeBlockCost} time blocks.";
        }
        
        return $"{alternatives.Count} alternative routes available with different cost/time trade-offs.";
    }

}

