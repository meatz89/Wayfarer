using Xunit;

/// <summary>
/// Comprehensive tests for DifficultyCalculationService.
/// Tests difficulty calculation across all modifier types: Understanding, Mastery, Familiarity, ConnectionTokens, HasItemCategory.
/// Critical for game balance: wrong thresholds make challenges too easy/hard and break progression.
///
/// Test Categories:
/// - Understanding Modifiers: Global mental expertise checks (5 tests)
/// - Mastery Modifiers: Physical expertise per challenge type (5 tests)
/// - Familiarity Modifiers: Location understanding (6 tests)
/// - ConnectionTokens Modifiers: NPC relationship strength (6 tests)
/// - HasItemCategory Modifiers: Equipment category presence (7 tests)
/// - Multiple Modifiers Combined: Cumulative effects (5 tests)
/// - Edge Cases: Null handling, clamping, empty lists (6 tests)
/// Total: 40 tests
/// </summary>
public class DifficultyCalculationServiceTests
{
    // ========== TEST INFRASTRUCTURE ==========

    private GameWorld CreateGameWorld()
    {
        GameWorld world = new GameWorld();
        world.Locations = new List<Location>();
        world.NPCs = new List<NPC>();
        world.Items = new List<Item>();
        return world;
    }

    private Player CreatePlayer(GameWorld world)
    {
        Player player = world.GetPlayer();
        player.Understanding = 0;
        player.Inventory = new Inventory(10);
        return player;
    }

    private Situation CreateSituation()
    {
        return new Situation
        {
            DifficultyModifiers = new List<DifficultyModifier>()
        };
    }

    private DifficultyCalculationService CreateService(GameWorld world)
    {
        return new DifficultyCalculationService(world);
    }

    private ItemRepository CreateItemRepository(GameWorld world)
    {
        return new ItemRepository(world);
    }

    // ========== CATEGORY 1: UNDERSTANDING MODIFIERS ==========

