public class RouteAccessResult
{
    public bool IsAllowed { get; private set; }
    public string BlockingReason { get; private set; }
    public List<string> Warnings { get; private set; }

    private RouteAccessResult(bool isAllowed, string blockingReason, List<string> warnings)
    {
        IsAllowed = isAllowed;
        BlockingReason = blockingReason;
        Warnings = warnings;
    }

    public static RouteAccessResult Allowed()
    {
        return new RouteAccessResult(true, "", new List<string>());
    }

    public static RouteAccessResult Blocked(string reason)
    {
        return new RouteAccessResult(false, reason, new List<string>());
    }

    public static RouteAccessResult AllowedWithWarning(string warning)
    {
        return new RouteAccessResult(true, "", new List<string> { warning });
    }
}

public enum TerrainCategory
{
    // Hard requirements (completely blocks access without equipment)
    Requires_Climbing,
    Requires_Water_Transport,
    Requires_Permission,

    // Conditional requirements (weather-dependent or warning-based)
    Wilderness_Terrain,    // Dangerous without navigation tools, blocked in fog/snow
    Exposed_Weather,       // Blocked in bad weather without protection
    Heavy_Cargo_Route,     // Increased fatigue without proper equipment
    Dark_Passage          // Dangerous at night without light source
}

// Seasons removed - game timeframe is only days/weeks, not months/seasons

public class RouteModification
{
    public int StaminaCostModifier { get; set; } = 0;
    public int CoinCostModifier { get; set; } = 0;
    // Removed BlocksRoute - weather makes routes difficult, not impossible
}

public class RouteOption
{
    // HIGHLANDER: Name is natural key, NO Id property
    public string Name { get; set; }

    // HIGHLANDER: Object references ONLY, no ID properties (Pattern A DELETED per 08_crosscutting_concepts.md)
    public Location OriginLocation { get; set; }
    public Location DestinationLocation { get; set; }

    public TravelMethods Method { get; set; }
    public int BaseCoinCost { get; set; }
    public int BaseStaminaCost { get; set; }
    public int TravelTimeSegments { get; set; }
    public TimeBlocks? DepartureTime { get; set; }
    public List<TerrainCategory> TerrainCategories { get; set; } = new List<TerrainCategory>();
    public int MaxItemCapacity { get; set; } = 3;
    public string Description { get; set; }

    // Route condition variations - DOMAIN COLLECTION PRINCIPLE: Explicit properties for fixed enum
    public RouteModification ClearWeatherModification { get; set; }
    public RouteModification RainWeatherModification { get; set; }
    public RouteModification SnowWeatherModification { get; set; }
    public RouteModification FogWeatherModification { get; set; }
    public RouteModification StormWeatherModification { get; set; }

    public RouteModification GetWeatherModification(WeatherCondition weather) => weather switch
    {
        WeatherCondition.Clear => ClearWeatherModification,
        WeatherCondition.Rain => RainWeatherModification,
        WeatherCondition.Snow => SnowWeatherModification,
        WeatherCondition.Fog => FogWeatherModification,
        WeatherCondition.Storm => StormWeatherModification,
        _ => null
    };

    public RouteType RouteType { get; set; }

    // Track if this specific route has been unlocked via permit
    public bool HasPermitUnlock { get; set; } = false;

    // PATH CARD SYSTEM - Route segments with path card options
    public List<RouteSegment> Segments { get; set; } = new List<RouteSegment>();

    // NOTE: EncounterDeckIds DELETED - if encounters needed, store encounter deck objects or query from templates
    // Starting stamina for this route
    public int StartingStamina { get; set; } = 3;

    // NOTE: Old SceneIds property removed - NEW Scene-Situation architecture
    // Scenes now spawn via Situation spawn rewards (SceneSpawnReward) instead of Route ownership
    // Routes no longer directly own scenes - scenes are managed by Situation lifecycle

    // Localized mastery - ExplorationCubes reveal hidden path options on THIS route only
    // 0-10 scale: 0 cubes = only basic paths visible, 10 cubes = all optimal paths revealed
    public int ExplorationCubes { get; set; } = 0;

    // Route familiarity - tracks player's knowledge of this specific route (0-5 scale)
    public int Familiarity { get; set; } = 0;

    // HEX-BASED TRAVEL SYSTEM - Procedural route generation from spatial scaffolding
    /// <summary>
    /// Underlying hex path connecting origin to destination
    /// Procedurally generated via A* pathfinding through HexMap
    /// Source of truth for route terrain/danger properties
    /// Empty list for legacy manually-authored routes
    /// </summary>
    public List<AxialCoordinates> HexPath { get; set; } = new List<AxialCoordinates>();

