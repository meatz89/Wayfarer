using Xunit;

namespace Wayfarer.Tests.Content;

public class EntityResolverTests
{
    // ========== NULL FILTER VALIDATION ==========

    [Fact]
    public void FindOrCreateLocation_NullFilter_ThrowsArgumentNullException()
    {
        EntityResolver resolver = CreateEntityResolver();

        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            resolver.FindOrCreateLocation(null));

        Assert.Equal("filter", ex.ParamName);
        Assert.Contains("PlacementFilter cannot be null", ex.Message);
    }

    [Fact]
    public void FindOrCreateNPC_NullFilter_ThrowsArgumentNullException()
    {
        EntityResolver resolver = CreateEntityResolver();

        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            resolver.FindOrCreateNPC(null));

        Assert.Equal("filter", ex.ParamName);
        Assert.Contains("PlacementFilter cannot be null", ex.Message);
    }

    [Fact]
    public void FindOrCreateRoute_NullFilter_ThrowsArgumentNullException()
    {
        EntityResolver resolver = CreateEntityResolver();

        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            resolver.FindOrCreateRoute(null));

        Assert.Equal("filter", ex.ParamName);
        Assert.Contains("PlacementFilter cannot be null", ex.Message);
    }

    // ========== LOCATION RESOLUTION ==========

    [Fact]
    public void FindOrCreateLocation_ExistingLocationMatches_ReturnsExisting()
    {
        GameWorld gameWorld = CreateGameWorld();

        // Add existing location with specific properties
        Location existingLocation = new Location
        {
            Id = "existing_tavern",
            Name = "The Old Tavern",
            LocationType = LocationTypes.Tavern,
            LocationProperties = new List<LocationPropertyType> { LocationPropertyType.Public }
        };
        gameWorld.Locations.Add(existingLocation);

        EntityResolver resolver = CreateEntityResolver(gameWorld);

        // Create filter matching existing location
        PlacementFilter filter = new PlacementFilter
        {
            LocationTypes = new List<LocationTypes> { LocationTypes.Tavern },
            LocationProperties = new List<LocationPropertyType> { LocationPropertyType.Public }
        };

        Location result = resolver.FindOrCreateLocation(filter);

        Assert.NotNull(result);
        Assert.Equal(existingLocation.Id, result.Id);
        Assert.Same(existingLocation, result);
    }

    [Fact]
    public void FindOrCreateLocation_NoMatch_CreatesNewLocation()
    {
        GameWorld gameWorld = CreateGameWorld();
        EntityResolver resolver = CreateEntityResolver(gameWorld);

        // Filter that won't match anything
        PlacementFilter filter = new PlacementFilter
        {
            LocationTypes = new List<LocationTypes> { LocationTypes.Temple },
            LocationProperties = new List<LocationPropertyType> { LocationPropertyType.Secured }
        };

        int initialCount = gameWorld.Locations.Count;
        Location result = resolver.FindOrCreateLocation(filter);

        Assert.NotNull(result);
        Assert.Equal(initialCount + 1, gameWorld.Locations.Count);
        Assert.Contains(result, gameWorld.Locations);
        Assert.Equal(LocationTypes.Temple, result.LocationType);
        Assert.Contains(LocationPropertyType.Secured, result.LocationProperties);
    }

    [Fact]
    public void FindOrCreateLocation_GeneratesContextualName()
    {
        GameWorld gameWorld = CreateGameWorld();
        EntityResolver resolver = CreateEntityResolver(gameWorld);

        PlacementFilter filter = new PlacementFilter
        {
            LocationTypes = new List<LocationTypes> { LocationTypes.Market },
            LocationProperties = new List<LocationPropertyType> { LocationPropertyType.Commercial }
        };

        Location result = resolver.FindOrCreateLocation(filter);

        Assert.NotNull(result.Name);
        Assert.Contains("Market", result.Name);
    }

    // ========== NPC RESOLUTION ==========

    [Fact]
    public void FindOrCreateNPC_ExistingNPCMatches_ReturnsExisting()
    {
        GameWorld gameWorld = CreateGameWorld();

        // Add existing NPC with specific properties
        NPC existingNPC = new NPC
        {
            ID = "existing_merchant",
            Name = "Shrewd Trader",
            PersonalityType = PersonalityType.MERCANTILE,
            Profession = Professions.Merchant,
            Tier = 2
        };
        gameWorld.NPCs.Add(existingNPC);

        EntityResolver resolver = CreateEntityResolver(gameWorld);

        // Create filter matching existing NPC
        PlacementFilter filter = new PlacementFilter
        {
            PersonalityTypes = new List<PersonalityType> { PersonalityType.MERCANTILE },
            Professions = new List<Professions> { Professions.Merchant },
            MinTier = 1,
            MaxTier = 3
        };

        NPC result = resolver.FindOrCreateNPC(filter);

        Assert.NotNull(result);
        Assert.Equal(existingNPC.ID, result.ID);
        Assert.Same(existingNPC, result);
    }

    [Fact]
    public void FindOrCreateNPC_NoMatch_CreatesNewNPC()
    {
        GameWorld gameWorld = CreateGameWorld();
        EntityResolver resolver = CreateEntityResolver(gameWorld);

        // Filter that won't match anything
        PlacementFilter filter = new PlacementFilter
        {
            PersonalityTypes = new List<PersonalityType> { PersonalityType.CUNNING },
            Professions = new List<Professions> { Professions.Scholar },
            MinTier = 3
        };

        int initialCount = gameWorld.NPCs.Count;
        NPC result = resolver.FindOrCreateNPC(filter);

        Assert.NotNull(result);
        Assert.Equal(initialCount + 1, gameWorld.NPCs.Count);
        Assert.Contains(result, gameWorld.NPCs);
        Assert.Equal(PersonalityType.CUNNING, result.PersonalityType);
        Assert.Equal(Professions.Scholar, result.Profession);
        Assert.Equal(3, result.Tier);
    }

    [Fact]
    public void FindOrCreateNPC_GeneratesContextualName()
    {
        GameWorld gameWorld = CreateGameWorld();
        EntityResolver resolver = CreateEntityResolver(gameWorld);

        PlacementFilter filter = new PlacementFilter
        {
            PersonalityTypes = new List<PersonalityType> { PersonalityType.DEVOTED },
            Professions = new List<Professions> { Professions.Healer }
        };

        NPC result = resolver.FindOrCreateNPC(filter);

        Assert.NotNull(result.Name);
        Assert.Contains("Healer", result.Name);
    }

    // ========== ROUTE RESOLUTION ==========

    [Fact]
    public void FindOrCreateRoute_ExistingRouteMatches_ReturnsExisting()
    {
        GameWorld gameWorld = CreateGameWorld();

        // Add existing route with specific properties
        RouteOption existingRoute = new RouteOption
        {
            Id = "existing_route",
            Name = "Forest Path",
            DangerRating = 2,
            TerrainCategories = new List<TerrainCategory> { TerrainCategory.Wilderness_Terrain }
        };
        gameWorld.Routes.Add(existingRoute);

        EntityResolver resolver = CreateEntityResolver(gameWorld);

        // Create filter matching existing route
        PlacementFilter filter = new PlacementFilter
        {
            TerrainTypes = new List<string> { "Wilderness_Terrain" },
            MinDifficulty = 1,
            MaxDifficulty = 3
        };

        RouteOption result = resolver.FindOrCreateRoute(filter);

        Assert.NotNull(result);
        Assert.Equal(existingRoute.Id, result.Id);
        Assert.Same(existingRoute, result);
    }

    [Fact]
    public void FindOrCreateRoute_NoMatch_CreatesNewRoute()
    {
        GameWorld gameWorld = CreateGameWorld();
        EntityResolver resolver = CreateEntityResolver(gameWorld);

        // Filter that won't match anything
        PlacementFilter filter = new PlacementFilter
        {
            TerrainTypes = new List<string> { "mountain" },
            MinDifficulty = 5
        };

        int initialCount = gameWorld.Routes.Count;
        RouteOption result = resolver.FindOrCreateRoute(filter);

        Assert.NotNull(result);
        Assert.Equal(initialCount + 1, gameWorld.Routes.Count);
        Assert.Contains(result, gameWorld.Routes);
        Assert.Equal(5, result.DangerRating);
    }

    [Fact]
    public void FindOrCreateRoute_GeneratesContextualName()
    {
        GameWorld gameWorld = CreateGameWorld();
        EntityResolver resolver = CreateEntityResolver(gameWorld);

        PlacementFilter filter = new PlacementFilter
        {
            TerrainTypes = new List<string> { "plains" }
        };

        RouteOption result = resolver.FindOrCreateRoute(filter);

        Assert.NotNull(result.Name);
        Assert.Contains("Road", result.Name);
    }

    // ========== SELECTION STRATEGIES ==========

    [Fact]
    public void ApplySelectionStrategy_First_ReturnsFirstLocation()
    {
        GameWorld gameWorld = CreateGameWorld();

        // Add multiple matching locations
        Location loc1 = new Location { Id = "loc1", Name = "Location 1", LocationType = LocationTypes.Inn };
        Location loc2 = new Location { Id = "loc2", Name = "Location 2", LocationType = LocationTypes.Inn };
        gameWorld.Locations.Add(loc1);
        gameWorld.Locations.Add(loc2);

        EntityResolver resolver = CreateEntityResolver(gameWorld);

        PlacementFilter filter = new PlacementFilter
        {
            LocationTypes = new List<LocationTypes> { LocationTypes.Inn },
            SelectionStrategy = SelectionStrategy.First
        };

        Location result = resolver.FindOrCreateLocation(filter);

        Assert.NotNull(result);
        Assert.Equal("loc1", result.Id);
    }

    [Fact]
    public void ApplySelectionStrategy_LeastRecent_ReturnsLowestVisitCount()
    {
        GameWorld gameWorld = CreateGameWorld();

        // Add locations with different visit counts
        Location loc1 = new Location { Id = "loc1", Name = "Location 1", LocationType = LocationTypes.Inn, VisitCount = 5 };
        Location loc2 = new Location { Id = "loc2", Name = "Location 2", LocationType = LocationTypes.Inn, VisitCount = 2 };
        Location loc3 = new Location { Id = "loc3", Name = "Location 3", LocationType = LocationTypes.Inn, VisitCount = 8 };
        gameWorld.Locations.Add(loc1);
        gameWorld.Locations.Add(loc2);
        gameWorld.Locations.Add(loc3);

        EntityResolver resolver = CreateEntityResolver(gameWorld);

        PlacementFilter filter = new PlacementFilter
        {
            LocationTypes = new List<LocationTypes> { LocationTypes.Inn },
            SelectionStrategy = SelectionStrategy.LeastRecent
        };

        Location result = resolver.FindOrCreateLocation(filter);

        Assert.NotNull(result);
        Assert.Equal("loc2", result.Id);
        Assert.Equal(2, result.VisitCount);
    }

    [Fact]
    public void ApplySelectionStrategy_WeightedRandom_ReturnsFromCandidates()
    {
        GameWorld gameWorld = CreateGameWorld();

        // Add multiple matching locations
        Location loc1 = new Location { Id = "loc1", Name = "Location 1", LocationType = LocationTypes.Tavern };
        Location loc2 = new Location { Id = "loc2", Name = "Location 2", LocationType = LocationTypes.Tavern };
        Location loc3 = new Location { Id = "loc3", Name = "Location 3", LocationType = LocationTypes.Tavern };
        gameWorld.Locations.Add(loc1);
        gameWorld.Locations.Add(loc2);
        gameWorld.Locations.Add(loc3);

        EntityResolver resolver = CreateEntityResolver(gameWorld);

        PlacementFilter filter = new PlacementFilter
        {
            LocationTypes = new List<LocationTypes> { LocationTypes.Tavern },
            SelectionStrategy = SelectionStrategy.WeightedRandom
        };

        // Run multiple times to verify randomness produces varied results
        Dictionary<string, int> selectionCounts = new Dictionary<string, int>();
        int iterations = 300;

        for (int i = 0; i < iterations; i++)
        {
            Location result = resolver.FindOrCreateLocation(filter);

            if (!selectionCounts.ContainsKey(result.Id))
            {
                selectionCounts[result.Id] = 0;
            }
            selectionCounts[result.Id]++;
        }

        // Verify all three locations were selected at least once
        Assert.Equal(3, selectionCounts.Count);
        Assert.Contains("loc1", selectionCounts.Keys);
        Assert.Contains("loc2", selectionCounts.Keys);
        Assert.Contains("loc3", selectionCounts.Keys);

        // Verify distribution is reasonable (each should be selected roughly 1/3 of the time)
        foreach (int count in selectionCounts.Values)
        {
            Assert.InRange(count, iterations / 6, iterations / 2);
        }
    }

    [Fact]
    public void ApplySelectionStrategy_HighestBond_ReturnsHighestRelationshipFlow()
    {
        GameWorld gameWorld = CreateGameWorld();

        // Add NPCs with different relationship flows
        NPC npc1 = new NPC { ID = "npc1", Name = "NPC 1", Profession = Professions.Merchant, RelationshipFlow = 10 };
        NPC npc2 = new NPC { ID = "npc2", Name = "NPC 2", Profession = Professions.Merchant, RelationshipFlow = 25 };
        NPC npc3 = new NPC { ID = "npc3", Name = "NPC 3", Profession = Professions.Merchant, RelationshipFlow = 15 };
        gameWorld.NPCs.Add(npc1);
        gameWorld.NPCs.Add(npc2);
        gameWorld.NPCs.Add(npc3);

        EntityResolver resolver = CreateEntityResolver(gameWorld);

        PlacementFilter filter = new PlacementFilter
        {
            Professions = new List<Professions> { Professions.Merchant },
            SelectionStrategy = SelectionStrategy.HighestBond
        };

        NPC result = resolver.FindOrCreateNPC(filter);

        Assert.NotNull(result);
        Assert.Equal("npc2", result.ID);
        Assert.Equal(25, result.RelationshipFlow);
    }

    // ========== HELPER METHODS ==========

    private EntityResolver CreateEntityResolver(GameWorld gameWorld = null)
    {
        gameWorld = gameWorld ?? CreateGameWorld();
        Player player = gameWorld.GetPlayer();
        SceneNarrativeService narrativeService = new SceneNarrativeService(gameWorld);

        return new EntityResolver(gameWorld, player, narrativeService);
    }

    private GameWorld CreateGameWorld()
    {
        return new GameWorld();
    }
}
