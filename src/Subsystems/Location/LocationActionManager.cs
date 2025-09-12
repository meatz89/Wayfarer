using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.LocationSubsystem
{
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
                    Title = $"{action.Icon} {action.Name}",
                    Detail = action.Description,
                    Cost = GetCostDisplay(action.Cost),
                    IsAvailable = CanPerformAction(action)
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

            // Check attention cost
            if (action.Cost.ContainsKey("attention"))
            {
                // For now, always return true until we have proper attention checking
                return true;
            }

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
                        Detail = "Spend 2 attention to earn 8 coins",
                        Cost = "2 attention",
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
                        Cost = "1 attention",
                        IsAvailable = HasAttention(1)
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
            // Need at least 2 attention to work
            return HasAttention(2);
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
            // Registration might have specific requirements
            return HasAttention(1);
        }

        private bool HasAttention(int amount)
        {
            // This would need proper attention checking through the attention manager
            // For now, return true to avoid null reference issues
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
                "work" => new ActionCost { AttentionCost = 2, TimeCost = 60 },
                "buy_food" => new ActionCost { CoinCost = 5 },
                "buy_drink" => new ActionCost { CoinCost = 2 },
                "rest" => new ActionCost { TimeCost = 60 },
                "heal" => new ActionCost { CoinCost = 10 },
                "register" => new ActionCost { AttentionCost = 1 },
                _ => new ActionCost()
            };
        }
    }

    /// <summary>
    /// Represents the cost of performing an action.
    /// </summary>
    public class ActionCost
    {
        public int AttentionCost { get; set; }
        public int CoinCost { get; set; }
        public int TimeCost { get; set; } // in segments
        public int HealthCost { get; set; }
    }
}