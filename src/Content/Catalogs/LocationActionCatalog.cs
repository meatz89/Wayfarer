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
    /// Generate actions based on location's capabilities.
    /// Each capability generates specific actions.
    /// </summary>
    private static List<LocationAction> GeneratePropertyBasedActions(Location location)
    {
        List<LocationAction> actions = new List<LocationAction>();

        // DIAGNOSTIC LOGGING
        Console.WriteLine($"[LocationActionCatalog] Generating actions for location '{location.Name}'");
        Console.WriteLine($"[LocationActionCatalog] Capabilities: {location.Capabilities}");

        // Crossroads capability → Travel action (opens route selection screen)
        // ATMOSPHERIC ACTION (FALLBACK SCENE): No ChoiceTemplate, free action (no costs/rewards)
        if (location.Capabilities.HasFlag(LocationCapability.Crossroads))
        {
            Console.WriteLine($"[LocationActionCatalog] ✅ Crossroads found - generating Travel action");
            actions.Add(new LocationAction
            {
                SourceLocation = location,
                Name = "Travel to Another Location",
                Description = "Select a route to travel to another location",
                ActionType = LocationActionType.Travel,
                Costs = new ActionCosts(),  // Free action (no costs)
                Rewards = new ActionRewards(),  // No rewards (opens travel screen)
                TimeRequired = 0,  // No time cost for initiating travel
                RequiredCapabilities = LocationCapability.Crossroads,
                OptionalCapabilities = LocationCapability.None,
                ExcludedCapabilities = LocationCapability.None,
                Availability = new List<TimeBlocks>(),  // Available at all times
                Priority = 100  // High priority - always shown first
            });
        }

        // Commercial capability → Work action
        if (location.Capabilities.HasFlag(LocationCapability.Commercial))
        {
            Console.WriteLine($"[LocationActionCatalog] ✅ Commercial found - generating Work action");
            actions.Add(new LocationAction
            {
                SourceLocation = location,
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
                RequiredCapabilities = LocationCapability.Commercial,
                OptionalCapabilities = LocationCapability.None,
                ExcludedCapabilities = LocationCapability.None,
                Availability = new List<TimeBlocks> { TimeBlocks.Morning, TimeBlocks.Midday, TimeBlocks.Afternoon },
                Priority = 150  // Match JSON priority
            });

            // Commercial capability → Job Board action (Core Loop Phase 3)
            Console.WriteLine($"[LocationActionCatalog] ✅ Commercial found - generating View Job Board action");
            actions.Add(new LocationAction
            {
                SourceLocation = location,
                Name = "View Job Board",
                Description = "Check available delivery jobs. Accept one job at a time to earn coins through deliveries.",
                ActionType = LocationActionType.ViewJobBoard,
                Costs = ActionCosts.None(),  // Viewing is free
                Rewards = ActionRewards.None(),  // No direct reward (opens modal)
                RequiredCapabilities = LocationCapability.Commercial,
                OptionalCapabilities = LocationCapability.None,
                ExcludedCapabilities = LocationCapability.None,
                Availability = new List<TimeBlocks>(),  // Available at all times (job board always accessible)
                Priority = 140  // Just below Work action
            });
        }

        // SleepingSpace capability → Rest action
        if (location.Capabilities.HasFlag(LocationCapability.SleepingSpace))
        {
            Console.WriteLine($"[LocationActionCatalog] ✅ SleepingSpace found - generating Rest action");
            actions.Add(new LocationAction
            {
                SourceLocation = location,
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
                RequiredCapabilities = LocationCapability.SleepingSpace,
                OptionalCapabilities = LocationCapability.None,
                ExcludedCapabilities = LocationCapability.None,
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
            Console.WriteLine($"[LocationActionCatalog] ⚠️ Location '{location.Name}' has no HexPosition - cannot generate movement actions");
            return actions;
        }

        // LOG: Source location details for debugging intra-venue movement
        Console.WriteLine($"[IntraVenueMovement] Source: '{location.Name}' (Venue: '{location.Venue?.Name ?? "NULL"}', Hex: {location.HexPosition})");

        // Find ADJACENT locations in the same venue (7-hex cluster pattern)
        // LOG: Evaluate each potential candidate to trace filter logic
        List<Location> adjacentSameVenueLocations = new List<Location>();
        foreach (Location candidate in allLocations)
        {
            if (candidate == location) continue; // Skip self

            bool venueMatch = candidate.Venue == location.Venue;
            bool hasHex = candidate.HexPosition.HasValue;
            bool adjacent = hasHex && AreHexesAdjacent(location.HexPosition.Value, candidate.HexPosition.Value);
            int distance = hasHex ? CalculateHexDistance(location.HexPosition.Value, candidate.HexPosition.Value) : -1;

            Console.WriteLine($"  Candidate: '{candidate.Name}' (Venue: '{candidate.Venue?.Name ?? "NULL"}', VenueMatch: {venueMatch}, Hex: {candidate.HexPosition?.ToString() ?? "NULL"}, Adjacent: {adjacent}, Distance: {distance})");

            // Apply filter: same venue AND has hex AND adjacent
            if (venueMatch && hasHex && adjacent)
            {
                adjacentSameVenueLocations.Add(candidate);
            }
        }

        // Only generate if there are adjacent locations in same venue
        if (!adjacentSameVenueLocations.Any())
            return actions;

        // Generate a "Move to X" action for each ADJACENT location in venue
        foreach (Location destination in adjacentSameVenueLocations)
        {
            actions.Add(new LocationAction
            {
                SourceLocation = location,
                DestinationLocation = destination,
                Name = $"Move to {destination.Name}",
                Description = $"Walk to {destination.Name} within the same venue (instant, free)",
                ActionType = LocationActionType.IntraVenueMove,  // Strongly typed: intra-venue movement (distinct from cross-venue Travel)
                Costs = ActionCosts.None(),  // Intra-venue movement is FREE because hexes are adjacent
                Rewards = ActionRewards.None(),
                RequiredCapabilities = LocationCapability.None,  // No capability requirements (always available)
                OptionalCapabilities = LocationCapability.None,
                ExcludedCapabilities = LocationCapability.None,
                Availability = new List<TimeBlocks>(),  // Always available at all times
                Priority = 90  // High priority, but below cross-venue Travel button
            });

            Console.WriteLine($"[IntraVenueMovement] ✅ Generated action: 'Move to {destination.Name}' from '{location.Name}'");
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

    /// <summary>
    /// Calculate hex distance in axial coordinates for debugging.
    /// Manhattan distance in axial hex grid.
    /// </summary>
    private static int CalculateHexDistance(AxialCoordinates hex1, AxialCoordinates hex2)
    {
        int dq = Math.Abs(hex1.Q - hex2.Q);
        int dr = Math.Abs(hex1.R - hex2.R);
        int ds = Math.Abs((hex1.Q + hex1.R) - (hex2.Q + hex2.R));
        return Math.Max(Math.Max(dq, dr), ds);
    }
}
