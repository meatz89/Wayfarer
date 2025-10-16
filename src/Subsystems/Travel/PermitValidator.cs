using System;
using System.Collections.Generic;
using System.Linq;

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
    /// AccessRequirement system eliminated - PRINCIPLE 4: Economic affordability determines access
    /// </summary>
    public bool HasRequiredPermit(RouteOption route)
    {
        // AccessRequirement system eliminated - all routes accessible based on coins/stamina
        return true;
    }

    /// <summary>
    /// Get missing permits for a route.
    /// AccessRequirement system eliminated - PRINCIPLE 4: Economic affordability determines access
    /// </summary>
    public List<string> GetMissingPermits(RouteOption route)
    {
        // AccessRequirement system eliminated - no permits gate routes
        return new List<string>();
    }

    /// <summary>
    /// Check if a Venue requires special access.
    /// </summary>
    public bool LocationRequiresSpecialAccess(string venueId)
    {
        // Certain locations always require permits
        List<string> restrictedLocations = new List<string>
        {
            "noble_district",
            "merchant_guild",
            "royal_palace"
        };

        return restrictedLocations.Contains(venueId);
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
    /// AccessRequirement system eliminated - PRINCIPLE 4: Economic affordability determines access
    /// </summary>
    public string GetAccessRequirementDescription(RouteOption route)
    {
        // AccessRequirement system eliminated - routes show cost/stamina requirements instead
        return "No special requirements";
    }
}
