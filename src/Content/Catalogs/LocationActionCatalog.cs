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
    /// Generate actions based on location's orthogonal categorical properties.
    /// Uses LocationRole and LocationPurpose instead of generic flags enum.
    /// </summary>
    private static List<LocationAction> GeneratePropertyBasedActions(Location location)
    {
        List<LocationAction> actions = new List<LocationAction>();

        // DIAGNOSTIC LOGGING
        Console.WriteLine($"[LocationActionCatalog] Generating actions for location '{location.Name}'");
        Console.WriteLine($"[LocationActionCatalog] Role: {location.Role}, Purpose: {location.Purpose}");

        // LocationRole.Connective or LocationRole.Hub → Travel action (opens route selection screen)
        // ATMOSPHERIC ACTION (FALLBACK SCENE): No ChoiceTemplate, free action (no costs/rewards)
        if (location.Role == LocationRole.Connective || location.Role == LocationRole.Hub)
        {
            Console.WriteLine($"[LocationActionCatalog] ✅ Travel hub role found - generating Travel action");
            actions.Add(new LocationAction
            {
                SourceLocation = location,
                Name = "Travel to Another Location",
                Description = "Select a route to travel to another location",
                ActionType = LocationActionType.Travel,
                Consequence = new Consequence(),  // Free action (no costs/rewards)
                TimeRequired = 0,  // No time cost for initiating travel
                Availability = new List<TimeBlocks>(),  // Available at all times
                Priority = 100  // High priority - always shown first
            });
        }

        // LocationPurpose.Commerce → Work action
        if (location.Purpose == LocationPurpose.Commerce)
        {
            Console.WriteLine($"[LocationActionCatalog] ✅ Commerce purpose found - generating Work action");
            actions.Add(new LocationAction
            {
                SourceLocation = location,
                Name = "Work",
                Description = "Earn coins through labor. Base pay 8 coins, reduced by hunger penalty.",
                ActionType = LocationActionType.Work,
                Consequence = new Consequence { Coins = 8 },  // HIGHLANDER: Reward as positive value
                Availability = new List<TimeBlocks> { TimeBlocks.Morning, TimeBlocks.Midday, TimeBlocks.Afternoon },
                Priority = 150  // Match JSON priority
            });

            // Commerce purpose → Job Board action (Core Loop Phase 3)
            Console.WriteLine($"[LocationActionCatalog] ✅ Commerce purpose found - generating View Job Board action");
            actions.Add(new LocationAction
            {
                SourceLocation = location,
                Name = "View Job Board",
                Description = "Check available delivery jobs. Accept one job at a time to earn coins through deliveries.",
                ActionType = LocationActionType.ViewJobBoard,
                Consequence = new Consequence(),  // Free action (opens modal)
                Availability = new List<TimeBlocks>(),  // Available at all times (job board always accessible)
                Priority = 140  // Just below Work action
            });
        }

        // LocationRole.Rest → Rest action (sleeping/recovery locations)
        if (location.Role == LocationRole.Rest)
        {
            Console.WriteLine($"[LocationActionCatalog] ✅ Rest role found - generating Rest action");
            actions.Add(new LocationAction
            {
                SourceLocation = location,
                Name = "Rest",
                Description = "Rest in the safety of this sleeping space. Advances 1 time segment. Restores +1 Health and +1 Stamina. Hunger increases by +5 automatically.",
                ActionType = LocationActionType.Rest,
                Consequence = new Consequence { Health = 1, Stamina = 1 },  // HIGHLANDER: Rewards as positive values
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
            Console.WriteLine($"[LocationActionCatalog] Location '{location.Name}' has no HexPosition - cannot generate movement actions");
            return actions;
        }

        // Find ADJACENT locations in the same venue (7-hex cluster pattern)
        List<Location> adjacentSameVenueLocations = new List<Location>();
        foreach (Location candidate in allLocations)
        {
            if (candidate == location) continue; // Skip self

            bool venueMatch = candidate.Venue == location.Venue;
            bool hasHex = candidate.HexPosition.HasValue;
            bool adjacent = hasHex && AreHexesAdjacent(location.HexPosition.Value, candidate.HexPosition.Value);

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
                Description = $"Walk to {destination.Name} within the same venue",
                ActionType = LocationActionType.IntraVenueMove,  // Strongly typed: intra-venue movement (distinct from cross-venue Travel)
                Consequence = new Consequence(),  // Free action (adjacent hex movement)
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

    /// <summary>
    /// PUBLIC: Regenerate intra-venue movement actions when a location is created at runtime.
    /// Called by PackageLoader.CreateSingleLocation to ensure scene-created locations have proper
    /// movement actions both FROM the new location TO neighbors AND FROM neighbors TO the new location.
    /// </summary>
    public static List<LocationAction> RegenerateIntraVenueActionsForNewLocation(Location newLocation, List<Location> allLocations)
    {
        List<LocationAction> newActions = new List<LocationAction>();

        if (newLocation.Venue == null || !newLocation.HexPosition.HasValue)
        {
            Console.WriteLine($"[LocationActionCatalog] Cannot regenerate actions for '{newLocation.Name}' - missing venue or hex position");
            return newActions;
        }

        // Generate actions FROM new location TO adjacent neighbors
        List<LocationAction> actionsFromNew = GenerateIntraVenueMovementActions(newLocation, allLocations);
        newActions.AddRange(actionsFromNew);
        Console.WriteLine($"[LocationActionCatalog] Generated {actionsFromNew.Count} movement actions FROM '{newLocation.Name}'");

        // Generate actions FROM adjacent neighbors TO new location
        foreach (Location neighbor in allLocations)
        {
            if (neighbor == newLocation) continue;
            if (neighbor.Venue != newLocation.Venue) continue;
            if (!neighbor.HexPosition.HasValue) continue;
            if (!AreHexesAdjacent(neighbor.HexPosition.Value, newLocation.HexPosition.Value)) continue;

            LocationAction moveToNew = new LocationAction
            {
                SourceLocation = neighbor,
                DestinationLocation = newLocation,
                Name = $"Move to {newLocation.Name}",
                Description = $"Walk to {newLocation.Name} within the same venue",
                ActionType = LocationActionType.IntraVenueMove,
                Consequence = new Consequence(),  // Free action (adjacent hex movement)
                Availability = new List<TimeBlocks>(),
                Priority = 90
            };
            newActions.Add(moveToNew);
            Console.WriteLine($"[LocationActionCatalog] Generated movement action FROM '{neighbor.Name}' TO '{newLocation.Name}'");
        }

        return newActions;
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
