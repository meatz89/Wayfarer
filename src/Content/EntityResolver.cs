/// <summary>
/// System 4: Entity Resolver
/// Finds existing entities OR creates new ones from categorical specifications
/// Receives PlacementFilterDTO from JSON, returns concrete entity objects
/// Separates entity resolution from scene instantiation (5-system architecture)
/// </summary>
public class EntityResolver
{
    private readonly GameWorld _gameWorld;
    private readonly Player _player;
    private readonly SceneNarrativeService _narrativeService;

    public EntityResolver(GameWorld gameWorld, Player player, SceneNarrativeService narrativeService)
    {
        _gameWorld = gameWorld;
        _player = player;
        _narrativeService = narrativeService;
    }

    /// <summary>
    /// Find existing Location OR create new Location from categorical specification
    /// NEVER returns null - always returns existing or newly created Location
    /// </summary>
    public Location FindOrCreateLocation(PlacementFilter filter)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter), "PlacementFilter cannot be null - scenes must specify location placement requirements");

        // Try to find existing location matching categories
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
    /// </summary>
    public NPC FindOrCreateNPC(PlacementFilter filter)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter), "PlacementFilter cannot be null - scenes must specify NPC placement requirements");

        // Try to find existing NPC matching categories
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
    /// </summary>
    public RouteOption FindOrCreateRoute(PlacementFilter filter)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter), "PlacementFilter cannot be null - scenes must specify route placement requirements");

        // Try to find existing route matching categories
        RouteOption existingRoute = FindMatchingRoute(filter);
        if (existingRoute != null)
            return existingRoute;

        // No match found - generate new route from categories
        RouteOption newRoute = CreateRouteFromCategories(filter);
        _gameWorld.Routes.Add(newRoute);
        return newRoute;
    }

    // ========== FIND EXISTING ENTITIES ==========

    private Location FindMatchingLocation(PlacementFilter filter)
    {
        List<Location> matchingLocations = _gameWorld.Locations.Where(loc =>
        {
            // Check location properties (if specified)
            if (filter.LocationProperties != null && filter.LocationProperties.Count > 0)
            {
                // Location must have ALL specified properties
                if (!filter.LocationProperties.All(prop => loc.LocationProperties.Contains(prop)))
                    return false;
            }

            // Check location type (if specified)
            if (filter.LocationTypes != null && filter.LocationTypes.Count > 0)
            {
                if (!filter.LocationTypes.Contains(loc.LocationType))
                    return false;
            }

            // Check accessibility requirements
            if (filter.IsPlayerAccessible.HasValue && filter.IsPlayerAccessible.Value)
            {
                // Check if location is physically accessible to player
                if (!loc.HasBeenVisited && loc.IsLocked)
                    return false;
            }

            return true;
        }).ToList();

        // Apply selection strategy if multiple matches
        if (matchingLocations.Count == 0)
            return null;

        return ApplySelectionStrategy(matchingLocations, filter.SelectionStrategy);
    }

    private NPC FindMatchingNPC(PlacementFilter filter)
    {
        List<NPC> matchingNPCs = _gameWorld.NPCs.Where(npc =>
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

            // Check tier (if specified)
            if (filter.MinTier.HasValue && npc.Tier < filter.MinTier.Value)
                return false;
            if (filter.MaxTier.HasValue && npc.Tier > filter.MaxTier.Value)
                return false;

            return true;
        }).ToList();

        // Apply selection strategy if multiple matches
        if (matchingNPCs.Count == 0)
            return null;

        return ApplySelectionStrategy(matchingNPCs, filter.SelectionStrategy);
    }

    private RouteOption FindMatchingRoute(PlacementFilter filter)
    {
        RouteOption matchingRoute = _gameWorld.Routes.FirstOrDefault(route =>
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
        });

        return matchingRoute;
    }

    // ========== CREATE NEW ENTITIES ==========

    private Location CreateLocationFromCategories(PlacementFilter filter)
    {
        Location newLocation = new Location
        {
            Id = $"generated_location_{Guid.NewGuid()}",
            Name = GenerateLocationName(filter),
            IsSkeleton = false,
            HexPosition = null,

            // Apply categorical properties
            LocationProperties = filter.LocationProperties ?? new List<LocationPropertyType>(),
            LocationType = filter.LocationTypes?.FirstOrDefault() ?? LocationTypes.Generic,
            Tier = filter.MinTier ?? 1,

            // Initialize required properties
            HasBeenVisited = false,
            VisitCount = 0,
            Familiarity = 0,
            MaxFamiliarity = 3,
            FlowModifier = 0,
            InvestigationCubes = 0,
            IsLocked = false
        };

        return newLocation;
    }

    private NPC CreateNPCFromCategories(PlacementFilter filter)
    {
        NPC newNPC = new NPC
        {
            ID = $"generated_npc_{Guid.NewGuid()}",
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
            Id = $"generated_route_{Guid.NewGuid()}",
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
