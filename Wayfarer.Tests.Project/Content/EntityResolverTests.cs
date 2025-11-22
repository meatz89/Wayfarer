using Xunit;

namespace Wayfarer.Tests.Content;

/// <summary>
/// Comprehensive tests for EntityResolver FindOrCreate pattern implementation.
/// Tests critical entity resolution: finding existing entities via categorical filters,
/// creating new entities when no match exists, selection strategies, filter matching logic.
/// MANDATORY per CLAUDE.md: Complex algorithms require complete test coverage.
/// </summary>
public class EntityResolverTests
{
    [Fact]
    public void FindOrCreateLocation_ExistingMatch_ReturnsExisting()
    {
        // Arrange: GameWorld with existing location matching filter
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        Location existingLocation = new Location("Existing Tavern")
        {
            Purpose = LocationPurpose.Dwelling,
            Privacy = LocationPrivacy.Public,
            Safety = LocationSafety.Safe
        };
        world.Locations.Add(existingLocation);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Purposes = new List<LocationPurpose> { LocationPurpose.Dwelling },
            PrivacyLevels = new List<LocationPrivacy> { LocationPrivacy.Public },
            SafetyLevels = new List<LocationSafety> { LocationSafety.Safe }
        };

        // Act
        Location result = resolver.FindOrCreateLocation(filter);

