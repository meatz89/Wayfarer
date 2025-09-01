using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.TravelSubsystem
{
    /// <summary>
    /// Validates access permits and travel restrictions.
    /// </summary>
    public class PermitValidator
    {
        private readonly GameWorld _gameWorld;
        private readonly ItemRepository _itemRepository;
        
        public PermitValidator(
            GameWorld gameWorld,
            ItemRepository itemRepository)
        {
            _gameWorld = gameWorld;
            _itemRepository = itemRepository;
        }
        
        /// <summary>
        /// Check if player has required permit for a route.
        /// </summary>
        public bool HasRequiredPermit(Route route)
        {
            if (route.AccessRequirements == null || !route.AccessRequirements.Any())
            {
                return true; // No permits required
            }
            
            var player = _gameWorld.GetPlayer();
            
            foreach (var requirement in route.AccessRequirements)
            {
                if (requirement.Type == "permit")
                {
                    // Check if player has the permit item
                    if (!player.Inventory.HasItem(requirement.Value))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get missing permits for a route.
        /// </summary>
        public List<string> GetMissingPermits(Route route)
        {
            var missingPermits = new List<string>();
            
            if (route.AccessRequirements == null)
            {
                return missingPermits;
            }
            
            var player = _gameWorld.GetPlayer();
            
            foreach (var requirement in route.AccessRequirements)
            {
                if (requirement.Type == "permit" && !player.Inventory.HasItem(requirement.Value))
                {
                    missingPermits.Add(requirement.Value);
                }
            }
            
            return missingPermits;
        }
        
        /// <summary>
        /// Check if a location requires special access.
        /// </summary>
        public bool LocationRequiresSpecialAccess(string locationId)
        {
            // Certain locations always require permits
            var restrictedLocations = new List<string>
            {
                "noble_district",
                "merchant_guild",
                "royal_palace"
            };
            
            return restrictedLocations.Contains(locationId);
        }
        
        /// <summary>
        /// Validate transport method compatibility with route.
        /// </summary>
        public bool IsTransportCompatible(Route route, TravelMethods transportMethod)
        {
            if (route.RestrictedTransportMethods == null || !route.RestrictedTransportMethods.Any())
            {
                return true; // No restrictions
            }
            
            // Check if transport method is restricted
            return !route.RestrictedTransportMethods.Contains(transportMethod);
        }
        
        /// <summary>
        /// Get access requirement description for UI.
        /// </summary>
        public string GetAccessRequirementDescription(Route route)
        {
            if (route.AccessRequirements == null || !route.AccessRequirements.Any())
            {
                return "No special requirements";
            }
            
            var descriptions = new List<string>();
            
            foreach (var requirement in route.AccessRequirements)
            {
                if (requirement.Type == "permit")
                {
                    var item = _itemRepository.GetItemById(requirement.Value);
                    descriptions.Add($"Requires: {item?.Name ?? requirement.Value}");
                }
                else if (requirement.Type == "flag")
                {
                    descriptions.Add($"Requires: {requirement.DisplayName ?? requirement.Value}");
                }
            }
            
            return string.Join(", ", descriptions);
        }
    }
}