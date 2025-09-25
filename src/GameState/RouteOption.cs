
public enum WeatherCondition
{
    Clear,
    Rain,
    Snow,
    Fog,
    Storm
}

public class RouteAccessResult
{
    public bool IsAllowed { get; private set; }
    public string BlockingReason { get; private set; }
    public List<string> Warnings { get; private set; }

    private RouteAccessResult(bool isAllowed, string blockingReason = "", List<string> warnings = null)
    {
        IsAllowed = isAllowed;
        BlockingReason = blockingReason ?? "";
        Warnings = warnings ?? new List<string>();
    }

    public static RouteAccessResult Allowed(List<string> warnings = null)
    {
        return new RouteAccessResult(true, "", warnings);
    }

    public static RouteAccessResult Blocked(string reason)
    {
        return new RouteAccessResult(false, reason);
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

public class RouteUnlockCondition
{
    public string? RequiredRouteUsage { get; set; }
    public int RequiredUsageCount { get; set; }
    public string? RequiredItem { get; set; }
    public int RequiredPlayerLevel { get; set; }
}

public class RouteOption
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string OriginLocationSpot { get; set; }
    public string DestinationLocationSpot { get; set; }

    public TravelMethods Method { get; set; }
    public int BaseCoinCost { get; set; }
    public int BaseStaminaCost { get; set; }
    public int TravelTimeSegments { get; set; }
    public TimeBlocks? DepartureTime { get; set; }
    public bool IsDiscovered { get; set; } = true;
    public List<TerrainCategory> TerrainCategories { get; set; } = new List<TerrainCategory>();
    public int MaxItemCapacity { get; set; } = 3;
    public string Description { get; set; }

    // Route condition variations
    public Dictionary<WeatherCondition, RouteModification> WeatherModifications { get; set; } = new Dictionary<WeatherCondition, RouteModification>();
    public RouteUnlockCondition? UnlockCondition { get; set; }

    // Enhanced Access Requirements (in addition to terrain categories)
    public AccessRequirement AccessRequirement { get; set; }
    public RouteType RouteType { get; set; }

    // Track if this specific route has been unlocked via permit
    public bool HasPermitUnlock { get; set; } = false;

    // PATH CARD SYSTEM - Route segments with path card options
    public List<RouteSegment> Segments { get; set; } = new List<RouteSegment>();

    // Encounter deck for this route
    public List<string> EncounterDeckIds { get; set; } = new List<string>();

    // Starting stamina for this route
    public int StartingStamina { get; set; } = 3;

    public bool CanTravel(ItemRepository itemRepository, Player player, int totalFocus)
    {
        // Use logical access system instead of efficiency calculations
        RouteAccessResult accessResult = CheckRouteAccess(itemRepository, player, WeatherCondition.Clear);
        if (!accessResult.IsAllowed)
        {
            return false;
        }

        // Calculate realistic costs without efficiency modifiers
        int staminaCost = CalculateLogicalStaminaCost(totalFocus, player.Inventory.GetAllItems().Count(i => i != null && i != string.Empty));
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

        return warnings.Any() ? RouteAccessResult.Allowed(warnings) : RouteAccessResult.Allowed();
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
        int itemCount = player.Inventory.GetAllItems().Count(i => i != null && i != string.Empty);
        int baseCost = CalculateLogicalStaminaCost(totalFocus, itemCount);

        // Apply weather modifications
        if (WeatherModifications.TryGetValue(weather, out RouteModification? modification))
        {
            baseCost += modification.StaminaCostModifier;
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

        foreach (string itemId in player.Inventory.GetAllItems())
        {
            if (itemId != null && itemId != string.Empty)
            {
                Item item = itemRepository.GetItemById(itemId);
                if (item != null)
                {
                    categories.AddRange(item.Categories);
                }
            }
        }

        return categories.Distinct().ToList();
    }

}

