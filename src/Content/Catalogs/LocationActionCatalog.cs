using Wayfarer.GameState.Enums;

namespace Wayfarer.Content.Catalogs;

/// <summary>
/// Catalogue for generating LocationActions from location categorical properties.
/// Called by Parser ONLY - runtime never touches this.
///
/// PARSE-TIME ENTITY GENERATION:
/// Parser calls GenerateActionsForLocation() → Catalogue generates complete LocationAction entities
/// → Parser adds to GameWorld.LocationActions → Runtime queries GameWorld (NO catalogue calls)
/// </summary>
public static class LocationActionCatalog
{
    /// <summary>
    /// Generate ALL LocationActions for a location based on its categorical properties.
    /// Includes both property-based actions (Travel, Work, Rest) and intra-venue movement.
    /// </summary>
    public static List<LocationAction> GenerateActionsForLocation(Location location, List<Location> allLocations)
    {
        List<LocationAction> actions = new List<LocationAction>();

        // Generate property-based actions
        actions.AddRange(GeneratePropertyBasedActions(location));

        // Generate intra-venue movement actions
        actions.AddRange(GenerateIntraVenueMovementActions(location, allLocations));

        return actions;
    }

    /// <summary>
    /// Generate actions based on location's LocationPropertyType enums.
    /// Each property generates specific actions.
    /// </summary>
    private static List<LocationAction> GeneratePropertyBasedActions(Location location)
    {
        List<LocationAction> actions = new List<LocationAction>();

        // DIAGNOSTIC LOGGING
        Console.WriteLine($"[LocationActionCatalog] Generating actions for location '{location.Id}'");
        Console.WriteLine($"[LocationActionCatalog] Properties: {string.Join(", ", location.LocationProperties)}");
        Console.WriteLine($"[LocationActionCatalog] Property count: {location.LocationProperties.Count}");

        // Crossroads property → Travel action (opens route selection screen)
        if (location.LocationProperties.Contains(LocationPropertyType.Crossroads))
        {
            Console.WriteLine($"[LocationActionCatalog] ✅ Crossroads found - generating Travel action");
            actions.Add(new LocationAction
            {
                Id = $"travel_{location.Id}",
                SourceLocationId = location.Id,  // Bind to specific location
                Name = "Travel to Another Location",
                Description = "Select a route to travel to another location",
                ActionType = LocationActionType.Travel,
                Costs = ActionCosts.None(),
                Rewards = ActionRewards.None(),
                RequiredProperties = new List<LocationPropertyType> { LocationPropertyType.Crossroads },
                OptionalProperties = new List<LocationPropertyType>(),
                ExcludedProperties = new List<LocationPropertyType>(),
                Availability = new List<TimeBlocks>(),  // Available at all times
                Priority = 100  // High priority - always shown first
            });
        }

        // Commercial property → Work action
        if (location.LocationProperties.Contains(LocationPropertyType.Commercial))
        {
            Console.WriteLine($"[LocationActionCatalog] ✅ Commercial found - generating Work action");
            actions.Add(new LocationAction
            {
                Id = $"work_{location.Id}",
                SourceLocationId = location.Id,  // Bind to specific location
                Name = "Work",
                Description = "Earn coins through labor. Base pay 8 coins, reduced by hunger penalty.",
                ActionType = LocationActionType.Work,
                Costs = new ActionCosts
                {
                    // Work costs time and stamina (handled by intent)
                },
                Rewards = new ActionRewards
                {
                    CoinReward = 8
                },
                RequiredProperties = new List<LocationPropertyType> { LocationPropertyType.Commercial },
                OptionalProperties = new List<LocationPropertyType>(),
                ExcludedProperties = new List<LocationPropertyType>(),
                Availability = new List<TimeBlocks> { TimeBlocks.Morning, TimeBlocks.Midday, TimeBlocks.Afternoon },
                Priority = 150  // Match JSON priority
            });
        }

        // Restful property → Rest action
        if (location.LocationProperties.Contains(LocationPropertyType.Restful))
        {
            Console.WriteLine($"[LocationActionCatalog] ✅ Restful found - generating Rest action");
            actions.Add(new LocationAction
            {
                Id = $"rest_{location.Id}",
                SourceLocationId = location.Id,  // Bind to specific location
                Name = "Rest",
                Description = "Take time to rest and recover. Advances 1 time segment. Restores +1 Health and +1 Stamina. Hunger increases by +5 automatically.",
                ActionType = LocationActionType.Rest,
                Costs = new ActionCosts
                {
                    // Rest costs time (handled by intent)
                },
                Rewards = new ActionRewards
                {
                    HealthRecovery = 1,
                    StaminaRecovery = 1
                },
                RequiredProperties = new List<LocationPropertyType> { LocationPropertyType.Restful },
                OptionalProperties = new List<LocationPropertyType>(),
                ExcludedProperties = new List<LocationPropertyType>(),
                Availability = new List<TimeBlocks> { TimeBlocks.Morning, TimeBlocks.Midday, TimeBlocks.Afternoon, TimeBlocks.Evening },
                Priority = 50  // Match JSON priority
            });
        }

        // Lodging property → Secure Room action
        if (location.LocationProperties.Contains(LocationPropertyType.Lodging))
        {
            Console.WriteLine($"[LocationActionCatalog] ✅ Lodging found - generating Secure Room action");
            actions.Add(new LocationAction
            {
                Id = $"secure_room_{location.Id}",
                SourceLocationId = location.Id,  // Bind to specific location
                Name = "Secure a Room (10 coins)",
                Description = "Purchase a room for the night. Full recovery of all resources. A bed, a roof, safety through the night.",
                ActionType = LocationActionType.SecureRoom,
                Costs = new ActionCosts
                {
                    Coins = 10  // Match JSON cost
                },
                Rewards = new ActionRewards
                {
                    FullRecovery = true  // Full health/stamina/focus recovery
                },
                RequiredProperties = new List<LocationPropertyType> { LocationPropertyType.Lodging },
                OptionalProperties = new List<LocationPropertyType>(),
                ExcludedProperties = new List<LocationPropertyType>(),
                Availability = new List<TimeBlocks> { TimeBlocks.Evening },  // Only at evening
                Priority = 100  // Match JSON priority
            });
        }

        return actions;
    }

