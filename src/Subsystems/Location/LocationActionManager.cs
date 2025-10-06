using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages location-specific actions and generates action options for spots.
/// Handles work, rest, services, and other location-based activities.
/// </summary>
public class LocationActionManager
{
    private readonly GameWorld _gameWorld;
    private readonly ActionGenerator _actionGenerator;
    private readonly TimeManager _timeManager;
    private readonly NPCRepository _npcRepository;

    public LocationActionManager(
        GameWorld gameWorld,
        ActionGenerator actionGenerator,
        TimeManager timeManager,
        NPCRepository npcRepository)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _actionGenerator = actionGenerator ?? throw new ArgumentNullException(nameof(actionGenerator));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
    }

    /// <summary>
    /// Get available actions for a location and spot.
    /// </summary>
    public List<LocationActionViewModel> GetLocationActions(Location location, LocationSpot spot)
    {
        if (location == null || spot == null) return new List<LocationActionViewModel>();

        // Get dynamic actions from GameWorld data
        List<LocationActionViewModel> dynamicActions = GetDynamicLocationActions(location.Id, spot.SpotID);

        // Get actions from ActionGenerator
        List<LocationActionViewModel> generatedActions = _actionGenerator.GenerateActionsForLocation(location, spot);

        // Combine both
        List<LocationActionViewModel> allActions = new List<LocationActionViewModel>();
        allActions.AddRange(dynamicActions);
        allActions.AddRange(generatedActions);
        // Get goal actions from location.Goals (investigation goals)
        List<LocationActionViewModel> goalActions = GetLocationGoalActions(location, spot);
        allActions.AddRange(goalActions);
        return allActions;
    }

    /// <summary>
    /// Get dynamic location actions from GameWorld data using property matching.
    /// </summary>
    private List<LocationActionViewModel> GetDynamicLocationActions(string locationId, string spotId)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

        // Get the spot to check its properties
        LocationSpot spot = _gameWorld.GetSpot(spotId);
        if (spot == null) return actions;

        // Get actions that match this spot's properties
        List<LocationAction> availableActions = _gameWorld.LocationActions
            .Where(action => action.MatchesSpot(spot, currentTime) &&
                            IsTimeAvailable(action, currentTime))
            .OrderBy(action => action.Priority)
            .ThenBy(action => action.Name)
            .ToList();

        foreach (LocationAction? action in availableActions)
        {
            LocationActionViewModel viewModel = new LocationActionViewModel
            {
                ActionType = action.ActionType ?? action.Id,
                Title = action.Name,
                Detail = action.Description,
                Cost = GetCostDisplay(action.Cost),
                IsAvailable = CanPerformAction(action),
                EngagementType = action.EngagementType,
                InvestigationLabel = null // Investigation system not yet integrated
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

        return action.Availability.Contains(currentTime.ToString());
    }

    /// <summary>
    /// Get display string for action costs.
    /// </summary>
    private string GetCostDisplay(Dictionary<string, int> costs)
    {
        if (costs.Count == 0) return "Free!";

        List<string> costStrings = costs.Select(kvp => $"{kvp.Value} {kvp.Key}").ToList();
        return string.Join(", ", costStrings);
    }

    /// <summary>
    /// Check if the player can perform this action.
    /// </summary>
    private bool CanPerformAction(LocationAction action)
    {
        Player player = _gameWorld.GetPlayer();


        // Check coin cost
        if (action.Cost.ContainsKey("coins"))
        {
            return player.Coins >= action.Cost["coins"];
        }

        return true;
    }

    /// <summary>
    /// Generate actions based on spot properties.
    /// </summary>
    public List<LocationActionViewModel> GenerateSpotActions(LocationSpot spot)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();

        if (spot == null) return actions;

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        List<SpotPropertyType> activeProperties = spot.GetActiveProperties(currentTime);

        // Generate actions based on active properties
        foreach (SpotPropertyType property in activeProperties)
        {
            List<LocationActionViewModel> propertyActions = GenerateActionsForProperty(property, spot);
            actions.AddRange(propertyActions);
        }

        // Add NPC-specific actions
        List<LocationActionViewModel> npcActions = GenerateNPCActions(spot, currentTime);
        actions.AddRange(npcActions);

        return actions;
    }

    /// <summary>
    /// Generate actions for a specific spot property.
    /// </summary>
    private List<LocationActionViewModel> GenerateActionsForProperty(SpotPropertyType property, LocationSpot spot)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();

        switch (property)
        {
            case SpotPropertyType.Commercial:
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
    /// Generate NPC-specific actions for a spot.
    /// </summary>
    private List<LocationActionViewModel> GenerateNPCActions(LocationSpot spot, TimeBlocks currentTime)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();

        // Get NPCs at this spot
        List<NPC> npcs = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);

        foreach (NPC npc in npcs)
        {
            // Check what services this NPC provides
            foreach (ServiceTypes service in npc.ProvidedServices)
            {
                LocationActionViewModel serviceAction = GenerateServiceAction(service, npc);
                if (serviceAction != null)
                {
                    actions.Add(serviceAction);
                }
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
                return null;
        }
    }

    // Method removed - LocationActionsViewModel doesn't have ClosedServices property
    // This functionality would need to be redesigned if needed

    /// <summary>
    /// Get a message for a closed service.
    /// </summary>
    private string GetClosedServiceMessage(SpotPropertyType property, TimeBlocks currentTime)
    {
        return property switch
        {
            SpotPropertyType.Commercial => "Market closed at this hour",
            _ => null
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
    public bool IsActionAvailable(string actionType, LocationSpot spot)
    {
        if (string.IsNullOrEmpty(actionType) || spot == null) return false;

        List<LocationActionViewModel> actions = GetLocationActions(null, spot);
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
    /// Get goal actions from location.Goals (investigation goals)
    /// </summary>
    private List<LocationActionViewModel> GetLocationGoalActions(Location location, LocationSpot spot)
    {
        List<LocationActionViewModel> actions = new List<LocationActionViewModel>();

        if (location == null || location.Goals == null || location.Goals.Count == 0)
            return actions;

        Player player = _gameWorld.GetPlayer();
        if (player == null) return actions;

        foreach (LocationGoal goal in location.Goals)
        {
            if (goal.IsCompleted) continue;
            if (!string.IsNullOrEmpty(goal.SpotId) && goal.SpotId != spot.SpotID) continue;
            if (!EvaluateGoalPrerequisites(goal, player, location.Id)) continue;

            string investigationLabel = null;
            if (!string.IsNullOrEmpty(goal.InvestigationId))
            {
                Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == goal.InvestigationId);
                if (investigation != null)
                {
                    investigationLabel = investigation.Name;
                }
            }

            LocationActionViewModel action = new LocationActionViewModel
            {
                ActionType = $"goal_{goal.Id}",
                Title = goal.Name,
                Detail = goal.Description,
                Cost = "1 segment",
                EngagementType = GetEngagementTypeDisplayName(goal.SystemType),
                InvestigationLabel = investigationLabel,
                IsAvailable = true
            };

            actions.Add(action);
        }

        return actions;
    }

    /// <summary>
    /// Evaluate goal prerequisites
    /// </summary>
    private bool EvaluateGoalPrerequisites(LocationGoal goal, Player player, string currentLocationId)
    {
        if (goal.Requirements == null) return true;

        foreach (string knowledgeId in goal.Requirements.RequiredKnowledge)
        {
            if (!player.Knowledge.HasKnowledge(knowledgeId))
                return false;
        }

        foreach (string equipmentId in goal.Requirements.RequiredEquipment)
        {
            if (!player.Inventory.HasItem(equipmentId))
                return false;
        }

        if (goal.Requirements.MinimumLocationFamiliarity > 0)
        {
            int familiarity = player.GetLocationFamiliarity(currentLocationId);
            if (familiarity < goal.Requirements.MinimumLocationFamiliarity)
                return false;
        }

        foreach (string requiredGoalId in goal.Requirements.CompletedGoals)
        {
            LocationGoal requiredGoal = _gameWorld.Locations
                .SelectMany(loc => loc.Goals)
                .FirstOrDefault(g => g.Id == requiredGoalId);

            if (requiredGoal == null || !requiredGoal.IsCompleted)
                return false;
        }

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
