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
    /// Generate actions for a location spot based on its properties
    /// LocationSpot is the gameplay entity - Location is just a container
    /// </summary>
    public List<LocationActionViewModel> GenerateActionsForSpot(LocationSpot spot)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

        // Generate spot-specific actions based on SpotProperties
        if (spot != null)
        {
            actions.AddRange(GenerateSpotActions(spot, currentTime));
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
