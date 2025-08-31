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
            
            // Use ActionGenerator for systematic action generation
            return _actionGenerator.GenerateActionsForLocation(location, spot);
        }
        
        /// <summary>
        /// Generate actions based on spot properties.
        /// </summary>
        public List<LocationActionViewModel> GenerateSpotActions(LocationSpot spot)
        {
            var actions = new List<LocationActionViewModel>();
            
            if (spot == null) return actions;
            
            var currentTime = _timeManager.GetCurrentTimeBlock();
            var activeProperties = spot.GetActiveProperties(currentTime);
            
            // Generate actions based on active properties
            foreach (var property in activeProperties)
            {
                var propertyActions = GenerateActionsForProperty(property, spot);
                actions.AddRange(propertyActions);
            }
            
            // Add NPC-specific actions
            var npcActions = GenerateNPCActions(spot, currentTime);
            actions.AddRange(npcActions);
            
            return actions;
        }
        
        /// <summary>
        /// Generate actions for a specific spot property.
        /// </summary>
        private List<LocationActionViewModel> GenerateActionsForProperty(SpotPropertyType property, LocationSpot spot)
        {
            var actions = new List<LocationActionViewModel>();
            
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
            var actions = new List<LocationActionViewModel>();
            
            // Get NPCs at this spot
            var npcs = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);
            
            foreach (var npc in npcs)
            {
                // Check what services this NPC provides
                foreach (var service in npc.ProvidedServices)
                {
                    var serviceAction = GenerateServiceAction(service, npc);
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
            var player = _gameWorld.GetPlayer();
            // Need at least 2 attention to work
            return HasAttention(2);
        }
        
        private bool CanBuyFood()
        {
            var player = _gameWorld.GetPlayer();
            return player.Coins >= 5;
        }
        
        private bool CanBuyDrink()
        {
            var player = _gameWorld.GetPlayer();
            return player.Coins >= 2;
        }
        
        private bool CanRest()
        {
            var player = _gameWorld.GetPlayer();
            // Can rest if health is not full
            return player.Health < player.MaxHealth;
        }
        
        private bool CanSeekTreatment()
        {
            var player = _gameWorld.GetPlayer();
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
            
            var actions = GetLocationActions(null, spot);
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
        public int TimeCost { get; set; } // in minutes
        public int HealthCost { get; set; }
    }
}