/// <summary>
/// System 4: Entity Resolver - FIND ONLY
/// Pure query service - searches for entities matching categorical filters.
/// Returns null if not found (caller decides whether to create or throw).
/// NO creation logic - that belongs to PackageLoader (HIGHLANDER principle).
///
/// VENUE-SCOPED RESOLUTION:
/// - When venue provided: searches ONLY within that venue (no cross-venue teleportation)
/// - When venue null: global search (parse-time resolution)
/// </summary>
public class EntityResolver
{
    private readonly GameWorld _gameWorld;
    private readonly Venue _currentVenue;

    /// <summary>
    /// Constructor for venue-scoped resolution (runtime scene activation).
    /// </summary>
    public EntityResolver(GameWorld gameWorld, Venue currentVenue)
    {
        _gameWorld = gameWorld;
        _currentVenue = currentVenue;
    }

    /// <summary>
    /// Constructor for global resolution (parse-time, no venue constraint).
    /// </summary>
    public EntityResolver(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
        _currentVenue = null;
    }

    /// <summary>
    /// Find existing Location matching filter. Returns null if not found.
    /// Caller decides whether to create (SceneInstantiator) or throw (parsers).
    /// </summary>
    public Location FindLocation(PlacementFilter filter, Location contextLocation)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter), "PlacementFilter cannot be null");

        // Handle proximity-based resolution FIRST
        if (filter.Proximity == PlacementProximity.SameLocation)
        {
            if (contextLocation == null)
                throw new InvalidOperationException("SameLocation proximity requires contextLocation");
            return contextLocation;
        }

        return FindMatchingLocation(filter);
    }

    /// <summary>
    /// Find existing NPC matching filter. Returns null if not found.
    /// Caller decides whether to create (SceneInstantiator) or throw (parsers).
    /// </summary>
    public NPC FindNPC(PlacementFilter filter, Location contextLocation)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter), "PlacementFilter cannot be null");

        // Handle proximity-based resolution: search NPCs AT contextLocation
        if (filter.Proximity == PlacementProximity.SameLocation)
        {
            if (contextLocation == null)
                throw new InvalidOperationException("SameLocation proximity requires contextLocation");
            return FindNPCAtLocation(contextLocation, filter);
        }

        return FindMatchingNPC(filter);
    }

    /// <summary>
    /// Find existing RouteOption matching filter. Returns null if not found.
    /// Caller decides whether to create (SceneInstantiator) or throw (parsers).
    /// </summary>
    public RouteOption FindRoute(PlacementFilter filter, Location contextLocation)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter), "PlacementFilter cannot be null");

        // Handle proximity-based resolution: search Routes FROM contextLocation
        if (filter.Proximity == PlacementProximity.SameLocation)
        {
            if (contextLocation == null)
                throw new InvalidOperationException("SameLocation proximity requires contextLocation");
            return FindRouteFromLocation(contextLocation, filter);
        }

        return FindMatchingRoute(filter);
    }

    /// <summary>
    /// Find existing Item by name.
    /// Returns null if item not found - Items are immutable content definitions.
    /// </summary>
    public Item FindItemByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
            return null;

        return _gameWorld.Items.FirstOrDefault(i => i.Name == itemName);
    }

    // ========== FIND MATCHING ENTITIES ==========

    private Location FindMatchingLocation(PlacementFilter filter)
    {
        List<Location> matchingLocations;

        if (_currentVenue != null)
        {
            matchingLocations = _gameWorld.Locations
                .Where(loc => loc.Venue == _currentVenue && LocationMatchesFilter(loc, filter))
                .ToList();
            Console.WriteLine($"[EntityResolver] Found {matchingLocations.Count} matching locations within venue '{_currentVenue.Name}'");
        }
        else
        {
            matchingLocations = _gameWorld.Locations
                .Where(loc => LocationMatchesFilter(loc, filter))
                .ToList();
            Console.WriteLine($"[EntityResolver] Found {matchingLocations.Count} matching locations (global search)");
        }

        if (matchingLocations.Count == 0)
            return null;

        return ApplyLocationSelectionStrategy(matchingLocations, filter.SelectionStrategy);
    }

    private bool LocationMatchesFilter(Location loc, PlacementFilter filter)
    {
        // Check location role (if specified) - SINGULAR property
        if (filter.LocationRole.HasValue && loc.Role != filter.LocationRole.Value)
            return false;

        // Check orthogonal categorical dimensions - SINGULAR properties
        if (filter.Privacy.HasValue && loc.Privacy != filter.Privacy.Value)
            return false;

        if (filter.Safety.HasValue && loc.Safety != filter.Safety.Value)
            return false;

        if (filter.Activity.HasValue && loc.Activity != filter.Activity.Value)
            return false;

        if (filter.Purpose.HasValue && loc.Purpose != filter.Purpose.Value)
            return false;

        // Check accessibility requirements
        if (filter.IsPlayerAccessible.HasValue && filter.IsPlayerAccessible.Value)
        {
            if (!loc.HasBeenVisited)
                return false;
        }

        return true;
    }

    private NPC FindMatchingNPC(PlacementFilter filter)
    {
        List<NPC> matchingNPCs;

        if (_currentVenue != null)
        {
            matchingNPCs = _gameWorld.NPCs
                .Where(npc => npc.Location?.Venue == _currentVenue && NPCMatchesFilter(npc, filter))
                .ToList();
            Console.WriteLine($"[EntityResolver] Found {matchingNPCs.Count} matching NPCs within venue '{_currentVenue.Name}'");
        }
        else
        {
            matchingNPCs = _gameWorld.NPCs
                .Where(npc => NPCMatchesFilter(npc, filter))
                .ToList();
            Console.WriteLine($"[EntityResolver] Found {matchingNPCs.Count} matching NPCs (global search)");
        }

        if (matchingNPCs.Count == 0)
            return null;

        return ApplyNPCSelectionStrategy(matchingNPCs, filter.SelectionStrategy);
    }

    private bool NPCMatchesFilter(NPC npc, PlacementFilter filter)
    {
        // SINGULAR property semantics: null = don't filter, value = must match exactly
        if (filter.PersonalityType.HasValue && npc.PersonalityType != filter.PersonalityType.Value)
            return false;

        if (filter.Profession.HasValue && npc.Profession != filter.Profession.Value)
            return false;

        if (filter.RequiredRelationship.HasValue && npc.PlayerRelationship != filter.RequiredRelationship.Value)
            return false;

        if (filter.SocialStanding.HasValue && npc.SocialStanding != filter.SocialStanding.Value)
            return false;

        if (filter.StoryRole.HasValue && npc.StoryRole != filter.StoryRole.Value)
            return false;

        if (filter.KnowledgeLevel.HasValue && npc.KnowledgeLevel != filter.KnowledgeLevel.Value)
            return false;

        return true;
    }

    private RouteOption FindMatchingRoute(PlacementFilter filter)
    {
        List<RouteOption> matchingRoutes = _gameWorld.Routes
            .Where(route => RouteMatchesFilter(route, filter))
            .ToList();

        if (matchingRoutes.Count == 0)
            return null;

        return ApplyRouteSelectionStrategy(matchingRoutes, filter.SelectionStrategy);
    }

    private bool RouteMatchesFilter(RouteOption route, PlacementFilter filter)
    {
        // ORTHOGONAL property - terrain type (now strongly typed enum)
        if (filter.Terrain.HasValue)
        {
            string dominantTerrain = route.GetDominantTerrainType();
            if (dominantTerrain != filter.Terrain.Value.ToString())
                return false;
        }

        if (filter.MinDifficulty.HasValue && route.DangerRating < filter.MinDifficulty.Value)
            return false;
        if (filter.MaxDifficulty.HasValue && route.DangerRating > filter.MaxDifficulty.Value)
            return false;

        return true;
    }

    // ========== LOCATION-SCOPED FIND (SameLocation Proximity) ==========

    private NPC FindNPCAtLocation(Location location, PlacementFilter filter)
    {
        List<NPC> npcsAtLocation = _gameWorld.NPCs
            .Where(npc => npc.Location == location && NPCMatchesFilter(npc, filter))
            .ToList();

        Console.WriteLine($"[EntityResolver] SameLocation: Found {npcsAtLocation.Count} NPCs at location '{location.Name}'");

        if (npcsAtLocation.Count == 0)
            return null;

        return ApplyNPCSelectionStrategy(npcsAtLocation, filter.SelectionStrategy);
    }

    private RouteOption FindRouteFromLocation(Location location, PlacementFilter filter)
    {
        List<RouteOption> routesFromLocation = _gameWorld.Routes
            .Where(route => route.OriginLocation == location && RouteMatchesFilter(route, filter))
            .ToList();

        Console.WriteLine($"[EntityResolver] SameLocation: Found {routesFromLocation.Count} routes from location '{location.Name}'");

        if (routesFromLocation.Count == 0)
            return null;

        return ApplyRouteSelectionStrategy(routesFromLocation, filter.SelectionStrategy);
    }

    // ========== SELECTION STRATEGIES ==========

    private Location ApplyLocationSelectionStrategy(List<Location> locations, PlacementSelectionStrategy strategy)
    {
        if (locations.Count == 0)
            return null;
        if (locations.Count == 1)
            return locations[0];

        // DDR-007: All selection is deterministic from categorical properties
        switch (strategy)
        {
            case PlacementSelectionStrategy.LeastRecent:
                return locations.OrderBy(loc => loc.VisitCount).ThenBy(loc => loc.Name).First();

            case PlacementSelectionStrategy.First:
            default:
                // Sort by Name for deterministic selection from categorical properties
                return locations.OrderBy(loc => loc.Name).First();
        }
    }

    /// <summary>
    /// DDR-007: All selection is deterministic from categorical properties.
    /// No Random - selection based on entity state (RelationshipFlow, StoryCubes, Name).
    /// </summary>
    private NPC ApplyNPCSelectionStrategy(List<NPC> npcs, PlacementSelectionStrategy strategy)
    {
        if (npcs.Count == 0)
            return null;
        if (npcs.Count == 1)
            return npcs[0];

        switch (strategy)
        {
            case PlacementSelectionStrategy.HighestBond:
                // Primary: highest bond, Secondary: Name for determinism
                return npcs.OrderByDescending(npc => npc.RelationshipFlow).ThenBy(npc => npc.Name).First();

            case PlacementSelectionStrategy.LeastRecent:
                // Primary: least interaction, Secondary: Name for determinism
                return npcs.OrderBy(npc => npc.StoryCubes).ThenBy(npc => npc.Name).First();

            case PlacementSelectionStrategy.First:
            default:
                // Sort by Name for deterministic selection
                return npcs.OrderBy(npc => npc.Name).First();
        }
    }

    /// <summary>
    /// DDR-007: All selection is deterministic from categorical properties.
    /// No Random - selection based on route properties (Name).
    /// </summary>
    private RouteOption ApplyRouteSelectionStrategy(List<RouteOption> routes, PlacementSelectionStrategy strategy)
    {
        if (routes.Count == 0)
            return null;
        if (routes.Count == 1)
            return routes[0];

        // DDR-007: All strategies use deterministic Name-based selection
        // Route has no interaction history, so all strategies fall back to Name ordering
        return routes.OrderBy(r => r.Name).First();
    }
}
