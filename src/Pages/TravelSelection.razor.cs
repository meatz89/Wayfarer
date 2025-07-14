using Microsoft.AspNetCore.Components;

public class TravelSelectionBase : ComponentBase
{
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameWorldManager { get; set; }
    // ItemRepository allowed for read-only UI data binding
    [Inject] public ItemRepository ItemRepository { get; set; }
    [Parameter] public Location CurrentLocation { get; set; }
    [Parameter] public List<Location> Locations { get; set; }
    [Parameter] public EventCallback<string> OnTravel { get; set; }
    [Parameter] public EventCallback<RouteOption> OnTravelRoute { get; set; }
    [Parameter] public EventCallback<(RouteOption route, TravelMethods transport)> OnTravelWithTransport { get; set; }

    public bool ShowEquipmentCategories
    {
        get
        {
            return GameWorld.GetPlayer().Inventory.ItemSlots
        .Select(name => ItemRepository.GetItemByName(name))
        .Any(item => item != null && item.Categories.Any());
        }
    }

    public WeatherCondition CurrentWeather
    {
        get
        {
            return GameWorld.CurrentWeather;
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    public List<Location> GetTravelableLocations()
    {
        return Locations?.Where(loc => loc.Id != CurrentLocation?.Id &&
                              GameWorldManager.CanTravelTo(loc.Id)).ToList() ?? new List<Location>();
    }


    public void ShowRouteOptions(GameWorld gameWorld, string destinationName, string destinationId)
    {
        List<RouteOption> availableRoutes = GameWorldManager.GetAvailableRoutes(CurrentLocation.Id, destinationId);

        Console.WriteLine($"=== Travel to {destinationName} ===");

        if (availableRoutes.Count == 0)
        {
            Console.WriteLine("No routes available at this time.");
            return;
        }

        int totalWeight = GameWorldManager.CalculateTotalWeight();
        string weightStatus = totalWeight <= 3 ? "Light load" :
                                (totalWeight <= 6 ? "Medium load (+1 stamina cost)" : "Heavy load (+2 stamina cost)");

        Console.WriteLine($"Current load: {weightStatus} ({totalWeight} weight units)");
        Console.WriteLine();

        Console.WriteLine("How do you want to travel?");

        for (int i = 0; i < availableRoutes.Count; i++)
        {
            RouteOption route = availableRoutes[i];

            // Calculate adjusted stamina cost
            int adjustedStaminaCost = route.BaseStaminaCost;
            if (totalWeight >= 4 && totalWeight <= 6)
            {
                adjustedStaminaCost += 1;
            }
            else if (totalWeight >= 7)
            {
                adjustedStaminaCost += 2;
            }

            string staminaCostDisplay = route.BaseStaminaCost == adjustedStaminaCost ?
                $"{adjustedStaminaCost} stamina" :
                $"{adjustedStaminaCost} stamina (base {route.BaseStaminaCost} + weight penalty)";

            string departureInfo = route.DepartureTime.HasValue ? $", departs at {route.DepartureTime}" : "";
            string affordabilityMarker = GameWorldManager.CanTravelRoute(route) ? "" : " (Cannot afford)";

            Console.WriteLine($"[{i + 1}] {route.Name} - {route.BaseCoinCost} coins, {staminaCostDisplay}{departureInfo}{affordabilityMarker}");

            // Show terrain categories if any
            if (route.TerrainCategories.Count > 0)
            {
                Console.WriteLine($"    Terrain: {string.Join(", ", route.TerrainCategories.Select(c => c.ToString().Replace('_', ' ')))}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Current resources: {gameWorld.PlayerCoins} coins, {gameWorld.PlayerStamina} stamina");

        // Show equipment categories
        List<string> equipmentItems = new List<string>();
        foreach (string itemName in gameWorld.PlayerInventory.ItemSlots)
        {
            if (itemName != null)
            {
                Item item = ItemRepository.GetItemByName(itemName);
                if (item != null && item.Categories.Count > 0)
                {
                    equipmentItems.Add($"{itemName} ({string.Join(", ", item.Categories.Select(c => c.ToString().Replace('_', ' ')))})");
                }
            }
        }

        if (equipmentItems.Count > 0)
        {
            Console.WriteLine("Equipment categories:");
            foreach (string itemInfo in equipmentItems)
            {
                Console.WriteLine($"- {itemInfo}");
            }
        }
    }


    /// <summary>
    /// Get equipment categories that are absolutely required for terrain types
    /// </summary>
    public List<EquipmentCategory> GetRequiredEquipment(List<TerrainCategory> terrainCategories)
    {
        List<EquipmentCategory> required = new List<EquipmentCategory>();

        foreach (TerrainCategory terrain in terrainCategories)
        {
            switch (terrain)
            {
                case TerrainCategory.Requires_Climbing:
                    required.Add(EquipmentCategory.Climbing_Equipment);
                    break;
                case TerrainCategory.Requires_Water_Transport:
                    required.Add(EquipmentCategory.Water_Transport);
                    break;
                case TerrainCategory.Requires_Permission:
                    required.Add(EquipmentCategory.Special_Access);
                    break;
            }
        }

        return required.Distinct().ToList();
    }

    /// <summary>
    /// Get equipment categories that are recommended for terrain types
    /// </summary>
    public List<EquipmentCategory> GetRecommendedEquipment(List<TerrainCategory> terrainCategories)
    {
        List<EquipmentCategory> recommended = new List<EquipmentCategory>();

        foreach (TerrainCategory terrain in terrainCategories)
        {
            switch (terrain)
            {
                case TerrainCategory.Wilderness_Terrain:
                    recommended.Add(EquipmentCategory.Navigation_Tools);
                    break;
                case TerrainCategory.Exposed_Weather:
                    recommended.Add(EquipmentCategory.Weather_Protection);
                    break;
                case TerrainCategory.Heavy_Cargo_Route:
                    recommended.Add(EquipmentCategory.Load_Distribution);
                    break;
                case TerrainCategory.Dark_Passage:
                    recommended.Add(EquipmentCategory.Light_Source);
                    break;
            }
        }

        return recommended.Distinct().ToList();
    }

    public List<string> GetWeatherTerrainEffects(List<TerrainCategory> terrainCategories, WeatherCondition weather)
    {
        List<string> effects = new List<string>();

        foreach (TerrainCategory terrain in terrainCategories)
        {
            switch (weather)
            {
                case WeatherCondition.Rain:
                    if (terrain == TerrainCategory.Exposed_Weather)
                        effects.Add("☔ Exposed terrain unsafe in rain - requires weather protection");
                    break;

                case WeatherCondition.Snow:
                    if (terrain == TerrainCategory.Exposed_Weather)
                        effects.Add("❄️ Exposed terrain impassable in snow - requires weather protection");
                    if (terrain == TerrainCategory.Wilderness_Terrain)
                        effects.Add("🌨️ Wilderness routes dangerous in snow - requires navigation tools");
                    break;

                case WeatherCondition.Fog:
                    if (terrain == TerrainCategory.Wilderness_Terrain)
                        effects.Add("🌫️ Cannot navigate wilderness in fog - requires navigation tools");
                    break;

                case WeatherCondition.Clear:
                    // No special effects for clear weather
                    break;
            }
        }

        return effects.Distinct().ToList();
    }

    public string GetWeatherIcon(WeatherCondition weather)
    {
        return weather switch
        {
            WeatherCondition.Clear => "☀️",
            WeatherCondition.Rain => "🌧️",
            WeatherCondition.Snow => "❄️",
            WeatherCondition.Fog => "🌫️",
            _ => "❓"
        };
    }

    /// <summary>
    /// Get all equipment categories currently owned by the player
    /// </summary>
    public List<EquipmentCategory> GetCurrentEquipmentCategories()
    {
        List<EquipmentCategory> ownedCategories = new List<EquipmentCategory>();

        foreach (string itemName in GameWorld.GetPlayer().Inventory.ItemSlots)
        {
            if (itemName != null)
            {
                Item item = ItemRepository.GetItemByName(itemName);
                if (item != null)
                {
                    ownedCategories.AddRange(item.Categories);
                }
            }
        }

        return ownedCategories.Distinct().ToList();
    }

    /// <summary>
    /// Analyze route accessibility and equipment requirements for strategic planning
    /// </summary>
    public RouteStrategicAnalysis AnalyzeRouteAccessibility()
    {
        RouteStrategicAnalysis analysis = new RouteStrategicAnalysis();
        List<EquipmentCategory> currentEquipment = GetCurrentEquipmentCategories();
        List<RouteOption> allRoutes = new List<RouteOption>();

        // Collect all routes from all destinations
        foreach (Location location in GetTravelableLocations())
        {
            List<RouteOption> routes = GameWorldManager.GetAvailableRoutes(CurrentLocation.Id, location.Id);
            allRoutes.AddRange(routes);
        }

        foreach (RouteOption route in allRoutes)
        {
            List<EquipmentCategory> requiredEquipment = GetRequiredEquipment(route.TerrainCategories);
            List<EquipmentCategory> recommendedEquipment = GetRecommendedEquipment(route.TerrainCategories);
            List<string> weatherEffects = GetWeatherTerrainEffects(route.TerrainCategories, CurrentWeather);

            // Check if route is accessible with current equipment
            bool hasAllRequired = requiredEquipment.All(req => currentEquipment.Contains(req));
            bool hasWeatherProtection = !weatherEffects.Any() ||
                                      weatherEffects.All(effect => HasEquipmentForWeatherEffect(effect, currentEquipment));

            RouteAccessibilityStatus routeStatus = new RouteAccessibilityStatus
            {
                Route = route,
                IsAccessibleNow = hasAllRequired && hasWeatherProtection,
                MissingRequiredEquipment = requiredEquipment.Where(req => !currentEquipment.Contains(req)).ToList(),
                MissingRecommendedEquipment = recommendedEquipment.Where(rec => !currentEquipment.Contains(rec)).ToList(),
                WeatherBlocked = weatherEffects.Any() && !hasWeatherProtection
            };

            if (routeStatus.IsAccessibleNow)
                analysis.AccessibleRoutes.Add(routeStatus);
            else
                analysis.BlockedRoutes.Add(routeStatus);
        }

        // Calculate equipment investment opportunities
        analysis.EquipmentInvestmentOpportunities = CalculateEquipmentInvestmentOpportunities(analysis.BlockedRoutes);

        return analysis;
    }

    private bool HasEquipmentForWeatherEffect(string weatherEffect, List<EquipmentCategory> currentEquipment)
    {
        if (weatherEffect.Contains("weather protection"))
            return currentEquipment.Contains(EquipmentCategory.Weather_Protection);
        if (weatherEffect.Contains("navigation tools"))
            return currentEquipment.Contains(EquipmentCategory.Navigation_Tools);

        return true; // Unknown effects assumed handled
    }

    private List<EquipmentInvestmentOpportunity> CalculateEquipmentInvestmentOpportunities(List<RouteAccessibilityStatus> blockedRoutes)
    {
        List<EquipmentInvestmentOpportunity> opportunities = new List<EquipmentInvestmentOpportunity>();
        Dictionary<EquipmentCategory, int> equipmentCounts = new Dictionary<EquipmentCategory, int>();

        // Count how many routes each missing equipment would unlock
        foreach (RouteAccessibilityStatus routeStatus in blockedRoutes)
        {
            foreach (EquipmentCategory missingEquipment in routeStatus.MissingRequiredEquipment)
            {
                if (!equipmentCounts.ContainsKey(missingEquipment))
                    equipmentCounts[missingEquipment] = 0;
                equipmentCounts[missingEquipment]++;
            }
        }

        // Create investment opportunities for equipment that unlocks multiple routes
        foreach (KeyValuePair<EquipmentCategory, int> kvp in equipmentCounts.Where(x => x.Value > 0))
        {
            List<string> blockedRoutesUnlocked = blockedRoutes
                .Where(r => r.MissingRequiredEquipment.Contains(kvp.Key))
                .Select(r => r.Route.Name)
                .ToList();

            opportunities.Add(new EquipmentInvestmentOpportunity
            {
                Equipment = kvp.Key,
                RoutesUnlocked = kvp.Value,
                RouteNames = blockedRoutesUnlocked
            });
        }

        return opportunities.OrderByDescending(o => o.RoutesUnlocked).ToList();
    }
}

// Supporting classes for strategic analysis
public class RouteStrategicAnalysis
{
    public List<RouteAccessibilityStatus> AccessibleRoutes { get; set; } = new();
    public List<RouteAccessibilityStatus> BlockedRoutes { get; set; } = new();
    public List<EquipmentInvestmentOpportunity> EquipmentInvestmentOpportunities { get; set; } = new();
}

public class RouteAccessibilityStatus
{
    public RouteOption Route { get; set; }
    public bool IsAccessibleNow { get; set; }
    public List<EquipmentCategory> MissingRequiredEquipment { get; set; } = new();
    public List<EquipmentCategory> MissingRecommendedEquipment { get; set; } = new();
    public bool WeatherBlocked { get; set; }
}

public class EquipmentInvestmentOpportunity
{
    public EquipmentCategory Equipment { get; set; }
    public int RoutesUnlocked { get; set; }
    public List<string> RouteNames { get; set; } = new();
}