    [Fact]
    public void CalculateDifficulty_UnderstandingBelowThreshold_ModifierNotApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 1; // Below threshold of 2

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Understanding,
            Threshold = 2,
            Effect = -3
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(10, result.FinalDifficulty); // No reduction
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_UnderstandingAtThreshold_ModifierApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 2; // Exactly at threshold

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Understanding,
            Threshold = 2,
            Effect = -3
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(7, result.FinalDifficulty); // Reduced by 3
        Assert.Single(result.AppliedModifiers);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_UnderstandingAboveThreshold_ModifierApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 5; // Above threshold of 2

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Understanding,
            Threshold = 2,
            Effect = -3
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(7, result.FinalDifficulty); // Reduced by 3
        Assert.Single(result.AppliedModifiers);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_MultipleUnderstandingModifiers_AllApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 5; // Meets both thresholds

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Understanding,
            Threshold = 2,
            Effect = -2
        });
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Understanding,
            Threshold = 4,
            Effect = -3
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 15, itemRepo);

        // Assert
        Assert.Equal(15, result.BaseDifficulty);
        Assert.Equal(10, result.FinalDifficulty); // Reduced by 2 + 3 = 5
        Assert.Equal(2, result.AppliedModifiers.Count);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_UnderstandingPositiveEffect_IncreasesDifficulty()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 5;

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Understanding,
            Threshold = 2,
            Effect = +3 // Positive effect increases difficulty
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(13, result.FinalDifficulty); // Increased by 3
    }

    // ========== CATEGORY 2: MASTERY MODIFIERS ==========

    [Fact]
    public void CalculateDifficulty_MasteryBelowThreshold_ModifierNotApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.MasteryCubes.SetMastery("Combat", 1); // Below threshold of 2

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Mastery,
            Context = "Combat",
            Threshold = 2,
            Effect = -4
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 12, itemRepo);

        // Assert
        Assert.Equal(12, result.BaseDifficulty);
        Assert.Equal(12, result.FinalDifficulty); // No reduction
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_MasteryAtThreshold_ModifierApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.MasteryCubes.SetMastery("Combat", 2); // Exactly at threshold

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Mastery,
            Context = "Combat",
            Threshold = 2,
            Effect = -4
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 12, itemRepo);

        // Assert
        Assert.Equal(12, result.BaseDifficulty);
        Assert.Equal(8, result.FinalDifficulty); // Reduced by 4
        Assert.Single(result.AppliedModifiers);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_MasteryAboveThreshold_ModifierApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.MasteryCubes.SetMastery("Combat", 3); // Above threshold of 2

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Mastery,
            Context = "Combat",
            Threshold = 2,
            Effect = -4
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 12, itemRepo);

        // Assert
        Assert.Equal(12, result.BaseDifficulty);
        Assert.Equal(8, result.FinalDifficulty); // Reduced by 4
        Assert.Single(result.AppliedModifiers);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_MasteryWrongContext_ModifierNotApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.MasteryCubes.SetMastery("Athletics", 3); // Wrong context (Combat needed)

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Mastery,
            Context = "Combat",
            Threshold = 2,
            Effect = -4
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 12, itemRepo);

        // Assert
        Assert.Equal(12, result.BaseDifficulty);
        Assert.Equal(12, result.FinalDifficulty); // No reduction
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_MasteryCorrectContext_ModifierApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.MasteryCubes.SetMastery("Finesse", 2);

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Mastery,
            Context = "Finesse",
            Threshold = 2,
            Effect = -3
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(7, result.FinalDifficulty); // Reduced by 3
    }

    // ========== CATEGORY 3: FAMILIARITY MODIFIERS ==========

    [Fact]
    public void CalculateDifficulty_FamiliarityBelowThreshold_ModifierNotApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Location location = new Location("Mill");
        player.SetLocationFamiliarity(location, 1); // Below threshold of 2

        Situation situation = CreateSituation();
        situation.Location = location;
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Familiarity,
            Threshold = 2,
            Effect = -2
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 8, itemRepo);

        // Assert
        Assert.Equal(8, result.BaseDifficulty);
        Assert.Equal(8, result.FinalDifficulty); // No reduction
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_FamiliarityAtThreshold_ModifierApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Location location = new Location("Mill");
        player.SetLocationFamiliarity(location, 2); // Exactly at threshold

        Situation situation = CreateSituation();
        situation.Location = location;
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Familiarity,
            Threshold = 2,
            Effect = -2
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 8, itemRepo);

        // Assert
        Assert.Equal(8, result.BaseDifficulty);
        Assert.Equal(6, result.FinalDifficulty); // Reduced by 2
        Assert.Single(result.AppliedModifiers);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_FamiliarityAboveThreshold_ModifierApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Location location = new Location("Mill");
        player.SetLocationFamiliarity(location, 3); // Above threshold of 2

        Situation situation = CreateSituation();
        situation.Location = location;
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Familiarity,
            Threshold = 2,
            Effect = -2
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 8, itemRepo);

        // Assert
        Assert.Equal(8, result.BaseDifficulty);
        Assert.Equal(6, result.FinalDifficulty); // Reduced by 2
        Assert.Single(result.AppliedModifiers);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_SituationNoLocation_FamiliarityModifierNotApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Situation situation = CreateSituation();
        situation.Location = null; // No location
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Familiarity,
            Threshold = 2,
            Effect = -2
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 8, itemRepo);

        // Assert
        Assert.Equal(8, result.BaseDifficulty);
        Assert.Equal(8, result.FinalDifficulty); // No reduction
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_MultipleFamiliarityThresholds_PartiallyApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Location location = new Location("Mill");
        player.SetLocationFamiliarity(location, 2); // Meets first, not second

        Situation situation = CreateSituation();
        situation.Location = location;
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Familiarity,
            Threshold = 1,
            Effect = -1
        });
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Familiarity,
            Threshold = 3,
            Effect = -2
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(9, result.FinalDifficulty); // Reduced by 1 only
        Assert.Single(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_DifferentLocationsDifferentFamiliarity_CorrectApplication()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Location mill = new Location("Mill");
        Location tavern = new Location("Tavern");
        player.SetLocationFamiliarity(mill, 3);
        player.SetLocationFamiliarity(tavern, 1);

        Situation situation = CreateSituation();
        situation.Location = tavern; // Using tavern (familiarity 1)
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.Familiarity,
            Threshold = 2,
            Effect = -2
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 8, itemRepo);

        // Assert
        Assert.Equal(8, result.BaseDifficulty);
        Assert.Equal(8, result.FinalDifficulty); // No reduction (tavern familiarity too low)
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    // ========== CATEGORY 4: CONNECTION TOKENS MODIFIERS ==========

    [Fact]
    public void CalculateDifficulty_TokensBelowThreshold_ModifierNotApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        NPC npc = new NPC { Name = "Smith" };
        player.SetNPCTokenCount(npc, ConnectionType.Trust, 3); // Below threshold of 5

        Situation situation = CreateSituation();
        situation.Npc = npc;
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.ConnectionTokens,
            Threshold = 5,
            Effect = -4
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 12, itemRepo);

        // Assert
        Assert.Equal(12, result.BaseDifficulty);
        Assert.Equal(12, result.FinalDifficulty); // No reduction
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_TokensAtThreshold_ModifierApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        NPC npc = new NPC { Name = "Smith" };
        player.SetNPCTokenCount(npc, ConnectionType.Trust, 5); // Exactly at threshold

        Situation situation = CreateSituation();
        situation.Npc = npc;
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.ConnectionTokens,
            Threshold = 5,
            Effect = -4
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 12, itemRepo);

        // Assert
        Assert.Equal(12, result.BaseDifficulty);
        Assert.Equal(8, result.FinalDifficulty); // Reduced by 4
        Assert.Single(result.AppliedModifiers);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_TokensAboveThreshold_ModifierApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        NPC npc = new NPC { Name = "Smith" };
        player.SetNPCTokenCount(npc, ConnectionType.Trust, 8); // Above threshold of 5

        Situation situation = CreateSituation();
        situation.Npc = npc;
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.ConnectionTokens,
            Threshold = 5,
            Effect = -4
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 12, itemRepo);

        // Assert
        Assert.Equal(12, result.BaseDifficulty);
        Assert.Equal(8, result.FinalDifficulty); // Reduced by 4
        Assert.Single(result.AppliedModifiers);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_SituationNoNPC_ConnectionModifierNotApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Situation situation = CreateSituation();
        situation.Npc = null; // No NPC
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.ConnectionTokens,
            Threshold = 5,
            Effect = -4
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 12, itemRepo);

        // Assert
        Assert.Equal(12, result.BaseDifficulty);
        Assert.Equal(12, result.FinalDifficulty); // No reduction
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_MultipleTokenThresholds_PartiallyApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        NPC npc = new NPC { Name = "Smith" };
        player.SetNPCTokenCount(npc, ConnectionType.Trust, 7); // Meets first, not second

        Situation situation = CreateSituation();
        situation.Npc = npc;
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.ConnectionTokens,
            Threshold = 5,
            Effect = -2
        });
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.ConnectionTokens,
            Threshold = 10,
            Effect = -3
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 15, itemRepo);

        // Assert
        Assert.Equal(15, result.BaseDifficulty);
        Assert.Equal(13, result.FinalDifficulty); // Reduced by 2 only
        Assert.Single(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_DifferentNPCsDifferentTokens_CorrectApplication()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        NPC smith = new NPC { Name = "Smith" };
        NPC miller = new NPC { Name = "Miller" };
        player.SetNPCTokenCount(smith, ConnectionType.Trust, 10);
        player.SetNPCTokenCount(miller, ConnectionType.Trust, 2);

        Situation situation = CreateSituation();
        situation.Npc = miller; // Using miller (tokens 2)
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.ConnectionTokens,
            Threshold = 5,
            Effect = -4
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 12, itemRepo);

        // Assert
        Assert.Equal(12, result.BaseDifficulty);
        Assert.Equal(12, result.FinalDifficulty); // No reduction (miller tokens too low)
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    // ========== CATEGORY 5: HAS ITEM CATEGORY MODIFIERS ==========

    [Fact]
    public void CalculateDifficulty_PlayerNoItems_HasItemCategoryNotApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        // Inventory is empty

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.HasItemCategory,
            Context = "Light_Source",
            Effect = -2
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(10, result.FinalDifficulty); // No reduction
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_PlayerHasItemWithCategory_ModifierApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Item lantern = new Item
        {
            Name = "Lantern",
            Categories = new List<ItemCategory> { ItemCategory.Light_Source }
        };
        player.Inventory.SetItemCount(lantern, 1);

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.HasItemCategory,
            Context = "Light_Source",
            Effect = -2
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(8, result.FinalDifficulty); // Reduced by 2
        Assert.Single(result.AppliedModifiers);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_PlayerHasItemWrongCategory_ModifierNotApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Item bread = new Item
        {
            Name = "Bread",
            Categories = new List<ItemCategory> { ItemCategory.Hunger }
        };
        player.Inventory.SetItemCount(bread, 1);

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.HasItemCategory,
            Context = "Light_Source",
            Effect = -2
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(10, result.FinalDifficulty); // No reduction
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_PlayerMultipleItemsOneMatches_ModifierApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Item bread = new Item { Name = "Bread", Categories = new List<ItemCategory> { ItemCategory.Hunger } };
        Item compass = new Item { Name = "Compass", Categories = new List<ItemCategory> { ItemCategory.Navigation_Tools } };
        player.Inventory.SetItemCount(bread, 1);
        player.Inventory.SetItemCount(compass, 1);

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.HasItemCategory,
            Context = "Navigation_Tools",
            Effect = -3
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 12, itemRepo);

        // Assert
        Assert.Equal(12, result.BaseDifficulty);
        Assert.Equal(9, result.FinalDifficulty); // Reduced by 3
        Assert.Single(result.AppliedModifiers);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_InvalidCategoryString_ModifierNotApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Item lantern = new Item
        {
            Name = "Lantern",
            Categories = new List<ItemCategory> { ItemCategory.Light_Source }
        };
        player.Inventory.SetItemCount(lantern, 1);

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.HasItemCategory,
            Context = "InvalidCategory", // Not a valid ItemCategory
            Effect = -2
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(10, result.FinalDifficulty); // No reduction
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_ItemMultipleCategoriesOneMatches_ModifierApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Item multiTool = new Item
        {
            Name = "MultiTool",
            Categories = new List<ItemCategory> { ItemCategory.Tools, ItemCategory.Valuables }
        };
        player.Inventory.SetItemCount(multiTool, 1);

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.HasItemCategory,
            Context = "Tools",
            Effect = -2
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(8, result.FinalDifficulty); // Reduced by 2
        Assert.Single(result.AppliedModifiers);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_EmptyInventory_HasItemCategoryNotApplied()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        // Inventory empty (capacity 10, no items)

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = ModifierType.HasItemCategory,
            Context = "Light_Source",
            Effect = -2
        });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(10, result.FinalDifficulty); // No reduction
        Assert.Empty(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    // ========== CATEGORY 6: MULTIPLE MODIFIERS COMBINED ==========

    [Fact]
    public void CalculateDifficulty_AllModifiersMet_AllAppliedCumulativeReduction()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 5;
        player.MasteryCubes.SetMastery("Combat", 2);

        Location location = new Location("Mill");
        player.SetLocationFamiliarity(location, 2);

        Situation situation = CreateSituation();
        situation.Location = location;
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Understanding, Threshold = 2, Effect = -2 });
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Mastery, Context = "Combat", Threshold = 2, Effect = -3 });
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Familiarity, Threshold = 2, Effect = -1 });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 15, itemRepo);

        // Assert
        Assert.Equal(15, result.BaseDifficulty);
        Assert.Equal(9, result.FinalDifficulty); // Reduced by 2 + 3 + 1 = 6
        Assert.Equal(3, result.AppliedModifiers.Count);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_NoModifiersMet_NoneAppliedBaseDifficulty()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 0;
        player.MasteryCubes.SetMastery("Combat", 0);

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Understanding, Threshold = 5, Effect = -2 });
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Mastery, Context = "Combat", Threshold = 3, Effect = -3 });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 15, itemRepo);

        // Assert
        Assert.Equal(15, result.BaseDifficulty);
        Assert.Equal(15, result.FinalDifficulty); // No reduction
        Assert.Empty(result.AppliedModifiers);
        Assert.Equal(2, result.UnappliedModifiers.Count);
    }

    [Fact]
    public void CalculateDifficulty_SomeModifiersMet_PartialApplication()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 5; // Meets this
        player.MasteryCubes.SetMastery("Combat", 1); // Doesn't meet this (needs 2)

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Understanding, Threshold = 2, Effect = -3 });
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Mastery, Context = "Combat", Threshold = 2, Effect = -4 });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 15, itemRepo);

        // Assert
        Assert.Equal(15, result.BaseDifficulty);
        Assert.Equal(12, result.FinalDifficulty); // Reduced by 3 only
        Assert.Single(result.AppliedModifiers);
        Assert.Single(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_ReductionExceedsBase_ClampedAtZero()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 5;

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Understanding, Threshold = 2, Effect = -20 }); // Huge reduction

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 5, itemRepo);

        // Assert
        Assert.Equal(5, result.BaseDifficulty);
        Assert.Equal(0, result.FinalDifficulty); // Clamped at 0 (not negative)
        Assert.Single(result.AppliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_MixedPositiveNegativeModifiers_CorrectCalculation()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 5;
        player.MasteryCubes.SetMastery("Combat", 2);

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Understanding, Threshold = 2, Effect = -5 }); // Reduce
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Mastery, Context = "Combat", Threshold = 2, Effect = +3 }); // Increase

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(8, result.FinalDifficulty); // 10 - 5 + 3 = 8
        Assert.Equal(2, result.AppliedModifiers.Count);
    }

    // ========== CATEGORY 7: EDGE CASES ==========

    [Fact]
    public void CalculateDifficulty_NullSituation_ThrowsArgumentNullException()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            service.CalculateDifficulty(situation: null, baseDifficulty: 10, itemRepo)
        );
    }

    [Fact]
    public void CalculateDifficulty_EmptyModifierList_BaseDifficultyUnchanged()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);

        Situation situation = CreateSituation();
        // No modifiers added

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 12, itemRepo);

        // Assert
        Assert.Equal(12, result.BaseDifficulty);
        Assert.Equal(12, result.FinalDifficulty);
        Assert.Empty(result.AppliedModifiers);
        Assert.Empty(result.UnappliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_BaseDifficultyZero_FinalDifficultyZero()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 5;

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Understanding, Threshold = 2, Effect = -3 });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 0, itemRepo);

        // Assert
        Assert.Equal(0, result.BaseDifficulty);
        Assert.Equal(0, result.FinalDifficulty); // Can't go negative
    }

    [Fact]
    public void CalculateDifficulty_NegativeBaseDifficulty_StillProcessesModifiers()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 5;

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Understanding, Threshold = 2, Effect = -3 });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: -5, itemRepo);

        // Assert
        Assert.Equal(-5, result.BaseDifficulty);
        Assert.Equal(0, result.FinalDifficulty); // -5 - 3 = -8, clamped to 0
        Assert.Single(result.AppliedModifiers);
    }

    [Fact]
    public void CalculateDifficulty_LargeDifficultyReduction_ClampedAtZero()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 10;
        player.MasteryCubes.SetMastery("Combat", 3);

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Understanding, Threshold = 5, Effect = -50 });
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Mastery, Context = "Combat", Threshold = 2, Effect = -50 });

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 20, itemRepo);

        // Assert
        Assert.Equal(20, result.BaseDifficulty);
        Assert.Equal(0, result.FinalDifficulty); // 20 - 50 - 50 = -80, clamped to 0
        Assert.Equal(2, result.AppliedModifiers.Count);
    }

    [Fact]
    public void CalculateDifficulty_ModifierWithZeroEffect_DoesNotChangeDifficulty()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Player player = CreatePlayer(world);
        player.Understanding = 5;

        Situation situation = CreateSituation();
        situation.DifficultyModifiers.Add(new DifficultyModifier { Type = ModifierType.Understanding, Threshold = 2, Effect = 0 }); // Zero effect

        DifficultyCalculationService service = CreateService(world);
        ItemRepository itemRepo = CreateItemRepository(world);

        // Act
        DifficultyResult result = service.CalculateDifficulty(situation, baseDifficulty: 10, itemRepo);

        // Assert
        Assert.Equal(10, result.BaseDifficulty);
        Assert.Equal(10, result.FinalDifficulty); // No change
        Assert.Single(result.AppliedModifiers); // Modifier still "applied" (threshold met)
    }
}
