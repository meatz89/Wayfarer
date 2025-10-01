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
    public List<LocationActionViewModel> GenerateActionsForLocation(Location location, LocationSpot spot)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();


        // ALWAYS show Wait action for testing purposes
        // Previously only showed when exhausted, but need it visible for testing time block transitions
        actions.Add(new LocationActionViewModel
        {
            Icon = "⏳",
            Title = "Wait",
            Detail = "Advance to next period",
            Cost = "NEXT PERIOD",
            ActionType = "wait" // Special type for handling
        });

        // Generate service-based actions
        if (location.AvailableServices != null)
        {
            foreach (ServiceTypes service in location.AvailableServices)
            {
                actions.AddRange(GenerateServiceActions(service, location, currentTime));
            }
        }

        // Generate spot-specific actions
        if (spot != null)
        {
            actions.AddRange(GenerateSpotActions(spot, currentTime));

            // Travel action is now defined in JSON with Crossroads property requirement
            // No need to hardcode it here - LocationActionManager will add it dynamically
        }

        // Generate location-level domain tag actions
        if (location.DomainTags != null)
        {
            foreach (string tag in location.DomainTags)
            {
                LocationActionViewModel action = GenerateTagAction(tag, spot);
                if (action != null && !actions.Any(a => a.Title == action.Title))
                {
                    actions.Add(action);
                }
            }
        }

        // Generate time-based actions
        actions.AddRange(GenerateTimeBasedActions(location, currentTime));

        // Generate atmosphere-based actions
        actions.AddRange(GenerateAtmosphereActions(location));

        // Add optional Rest action
        if (actions.Count < 5)
        {
            actions.Add(new LocationActionViewModel
            {
                Icon = "🕐",
                Title = "Rest a While",
                Detail = "Advance to next period",
                Cost = "TIME PASSES",
                ActionType = "wait"
            });
        }

        // Limit to 4-5 actions max (per mockup)
        return actions.Take(5).ToList();
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

    private List<LocationActionViewModel> GenerateTimeBasedActions(Location location, TimeBlocks currentTime)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();
        TierLevel playerTier = GetPlayerTier(_gameWorld.GetPlayer().Level);

        switch (currentTime)
        {
            case TimeBlocks.Morning:
                if (location.MorningProperties?.Contains("market_day") == true)
                {
                    // Basic purchases are T1
                    actions.Add(CreateActionWithTierCheck(
                        "🥖", "Fresh Bread", "Still warm", "2c",
                        TierLevel.T1, playerTier, "purchase_bread"
                    ));
                }
                break;

            case TimeBlocks.Midday:
                if (location.Population?.GetPropertyValue() == "Crowded")
                {
                    // People watching for information requires T2
                    actions.Add(CreateActionWithTierCheck(
                        "👥", "People Watch", "Learn patterns", "1 seg",
                        TierLevel.T2, playerTier, "observe_patterns"
                    ));
                }
                break;

            case TimeBlocks.Afternoon:
                if (location.Illumination?.GetPropertyValue() == "Thiefy" || location.Illumination?.GetPropertyValue() == "Dark")
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

    private List<LocationActionViewModel> GenerateAtmosphereActions(Location location)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();
        TierLevel playerTier = GetPlayerTier(_gameWorld.GetPlayer().Level);

        // Generate actions based on atmosphere
        string? atmosphereValue = location.Atmosphere?.GetPropertyValue();
        switch (atmosphereValue)
        {
            case "Guarded":
                // Basic awareness is T1
                actions.Add(CreateActionWithTierCheck(
                    "<span class='icon-warning'></span>", "Stay Alert", "Watch carefully", "FREE",
                    TierLevel.T1, playerTier, "alert"
                ));
                break;

            case "Chaotic":
                // Navigating chaos effectively requires T2
                actions.Add(CreateActionWithTierCheck(
                    "🎉", "Navigate Chaos", "Find your way", "1 seg",
                    TierLevel.T2, playerTier, "navigate_chaos"
                ));
                break;

            case "Formal":
                // Formal settings require T2 for proper etiquette
                actions.Add(CreateActionWithTierCheck(
                    "🤫", "Respectful Silence", "Observe quietly", "FREE",
                    TierLevel.T2, playerTier, "formal_observe"
                ));
                break;
        }

        // Generate actions based on physical properties
        string? physicalValue = location.Physical?.GetPropertyValue();
        switch (physicalValue)
        {
            case "Expansive":
                // Basic surveying is T1
                actions.Add(CreateActionWithTierCheck(
                    "👀", "Survey Area", "Get bearings", "1 seg",
                    TierLevel.T1, playerTier, "survey"
                ));
                break;

            case "Confined":
                // Finding hidden exits requires T2
                actions.Add(CreateActionWithTierCheck(
                    "🚪", "Find Exit", "Note escape routes", "FREE",
                    TierLevel.T2, playerTier, "find_exits"
                ));
                break;

            case "Hazardous":
                // Basic safety is T1
                actions.Add(CreateActionWithTierCheck(
                    "<span class='icon-warning'></span>", "Check Safety", "Avoid dangers", "FREE",
                    TierLevel.T1, playerTier, "safety_check"
                ));
                break;
        }

        return actions;
    }

    // Helper methods for generating contextual details
    private string GetRestIcon(Location location)
    {
        if (location.LocationType == LocationTypes.Landmark) return "🕯️";
        if (location.LocationType == LocationTypes.Forest) return "🌳";
        if (location.Physical?.GetPropertyValue() == "Expansive") return "⛲";
        return "🪑";
    }

    private string GetRestTitle(Location location)
    {
        if (location.LocationType == LocationTypes.Landmark) return "Quiet Rest";
        if (location.LocationType == LocationTypes.Forest) return "Rest in Shade";
        if (location.Physical?.GetPropertyValue() == "Expansive") return "Rest at Fountain";
        return "Take a Seat";
    }

    private string GetRestDetail(Location location, TimeBlocks time)
    {
        string baseTime = location.Physical?.GetPropertyValue() == "Expansive" ? "1 segment" : "1 segment";
        string detail = time == TimeBlocks.Morning ? "Clear head" : "Catch breath";
        return $"{baseTime} <span class='icon-bullet'></span> {detail}";
    }

    private string GetRestCost(Location location)
    {
        if (location.LocationType == LocationTypes.Landmark) return "1c"; // Donation
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
