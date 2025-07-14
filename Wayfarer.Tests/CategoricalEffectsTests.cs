using Wayfarer.Game.ActionSystem;
using Wayfarer.Game.MainSystem;
using Xunit;

namespace Wayfarer.Tests;

/// <summary>
/// Categorical effects tests using the superior test pattern.
/// Tests effect application using synchronous test setup and real GameWorld objects.
/// </summary>
public class CategoricalEffectsTests
{
    private GameWorld CreateTestGameWorld()
    {
        TestScenarioBuilder scenario = new TestScenarioBuilder()
            .WithPlayer(p => p
                .StartAt("dusty_flagon")
                .WithCoins(50)
                .WithStamina(3)
                .WithActionPoints(10))
            .WithTimeState(t => t
                .Day(1)
                .TimeBlock(TimeBlocks.Morning));

        return TestGameWorldInitializer.CreateTestWorld(scenario);
    }

    [Fact]
    public void PhysicalRecoveryEffect_Should_Restore_Stamina()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        Player player = gameWorld.GetPlayer();
        player.Stamina = 3; // Set to less than max
        player.MaxStamina = 10;

        PhysicalRecoveryEffect effect = new PhysicalRecoveryEffect(2, "test rest");
        EncounterState encounterState = new EncounterState(player, 5, 8, 10);

        // Act
        effect.Apply(encounterState);

        // Assert
        Assert.Equal(5, player.Stamina); // Should increase by 2
    }

    [Fact]
    public void PhysicalRecoveryEffect_Should_Not_Exceed_Max_Stamina()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        Player player = gameWorld.GetPlayer();
        player.Stamina = 9;
        player.MaxStamina = 10;

        PhysicalRecoveryEffect effect = new PhysicalRecoveryEffect(5, "big rest");
        EncounterState encounterState = new EncounterState(player, 5, 8, 10);

        // Act
        effect.Apply(encounterState);

        // Assert
        Assert.Equal(10, player.Stamina); // Should cap at max stamina
    }

    [Fact]
    public void PhysicalRecoveryEffect_Should_Provide_Clear_Description()
    {
        // Arrange
        PhysicalRecoveryEffect effect = new PhysicalRecoveryEffect(3, "hearth rest");

        // Act
        string description = effect.GetDescriptionForPlayer();

        // Assert
        Assert.Contains("3 stamina", description.ToLower());
        Assert.Contains("hearth rest", description.ToLower());
    }

    [Fact]
    public void ActionFactory_Should_Create_Physical_Recovery_Effects()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ActionRepository actionRepository = new ActionRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        // Create repositories using new pattern
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractValidationService contractValidation = new ContractValidationService(contractRepository, itemRepository);

        ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, itemRepository, contractRepository, contractValidation);

        ActionDefinition actionDef = new ActionDefinition("rest_action", "Rest by Fire", "hearth")
        {
            PhysicalDemand = PhysicalDemand.None, // Should provide recovery
            EffectCategories = new List<EffectCategory> { EffectCategory.Physical_Recovery },
            ActionPointCost = 1
        };

        // Act
        LocationAction action = actionFactory.CreateActionFromTemplate(
            actionDef, "test_location", "test_spot", ActionExecutionTypes.Instant);

        // Assert
        Assert.NotNull(action.Effects);
        Assert.True(action.Effects.Count > 0);
        Assert.Contains(action.Effects, e => e is PhysicalRecoveryEffect);
    }

    [Fact]
    public void ActionFactory_Should_Determine_Recovery_Amount_By_Physical_Demand()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ActionRepository actionRepository = new ActionRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        // Create repositories using new pattern
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractValidationService contractValidation = new ContractValidationService(contractRepository, itemRepository);

        ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, itemRepository, contractRepository, contractValidation);

        // Create actions with different physical demands
        ActionDefinition lightAction = new ActionDefinition("light_action", "Light Reading", "library")
        {
            PhysicalDemand = PhysicalDemand.Light,
            EffectCategories = new List<EffectCategory> { EffectCategory.Physical_Recovery },
            ActionPointCost = 1
        };

        ActionDefinition noneAction = new ActionDefinition("none_action", "Meditation", "quiet_room")
        {
            PhysicalDemand = PhysicalDemand.None,
            EffectCategories = new List<EffectCategory> { EffectCategory.Physical_Recovery },
            ActionPointCost = 1
        };

        // Act
        LocationAction lightLocationAction = actionFactory.CreateActionFromTemplate(
            lightAction, "test_location", "test_spot", ActionExecutionTypes.Instant);
        LocationAction noneLocationAction = actionFactory.CreateActionFromTemplate(
            noneAction, "test_location", "test_spot", ActionExecutionTypes.Instant);

        // Assert
        PhysicalRecoveryEffect lightRecoveryEffect = lightLocationAction.Effects.OfType<PhysicalRecoveryEffect>().First();
        PhysicalRecoveryEffect noneRecoveryEffect = noneLocationAction.Effects.OfType<PhysicalRecoveryEffect>().First();

        // None demand should provide more recovery than Light demand
        Assert.Contains("1 stamina", lightRecoveryEffect.GetDescriptionForPlayer());
        Assert.Contains("2 stamina", noneRecoveryEffect.GetDescriptionForPlayer());
    }

}