    /// <summary>
    /// Generate intra-venue movement actions for locations in the same venue.
    /// These are FREE, INSTANT movement actions between locations in the same venue.
    /// NOTE: Destination is encoded in action ID - intent handler will parse it.
    /// </summary>
    private static List<LocationAction> GenerateIntraVenueMovementActions(Location location, List<Location> allLocations)
    {
        List<LocationAction> actions = new List<LocationAction>();

        // Find other locations in the same venue
        List<Location> sameVenueLocations = allLocations
            .Where(l => l.VenueId == location.VenueId && l.Id != location.Id)
            .ToList();

        // Only generate if venue has multiple locations
        if (!sameVenueLocations.Any())
            return actions;

        // Generate a "Move to X" action for each other location in venue
        foreach (Location destination in sameVenueLocations)
        {
            actions.Add(new LocationAction
            {
                // ID encodes destination for intent handler parsing
                Id = $"move_to_{destination.Id}",
                SourceLocationId = location.Id,  // Bind to specific source location
                Name = $"Move to {destination.Name}",
                Description = $"Walk to {destination.Name} within the same venue (instant, free)",
                ActionType = LocationActionType.Travel,  // Use Travel type for intra-venue movement
                Costs = ActionCosts.None(),  // Intra-venue movement is FREE
                Rewards = ActionRewards.None(),
                RequiredProperties = new List<LocationPropertyType>(),  // No property requirements (always available)
                OptionalProperties = new List<LocationPropertyType>(),
                ExcludedProperties = new List<LocationPropertyType>(),
                Availability = new List<TimeBlocks>(),  // Always available at all times
                Priority = 90  // High priority, but below cross-venue Travel button
            });
        }

        return actions;
    }
}
