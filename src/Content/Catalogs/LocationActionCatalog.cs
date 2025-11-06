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

            // Commercial property → Job Board action (Core Loop Phase 3)
            Console.WriteLine($"[LocationActionCatalog] ✅ Commercial found - generating View Job Board action");
            actions.Add(new LocationAction
            {
                Id = $"view_job_board_{location.Id}",
                SourceLocationId = location.Id,  // Bind to specific location
                Name = "View Job Board",
                Description = "Check available delivery jobs. Accept one job at a time to earn coins through deliveries.",
                ActionType = LocationActionType.ViewJobBoard,
                Costs = ActionCosts.None(),  // Viewing is free
                Rewards = ActionRewards.None(),  // No direct reward (opens modal)
                RequiredProperties = new List<LocationPropertyType> { LocationPropertyType.Commercial },
                OptionalProperties = new List<LocationPropertyType>(),
                ExcludedProperties = new List<LocationPropertyType>(),
                Availability = new List<TimeBlocks>(),  // Available at all times (job board always accessible)
                Priority = 140  // Just below Work action
            });
        }

        // SleepingSpace property → Rest action
        if (location.LocationProperties.Contains(LocationPropertyType.SleepingSpace))
        {
            Console.WriteLine($"[LocationActionCatalog] ✅ SleepingSpace found - generating Rest action");
            actions.Add(new LocationAction
            {
                Id = $"rest_{location.Id}",
                SourceLocationId = location.Id,  // Bind to specific location
                Name = "Rest",
                Description = "Rest in the safety of this sleeping space. Advances 1 time segment. Restores +1 Health and +1 Stamina. Hunger increases by +5 automatically.",
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
                RequiredProperties = new List<LocationPropertyType> { LocationPropertyType.SleepingSpace },
                OptionalProperties = new List<LocationPropertyType>(),
                ExcludedProperties = new List<LocationPropertyType>(),
                Availability = new List<TimeBlocks> { TimeBlocks.Morning, TimeBlocks.Midday, TimeBlocks.Afternoon, TimeBlocks.Evening },
                Priority = 130  // High priority - safe recovery
            });
        }

        return actions;
    }

    /// <summary>
    /// Generate intra-venue movement actions for ADJACENT hexes in the same venue.
    /// VENUE = 7-hex cluster (center + 6 adjacent hexes). Movement is instant/free BECAUSE hexes are adjacent.
    /// ALL locations must have hex positions - movement requires spatial adjacency verification.
    /// </summary>
    private static List<LocationAction> GenerateIntraVenueMovementActions(Location location, List<Location> allLocations)
    {
        List<LocationAction> actions = new List<LocationAction>();

        // Source location must have hex position
        if (!location.HexPosition.HasValue)
        {
            Console.WriteLine($"[LocationActionCatalog] ⚠️ Location '{location.Id}' has no HexPosition - cannot generate movement actions");
            return actions;
        }

        // Find ADJACENT locations in the same venue (7-hex cluster pattern)
        List<Location> adjacentSameVenueLocations = allLocations
            .Where(l =>
                l.VenueId == location.VenueId &&  // Same venue (7-hex cluster)
                l.Id != location.Id &&  // Different location
                l.HexPosition.HasValue &&  // Destination must have hex position
                AreHexesAdjacent(location.HexPosition.Value, l.HexPosition.Value))  // Must be adjacent hexes
            .ToList();

        // Only generate if there are adjacent locations in same venue
        if (!adjacentSameVenueLocations.Any())
            return actions;

        // Generate a "Move to X" action for each ADJACENT location in venue
        foreach (Location destination in adjacentSameVenueLocations)
        {
            actions.Add(new LocationAction
            {
                // ID for debugging/uniqueness only (no parsing)
                Id = $"move_to_{destination.Id}",
                SourceLocationId = location.Id,  // Bind to specific source location
                DestinationLocationId = destination.Id,  // ✅ Strongly-typed property (replaces ID parsing antipattern)
                Name = $"Move to {destination.Name}",
                Description = $"Walk to {destination.Name} within the same venue (instant, free)",
                ActionType = LocationActionType.IntraVenueMove,  // Strongly typed: intra-venue movement (distinct from cross-venue Travel)
                Costs = ActionCosts.None(),  // Intra-venue movement is FREE because hexes are adjacent
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

    /// <summary>
    /// Check if two hexes are adjacent in axial coordinates.
    /// Adjacent = differ by exactly one step in axial hex grid directions.
    /// </summary>
    private static bool AreHexesAdjacent(AxialCoordinates hex1, AxialCoordinates hex2)
    {
        int dq = Math.Abs(hex1.Q - hex2.Q);
        int dr = Math.Abs(hex1.R - hex2.R);

        // In axial coordinates, adjacent hexes have one of these patterns:
        // (±1, 0), (0, ±1), or (±1, ∓1)
        return (dq == 1 && dr == 0) ||  // Horizontal neighbors
               (dq == 0 && dr == 1) ||  // Vertical neighbors
               (dq == 1 && dr == 1);    // Diagonal neighbors
    }
}
