using Xunit;

/// <summary>
/// Comprehensive tests for SpawnConditionsEvaluator - game progression gating logic
/// Tests all three evaluation dimensions (PlayerState, WorldState, EntityState)
/// Tests combination logic (AND/OR) and sentinel values (AlwaysEligible)
/// Critical for preventing soft-locks and ensuring content appears when intended
/// </summary>
public class SpawnConditionsEvaluatorTests
{
    // ========== TEST INFRASTRUCTURE ==========

    private GameWorld CreateGameWorld()
    {
        GameWorld world = new GameWorld();
        world.Locations = new List<Location>();
        world.Routes = new List<RouteOption>();
        world.Items = new List<Item>();

        // Set default world state
        world.CurrentWeather = WeatherCondition.Clear;
        world.CurrentTimeBlock = TimeBlocks.Morning;
        world.CurrentDay = 5;

        return world;
    }

    private Player CreatePlayer(GameWorld world)
    {
        Player player = world.GetPlayer();
        player.Name = "Test Player";
        player.Inventory = new Inventory();

        // Initialize scales to 0
        player.Scales = new PlayerScales
        {
            Morality = 0,
            Lawfulness = 0,
            Method = 0,
            Caution = 0,
            Transparency = 0,
            Fame = 0
        };

        return player;
    }

    private SpawnConditionsEvaluator CreateEvaluator(GameWorld world)
    {
        return new SpawnConditionsEvaluator(world);
    }

    // ========== SENTINEL & NULL HANDLING TESTS (3 tests) ==========

