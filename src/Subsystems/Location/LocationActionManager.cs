/// <summary>
/// Manages location-specific actions and generates action options for Locations.
/// Handles work, rest, services, and other location-based activities.
/// </summary>
public class LocationActionManager
{
    private readonly GameWorld _gameWorld;
    // ActionGenerator DELETED - violates three-tier timing (actions created at wrong time)
    private readonly TimeManager _timeManager;
    private readonly NPCRepository _npcRepository;
    private readonly LocationAccessibilityService _accessibilityService;

    public LocationActionManager(
        GameWorld gameWorld,
        TimeManager timeManager,
        NPCRepository npcRepository,
        LocationAccessibilityService accessibilityService)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _accessibilityService = accessibilityService ?? throw new ArgumentNullException(nameof(accessibilityService));
    }

    /// <summary>
    /// Get available actions for a Venue and location.
    /// HIGHLANDER: Accept typed objects, pass typed objects
    /// </summary>
    public List<LocationActionViewModel> GetLocationActions(Venue venue, Location location)
    {
        if (venue == null)
            throw new ArgumentNullException(nameof(venue));
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        // Get dynamic actions from GameWorld data
        List<LocationActionViewModel> dynamicActions = GetDynamicLocationActions(venue, location);

        // ActionGenerator DELETED - generated actions now come from SceneFacade at query time
        // Purpose-based actions (from orthogonal properties) filtered at query time
        // Scene-based actions (from ChoiceTemplates) created by SceneFacade when Situation activates

        return dynamicActions;
    }

    /// <summary>
    /// Get dynamic Venue actions from GameWorld data using property matching.
    /// HIGHLANDER: Accept Venue and Location objects, compare objects directly
    /// </summary>
    private List<LocationActionViewModel> GetDynamicLocationActions(Venue venue, Location location)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

        List<LocationAction> availableActions = _gameWorld.LocationActions
            .Where(action => action.MatchesLocation(location, currentTime) &&
                            IsTimeAvailable(action, currentTime) &&
                            IsDestinationAccessible(action))
            .OrderBy(action => action.Priority)
            .ThenBy(action => action.Name)
            .ToList();

        // DELIVERY JOB SYSTEM: Add dynamic CompleteDelivery action if player is at destination
        Player player = _gameWorld.GetPlayer();
        if (player.HasActiveDeliveryJob)
        {
            DeliveryJob activeJob = player.ActiveDeliveryJob;
            // HIGHLANDER: Compare Location objects directly
            if (activeJob != null && activeJob.DestinationLocation == location)
            {
                // Create dynamic ViewModel directly (no domain entity for dynamic actions)
                actions.Add(new LocationActionViewModel
                {
                    ActionType = "completedelivery",
                    Title = $"Complete Delivery ({activeJob.Payment} coins)",
                    Detail = $"Deliver {activeJob.CargoDescription} and receive {activeJob.Payment} coins payment.",
                    Cost = "",  // No cost to complete
                    IsAvailable = true,
                    EngagementType = "Action"  // String, not enum
                });
            }
        }

        foreach (LocationAction action in availableActions)
        {
            bool isAvailable = CanPerformAction(action);
            string lockReason = null;

            // ADR-012: Accessibility filtering implemented via IsDestinationAccessible() in LINQ query
            // Scene-created locations filtered out unless active scene's current situation is there
            // Authored locations always pass accessibility check (TIER 1 No Soft-Locks)

            LocationActionViewModel viewModel = new LocationActionViewModel
            {
                ActionType = action.ActionType.ToString().ToLower(),
                Title = action.Name,
                Detail = action.Description,
                Cost = GetCostDisplay(action.Costs),
                IsAvailable = isAvailable,
                LockReason = lockReason,
                EngagementType = action.EngagementType,
                DestinationLocation = action.DestinationLocation  // HIGHLANDER: Object reference
            };
            actions.Add(viewModel);
        }

        return actions;
    }

    /// <summary>
    /// Check if action is available at the current time.
    /// </summary>
    private bool IsTimeAvailable(LocationAction action, TimeBlocks currentTime)
    {
        if (action.Availability.Count == 0) return true; // Available at all times

        return action.Availability.Contains(currentTime);
    }

    /// <summary>
    /// Check if destination location is accessible for movement actions.
    /// ADR-012: Scene-created locations only accessible when active scene's current situation is there.
    /// Non-movement actions always pass this check.
    /// </summary>
    private bool IsDestinationAccessible(LocationAction action)
    {
        if (action.ActionType != LocationActionType.IntraVenueMove)
            return true;

        if (action.DestinationLocation == null)
            return true;

        return _accessibilityService.IsLocationAccessible(action.DestinationLocation);
    }

    /// <summary>
    /// Get display string for action costs.
    /// </summary>
    private string GetCostDisplay(ActionCosts costs)
    {
        List<string> costParts = new List<string>();

        if (costs.Coins > 0)
            costParts.Add($"{costs.Coins} coins");

        if (costs.Focus > 0)
            costParts.Add($"{costs.Focus} focus");

        if (costs.Stamina > 0)
            costParts.Add($"{costs.Stamina} stamina");

        if (costs.Health > 0)
            costParts.Add($"{costs.Health} health");

        if (costParts.Count == 0)
            return "Free!";

        return string.Join(", ", costParts);
    }

    /// <summary>
    /// Check if the player can perform this action.
    /// </summary>
    private bool CanPerformAction(LocationAction action)
    {
        Player player = _gameWorld.GetPlayer();

        // HIGHLANDER: Use CompoundRequirement for coin affordability check
        if (action.Costs.Coins > 0)
        {
            Consequence cost = new Consequence { Coins = -action.Costs.Coins };
            CompoundRequirement resourceReq = CompoundRequirement.CreateForConsequence(cost);
            return resourceReq.IsAnySatisfied(player, _gameWorld);
        }

        return true;
    }

    /// <summary>
    /// Generate actions based on location properties.
    /// </summary>
    public List<LocationActionViewModel> GenerateLocationActions(Location location)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

        // Generate actions based on orthogonal properties (no time variation)
        List<LocationActionViewModel> purposeActions = GenerateActionsForPurpose(location);
        actions.AddRange(purposeActions);

        // Add NPC-specific actions
        List<LocationActionViewModel> npcActions = GenerateNPCActions(location, currentTime);
        actions.AddRange(npcActions);

        return actions;
    }

    /// <summary>
    /// Generate actions based on location purpose (orthogonal property).
    /// </summary>
    private List<LocationActionViewModel> GenerateActionsForPurpose(Location location)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();

        // Check purpose using orthogonal categorical property
        if (location.Purpose == LocationPurpose.Commerce)
        {
            actions.Add(new LocationActionViewModel
            {
                ActionType = "work",
                Title = "Work for Coins",
                Detail = "Earn 8 coins",
                Cost = "Free!",
                IsAvailable = CanPerformWork()
            });
        }

        // Other actions are NPC-based services, not purpose-based

        return actions;
    }

    /// <summary>
    /// Generate NPC-specific actions for a location.
    /// </summary>
    private List<LocationActionViewModel> GenerateNPCActions(Location location, TimeBlocks currentTime)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();

        // Get NPCs at this location
        List<NPC> npcs = _npcRepository.GetNPCsForLocationAndTime(location, currentTime);

        // Service-based actions removed - use Scene-Situation architecture instead

        return actions;
    }
    // Method removed - LocationActionsViewModel doesn't have ClosedServices property
    // Validation methods

    private bool CanPerformWork()
    {
        Player player = _gameWorld.GetPlayer();
        return true;
    }

    private bool CanBuyFood()
    {
        // HIGHLANDER: Use CompoundRequirement for coin affordability check
        Player player = _gameWorld.GetPlayer();
        Consequence cost = new Consequence { Coins = -5 };
        CompoundRequirement resourceReq = CompoundRequirement.CreateForConsequence(cost);
        return resourceReq.IsAnySatisfied(player, _gameWorld);
    }

    private bool CanBuyDrink()
    {
        // HIGHLANDER: Use CompoundRequirement for coin affordability check
        Player player = _gameWorld.GetPlayer();
        Consequence cost = new Consequence { Coins = -2 };
        CompoundRequirement resourceReq = CompoundRequirement.CreateForConsequence(cost);
        return resourceReq.IsAnySatisfied(player, _gameWorld);
    }

    private bool CanRest()
    {
        Player player = _gameWorld.GetPlayer();
        // Can rest if health is not full
        return player.Health < player.MaxHealth;
    }

    private bool CanSeekTreatment()
    {
        // HIGHLANDER: Use CompoundRequirement for resource affordability checks
        Player player = _gameWorld.GetPlayer();
        Consequence cost = new Consequence { Coins = -10, Health = 0 };
        CompoundRequirement resourceReq = CompoundRequirement.CreateForConsequence(cost);
        bool canAfford = resourceReq.IsAnySatisfied(player, _gameWorld);
        return canAfford && player.Health < player.MaxHealth;
    }

    private bool CanRegister()
    {
        return true;
    }

    /// <summary>
    /// Check if a specific action is available at the current location.
    /// </summary>
    public bool IsActionAvailable(string actionType, Location location)
    {
        if (string.IsNullOrEmpty(actionType))
            throw new ArgumentException("Action type cannot be null or empty", nameof(actionType));
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        List<LocationActionViewModel> actions = GetLocationActions(null, location);
        return actions.Any(a => a.ActionType.Equals(actionType, StringComparison.OrdinalIgnoreCase) && a.IsAvailable);
    }

    /// <summary>
    /// Get the cost for performing an action.
    /// </summary>
    public ActionCost GetActionCost(string actionType)
    {
        return actionType switch
        {
            "work" => new ActionCost { TimeCost = 60 },
            "buy_food" => new ActionCost { CoinCost = 5 },
            "buy_drink" => new ActionCost { CoinCost = 2 },
            "rest" => new ActionCost { TimeCost = 60 },
            "heal" => new ActionCost { CoinCost = 10 },
            "register" => new ActionCost(),
            _ => new ActionCost()
        };
    }

    /// <summary>
    /// Get display name for tactical system type
    /// </summary>
    private string GetEngagementTypeDisplayName(TacticalSystemType systemType)
    {
        return systemType switch
        {
            TacticalSystemType.Mental => "Mental",
            TacticalSystemType.Physical => "Physical",
            TacticalSystemType.Social => "Conversation",
            _ => systemType.ToString()
        };
    }

    /// <summary>
    /// Evaluate situation prerequisites
    /// SituationRequirements system eliminated - situations always visible, difficulty varies via DifficultyModifiers
    /// Boolean gate elimination: No more hiding situations based on equipment/knowledge/stats
    /// </summary>
    private bool EvaluateSituationPrerequisites(Situation situation, Player player, string currentVenueId)
    {
        // Situations always visible - difficulty adjusts based on DifficultyModifiers instead
        return true;
    }
}

/// <summary>
/// Represents the cost of performing an action.
/// </summary>
public class ActionCost
{
    public int CoinCost { get; set; }
    public int TimeCost { get; set; } // in segments
    public int HealthCost { get; set; }
}