        // Assert: Returns existing location, does not create new
        Assert.Same(existingLocation, result);
        Assert.Equal(1, world.Locations.Count); // No new location created
    }

    [Fact]
    public void FindOrCreateLocation_NoMatch_CreatesNew()
    {
        // Arrange: GameWorld with NO matching location
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Purposes = new List<LocationPurpose> { LocationPurpose.Commerce },
            PrivacyLevels = new List<LocationPrivacy> { LocationPrivacy.Public }
        };

        // Act
        Location result = resolver.FindOrCreateLocation(filter);

        // Assert: Creates new location
        Assert.NotNull(result);
        Assert.Equal(LocationPurpose.Commerce, result.Purpose);
        Assert.Equal(LocationPrivacy.Public, result.Privacy);
        Assert.Equal(1, world.Locations.Count); // New location added
        Assert.Contains(result, world.Locations);
    }

    [Fact]
    public void FindOrCreateLocation_NullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => resolver.FindOrCreateLocation(null)
        );
        Assert.Equal("filter", exception.ParamName);
        Assert.Contains("PlacementFilter cannot be null", exception.Message);
    }

    [Fact]
    public void FindOrCreateLocation_EmptyWorld_CreatesNew()
    {
        // Arrange: Empty world (no locations)
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Purposes = new List<LocationPurpose> { LocationPurpose.Worship }
        };

        // Act
        Location result = resolver.FindOrCreateLocation(filter);

        // Assert: Creates new location
        Assert.NotNull(result);
        Assert.Equal(LocationPurpose.Worship, result.Purpose);
        Assert.Single(world.Locations);
    }

    [Fact]
    public void FindOrCreateLocation_MultipleMatches_AppliesSelectionStrategy()
    {
        // Arrange: Multiple locations matching filter
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        Location location1 = new Location("Location 1")
        {
            Purpose = LocationPurpose.Commerce,
            VisitCount = 5
        };

        Location location2 = new Location("Location 2")
        {
            Purpose = LocationPurpose.Commerce,
            VisitCount = 2 // Less recently visited
        };

        world.Locations.Add(location1);
        world.Locations.Add(location2);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Purposes = new List<LocationPurpose> { LocationPurpose.Commerce },
            SelectionStrategy = PlacementSelectionStrategy.LeastRecent
        };

        // Act
        Location result = resolver.FindOrCreateLocation(filter);

        // Assert: Selects location2 (least recent = lowest VisitCount)
        Assert.Same(location2, result);
    }

    [Fact]
    public void FindOrCreateLocation_CapabilitiesFilter_MatchesFlags()
    {
        // Arrange: Location with specific capabilities
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        Location locationWithCapabilities = new Location("Crossroads Market")
        {
            Capabilities = LocationCapability.Crossroads | LocationCapability.Commercial
        };

        Location locationWithoutCapabilities = new Location("Simple Room")
        {
            Capabilities = LocationCapability.None
        };

        world.Locations.Add(locationWithCapabilities);
        world.Locations.Add(locationWithoutCapabilities);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            RequiredCapabilities = LocationCapability.Crossroads | LocationCapability.Commercial
        };

        // Act
        Location result = resolver.FindOrCreateLocation(filter);

        // Assert: Finds location with required capabilities
        Assert.Same(locationWithCapabilities, result);
    }

    [Fact]
    public void FindOrCreateNPC_ExistingMatch_ReturnsExisting()
    {
        // Arrange: GameWorld with existing NPC matching filter
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        NPC existingNPC = new NPC
        {
            Name = "Existing Guard",
            Profession = Professions.Guard,
            PersonalityType = PersonalityType.STEADFAST,
            Tier = 2
        };
        world.NPCs.Add(existingNPC);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            Professions = new List<Professions> { Professions.Guard },
            PersonalityTypes = new List<PersonalityType> { PersonalityType.STEADFAST }
        };

        // Act
        NPC result = resolver.FindOrCreateNPC(filter);

        // Assert: Returns existing NPC
        Assert.Same(existingNPC, result);
        Assert.Equal(1, world.NPCs.Count);
    }

    [Fact]
    public void FindOrCreateNPC_NoMatch_CreatesNew()
    {
        // Arrange: GameWorld with NO matching NPC
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            Professions = new List<Professions> { Professions.Merchant },
            PersonalityTypes = new List<PersonalityType> { PersonalityType.MERCANTILE }
        };

        // Act
        NPC result = resolver.FindOrCreateNPC(filter);

        // Assert: Creates new NPC
        Assert.NotNull(result);
        Assert.Equal(Professions.Merchant, result.Profession);
        Assert.Equal(PersonalityType.MERCANTILE, result.PersonalityType);
        Assert.Single(world.NPCs);
        Assert.Contains(result, world.NPCs);
    }

    [Fact]
    public void FindOrCreateNPC_NullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => resolver.FindOrCreateNPC(null)
        );
        Assert.Equal("filter", exception.ParamName);
    }

    [Fact]
    public void FindOrCreateNPC_TierFilter_MatchesRange()
    {
        // Arrange: NPCs with different tiers
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        NPC npcTier1 = new NPC { Name = "Novice", Tier = 1, Profession = Professions.Guard };
        NPC npcTier3 = new NPC { Name = "Veteran", Tier = 3, Profession = Professions.Guard };
        NPC npcTier5 = new NPC { Name = "Elite", Tier = 5, Profession = Professions.Guard };

        world.NPCs.Add(npcTier1);
        world.NPCs.Add(npcTier3);
        world.NPCs.Add(npcTier5);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            Professions = new List<Professions> { Professions.Guard },
            MinTier = 2,
            MaxTier = 4
        };

        // Act
        NPC result = resolver.FindOrCreateNPC(filter);

        // Assert: Finds tier 3 NPC (within range 2-4)
        Assert.Same(npcTier3, result);
    }

    [Fact]
    public void FindOrCreateNPC_RelationshipFilter_MatchesRelationship()
    {
        // Arrange: NPCs with different relationship states
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        NPC friendlyNPC = new NPC
        {
            Name = "Friendly Merchant",
            Profession = Professions.Merchant,
            PlayerRelationship = NPCRelationship.Allied
        };

        NPC neutralNPC = new NPC
        {
            Name = "Neutral Merchant",
            Profession = Professions.Merchant,
            PlayerRelationship = NPCRelationship.Neutral
        };

        world.NPCs.Add(friendlyNPC);
        world.NPCs.Add(neutralNPC);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            Professions = new List<Professions> { Professions.Merchant },
            RequiredRelationships = new List<NPCRelationship> { NPCRelationship.Allied }
        };

        // Act
        NPC result = resolver.FindOrCreateNPC(filter);

        // Assert: Finds friendly NPC
        Assert.Same(friendlyNPC, result);
    }

    [Fact]
    public void FindOrCreateNPC_MultipleMatches_HighestBondStrategy()
    {
        // Arrange: Multiple NPCs matching filter
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        NPC npc1 = new NPC
        {
            Name = "NPC 1",
            Profession = Professions.Guard,
            RelationshipFlow = 15 // Lower bond
        };

        NPC npc2 = new NPC
        {
            Name = "NPC 2",
            Profession = Professions.Guard,
            RelationshipFlow = 22 // Higher bond
        };

        world.NPCs.Add(npc1);
        world.NPCs.Add(npc2);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            Professions = new List<Professions> { Professions.Guard },
            SelectionStrategy = PlacementSelectionStrategy.HighestBond
        };

        // Act
        NPC result = resolver.FindOrCreateNPC(filter);

        // Assert: Selects NPC2 (highest relationship flow)
        Assert.Same(npc2, result);
    }

    [Fact]
    public void FindOrCreateRoute_ExistingMatch_ReturnsExisting()
    {
        // Arrange: GameWorld with existing route matching filter
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        RouteOption existingRoute = new RouteOption
        {
            Name = "Existing Route",
            DangerRating = 20
        };
        world.Routes.Add(existingRoute);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Route,
            MinDifficulty = 10,
            MaxDifficulty = 30
        };

        // Act
        RouteOption result = resolver.FindOrCreateRoute(filter);

        // Assert: Returns existing route
        Assert.Same(existingRoute, result);
        Assert.Equal(1, world.Routes.Count);
    }

    [Fact]
    public void FindOrCreateRoute_NoMatch_CreatesNew()
    {
        // Arrange: GameWorld with NO matching route
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Route,
            MinDifficulty = 15
        };

        // Act
        RouteOption result = resolver.FindOrCreateRoute(filter);

        // Assert: Creates new route
        Assert.NotNull(result);
        Assert.Equal(15, result.DangerRating); // Uses MinDifficulty
        Assert.Equal(75, result.BaseStaminaCost); // 15 * 5
        Assert.Single(world.Routes);
    }

    [Fact]
    public void FindOrCreateRoute_NullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => resolver.FindOrCreateRoute(null)
        );
        Assert.Equal("filter", exception.ParamName);
    }

    [Fact]
    public void FindOrCreateRoute_DifficultyRange_MatchesRange()
    {
        // Arrange: Routes with different danger ratings
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        RouteOption routeEasy = new RouteOption { Name = "Easy", DangerRating = 5 };
        RouteOption routeMedium = new RouteOption { Name = "Medium", DangerRating = 25 };
        RouteOption routeHard = new RouteOption { Name = "Hard", DangerRating = 50 };

        world.Routes.Add(routeEasy);
        world.Routes.Add(routeMedium);
        world.Routes.Add(routeHard);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Route,
            MinDifficulty = 20,
            MaxDifficulty = 40
        };

        // Act
        RouteOption result = resolver.FindOrCreateRoute(filter);

        // Assert: Finds medium route (within range 20-40)
        Assert.Same(routeMedium, result);
    }

    [Fact]
    public void FindItemByName_ExistingItem_ReturnsItem()
    {
        // Arrange: GameWorld with existing item
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        Item existingItem = new Item { Name = "Test Item" };
        world.Items.Add(existingItem);

        // Act
        Item result = resolver.FindItemByName("Test Item");

        // Assert
        Assert.Same(existingItem, result);
    }

    [Fact]
    public void FindItemByName_NoMatch_ReturnsNull()
    {
        // Arrange: GameWorld without matching item
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        // Act
        Item result = resolver.FindItemByName("Nonexistent Item");

        // Assert: Returns null (items are NOT generated)
        Assert.Null(result);
    }

    [Fact]
    public void FindItemByName_NullOrEmptyName_ReturnsNull()
    {
        // Arrange
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        // Act
        Item resultNull = resolver.FindItemByName(null);
        Item resultEmpty = resolver.FindItemByName("");

        // Assert
        Assert.Null(resultNull);
        Assert.Null(resultEmpty);
    }

    [Fact]
    public void FindOrCreateLocation_MultipleFilters_AllMustMatch()
    {
        // Arrange: Location matching some but not all filters
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        Location partialMatch = new Location("Partial")
        {
            Purpose = LocationPurpose.Commerce,
            Privacy = LocationPrivacy.Public,
            Safety = LocationSafety.Dangerous // Does NOT match filter
        };

        world.Locations.Add(partialMatch);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Purposes = new List<LocationPurpose> { LocationPurpose.Commerce },
            PrivacyLevels = new List<LocationPrivacy> { LocationPrivacy.Public },
            SafetyLevels = new List<LocationSafety> { LocationSafety.Safe } // Requires Safe
        };

        // Act
        Location result = resolver.FindOrCreateLocation(filter);

        // Assert: Creates new location (partial match doesn't satisfy filter)
        Assert.NotSame(partialMatch, result);
        Assert.Equal(2, world.Locations.Count); // Partial match + new creation
    }

    [Fact]
    public void FindOrCreateNPC_OrthogonalDimensions_ComposesCorrectly()
    {
        // Arrange: NPC with orthogonal categorical dimensions
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        NPC npc = new NPC
        {
            Name = "Gatekeeper",
            Profession = Professions.Guard,
            SocialStanding = NPCSocialStanding.Notable,
            StoryRole = NPCStoryRole.Obstacle,
            KnowledgeLevel = NPCKnowledgeLevel.Informed
        };

        world.NPCs.Add(npc);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            Professions = new List<Professions> { Professions.Guard },
            SocialStandings = new List<NPCSocialStanding> { NPCSocialStanding.Notable },
            StoryRoles = new List<NPCStoryRole> { NPCStoryRole.Obstacle },
            KnowledgeLevels = new List<NPCKnowledgeLevel> { NPCKnowledgeLevel.Informed }
        };

        // Act
        NPC result = resolver.FindOrCreateNPC(filter);

        // Assert: Finds NPC matching all orthogonal dimensions
        Assert.Same(npc, result);
    }

    [Fact]
    public void FindOrCreateLocation_AccessibilityFilter_OnlyVisited()
    {
        // Arrange: Locations with different visited states
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        Location visitedLocation = new Location("Visited")
        {
            Purpose = LocationPurpose.Commerce,
            HasBeenVisited = true
        };

        Location unvisitedLocation = new Location("Unvisited")
        {
            Purpose = LocationPurpose.Commerce,
            HasBeenVisited = false
        };

        world.Locations.Add(visitedLocation);
        world.Locations.Add(unvisitedLocation);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Purposes = new List<LocationPurpose> { LocationPurpose.Commerce },
            IsPlayerAccessible = true // Requires visited
        };

        // Act
        Location result = resolver.FindOrCreateLocation(filter);

        // Assert: Finds visited location only
        Assert.Same(visitedLocation, result);
    }

    [Fact]
    public void FindOrCreateLocation_RandomSelection_PicksFromMultiple()
    {
        // Arrange: Multiple matching locations
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        Location location1 = new Location("Location 1") { Purpose = LocationPurpose.Commerce };
        Location location2 = new Location("Location 2") { Purpose = LocationPurpose.Commerce };
        Location location3 = new Location("Location 3") { Purpose = LocationPurpose.Commerce };

        world.Locations.Add(location1);
        world.Locations.Add(location2);
        world.Locations.Add(location3);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Purposes = new List<LocationPurpose> { LocationPurpose.Commerce },
            SelectionStrategy = PlacementSelectionStrategy.Random
        };

        // Act: Call multiple times, check if different locations returned (statistical test)
        HashSet<Location> selectedLocations = new HashSet<Location>();
        for (int i = 0; i < 50; i++)
        {
            Location result = resolver.FindOrCreateLocation(filter);
            selectedLocations.Add(result);
        }

        // Assert: At least 2 different locations selected (randomness works)
        Assert.True(selectedLocations.Count >= 2, "Random selection should pick different locations");
    }

    [Fact]
    public void FindOrCreateNPC_EmptyWorld_CreatesNew()
    {
        // Arrange: Empty world
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            Professions = new List<Professions> { Professions.Scholar }
        };

        // Act
        NPC result = resolver.FindOrCreateNPC(filter);

        // Assert: Creates new NPC
        Assert.NotNull(result);
        Assert.Equal(Professions.Scholar, result.Profession);
        Assert.Single(world.NPCs);
    }

    [Fact]
    public void FindOrCreateRoute_EmptyWorld_CreatesNew()
    {
        // Arrange: Empty world
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Route,
            MinDifficulty = 10
        };

        // Act
        RouteOption result = resolver.FindOrCreateRoute(filter);

        // Assert: Creates new route
        Assert.NotNull(result);
        Assert.Single(world.Routes);
    }

    [Fact]
    public void FindOrCreateLocation_FirstSelectionStrategy_ReturnsFirst()
    {
        // Arrange: Multiple matches
        (GameWorld world, Player player, EntityResolver resolver) = CreateTestContext();

        Location location1 = new Location("First") { Purpose = LocationPurpose.Commerce };
        Location location2 = new Location("Second") { Purpose = LocationPurpose.Commerce };

        world.Locations.Add(location1);
        world.Locations.Add(location2);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Purposes = new List<LocationPurpose> { LocationPurpose.Commerce },
            SelectionStrategy = PlacementSelectionStrategy.First
        };

        // Act
        Location result = resolver.FindOrCreateLocation(filter);

        // Assert: Returns first match
        Assert.Same(location1, result);
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Create test context with GameWorld, Player, and EntityResolver
    /// </summary>
    private (GameWorld world, Player player, EntityResolver resolver) CreateTestContext()
    {
        GameWorld world = new GameWorld();
        Player player = new Player
        {
            Name = "TestPlayer",
            CurrentPosition = new AxialCoordinates(0, 0)
        };

        SceneNarrativeService narrativeService = new SceneNarrativeService(world);
        EntityResolver resolver = new EntityResolver(world, player, narrativeService);

        return (world, player, resolver);
    }
}