    [Fact]
    public void EvaluateAll_AlwaysEligible_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        // Act
        bool result = evaluator.EvaluateAll(SpawnConditions.AlwaysEligible, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_AlwaysEligible_BypassesAllConditions()
    {
        // Arrange - Set world state that would fail all conditions
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Storm;
        world.CurrentTimeBlock = TimeBlocks.Evening;
        world.CurrentDay = 999;

        Player player = CreatePlayer(world);
        player.Scales.Morality = -100; // Would fail stat checks

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        // Act - AlwaysEligible should still return true
        bool result = evaluator.EvaluateAll(SpawnConditions.AlwaysEligible, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_NullConditions_ThrowsArgumentNullException()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => evaluator.EvaluateAll(null, player));
    }

    // ========== PLAYERSTATECONDIT IONS TESTS (12 tests) ==========

    [Fact]
    public void EvaluateAll_MinStats_SingleStatMeetsThreshold_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Scales.Morality = 10;

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                MinMorality = 5
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_MinStats_SingleStatBelowThreshold_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Scales.Morality = 3;

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                MinMorality = 5
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_MinStats_MultipleStatsAllMeet_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Scales.Morality = 10;
        player.Scales.Lawfulness = 8;
        player.Scales.Fame = 5;

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                MinMorality = 5,
                MinLawfulness = 7,
                MinFame = 5
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_MinStats_MultipleStatsOneFails_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Scales.Morality = 10;
        player.Scales.Lawfulness = 3; // Below threshold
        player.Scales.Fame = 5;

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                MinMorality = 5,
                MinLawfulness = 7,
                MinFame = 5
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_MinStats_EmptyDictionary_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                // DOMAIN COLLECTION PRINCIPLE: Explicit properties, all null by default (no requirements)
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_RequiredItems_SingleItemPossessed_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Item requiredItem = new Item { Name = "investigation_notes" };
        player.Inventory.AddItems(requiredItem, 1);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                RequiredItems = new List<string> { "investigation_notes" }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_RequiredItems_SingleItemNotPossessed_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        // Inventory is empty

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                RequiredItems = new List<string> { "investigation_notes" }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_RequiredItems_MultipleItemsAllPossessed_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Item item1 = new Item { Name = "investigation_notes" };
        Item item2 = new Item { Name = "key" };
        Item item3 = new Item { Name = "letter" };
        player.Inventory.AddItems(item1, 1);
        player.Inventory.AddItems(item2, 1);
        player.Inventory.AddItems(item3, 1);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                RequiredItems = new List<string> { "investigation_notes", "key", "letter" }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_RequiredItems_MultipleItemsOneMissing_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Item item1 = new Item { Name = "investigation_notes" };
        Item item3 = new Item { Name = "letter" };
        player.Inventory.AddItems(item1, 1);
        player.Inventory.AddItems(item3, 1);
        // Missing "key"

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                RequiredItems = new List<string> { "investigation_notes", "key", "letter" }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_RequiredItems_EmptyList_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                RequiredItems = new List<string>() // Empty
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_LocationVisits_FamiliarityMeetsThreshold_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location tavern = new Location("Tavern") { HexPosition = new AxialCoordinates(0, 0) };
        world.Locations.Add(tavern);

        Player player = CreatePlayer(world);
        player.SetLocationFamiliarity(tavern, 3); // High familiarity

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                LocationVisits = new List<LocationVisitEntry>
                {
                    new LocationVisitEntry { LocationId = "Tavern", VisitCount = 2 } // Require 2 visits (familiarity maps to visits)
                }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_LocationVisits_FamiliarityBelowThreshold_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location tavern = new Location("Tavern") { HexPosition = new AxialCoordinates(0, 0) };
        world.Locations.Add(tavern);

        Player player = CreatePlayer(world);
        player.SetLocationFamiliarity(tavern, 1); // Low familiarity

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                LocationVisits = new List<LocationVisitEntry>
                {
                    new LocationVisitEntry { LocationId = "Tavern", VisitCount = 3 } // Require 3 visits
                }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    // ========== WORLDSTATECONDITIONS TESTS (13 tests) ==========

    [Fact]
    public void EvaluateAll_Weather_MatchesRequired_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Rain;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                Weather = WeatherCondition.Rain
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_Weather_DoesNotMatch_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Clear;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                Weather = WeatherCondition.Rain
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_Weather_NullAllowsAny_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Storm;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                Weather = null // Any weather allowed
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_TimeBlock_MatchesRequired_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentTimeBlock = TimeBlocks.Evening;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                TimeBlock = TimeBlocks.Evening
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_TimeBlock_DoesNotMatch_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentTimeBlock = TimeBlocks.Morning;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                TimeBlock = TimeBlocks.Evening
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_TimeBlock_NullAllowsAny_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentTimeBlock = TimeBlocks.Evening;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                TimeBlock = null // Any time allowed
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_MinDay_CurrentDayEquals_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentDay = 7;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                MinDay = 7
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_MinDay_CurrentDayAbove_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentDay = 10;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                MinDay = 7
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_MinDay_CurrentDayBelow_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentDay = 5;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                MinDay = 7
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_MaxDay_CurrentDayEquals_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentDay = 14;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                MaxDay = 14
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_MaxDay_CurrentDayBelow_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentDay = 10;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                MaxDay = 14
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_MaxDay_CurrentDayAbove_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentDay = 20;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                MaxDay = 14
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_MinMaxDay_CurrentDayInRange_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentDay = 10;

        Player player = CreatePlayer(world);
        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            WorldState = new WorldStateConditions
            {
                MinDay = 7,
                MaxDay = 14
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    // ========== ENTITYSTATECONDITIONS TESTS (9 tests) ==========

    [Fact]
    public void EvaluateAll_NPCBond_MeetsThreshold_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Relationships.SetLevel("elena", 15);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            EntityState = new EntityStateConditions
            {
                NPCBond = new List<NPCBondEntry> { new NPCBondEntry { NpcId = "elena", BondStrength = 10 } }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_NPCBond_BelowThreshold_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Relationships.SetLevel("elena", 5);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            EntityState = new EntityStateConditions
            {
                NPCBond = new List<NPCBondEntry> { new NPCBondEntry { NpcId = "elena", BondStrength = 10 } }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_NPCBond_MultipleBondsAllMeet_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Relationships.SetLevel("elena", 15);
        player.Relationships.SetLevel("marcus", 12);
        player.Relationships.SetLevel("sara", 8);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            EntityState = new EntityStateConditions
            {
                NPCBond = new List<NPCBondEntry>
                {
                    new NPCBondEntry { NpcId = "elena", BondStrength = 10 },
                    new NPCBondEntry { NpcId = "marcus", BondStrength = 10 },
                    new NPCBondEntry { NpcId = "sara", BondStrength = 5 }
                }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_NPCBond_MultipleBondsOneFails_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Relationships.SetLevel("elena", 15);
        player.Relationships.SetLevel("marcus", 5); // Below threshold
        player.Relationships.SetLevel("sara", 8);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            EntityState = new EntityStateConditions
            {
                NPCBond = new List<NPCBondEntry>
                {
                    new NPCBondEntry { NpcId = "elena", BondStrength = 10 },
                    new NPCBondEntry { NpcId = "marcus", BondStrength = 10 },
                    new NPCBondEntry { NpcId = "sara", BondStrength = 5 }
                }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_NPCBond_EmptyDictionary_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            EntityState = new EntityStateConditions
            {
                // DOMAIN COLLECTION PRINCIPLE: NPCBond defaults to empty List (no requirements)
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_RouteTravelCount_FamiliarityMeetsThreshold_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        RouteOption route = new RouteOption { Name = "mountain_pass" };
        world.Routes.Add(route);

        Player player = CreatePlayer(world);
        player.SetRouteFamiliarity(route, 5); // High familiarity

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            EntityState = new EntityStateConditions
            {
                RouteTravelCount = new List<RouteTravelCountEntry>
                {
                    new RouteTravelCountEntry { RouteId = "mountain_pass", TravelCount = 3 }
                }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_RouteTravelCount_FamiliarityBelowThreshold_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        RouteOption route = new RouteOption { Name = "mountain_pass" };
        world.Routes.Add(route);

        Player player = CreatePlayer(world);
        player.SetRouteFamiliarity(route, 2); // Low familiarity

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            EntityState = new EntityStateConditions
            {
                RouteTravelCount = new List<RouteTravelCountEntry>
                {
                    new RouteTravelCountEntry { RouteId = "mountain_pass", TravelCount = 3 }
                }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_RouteTravelCount_RouteNotFound_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        // Route not added to world

        Player player = CreatePlayer(world);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            EntityState = new EntityStateConditions
            {
                RouteTravelCount = new List<RouteTravelCountEntry>
                {
                    new RouteTravelCountEntry { RouteId = "mountain_pass", TravelCount = 3 }
                }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_LocationReputation_AlwaysPasses_NotImplemented()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            EntityState = new EntityStateConditions
            {
                LocationReputation = new List<LocationReputationEntry>
                {
                    new LocationReputationEntry { LocationId = "market_square", ReputationScore = 10 } // Not implemented, should pass
                }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert - LocationReputation currently not implemented, always passes
        Assert.True(result);
    }

    // ========== COMBINATIONLOGIC TESTS (8 tests) ==========

    [Fact]
    public void EvaluateAll_AND_AllThreeDimensionsPass_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Rain;

        Player player = CreatePlayer(world);
        player.Scales.Morality = 10;
        player.Relationships.SetLevel("elena", 15);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                MinMorality = 5
            },
            WorldState = new WorldStateConditions
            {
                Weather = WeatherCondition.Rain
            },
            EntityState = new EntityStateConditions
            {
                NPCBond = new List<NPCBondEntry> { new NPCBondEntry { NpcId = "elena", BondStrength = 10 } }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_AND_PlayerStateFails_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Rain;

        Player player = CreatePlayer(world);
        player.Scales.Morality = 2; // Fails
        player.Relationships.SetLevel("elena", 15);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                MinMorality = 5
            },
            WorldState = new WorldStateConditions
            {
                Weather = WeatherCondition.Rain
            },
            EntityState = new EntityStateConditions
            {
                NPCBond = new List<NPCBondEntry> { new NPCBondEntry { NpcId = "elena", BondStrength = 10 } }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_AND_WorldStateFails_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Clear; // Fails

        Player player = CreatePlayer(world);
        player.Scales.Morality = 10;
        player.Relationships.SetLevel("elena", 15);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                MinMorality = 5
            },
            WorldState = new WorldStateConditions
            {
                Weather = WeatherCondition.Rain
            },
            EntityState = new EntityStateConditions
            {
                NPCBond = new List<NPCBondEntry> { new NPCBondEntry { NpcId = "elena", BondStrength = 10 } }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_AND_EntityStateFails_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Rain;

        Player player = CreatePlayer(world);
        player.Scales.Morality = 10;
        player.Relationships.SetLevel("elena", 5); // Fails

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                MinMorality = 5
            },
            WorldState = new WorldStateConditions
            {
                Weather = WeatherCondition.Rain
            },
            EntityState = new EntityStateConditions
            {
                NPCBond = new List<NPCBondEntry> { new NPCBondEntry { NpcId = "elena", BondStrength = 10 } }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_AND_TwoDimensionsFail_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Clear; // Fails

        Player player = CreatePlayer(world);
        player.Scales.Morality = 2; // Fails
        player.Relationships.SetLevel("elena", 15);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                MinMorality = 5
            },
            WorldState = new WorldStateConditions
            {
                Weather = WeatherCondition.Rain
            },
            EntityState = new EntityStateConditions
            {
                NPCBond = new List<NPCBondEntry> { new NPCBondEntry { NpcId = "elena", BondStrength = 10 } }
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_OR_AllThreeDimensionsPass_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Rain;

        Player player = CreatePlayer(world);
        player.Scales.Morality = 10;
        player.Relationships.SetLevel("elena", 15);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                MinMorality = 5
            },
            WorldState = new WorldStateConditions
            {
                Weather = WeatherCondition.Rain
            },
            EntityState = new EntityStateConditions
            {
                NPCBond = new List<NPCBondEntry> { new NPCBondEntry { NpcId = "elena", BondStrength = 10 } }
            },
            CombinationLogic = CombinationLogic.OR
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_OR_OnlyOneDimensionPasses_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Clear; // Fails

        Player player = CreatePlayer(world);
        player.Scales.Morality = 2; // Fails
        player.Relationships.SetLevel("elena", 15); // Passes

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                MinMorality = 5
            },
            WorldState = new WorldStateConditions
            {
                Weather = WeatherCondition.Rain
            },
            EntityState = new EntityStateConditions
            {
                NPCBond = new List<NPCBondEntry> { new NPCBondEntry { NpcId = "elena", BondStrength = 10 } }
            },
            CombinationLogic = CombinationLogic.OR
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_OR_AllDimensionsFail_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Clear; // Fails

        Player player = CreatePlayer(world);
        player.Scales.Morality = 2; // Fails
        player.Relationships.SetLevel("elena", 5); // Fails

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            PlayerState = new PlayerStateConditions
            {
                MinMorality = 5
            },
            WorldState = new WorldStateConditions
            {
                Weather = WeatherCondition.Rain
            },
            EntityState = new EntityStateConditions
            {
                NPCBond = new List<NPCBondEntry> { new NPCBondEntry { NpcId = "elena", BondStrength = 10 } }
            },
            CombinationLogic = CombinationLogic.OR
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions, player);

        // Assert
        Assert.False(result);
    }
}
