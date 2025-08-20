using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Factory for creating routes with guaranteed valid references.
/// Ensures routes can only be created with existing locations.
/// </summary>
public class RouteFactory
{
    public RouteFactory()
    {
        // No dependencies - factory is stateless
    }

    /// <summary>
    /// Create a minimal route with just an ID.
    /// Used for dummy/placeholder creation when references are missing.
    /// </summary>
    public RouteOption CreateMinimalRoute(string id, string originId = null, string destinationId = null)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Route ID cannot be empty", nameof(id));

        string name = FormatIdAsName(id);

        return new RouteOption
        {
            Id = id,
            Name = name,
            Origin = originId ?? "unknown_origin",
            Destination = destinationId ?? "unknown_destination",
            Method = TravelMethods.Walking, // Most basic method
            TravelTimeMinutes = 480, // Standard day travel (8 hours in minutes)
            BaseStaminaCost = 2, // Minimal cost
            BaseCoinCost = 0, // Free
            Description = $"A route called {name}",
            IsDiscovered = true, // Make it available immediately
            MaxItemCapacity = 3 // Basic walking capacity
        };
    }

    private string FormatIdAsName(string id)
    {
        // Convert snake_case or kebab-case to Title Case
        return string.Join(" ",
            id.Replace('_', ' ').Replace('-', ' ')
              .Split(' ')
              .Select(word => string.IsNullOrEmpty(word) ? "" :
                  char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    /// <summary>
    /// Create a route with validated location references
    /// </summary>
    public RouteOption CreateRoute(
        string id,
        string name,
        Location origin,      // Not string - actual Location object
        Location destination, // Not string - actual Location object
        TravelMethods method,
        int travelTimeMinutes,
        int baseStaminaCost = 2,
        int baseCoinCost = 0,
        string description = "")
    {
        if (origin == null)
            throw new ArgumentNullException(nameof(origin), "Origin location cannot be null");
        if (destination == null)
            throw new ArgumentNullException(nameof(destination), "Destination location cannot be null");
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Route ID cannot be empty", nameof(id));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Route name cannot be empty", nameof(name));

        // Create route with validated references
        RouteOption route = new RouteOption
        {
            Id = id,
            Name = name,
            Origin = origin.Id,           // Extract ID from validated object
            Destination = destination.Id, // Extract ID from validated object
            Method = method,
            TravelTimeMinutes = travelTimeMinutes,
            BaseStaminaCost = baseStaminaCost,
            BaseCoinCost = baseCoinCost,
            Description = description ?? $"Travel from {origin.Name} to {destination.Name}",
            IsDiscovered = false, // Routes start undiscovered by default
            MaxItemCapacity = method == TravelMethods.Walking ? 3 : 5
        };

        return route;
    }

    /// <summary>
    /// Create a route from string IDs with validation
    /// </summary>
    public RouteOption CreateRouteFromIds(
        string id,
        string name,
        string originId,
        string destinationId,
        IEnumerable<Location> availableLocations,
        TravelMethods method,
        int travelTimeMinutes,
        int baseStaminaCost = 2,
        int baseCoinCost = 0,
        string description = "")
    {
        // Resolve to actual objects
        Location? origin = availableLocations.FirstOrDefault(l => l.Id == originId);
        Location? destination = availableLocations.FirstOrDefault(l => l.Id == destinationId);

        if (origin == null)
            throw new InvalidOperationException($"Cannot create route: origin location '{originId}' not found");
        if (destination == null)
            throw new InvalidOperationException($"Cannot create route: destination location '{destinationId}' not found");

        return CreateRoute(id, name, origin, destination, method, travelTimeMinutes,
                          baseStaminaCost, baseCoinCost, description);
    }
}