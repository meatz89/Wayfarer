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
    /// </summary>
    public Location FindOrCreateLocation(PlacementFilter filter)
    {
        if (filter == null)
            return null;

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
    /// </summary>
    public NPC FindOrCreateNPC(PlacementFilter filter)
    {
        if (filter == null)
            return null;

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
    /// </summary>
    public RouteOption FindOrCreateRoute(PlacementFilter filter)
    {
        if (filter == null)
            return null;

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

            // Check difficulty
            if (filter.MinDifficulty.HasValue && route.DifficultyRating < filter.MinDifficulty.Value)
                return false;
            if (filter.MaxDifficulty.HasValue && route.DifficultyRating > filter.MaxDifficulty.Value)
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
            Id = $"generated_location_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Name = GenerateLocationName(filter),
            IsSkeleton = false,

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
            ID = $"generated_npc_{Guid.NewGuid().ToString().Substring(0, 8)}",
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
            Id = $"generated_route_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Name = GenerateRouteName(filter),

            // Apply categorical properties from filter
            DifficultyRating = filter.MinDifficulty ?? 1,
            StaminaCost = (filter.MinDifficulty ?? 1) * 5, // Scale with difficulty

            // Initialize required properties
            HasBeenUsed = false,
            IsKnown = true // Generated routes are immediately known
        };

        return newRoute;
    }

    // ========== SELECTION STRATEGIES ==========

    private Location ApplySelectionStrategy(List<Location> locations, SelectionStrategy? strategy)
    {
        if (locations.Count == 0)
            return null;
        if (locations.Count == 1)
            return locations[0];

        strategy = strategy ?? SelectionStrategy.First;

        switch (strategy.Value)
        {
            case SelectionStrategy.Closest:
                // Return location closest to player current position
                return locations.OrderBy(loc =>
                    CalculateDistance(loc.HexPosition, _player.CurrentLocationId)).First();

            case SelectionStrategy.LeastRecent:
                // Return least recently visited location
                return locations.OrderBy(loc => loc.VisitCount).First();

            case SelectionStrategy.WeightedRandom:
                // Random selection weighted by familiarity (less familiar = higher weight)
                return WeightedRandomLocation(locations);

            case SelectionStrategy.First:
            default:
                return locations.First();
        }
    }

    private NPC ApplySelectionStrategy(List<NPC> npcs, SelectionStrategy? strategy)
    {
        if (npcs.Count == 0)
            return null;
        if (npcs.Count == 1)
            return npcs[0];

        strategy = strategy ?? SelectionStrategy.First;

        switch (strategy.Value)
        {
            case SelectionStrategy.HighestBond:
                // Return NPC with highest relationship flow
                return npcs.OrderByDescending(npc => npc.RelationshipFlow).First();

            case SelectionStrategy.LeastRecent:
                // Return NPC with lowest story cubes (least interaction)
                return npcs.OrderBy(npc => npc.StoryCubes).First();

            case SelectionStrategy.WeightedRandom:
                // Random selection weighted by relationship state
                return WeightedRandomNPC(npcs);

            case SelectionStrategy.First:
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

    private int CalculateDistance(AxialCoordinates? hexPosition, string playerLocationId)
    {
        if (!hexPosition.HasValue)
            return int.MaxValue;

        Location playerLocation = _gameWorld.Locations.FirstOrDefault(loc => loc.Id == playerLocationId);
        if (playerLocation?.HexPosition == null)
            return int.MaxValue;

        return hexPosition.Value.DistanceTo(playerLocation.HexPosition.Value);
    }

    private Location WeightedRandomLocation(List<Location> locations)
    {
        // Simple random for now - can be enhanced with sophisticated weighting
        return locations[Random.Shared.Next(locations.Count)];
    }

    private NPC WeightedRandomNPC(List<NPC> npcs)
    {
        // Simple random for now - can be enhanced with sophisticated weighting
        return npcs[Random.Shared.Next(npcs.Count)];
    }
}
