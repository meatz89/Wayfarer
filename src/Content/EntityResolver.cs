/// <summary>
/// System 4: Entity Resolver
/// Finds existing entities OR creates new ones from categorical specifications
/// Receives PlacementFilterDTO from JSON, returns concrete entity objects
/// Separates entity resolution from scene instantiation (5-system architecture)
///
/// VENUE-SCOPED RESOLUTION:
/// - Searches ONLY within the specified venue (no cross-venue teleportation)
/// - Ensures all situation entities exist within the same physical location group
/// - Creates dependent entities within the same venue if not found
/// </summary>
public class EntityResolver
{
    private readonly GameWorld _gameWorld;
    private readonly Player _player;
    private readonly SceneNarrativeService _narrativeService;
    private readonly Venue? _currentVenue;

    public EntityResolver(GameWorld gameWorld, Player player, SceneNarrativeService narrativeService, Venue? currentVenue = null)
    {
        _gameWorld = gameWorld;
        _player = player;
        _narrativeService = narrativeService;
        _currentVenue = currentVenue;
    }

    /// <summary>
    /// Find existing Location OR create new Location from categorical specification
    /// NEVER returns null - always returns existing or newly created Location
    /// Supports Proximity-based resolution: SameLocation returns contextLocation directly
    /// </summary>
    public Location FindOrCreateLocation(PlacementFilter filter, Location contextLocation = null)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter), "PlacementFilter cannot be null - scenes must specify location placement requirements");

        // Handle proximity-based resolution FIRST (before categorical search)
        if (filter.Proximity == PlacementProximity.SameLocation)
        {
            if (contextLocation == null)
                throw new InvalidOperationException("SameLocation proximity requires contextLocation - situation must have spawn context");
            return contextLocation;
        }

        // Standard categorical search
        Location existingLocation = FindMatchingLocation(filter);
        if (existingLocation != null)
            return existingLocation;

        // No match found - generate new location from categories
        Location newLocation = CreateLocationFromCategories(filter);
        _gameWorld.Locations.Add(newLocation);
        return newLocation;
    }

    /// <summary>
    /// Find existing NPC OR create new NPC from categorical specification
    /// NEVER returns null - always returns existing or newly created NPC
    /// Supports Proximity-based resolution: SameLocation searches NPCs AT contextLocation
    /// </summary>
    public NPC FindOrCreateNPC(PlacementFilter filter, Location contextLocation = null)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter), "PlacementFilter cannot be null - scenes must specify NPC placement requirements");

        // Handle proximity-based resolution FIRST (before categorical search)
        if (filter.Proximity == PlacementProximity.SameLocation)
        {
            if (contextLocation == null)
                throw new InvalidOperationException("SameLocation proximity requires contextLocation - situation must have spawn context");
            // Search NPCs AT this specific location
            return FindOrCreateNPCAtLocation(contextLocation, filter);
        }

        // Standard categorical search
        NPC existingNPC = FindMatchingNPC(filter);
        if (existingNPC != null)
            return existingNPC;

        // No match found - generate new NPC from categories
        NPC newNPC = CreateNPCFromCategories(filter);
        _gameWorld.NPCs.Add(newNPC);
        return newNPC;
    }

    /// <summary>
    /// Find existing RouteOption OR create new RouteOption from categorical specification
    /// NEVER returns null - always returns existing or newly created RouteOption
    /// Supports Proximity-based resolution: SameLocation searches Routes FROM contextLocation
    /// </summary>
    public RouteOption FindOrCreateRoute(PlacementFilter filter, Location contextLocation = null)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter), "PlacementFilter cannot be null - scenes must specify route placement requirements");

        // Handle proximity-based resolution FIRST (before categorical search)
        if (filter.Proximity == PlacementProximity.SameLocation)
        {
            if (contextLocation == null)
                throw new InvalidOperationException("SameLocation proximity requires contextLocation - situation must have spawn context");
            // Search routes originating FROM this specific location
            return FindOrCreateRouteFromLocation(contextLocation, filter);
        }

        // Standard categorical search
        RouteOption existingRoute = FindMatchingRoute(filter);
        if (existingRoute != null)
            return existingRoute;

        // No match found - generate new route from categories
        RouteOption newRoute = CreateRouteFromCategories(filter);
        _gameWorld.Routes.Add(newRoute);
        return newRoute;
    }

    /// <summary>
    /// Find existing Item by name
    /// Returns null if item not found - Items are immutable content definitions, NOT generated
    /// </summary>
    public Item FindItemByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
            return null;

        return _gameWorld.Items.FirstOrDefault(i => i.Name == itemName);
    }

    // ========== FIND EXISTING ENTITIES ==========

    private Location FindMatchingLocation(PlacementFilter filter)
    {
        // VENUE-SCOPED SEARCH: When currentVenue provided, search only within that venue
        // GLOBAL SEARCH: When currentVenue is null (parse-time), search all locations
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
            Console.WriteLine($"[EntityResolver] Found {matchingLocations.Count} matching locations (global search, no venue constraint)");
        }

        // Apply selection strategy if multiple matches
        if (matchingLocations.Count == 0)
            return null;

        return ApplySelectionStrategy(matchingLocations, filter.SelectionStrategy);
    }

    private bool LocationMatchesFilter(Location loc, PlacementFilter filter)
    {
        // Check location capabilities (if specified)
        if (filter.RequiredCapabilities != LocationCapability.None)
        {
            // Location must have ALL specified capabilities (bitwise AND check)
            if ((loc.Capabilities & filter.RequiredCapabilities) != filter.RequiredCapabilities)
                return false;
        }

        // Check location type (if specified)
        if (filter.LocationTypes != null && filter.LocationTypes.Count > 0)
        {
            if (!filter.LocationTypes.Contains(loc.LocationType))
                return false;
        }

        // Check orthogonal categorical dimensions
        // Empty list = don't filter, Non-empty = location must match ONE OF the values

        if (filter.PrivacyLevels != null && filter.PrivacyLevels.Count > 0)
        {
            if (!filter.PrivacyLevels.Contains(loc.Privacy))
                return false;
        }

        if (filter.SafetyLevels != null && filter.SafetyLevels.Count > 0)
        {
            if (!filter.SafetyLevels.Contains(loc.Safety))
                return false;
        }

        if (filter.ActivityLevels != null && filter.ActivityLevels.Count > 0)
        {
            if (!filter.ActivityLevels.Contains(loc.Activity))
                return false;
        }

        if (filter.Purposes != null && filter.Purposes.Count > 0)
        {
            if (!filter.Purposes.Contains(loc.Purpose))
                return false;
        }

        // Check accessibility requirements
        if (filter.IsPlayerAccessible.HasValue && filter.IsPlayerAccessible.Value)
        {
            // Check if location has been visited by player
            // NEW ARCHITECTURE: Accessibility controlled by LocationAccessibilityService query
            if (!loc.HasBeenVisited)
                return false;
        }

        return true;
    }

    private NPC FindMatchingNPC(PlacementFilter filter)
    {
        // VENUE-SCOPED SEARCH: When currentVenue provided, search only NPCs within that venue
        // GLOBAL SEARCH: When currentVenue is null (parse-time), search all NPCs
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
            Console.WriteLine($"[EntityResolver] Found {matchingNPCs.Count} matching NPCs (global search, no venue constraint)");
        }

        // Apply selection strategy if multiple matches
        if (matchingNPCs.Count == 0)
            return null;

        return ApplySelectionStrategy(matchingNPCs, filter.SelectionStrategy);
    }

    private bool NPCMatchesFilter(NPC npc, PlacementFilter filter)
    {
        // Check personality type (if specified)
        if (filter.PersonalityTypes != null && filter.PersonalityTypes.Count > 0)
        {
            if (!filter.PersonalityTypes.Contains(npc.PersonalityType))
                return false;
        }

        // Check profession (if specified)
        if (filter.Professions != null && filter.Professions.Count > 0)
        {
            if (!filter.Professions.Contains(npc.Profession))
                return false;
        }

        // Check relationship state (if specified)
        if (filter.RequiredRelationships != null && filter.RequiredRelationships.Count > 0)
        {
            if (!filter.RequiredRelationships.Contains(npc.PlayerRelationship))
                return false;
        }

        // Check orthogonal categorical dimensions
        // Empty list = don't filter, Non-empty = NPC must match ONE OF the values

        if (filter.SocialStandings != null && filter.SocialStandings.Count > 0)
        {
            if (!filter.SocialStandings.Contains(npc.SocialStanding))
                return false;
        }

        if (filter.StoryRoles != null && filter.StoryRoles.Count > 0)
        {
            if (!filter.StoryRoles.Contains(npc.StoryRole))
                return false;
        }

        if (filter.KnowledgeLevels != null && filter.KnowledgeLevels.Count > 0)
        {
            if (!filter.KnowledgeLevels.Contains(npc.KnowledgeLevel))
                return false;
        }

        // Check tier (if specified)
        if (filter.MinTier.HasValue && npc.Tier < filter.MinTier.Value)
            return false;
        if (filter.MaxTier.HasValue && npc.Tier > filter.MaxTier.Value)
            return false;

        return true;
    }

    private RouteOption FindMatchingRoute(PlacementFilter filter)
    {
        RouteOption matchingRoute = _gameWorld.Routes
            .FirstOrDefault(route => RouteMatchesFilter(route, filter));

        return matchingRoute;
    }

    private bool RouteMatchesFilter(RouteOption route, PlacementFilter filter)
    {
        // Check terrain types (dominant terrain from TerrainCategories)
        if (filter.TerrainTypes != null && filter.TerrainTypes.Count > 0)
        {
            string dominantTerrain = route.GetDominantTerrainType();
            if (!filter.TerrainTypes.Contains(dominantTerrain))
                return false;
        }

        // Check difficulty (using DangerRating)
        if (filter.MinDifficulty.HasValue && route.DangerRating < filter.MinDifficulty.Value)
            return false;
        if (filter.MaxDifficulty.HasValue && route.DangerRating > filter.MaxDifficulty.Value)
            return false;

        return true;
    }

    // ========== LOCATION-SCOPED ENTITY RESOLUTION (SameLocation Proximity) ==========

    /// <summary>
    /// Find NPC AT specific location, or create new NPC placed at that location
    /// Used when Proximity = SameLocation - semantic: "find/create NPC at THIS location"
    /// </summary>
    private NPC FindOrCreateNPCAtLocation(Location location, PlacementFilter filter)
    {
        // Search NPCs specifically AT this location
        List<NPC> npcsAtLocation = _gameWorld.NPCs
            .Where(npc => npc.Location == location && NPCMatchesFilter(npc, filter))
            .ToList();

        Console.WriteLine($"[EntityResolver] SameLocation: Found {npcsAtLocation.Count} NPCs at location '{location.Name}'");

        if (npcsAtLocation.Count > 0)
            return ApplySelectionStrategy(npcsAtLocation, filter.SelectionStrategy);

        // No NPC at this location - create new one and place AT this location
        NPC newNPC = CreateNPCFromCategories(filter);
        newNPC.Location = location; // CRITICAL: Place NPC AT this location
        _gameWorld.NPCs.Add(newNPC);

        Console.WriteLine($"[EntityResolver] SameLocation: Created new NPC '{newNPC.Name}' at location '{location.Name}'");
        return newNPC;
    }

    /// <summary>
    /// Find Route FROM specific location, or create new Route originating from that location
    /// Used when Proximity = SameLocation - semantic: "find/create Route FROM THIS location"
    /// </summary>
    private RouteOption FindOrCreateRouteFromLocation(Location location, PlacementFilter filter)
    {
        // Search routes originating FROM this location
        List<RouteOption> routesFromLocation = _gameWorld.Routes
            .Where(route => route.OriginLocation == location && RouteMatchesFilter(route, filter))
            .ToList();

        Console.WriteLine($"[EntityResolver] SameLocation: Found {routesFromLocation.Count} routes from location '{location.Name}'");

        if (routesFromLocation.Count > 0)
            return ApplySelectionStrategy(routesFromLocation, filter.SelectionStrategy);

        // No route from this location - create new one originating FROM this location
        RouteOption newRoute = CreateRouteFromCategories(filter);
        newRoute.OriginLocation = location; // CRITICAL: Route originates FROM this location
        _gameWorld.Routes.Add(newRoute);

        Console.WriteLine($"[EntityResolver] SameLocation: Created new route '{newRoute.Name}' from location '{location.Name}'");
        return newRoute;
    }

    /// <summary>
    /// Apply selection strategy for RouteOption candidates
    /// </summary>
    private RouteOption ApplySelectionStrategy(List<RouteOption> routes, PlacementSelectionStrategy? strategy)
    {
        if (routes.Count == 0)
            return null;
        if (routes.Count == 1)
            return routes[0];

        strategy = strategy ?? PlacementSelectionStrategy.First;

        switch (strategy.Value)
        {
            case PlacementSelectionStrategy.Random:
                return routes[Random.Shared.Next(routes.Count)];

            case PlacementSelectionStrategy.First:
            default:
                return routes.First();
        }
    }

    // ========== CREATE NEW ENTITIES ==========

    private Location CreateLocationFromCategories(PlacementFilter filter)
    {
        string locationName = GenerateLocationName(filter);

        // ADR-007: Constructor uses Name only (no Id parameter or generation)
        Location newLocation = new Location(locationName)
        {
            IsSkeleton = false,
            HexPosition = null,

            // Apply categorical properties
            Capabilities = filter.RequiredCapabilities,
            LocationType = filter.LocationTypes?.FirstOrDefault() ?? LocationTypes.Generic,
            Privacy = filter.PrivacyLevels?.FirstOrDefault() ?? LocationPrivacy.Public,
            Safety = filter.SafetyLevels?.FirstOrDefault() ?? LocationSafety.Neutral,
            Activity = filter.ActivityLevels?.FirstOrDefault() ?? LocationActivity.Moderate,
            Purpose = filter.Purposes?.FirstOrDefault() ?? LocationPurpose.Generic,
            Tier = filter.MinTier ?? 1,

            // Initialize required properties
            HasBeenVisited = false,
            VisitCount = 0,
            Familiarity = 0,
            MaxFamiliarity = 3,
            FlowModifier = 0,
            InvestigationCubes = 0
        };

        return newLocation;
    }

    private NPC CreateNPCFromCategories(PlacementFilter filter)
    {
        NPC newNPC = new NPC
        {
            Name = GenerateNPCName(filter),
            IsSkeleton = false,

            // Apply categorical properties
            PersonalityType = filter.PersonalityTypes?.FirstOrDefault() ?? PersonalityType.Neutral,
            Profession = filter.Professions?.FirstOrDefault() ?? Professions.Commoner,
            Tier = filter.MinTier ?? 1,
            Level = filter.MinTier ?? 1,

            // Initialize required properties
            PlayerRelationship = NPCRelationship.Neutral,
            RelationshipFlow = 12, // NEUTRAL with 0 flow
            StoryCubes = 0,
            Crisis = CrisisType.None,
            ConversationDifficulty = 1,
            Role = "Generated NPC",
            Description = "A person you've encountered"
        };

        return newNPC;
    }

    private RouteOption CreateRouteFromCategories(PlacementFilter filter)
    {
        RouteOption newRoute = new RouteOption
        {
            Name = GenerateRouteName(filter),

            // Apply categorical properties from filter
            DangerRating = filter.MinDifficulty ?? 1,
            BaseStaminaCost = (filter.MinDifficulty ?? 1) * 5, // Scale with difficulty

            // Initialize required properties
            Method = TravelMethods.Walking,
            BaseCoinCost = 0,
            TravelTimeSegments = 1
        };

        return newRoute;
    }

    // ========== SELECTION STRATEGIES ==========

    private Location ApplySelectionStrategy(List<Location> locations, PlacementSelectionStrategy? strategy)
    {
        if (locations.Count == 0)
            return null;
        if (locations.Count == 1)
            return locations[0];

        strategy = strategy ?? PlacementSelectionStrategy.First;

        switch (strategy.Value)
        {
            case PlacementSelectionStrategy.Closest:
                // Return location closest to player current position
                return locations.OrderBy(loc =>
                    CalculateDistance(loc.HexPosition, _player.CurrentPosition)).First();

            case PlacementSelectionStrategy.LeastRecent:
                // Return least recently visited location
                return locations.OrderBy(loc => loc.VisitCount).First();

            case PlacementSelectionStrategy.Random:
                // Uniform random selection from all matching candidates
                return RandomLocation(locations);

            case PlacementSelectionStrategy.First:
            default:
                return locations.First();
        }
    }

    private NPC ApplySelectionStrategy(List<NPC> npcs, PlacementSelectionStrategy? strategy)
    {
        if (npcs.Count == 0)
            return null;
        if (npcs.Count == 1)
            return npcs[0];

        strategy = strategy ?? PlacementSelectionStrategy.First;

        switch (strategy.Value)
        {
            case PlacementSelectionStrategy.HighestBond:
                // Return NPC with highest relationship flow
                return npcs.OrderByDescending(npc => npc.RelationshipFlow).First();

            case PlacementSelectionStrategy.LeastRecent:
                // Return NPC with lowest story cubes (least interaction)
                return npcs.OrderBy(npc => npc.StoryCubes).First();

            case PlacementSelectionStrategy.Random:
                // Uniform random selection from all matching candidates
                return RandomNPC(npcs);

            case PlacementSelectionStrategy.First:
            default:
                return npcs.First();
        }
    }

    // ========== NAME GENERATION ==========

    private string GenerateLocationName(PlacementFilter filter)
    {
        return _narrativeService.GenerateLocationName(filter);
    }

    private string GenerateNPCName(PlacementFilter filter)
    {
        return _narrativeService.GenerateNPCName(filter);
    }

    private string GenerateRouteName(PlacementFilter filter)
    {
        return _narrativeService.GenerateRouteName(filter);
    }

    // ========== HELPER METHODS ==========

    private int CalculateDistance(AxialCoordinates? hexPosition, AxialCoordinates playerPosition)
    {
        if (!hexPosition.HasValue)
            return int.MaxValue;

        return hexPosition.Value.DistanceTo(playerPosition);
    }

    private Location RandomLocation(List<Location> locations)
    {
        return locations[Random.Shared.Next(locations.Count)];
    }

    private NPC RandomNPC(List<NPC> npcs)
    {
        return npcs[Random.Shared.Next(npcs.Count)];
    }
}
