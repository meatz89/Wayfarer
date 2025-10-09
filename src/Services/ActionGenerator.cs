public class ActionGenerator
{
    private readonly TimeManager _timeManager;
    private readonly GameWorld _gameWorld;

    public ActionGenerator(
        TimeManager timeManager,
        GameWorld gameWorld)
    {
        _timeManager = timeManager;
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Generate actions for a location based on its data and current time
    /// </summary>
    public List<LocationActionViewModel> GenerateActionsForLocationSpot(LocationSpot locationSpot, LocationSpot spot)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

        // Generate spot-specific actions
        if (spot != null)
        {
            actions.AddRange(GenerateSpotActions(spot, currentTime));

            // Travel action is now defined in JSON with Crossroads property requirement
            // No need to hardcode it here - LocationActionManager will add it dynamically
        }

        // Generate time-based actions
        actions.AddRange(GenerateTimeBasedActions(locationSpot, currentTime));

        // Generate atmosphere-based actions
        actions.AddRange(GenerateAtmosphereActions(locationSpot));

        // Limit to 4-5 actions max (per mockup)
        return actions.ToList();
    }

    private List<LocationActionViewModel> GenerateServiceActions(ServiceTypes service, Location location, TimeBlocks currentTime)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();
        TierLevel playerTier = GetPlayerTier(_gameWorld.GetPlayer().Level);

        switch (service)
        {
            case ServiceTypes.Rest:
                // Basic rest is always available (T1)
                actions.Add(CreateActionWithTierCheck(
                    GetRestIcon(location),
                    GetRestTitle(location),
                    GetRestDetail(location, currentTime),
                    GetRestCost(location),
                    TierLevel.T1,
                    playerTier,
                    "rest"
                ));
                break;

            case ServiceTypes.Trade:
                if (IsMarketOpen(currentTime))
                {
                    // Basic trading is always T1 for now
                    TierLevel tradeTier = TierLevel.T1;
                    actions.Add(CreateActionWithTierCheck(
                        "🛍️",
                        "Browse Wares",
                        GetMarketDetail(location, currentTime),
                        "FREE",
                        tradeTier,
                        playerTier,
                        "trade"
                    ));
                }
                break;

            case ServiceTypes.Information:
                // Information gathering requires T2 (Associate) status
                actions.Add(CreateActionWithTierCheck(
                    "👂",
                    "Listen In",
                    GetInformationDetail(location, currentTime),
                    "10m",
                    TierLevel.T2,
                    playerTier,
                    "information"
                ));
                break;
        }

        return actions;
    }

    private List<LocationActionViewModel> GenerateSpotActions(LocationSpot spot, TimeBlocks currentTime)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();

        // Generate actions based on spot's properties
        if (spot.SpotProperties != null)
        {
            foreach (SpotPropertyType prop in spot.SpotProperties)
            {
                LocationActionViewModel action = GenerateTagAction(prop.ToString(), spot);
                if (action != null)
                    actions.Add(action);
            }
        }

        return actions;
    }

    private LocationActionViewModel GenerateTagAction(string tag, LocationSpot spot)
    {
        TierLevel playerTier = GetPlayerTier(_gameWorld.GetPlayer().Level);

        return tag.ToLower() switch
        {
            "public_square" => CreateActionWithTierCheck(
                "⛲", "Rest at Fountain", "Clear your thoughts", "1 seg",
                TierLevel.T1, playerTier, "rest"),

            "crowded" => CreateActionWithTierCheck(
                "📢", "Listen to Town Crier", "Hear proclamations", "1 seg",
                TierLevel.T1, playerTier, "observe"),

            "crossroads" => CreateActionWithTierCheck(
                "🗺️", "Travel", "Leave for another district", "Various times",
                TierLevel.T1, playerTier, "travel"),

            "diplomacy" => CreateActionWithTierCheck(
                "🛍️", "Purchase Provisions", "Hunger and supplies", "1-5 coins",
                TierLevel.T1, playerTier, "purchase"),

            "social" => CreateActionWithTierCheck(
                "💬", "Join Conversation", "Locals chatting", "1 seg",
                TierLevel.T1, playerTier, null),

            "religious" => CreateActionWithTierCheck(
                "🕯️", "Light Candle", "Quiet moment", "1c",
                TierLevel.T1, playerTier, null),

            "nature" => CreateActionWithTierCheck(
                "🌿", "Gather Herbs", "If you know them", "2 seg",
                TierLevel.T2, playerTier, "herb_gathering"),

            "shadow" => CreateActionWithTierCheck(
                "🎭", "Whispered Deal", "Risky business", "2s",
                TierLevel.T3, playerTier, "shadow_dealings"),

            _ => null
        };
    }

    /// <summary>
    /// Creates an action with tier checking and appropriate lock messages.
    /// </summary>
    private LocationActionViewModel CreateActionWithTierCheck(
        string icon, string title, string detail, string cost,
        TierLevel requiredTier, TierLevel playerTier, string actionType)
    {
        bool isAvailable = playerTier >= requiredTier;
        string lockReason = null;

        if (!isAvailable)
        {
            lockReason = requiredTier switch
            {
                TierLevel.T2 => "Requires Associate status",
                TierLevel.T3 => "Requires Confidant status",
                _ => null
            };
        }

        return new LocationActionViewModel
        {
            Icon = isAvailable ? icon : "🔒",
            Title = title,
            Detail = isAvailable ? detail : lockReason,
            Cost = isAvailable ? cost : "LOCKED",
            ActionType = actionType,
            IsAvailable = isAvailable,
            LockReason = lockReason,
        };
    }

    private List<LocationActionViewModel> GenerateTimeBasedActions(LocationSpot spot, TimeBlocks currentTime)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();
        TierLevel playerTier = GetPlayerTier(_gameWorld.GetPlayer().Level);

        switch (currentTime)
        {
            case TimeBlocks.Morning:
                if (spot.MorningProperties?.Contains("market_day") == true)
                {
                    // Basic purchases are T1
                    actions.Add(CreateActionWithTierCheck(
                        "🥖", "Fresh Bread", "Still warm", "2c",
                        TierLevel.T1, playerTier, "purchase_bread"
                    ));
                }
                break;

            case TimeBlocks.Midday:
                if (spot.Population?.GetPropertyValue() == "Crowded")
                {
                    // People watching for information requires T2
                    actions.Add(CreateActionWithTierCheck(
                        "👥", "People Watch", "Learn patterns", "1 seg",
                        TierLevel.T2, playerTier, "observe_patterns"
                    ));
                }
                break;

            case TimeBlocks.Afternoon:
                if (spot.Illumination?.GetPropertyValue() == "Thiefy" || spot.Illumination?.GetPropertyValue() == "Dark")
                {
                    // Shadow activities require T3 (Confidant)
                    actions.Add(CreateActionWithTierCheck(
                        "🕯️", "Find Shadows", "Avoid notice", "FREE",
                        TierLevel.T3, playerTier, "shadow_movement"
                    ));
                }
                break;

            case TimeBlocks.Evening:
                // Most actions unavailable at night
                break;
        }

        return actions;
    }

    private List<LocationActionViewModel> GenerateAtmosphereActions(LocationSpot locationSpot)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();
        TierLevel playerTier = GetPlayerTier(_gameWorld.GetPlayer().Level);

        // Generate actions based on atmosphere
        return actions;
    }

    private string GetRestTitle(LocationSpot locationSpot)
    {
        var location = locationSpot.Location;
        if (locationSpot.LocationType == LocationTypes.Landmark) return "Quiet Rest";
        if (locationSpot.LocationType == LocationTypes.Forest) return "Rest in Shade";
        return "Take a Seat";
    }

    private string GetRestDetail(LocationSpot locationSpot, TimeBlocks time)
    {
        string detail = time == TimeBlocks.Morning ? "Clear head" : "Catch breath";
        return $"1 Segement <span class='icon-bullet'></span> {detail}";
    }

    private string GetRestCost(LocationSpot locationSpot)
    {
        if (locationSpot.LocationType == LocationTypes.Landmark) return "1c"; // Donation
        return "FREE";
    }

    private bool IsMarketOpen(TimeBlocks time)
    {
        return time == TimeBlocks.Morning || time == TimeBlocks.Midday;
    }

    private string GetMarketDetail(Location location, TimeBlocks time)
    {
        if (time == TimeBlocks.Morning) return "Fresh goods";
        if (time == TimeBlocks.Midday) return "Closing soon";
        return "Limited stock";
    }

    private string GetInformationDetail(Location location, TimeBlocks time)
    {
        if (location.Population?.GetPropertyValue() == "Crowded") return "Rumors abound";
        if (time == TimeBlocks.Afternoon) return "Whispered tales";
        return "Local gossip";
    }

    /// <summary>
    /// Convert player level to tier level for action availability
    /// </summary>
    private TierLevel GetPlayerTier(int level)
    {
        if (level >= 7) return TierLevel.T3;
        if (level >= 4) return TierLevel.T2;
        return TierLevel.T1;
    }
}
