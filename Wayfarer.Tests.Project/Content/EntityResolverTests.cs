using Xunit;

namespace Wayfarer.Tests.Content;

/// <summary>
/// Comprehensive tests for EntityResolver FIND-ONLY pattern implementation.
/// EntityResolver now ONLY finds existing entities - returns null if not found.
/// Creation logic moved to SceneInstantiator which orchestrates find-or-create.
/// MANDATORY per CLAUDE.md: Complex algorithms require complete test coverage.
/// </summary>
public class EntityResolverTests
{
    [Fact]
    public void FindLocation_ExistingMatch_ReturnsExisting()
    {
        // Arrange: GameWorld with existing location matching filter
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

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
            Purpose = LocationPurpose.Dwelling,
            Privacy = LocationPrivacy.Public,
            Safety = LocationSafety.Safe
        };

        // Act
        Location result = resolver.FindLocation(filter, null);

        // Assert: Returns existing location
        Assert.Same(existingLocation, result);
    }

    [Fact]
    public void FindLocation_NoMatch_ReturnsNull()
    {
        // Arrange: GameWorld with NO matching location
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Purpose = LocationPurpose.Commerce,
            Privacy = LocationPrivacy.Public
        };

        // Act
        Location result = resolver.FindLocation(filter, null);

        // Assert: Returns null (find-only, caller decides what to do)
        Assert.Null(result);
    }

    [Fact]
    public void FindLocation_NullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => resolver.FindLocation(null, null)
        );
        Assert.Equal("filter", exception.ParamName);
        Assert.Contains("PlacementFilter cannot be null", exception.Message);
    }

    [Fact]
    public void FindLocation_EmptyWorld_ReturnsNull()
    {
        // Arrange: Empty world (no locations)
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Purpose = LocationPurpose.Worship
        };

        // Act
        Location result = resolver.FindLocation(filter, null);

        // Assert: Returns null (find-only)
        Assert.Null(result);
    }

    [Fact]
    public void FindLocation_MultipleMatches_AppliesSelectionStrategy()
    {
        // Arrange: Multiple locations matching filter
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

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
            Purpose = LocationPurpose.Commerce,
            SelectionStrategy = PlacementSelectionStrategy.LeastRecent
        };

        // Act
        Location result = resolver.FindLocation(filter, null);

        // Assert: Selects location2 (least recent = lowest VisitCount)
        Assert.Same(location2, result);
    }

    [Fact]
    public void FindNPC_ExistingMatch_ReturnsExisting()
    {
        // Arrange: GameWorld with existing NPC matching filter
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        NPC existingNPC = new NPC
        {
            Name = "Existing Guard",
            Profession = Professions.Guard,
            PersonalityType = PersonalityType.STEADFAST
        };
        world.NPCs.Add(existingNPC);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            Profession = Professions.Guard,
            PersonalityType = PersonalityType.STEADFAST
        };

        // Act
        NPC result = resolver.FindNPC(filter, null);

        // Assert: Returns existing NPC
        Assert.Same(existingNPC, result);
    }

    [Fact]
    public void FindNPC_NoMatch_ReturnsNull()
    {
        // Arrange: GameWorld with NO matching NPC
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            Profession = Professions.Merchant,
            PersonalityType = PersonalityType.MERCANTILE
        };

        // Act
        NPC result = resolver.FindNPC(filter, null);

        // Assert: Returns null (find-only, caller decides what to do)
        Assert.Null(result);
    }

    [Fact]
    public void FindNPC_NullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => resolver.FindNPC(null, null)
        );
        Assert.Equal("filter", exception.ParamName);
    }


    [Fact]
    public void FindNPC_RelationshipFilter_MatchesRelationship()
    {
        // Arrange: NPCs with different relationship states
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

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
            Profession = Professions.Merchant,
            RequiredRelationship = NPCRelationship.Allied
        };

        // Act
        NPC result = resolver.FindNPC(filter, null);

        // Assert: Finds friendly NPC
        Assert.Same(friendlyNPC, result);
    }

    [Fact]
    public void FindNPC_MultipleMatches_HighestBondStrategy()
    {
        // Arrange: Multiple NPCs matching filter
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

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
            Profession = Professions.Guard,
            SelectionStrategy = PlacementSelectionStrategy.HighestBond
        };

        // Act
        NPC result = resolver.FindNPC(filter, null);

        // Assert: Selects NPC2 (highest relationship flow)
        Assert.Same(npc2, result);
    }

    [Fact]
    public void FindRoute_ExistingMatch_ReturnsExisting()
    {
        // Arrange: GameWorld with existing route matching filter
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

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
        RouteOption result = resolver.FindRoute(filter, null);

        // Assert: Returns existing route
        Assert.Same(existingRoute, result);
    }

    [Fact]
    public void FindRoute_NoMatch_ReturnsNull()
    {
        // Arrange: GameWorld with NO matching route
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Route,
            MinDifficulty = 15
        };

        // Act
        RouteOption result = resolver.FindRoute(filter, null);

        // Assert: Returns null (find-only)
        Assert.Null(result);
    }

    [Fact]
    public void FindRoute_NullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => resolver.FindRoute(null, null)
        );
        Assert.Equal("filter", exception.ParamName);
    }

    [Fact]
    public void FindRoute_DifficultyRange_MatchesRange()
    {
        // Arrange: Routes with different danger ratings
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

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
        RouteOption result = resolver.FindRoute(filter, null);

        // Assert: Finds medium route (within range 20-40)
        Assert.Same(routeMedium, result);
    }

    [Fact]
    public void FindItemByName_ExistingItem_ReturnsItem()
    {
        // Arrange: GameWorld with existing item
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

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
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        // Act
        Item result = resolver.FindItemByName("Nonexistent Item");

        // Assert: Returns null (items are NOT generated)
        Assert.Null(result);
    }

    [Fact]
    public void FindItemByName_NullOrEmptyName_ReturnsNull()
    {
        // Arrange
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        // Act
        Item resultNull = resolver.FindItemByName(null);
        Item resultEmpty = resolver.FindItemByName("");

        // Assert
        Assert.Null(resultNull);
        Assert.Null(resultEmpty);
    }

    [Fact]
    public void FindLocation_MultipleFilters_AllMustMatch()
    {
        // Arrange: Location matching some but not all filters
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

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
            Purpose = LocationPurpose.Commerce,
            Privacy = LocationPrivacy.Public,
            Safety = LocationSafety.Safe // Requires Safe
        };

        // Act
        Location result = resolver.FindLocation(filter, null);

        // Assert: Returns null (partial match doesn't satisfy filter)
        Assert.Null(result);
    }

    [Fact]
    public void FindNPC_OrthogonalDimensions_ComposesCorrectly()
    {
        // Arrange: NPC with orthogonal categorical dimensions
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

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
            Profession = Professions.Guard,
            SocialStanding = NPCSocialStanding.Notable,
            StoryRole = NPCStoryRole.Obstacle,
            KnowledgeLevel = NPCKnowledgeLevel.Informed
        };

        // Act
        NPC result = resolver.FindNPC(filter, null);

        // Assert: Finds NPC matching all orthogonal dimensions
        Assert.Same(npc, result);
    }

    [Fact]
    public void FindLocation_AccessibilityFilter_OnlyVisited()
    {
        // Arrange: Locations with different visited states
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

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
            Purpose = LocationPurpose.Commerce,
            IsPlayerAccessible = true // Requires visited
        };

        // Act
        Location result = resolver.FindLocation(filter, null);

        // Assert: Finds visited location only
        Assert.Same(visitedLocation, result);
    }

    [Fact]
    public void FindLocation_RandomSelection_PicksFromMultiple()
    {
        // Arrange: Multiple matching locations
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        Location location1 = new Location("Location 1") { Purpose = LocationPurpose.Commerce };
        Location location2 = new Location("Location 2") { Purpose = LocationPurpose.Commerce };
        Location location3 = new Location("Location 3") { Purpose = LocationPurpose.Commerce };

        world.Locations.Add(location1);
        world.Locations.Add(location2);
        world.Locations.Add(location3);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Purpose = LocationPurpose.Commerce,
            SelectionStrategy = PlacementSelectionStrategy.First
        };

        // Act: Call multiple times, check deterministic behavior (DDR-007 compliance)
        Location firstResult = resolver.FindLocation(filter, null);
        Location secondResult = resolver.FindLocation(filter, null);

        // Assert: DDR-007 - same filter produces same result (deterministic)
        Assert.NotNull(firstResult);
        Assert.Equal(firstResult, secondResult);
    }

    [Fact]
    public void FindNPC_EmptyWorld_ReturnsNull()
    {
        // Arrange: Empty world
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            Profession = Professions.Scholar
        };

        // Act
        NPC result = resolver.FindNPC(filter, null);

        // Assert: Returns null (find-only)
        Assert.Null(result);
    }

    [Fact]
    public void FindRoute_EmptyWorld_ReturnsNull()
    {
        // Arrange: Empty world
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Route,
            MinDifficulty = 10
        };

        // Act
        RouteOption result = resolver.FindRoute(filter, null);

        // Assert: Returns null (find-only)
        Assert.Null(result);
    }

    [Fact]
    public void FindLocation_FirstSelectionStrategy_ReturnsFirst()
    {
        // Arrange: Multiple matches
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        Location location1 = new Location("First") { Purpose = LocationPurpose.Commerce };
        Location location2 = new Location("Second") { Purpose = LocationPurpose.Commerce };

        world.Locations.Add(location1);
        world.Locations.Add(location2);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Purpose = LocationPurpose.Commerce,
            SelectionStrategy = PlacementSelectionStrategy.First
        };

        // Act
        Location result = resolver.FindLocation(filter, null);

        // Assert: Returns first match
        Assert.Same(location1, result);
    }

    [Fact]
    public void FindLocation_SameLocationProximity_ReturnsContextLocation()
    {
        // Arrange: Context location for SameLocation proximity
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        Location contextLocation = new Location("Context Location")
        {
            Purpose = LocationPurpose.Commerce
        };
        world.Locations.Add(contextLocation);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Proximity = PlacementProximity.SameLocation
        };

        // Act
        Location result = resolver.FindLocation(filter, contextLocation);

        // Assert: Returns context location directly
        Assert.Same(contextLocation, result);
    }

    [Fact]
    public void FindLocation_SameLocationProximity_NullContext_Throws()
    {
        // Arrange: No context location for SameLocation proximity
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Proximity = PlacementProximity.SameLocation
        };

        // Act & Assert: Should throw because context is required
        Assert.Throws<InvalidOperationException>(() => resolver.FindLocation(filter, null));
    }

    [Fact]
    public void FindNPC_SameLocationProximity_FindsNPCAtContextLocation()
    {
        // Arrange: NPCs at different locations
        (GameWorld world, EntityResolver resolver) = CreateTestContext();

        Location contextLocation = new Location("Context");
        Location otherLocation = new Location("Other");
        world.Locations.Add(contextLocation);
        world.Locations.Add(otherLocation);

        NPC npcAtContext = new NPC
        {
            Name = "NPC at Context",
            Location = contextLocation,
            Profession = Professions.Guard
        };

        NPC npcElsewhere = new NPC
        {
            Name = "NPC Elsewhere",
            Location = otherLocation,
            Profession = Professions.Guard
        };

        world.NPCs.Add(npcAtContext);
        world.NPCs.Add(npcElsewhere);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            Proximity = PlacementProximity.SameLocation,
            Profession = Professions.Guard
        };

        // Act
        NPC result = resolver.FindNPC(filter, contextLocation);

        // Assert: Finds NPC at context location
        Assert.Same(npcAtContext, result);
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Create test context with GameWorld and EntityResolver (find-only).
    /// EntityResolver no longer requires Player or NarrativeService.
    /// </summary>
    private (GameWorld world, EntityResolver resolver) CreateTestContext()
    {
        GameWorld world = new GameWorld();
        EntityResolver resolver = new EntityResolver(world);

        return (world, resolver);
    }
}
