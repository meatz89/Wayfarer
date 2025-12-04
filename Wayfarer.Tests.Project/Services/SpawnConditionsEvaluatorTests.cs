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
        bool result = evaluator.EvaluateAll(SpawnConditions.AlwaysEligible);

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
        bool result = evaluator.EvaluateAll(SpawnConditions.AlwaysEligible);

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
        Assert.Throws<ArgumentNullException>(() => evaluator.EvaluateAll(null));
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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

        // Assert
        Assert.True(result);
    }

    // LocationVisits tests DELETED - §8.30: Feature removed (used string IDs instead of object refs)

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

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
        bool result = evaluator.EvaluateAll(conditions);

        // Assert
        Assert.True(result);
    }

    // ========== ENTITYSTATECONDITIONS TESTS ==========
    // NPCBond, RouteTravelCount, LocationReputation tests DELETED - §8.30: Features removed (used string IDs instead of object refs)
    // EntityStateConditions now only contains Properties field

    [Fact]
    public void EvaluateAll_EntityState_EmptyProperties_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        SpawnConditionsEvaluator evaluator = CreateEvaluator(world);

        SpawnConditions conditions = new SpawnConditions
        {
            EntityState = new EntityStateConditions
            {
                // Empty Properties = no requirements
            },
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions);

        // Assert
        Assert.True(result);
    }

    // ========== COMBINATIONLOGIC TESTS ==========
    // Tests updated to not use NPCBond (deleted per §8.30)
    // EntityState with empty Properties always passes, so combination logic tested via PlayerState + WorldState

    [Fact]
    public void EvaluateAll_AND_BothDimensionsPass_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Rain;

        Player player = CreatePlayer(world);
        player.Scales.Morality = 10;

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
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions);

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
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions);

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
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_AND_BothDimensionsFail_ReturnsFalse()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Clear; // Fails

        Player player = CreatePlayer(world);
        player.Scales.Morality = 2; // Fails

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
            CombinationLogic = CombinationLogic.AND
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateAll_OR_BothDimensionsPass_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Rain;

        Player player = CreatePlayer(world);
        player.Scales.Morality = 10;

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
            CombinationLogic = CombinationLogic.OR
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_OR_OnlyPlayerStatePasses_ReturnsTrue()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.CurrentWeather = WeatherCondition.Clear; // Fails

        Player player = CreatePlayer(world);
        player.Scales.Morality = 10; // Passes

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
            CombinationLogic = CombinationLogic.OR
        };

        // Act
        bool result = evaluator.EvaluateAll(conditions);

        // Assert
        Assert.True(result);
    }

}
