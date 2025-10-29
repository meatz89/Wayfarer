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

    public LocationActionManager(
        GameWorld gameWorld,
        TimeManager timeManager,
        NPCRepository npcRepository)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
    }

    /// <summary>
    /// Get available actions for a Venue and location.
    /// </summary>
    public List<LocationActionViewModel> GetLocationActions(Venue venue, Location location)
    {
        if (venue == null)
            throw new ArgumentNullException(nameof(venue));
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        // Get dynamic actions from GameWorld data
        List<LocationActionViewModel> dynamicActions = GetDynamicLocationActions(venue.Id, location.Id);

        // ActionGenerator DELETED - generated actions now come from SceneFacade at query time
        // Property-based actions (from LocationPropertyType) remain here as legacy system
        // Scene-based actions (from ChoiceTemplates) created by SceneFacade when Situation activates

        return dynamicActions;
    }

    /// <summary>
    /// Get dynamic Venue actions from GameWorld data using property matching.
    /// </summary>
    private List<LocationActionViewModel> GetDynamicLocationActions(string venueId, string LocationId)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

        // Get the location to check its properties
        Location location = _gameWorld.GetLocation(LocationId);
        if (location == null)
            throw new InvalidOperationException($"Location not found: {LocationId}");

        // Get actions that match this location's properties
        List<LocationAction> availableActions = _gameWorld.LocationActions
            .Where(action => action.MatchesLocation(location, currentTime) &&
                            IsTimeAvailable(action, currentTime))
            .OrderBy(action => action.Priority)
            .ThenBy(action => action.Name)
            .ToList();

        foreach (LocationAction action in availableActions)
        {
            LocationActionViewModel viewModel = new LocationActionViewModel
            {
                Id = action.Id,
                ActionType = action.ActionType.ToString().ToLower(),  // Convert enum to lowercase string for ViewModel
                Title = action.Name,
                Detail = action.Description,
                Cost = GetCostDisplay(action.Costs),
                IsAvailable = CanPerformAction(action),
                EngagementType = action.EngagementType
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

        // Check coin cost
        if (action.Costs.Coins > 0)
        {
            return player.Coins >= action.Costs.Coins;
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
        List<LocationPropertyType> activeProperties = location.GetActiveProperties(currentTime);

        // Generate actions based on active properties
        foreach (LocationPropertyType property in activeProperties)
        {
            List<LocationActionViewModel> propertyActions = GenerateActionsForProperty(property, location);
            actions.AddRange(propertyActions);
        }

        // Add NPC-specific actions
        List<LocationActionViewModel> npcActions = GenerateNPCActions(location, currentTime);
        actions.AddRange(npcActions);

        return actions;
    }

    /// <summary>
    /// Generate actions for a specific location property.
    /// </summary>
    private List<LocationActionViewModel> GenerateActionsForProperty(LocationPropertyType property, Location location)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();

        switch (property)
        {
            case LocationPropertyType.Commercial:
                actions.Add(new LocationActionViewModel
                {
                    ActionType = "work",
                    Title = "Work for Coins",
                    Detail = "Earn 8 coins",
                    Cost = "Free!",
                    IsAvailable = CanPerformWork()
                });
                break;

                // Removed invalid property types - only Commercial exists
                // Other actions should be NPC-based services
        }

        return actions;
    }

    /// <summary>
    /// Generate NPC-specific actions for a location.
    /// </summary>
    private List<LocationActionViewModel> GenerateNPCActions(Location location, TimeBlocks currentTime)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();

        // Get NPCs at this location
        List<NPC> npcs = _npcRepository.GetNPCsForLocationAndTime(location.Id, currentTime);

        foreach (NPC npc in npcs)
        {
            // Check what services this NPC provides
            foreach (ServiceTypes service in npc.ProvidedServices)
            {
                LocationActionViewModel serviceAction = GenerateServiceAction(service, npc);
                if (serviceAction == null)
                    throw new InvalidOperationException($"Unsupported service type: {service}");
                actions.Add(serviceAction);
            }
        }

        return actions;
    }

    /// <summary>
    /// Generate an action for a specific service.
    /// </summary>
    private LocationActionViewModel GenerateServiceAction(ServiceTypes service, NPC provider)
    {
        switch (service)
        {
            case ServiceTypes.Trading:
                return new LocationActionViewModel
                {
                    ActionType = "letter_board",
                    Title = $"Check {provider.Name}'s Letter Board",
                    Detail = "View available letters for delivery",
                    IsAvailable = true
                };

            case ServiceTypes.Market:
                return new LocationActionViewModel
                {
                    ActionType = "trade",
                    Title = $"Trade with {provider.Name}",
                    Detail = "Buy or sell goods",
                    IsAvailable = true
                };

            case ServiceTypes.Information:
                return new LocationActionViewModel
                {
                    ActionType = "inquire",
                    Title = $"Ask {provider.Name} for Information",
                    Detail = "Learn about local events and opportunities",
                    Cost = "Free!",
                    IsAvailable = true
                };

            default:
                throw new InvalidOperationException($"Unsupported service type: {service}");
        }
    }

    // Method removed - LocationActionsViewModel doesn't have ClosedServices property
    // This functionality would need to be redesigned if needed

    /// <summary>
    /// Get a message for a closed service.
    /// </summary>
    private string GetClosedServiceMessage(LocationPropertyType property, TimeBlocks currentTime)
    {
        return property switch
        {
            LocationPropertyType.Commercial => "Market closed at this hour",
            _ => throw new InvalidOperationException($"Unsupported property type: {property}")
        };
    }

    // Validation methods

    private bool CanPerformWork()
    {
        Player player = _gameWorld.GetPlayer();
        return true;
    }

    private bool CanBuyFood()
    {
        Player player = _gameWorld.GetPlayer();
        return player.Coins >= 5;
    }

    private bool CanBuyDrink()
    {
        Player player = _gameWorld.GetPlayer();
        return player.Coins >= 2;
    }

    private bool CanRest()
    {
        Player player = _gameWorld.GetPlayer();
        // Can rest if health is not full
        return player.Health < player.MaxHealth;
    }

    private bool CanSeekTreatment()
    {
        Player player = _gameWorld.GetPlayer();
        return player.Coins >= 10 && player.Health < player.MaxHealth;
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
