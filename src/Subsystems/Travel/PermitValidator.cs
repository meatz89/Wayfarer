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
        public bool HasRequiredPermit(RouteOption route)
        {
            // RouteOption has AccessRequirement (singular), not AccessRequirements (plural)
            if (route.AccessRequirement == null)
            {
                return true; // No permits required
            }
            
            var player = _gameWorld.GetPlayer();
            
            // Check if permit has been received (using HasReceivedPermit flag)
            if (!route.AccessRequirement.HasReceivedPermit && route.AccessRequirement.AlternativeLetterUnlock != null)
            {
                return false; // Permit letter required but not received
            }
            
            // Check if player has required items
            if (route.AccessRequirement.RequiredItemIds != null && route.AccessRequirement.RequiredItemIds.Any())
            {
                bool hasRequiredItem = route.AccessRequirement.RequiredItemIds
                    .Any(itemId => player.Inventory.ItemSlots.Contains(itemId));
                if (!hasRequiredItem)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get missing permits for a route.
        /// </summary>
        public List<string> GetMissingPermits(RouteOption route)
        {
            var missingPermits = new List<string>();
            
            if (route.AccessRequirement == null)
            {
                return missingPermits;
            }
            
            // Check if permit has not been received
            if (!route.AccessRequirement.HasReceivedPermit && route.AccessRequirement.AlternativeLetterUnlock != null)
            {
                string permitName = route.AccessRequirement.Name ?? "Travel Permit";
                missingPermits.Add(permitName);
            }
            
            // Check for missing required items
            if (route.AccessRequirement.RequiredItemIds != null && route.AccessRequirement.RequiredItemIds.Any())
            {
                var player = _gameWorld.GetPlayer();
                foreach (var itemId in route.AccessRequirement.RequiredItemIds)
                {
                    if (!player.Inventory.ItemSlots.Contains(itemId))
                    {
                        var item = _itemRepository.GetItemById(itemId);
                        missingPermits.Add(item?.Name ?? itemId);
                    }
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
        public bool IsTransportCompatible(RouteOption route, TravelMethods transportMethod)
        {
            // RouteOption has a Method property that defines what transport it uses
            // For now, we'll check if the transport method matches the route's method
            return route.Method == transportMethod || transportMethod == TravelMethods.Walking;
        }
        
        /// <summary>
        /// Get access requirement description for UI.
        /// </summary>
        public string GetAccessRequirementDescription(RouteOption route)
        {
            if (route.AccessRequirement == null)
            {
                return "No special requirements";
            }
            
            var descriptions = new List<string>();
            
            if (!route.AccessRequirement.HasReceivedPermit && route.AccessRequirement.AlternativeLetterUnlock != null)
            {
                string permitName = route.AccessRequirement.Name ?? "Special Permit";
                descriptions.Add($"Requires: {permitName}");
            }
            
            if (route.AccessRequirement.RequiredItemIds != null && route.AccessRequirement.RequiredItemIds.Any())
            {
                foreach (var itemId in route.AccessRequirement.RequiredItemIds)
                {
                    var item = _itemRepository.GetItemById(itemId);
                    descriptions.Add($"Requires: {item?.Name ?? itemId}");
                }
            }
            
            return string.Join(", ", descriptions);
        }
    }
}