    /// <summary>
    /// Total danger rating (0-100) summed from hex danger levels along HexPath
    /// Calculated from: Sum(hex.DangerLevel for hex in HexPath)
    /// Used for Scene template filtering and risk assessment
    /// 0 for legacy manually-authored routes
    /// </summary>
    public int DangerRating { get; set; } = 0;

    /// <summary>
    /// Get dominant terrain type from HexPath for Scene template filtering
    /// Returns first terrain category as string, or "Urban" if no categories
    /// PlacementFilter expects List of string terrain types, not enum
    /// </summary>
    public string GetDominantTerrainType()
    {
        if (TerrainCategories == null || TerrainCategories.Count == 0)
            return "Urban"; // Default for legacy routes without terrain

        // Convert first terrain category enum to string for filtering
        // Remove "Requires_" prefix to get base terrain type
        string firstTerrain = TerrainCategories[0].ToString();
        return firstTerrain.Replace("Requires_", "").Replace("_", " ");
    }

    public bool CanTravel(ItemRepository itemRepository, Player player, int totalFocus)
    {
        // Use logical access system instead of efficiency calculations
        RouteAccessResult accessResult = CheckRouteAccess(itemRepository, player, WeatherCondition.Clear);
        if (!accessResult.IsAllowed)
        {
            return false;
        }

        // Calculate realistic costs without efficiency modifiers
        int staminaCost = CalculateLogicalStaminaCost(totalFocus, player.Inventory.GetAllItems().Count(i => i != null));
        int coinCost = BaseCoinCost;

        // Check if player has enough resources
        if (player.Coins < coinCost)
        {
            return false;
        }

        if (player.Hunger + staminaCost > player.MaxHunger)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check logical route access based on equipment requirements and terrain
    /// Note: AccessRequirement checks are done separately in TravelManager for narrative messaging
    /// </summary>
    public RouteAccessResult CheckRouteAccess(ItemRepository itemRepository, Player player, WeatherCondition weather)
    {
        List<ItemCategory> playerEquipment = GetPlayerEquipmentCategories(itemRepository, player);
        List<string> warnings = new List<string>();

        // Check hard requirements first
        foreach (TerrainCategory terrain in TerrainCategories)
        {
            // Hard blocking requirements
            switch (terrain)
            {
                case TerrainCategory.Requires_Climbing:
                    if (!playerEquipment.Contains(ItemCategory.Climbing_Equipment))
                        return RouteAccessResult.Blocked("Steep terrain requires climbing equipment");
                    break;
                case TerrainCategory.Requires_Water_Transport:
                    if (!playerEquipment.Contains(ItemCategory.Water_Transport))
                        return RouteAccessResult.Blocked("River crossing requires water transport");
                    break;
                case TerrainCategory.Requires_Permission:
                    if (!playerEquipment.Contains(ItemCategory.Special_Access))
                        return RouteAccessResult.Blocked("Official route requires proper documentation");
                    break;
            }
        }

        // Check weather-terrain interactions
        foreach (TerrainCategory terrain in TerrainCategories)
        {
            RouteAccessResult weatherCheck = CheckWeatherTerrainInteraction(terrain, weather, playerEquipment);
            if (!weatherCheck.IsAllowed)
                return weatherCheck;

            warnings.AddRange(weatherCheck.Warnings);
        }

        // Check conditional requirements that create warnings
        foreach (TerrainCategory terrain in TerrainCategories)
        {
            switch (terrain)
            {
                case TerrainCategory.Wilderness_Terrain:
                    if (!playerEquipment.Contains(ItemCategory.Navigation_Tools))
                        warnings.Add("Wilderness route without navigation tools - risk of getting lost");
                    break;
                case TerrainCategory.Dark_Passage:
                    if (!playerEquipment.Contains(ItemCategory.Light_Source))
                        warnings.Add("Dark passage without light source - dangerous at night");
                    break;
                case TerrainCategory.Heavy_Cargo_Route:
                    if (!playerEquipment.Contains(ItemCategory.Load_Distribution))
                        warnings.Add("Heavy cargo route without proper equipment - increased fatigue");
                    break;
            }
        }

        return RouteAccessResult.Allowed();
    }

    /// <summary>
    /// Check weather and terrain interactions for logical blocking
    /// </summary>
    private RouteAccessResult CheckWeatherTerrainInteraction(TerrainCategory terrain, WeatherCondition weather, List<ItemCategory> playerEquipment)
    {
        switch (weather)
        {
            case WeatherCondition.Rain:
                if (terrain == TerrainCategory.Exposed_Weather && !playerEquipment.Contains(ItemCategory.Weather_Protection))
                    return RouteAccessResult.Blocked("Exposed terrain unsafe in rain without weather protection");
                break;

            case WeatherCondition.Snow:
                if (terrain == TerrainCategory.Exposed_Weather && !playerEquipment.Contains(ItemCategory.Weather_Protection))
                    return RouteAccessResult.Blocked("Exposed terrain impassable in snow without weather protection");
                if (terrain == TerrainCategory.Wilderness_Terrain && !playerEquipment.Contains(ItemCategory.Navigation_Tools))
                    return RouteAccessResult.Blocked("Wilderness routes too dangerous in snow without navigation tools");
                break;

            case WeatherCondition.Fog:
                if (terrain == TerrainCategory.Wilderness_Terrain && !playerEquipment.Contains(ItemCategory.Navigation_Tools))
                    return RouteAccessResult.Blocked("Cannot navigate wilderness in fog without proper tools");
                break;
        }

        return RouteAccessResult.Allowed();
    }

    public int GetActualStaminaCost()
    {
        return BaseStaminaCost;
    }

    public int GetActualTimeCost()
    {
        return TravelTimeSegments;
    }

    public int CalculateFocusAdjustedStaminaCost(int totalFocus)
    {
        int adjustedStaminaCost = BaseStaminaCost;

        // Apply focus penalties
        if (totalFocus > GameConstants.LoadFocus.LIGHT_LOAD_MAX && totalFocus <= GameConstants.LoadFocus.MEDIUM_LOAD_MAX)
        {
            adjustedStaminaCost += GameConstants.LoadFocus.MEDIUM_LOAD_HUNGER_INCREASE;
        }
        else if (totalFocus > GameConstants.LoadFocus.MEDIUM_LOAD_MAX)
        {
            adjustedStaminaCost += GameConstants.LoadFocus.HEAVY_LOAD_HUNGER_INCREASE;
        }

        return adjustedStaminaCost;
    }

    /// <summary>
    /// Calculate stamina cost based on logical factors - no efficiency multipliers
    /// </summary>
    public int CalculateStaminaCost(int totalFocus, WeatherCondition weather, ItemRepository itemRepository, Player player)
    {
        int itemCount = player.Inventory.GetAllItems().Count(i => i != null);
        int baseCost = CalculateLogicalStaminaCost(totalFocus, itemCount);

        // Apply weather modifications - DOMAIN COLLECTION PRINCIPLE: Use explicit properties
        RouteModification weatherMod = GetWeatherModification(weather);
        if (weatherMod != null)
        {
            baseCost += weatherMod.StaminaCostModifier;
        }

        return Math.Max(1, baseCost);
    }

    /// <summary>
    /// Calculate stamina cost using logical factors instead of efficiency multipliers
    /// </summary>
    private int CalculateLogicalStaminaCost(int totalFocus, int itemCount)
    {
        int staminaCost = BaseStaminaCost;

        // Physical focus penalties (realistic cargo limitations)
        if (totalFocus > GameConstants.LoadFocus.LIGHT_LOAD_MAX && totalFocus <= GameConstants.LoadFocus.MEDIUM_LOAD_MAX)
        {
            staminaCost += GameConstants.LoadFocus.MEDIUM_LOAD_HUNGER_INCREASE; // Moderate load
        }
        else if (totalFocus > GameConstants.LoadFocus.MEDIUM_LOAD_MAX)
        {
            staminaCost += GameConstants.LoadFocus.HEAVY_LOAD_HUNGER_INCREASE; // Heavy load
        }

        // Overload penalties (instead of blocking - player choice with consequences)
        if (itemCount > MaxItemCapacity)
        {
            int overload = itemCount - MaxItemCapacity;
            staminaCost += overload * GameConstants.LoadFocus.MEDIUM_LOAD_HUNGER_INCREASE; // +1 stamina per item over capacity
        }

        return Math.Max(1, staminaCost);
    }

    /// <summary>
    /// Calculate coin cost - weather does not affect cost
    /// </summary>
    public int CalculateCoinCost(WeatherCondition weather)
    {
        return BaseCoinCost;
    }

    // Season availability removed - game timeframe is only days/weeks

    /// <summary>
    /// Get all equipment categories from player's inventory
    /// </summary>
    private List<ItemCategory> GetPlayerEquipmentCategories(ItemRepository itemRepository, Player player)
    {
        List<ItemCategory> categories = new List<ItemCategory>();

        foreach (Item item in player.Inventory.GetAllItems())
        {
            if (item != null)
            {
                categories.AddRange(item.Categories);
            }
        }

        return categories.Distinct().ToList();
    }

}

