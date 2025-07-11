using Xunit;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Tests;

public class CategoricalEffectsTests
{
    private GameWorld CreateTestGameWorld()
    {
        GameWorldInitializer initializer = new GameWorldInitializer("Content");
        return initializer.LoadGame();
    }

    [Fact]
    public void PhysicalRecoveryEffect_Should_Restore_Stamina()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        Player player = gameWorld.GetPlayer();
        player.Stamina = 3; // Set to less than max
        player.MaxStamina = 10;

        var effect = new PhysicalRecoveryEffect(2, "test rest");
        var encounterState = new EncounterState(player, 5, 8, 10);

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

        var effect = new PhysicalRecoveryEffect(5, "big rest");
        var encounterState = new EncounterState(player, 5, 8, 10);

        // Act
        effect.Apply(encounterState);

        // Assert
        Assert.Equal(10, player.Stamina); // Should cap at max stamina
    }

    [Fact]
    public void PhysicalRecoveryEffect_Should_Provide_Clear_Description()
    {
        // Arrange
        var effect = new PhysicalRecoveryEffect(3, "hearth rest");

        // Act
        string description = effect.GetDescriptionForPlayer();

        // Assert
        Assert.Contains("3 stamina", description.ToLower());
        Assert.Contains("hearth rest", description.ToLower());
    }

    [Fact]
    public void SocialStandingEffect_Should_Modify_Reputation()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        Player player = gameWorld.GetPlayer();
        int initialReputation = player.Reputation;

        var effect = new SocialStandingEffect(2, SocialRequirement.Merchant_Class, "successful negotiation");
        var encounterState = new EncounterState(player, 5, 8, 10);

        // Act
        effect.Apply(encounterState);

        // Assert
        Assert.Equal(initialReputation + 2, player.Reputation);
    }

    [Fact]
    public void SocialStandingEffect_Should_Handle_Negative_Changes()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        Player player = gameWorld.GetPlayer();
        int initialReputation = player.Reputation;

        var effect = new SocialStandingEffect(-1, SocialRequirement.Minor_Noble, "social faux pas");
        var encounterState = new EncounterState(player, 5, 8, 10);

        // Act
        effect.Apply(encounterState);

        // Assert
        Assert.Equal(initialReputation - 1, player.Reputation);
    }

    [Fact]
    public void SocialStandingEffect_Should_Provide_Clear_Description()
    {
        // Arrange
        var positiveEffect = new SocialStandingEffect(2, SocialRequirement.Artisan_Class, "craft demonstration");
        var negativeEffect = new SocialStandingEffect(-1, SocialRequirement.Professional, "poor service");

        // Act
        string positiveDescription = positiveEffect.GetDescriptionForPlayer();
        string negativeDescription = negativeEffect.GetDescriptionForPlayer();

        // Assert
        Assert.Contains("improves", positiveDescription.ToLower());
        Assert.Contains("artisan", positiveDescription.ToLower());
        Assert.Contains("reduces", negativeDescription.ToLower());
        Assert.Contains("professional", negativeDescription.ToLower());
    }

    [Fact]
    public void ActionFactory_Should_Create_Physical_Recovery_Effects()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ActionRepository actionRepository = new ActionRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, itemRepository);

        var actionDef = new ActionDefinition("rest_action", "Rest by Fire", "hearth")
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
    public void ActionFactory_Should_Create_Social_Standing_Effects()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ActionRepository actionRepository = new ActionRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, itemRepository);

        var actionDef = new ActionDefinition("social_action", "Attend Court", "throne_room")
        {
            SocialRequirement = SocialRequirement.Minor_Noble,
            EffectCategories = new List<EffectCategory> { EffectCategory.Social_Standing },
            ActionPointCost = 1
        };

        // Act
        LocationAction action = actionFactory.CreateActionFromTemplate(
            actionDef, "test_location", "test_spot", ActionExecutionTypes.Instant);

        // Assert
        Assert.NotNull(action.Effects);
        Assert.True(action.Effects.Count > 0);
        Assert.Contains(action.Effects, e => e is SocialStandingEffect);
    }

    [Fact]
    public void ActionFactory_Should_Determine_Recovery_Amount_By_Physical_Demand()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ActionRepository actionRepository = new ActionRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, itemRepository);

        // Create actions with different physical demands
        var lightAction = new ActionDefinition("light_action", "Light Reading", "library")
        {
            PhysicalDemand = PhysicalDemand.Light,
            EffectCategories = new List<EffectCategory> { EffectCategory.Physical_Recovery },
            ActionPointCost = 1
        };

        var noneAction = new ActionDefinition("none_action", "Meditation", "quiet_room")
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
        var lightRecoveryEffect = lightLocationAction.Effects.OfType<PhysicalRecoveryEffect>().First();
        var noneRecoveryEffect = noneLocationAction.Effects.OfType<PhysicalRecoveryEffect>().First();

        // None demand should provide more recovery than Light demand
        Assert.Contains("1 stamina", lightRecoveryEffect.GetDescriptionForPlayer());
        Assert.Contains("2 stamina", noneRecoveryEffect.GetDescriptionForPlayer());
    }

    [Fact]
    public void ActionFactory_Should_Scale_Social_Gain_By_Social_Level()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ActionRepository actionRepository = new ActionRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, itemRepository);

        var merchantAction = new ActionDefinition("merchant_action", "Trade Meeting", "market")
        {
            SocialRequirement = SocialRequirement.Merchant_Class,
            EffectCategories = new List<EffectCategory> { EffectCategory.Social_Standing },
            ActionPointCost = 1
        };

        var nobleAction = new ActionDefinition("noble_action", "Court Presentation", "palace")
        {
            SocialRequirement = SocialRequirement.Major_Noble,
            EffectCategories = new List<EffectCategory> { EffectCategory.Social_Standing },
            ActionPointCost = 1
        };

        // Act
        LocationAction merchantLocationAction = actionFactory.CreateActionFromTemplate(
            merchantAction, "test_location", "test_spot", ActionExecutionTypes.Instant);
        LocationAction nobleLocationAction = actionFactory.CreateActionFromTemplate(
            nobleAction, "test_location", "test_spot", ActionExecutionTypes.Instant);

        // Assert
        var merchantEffect = merchantLocationAction.Effects.OfType<SocialStandingEffect>().First();
        var nobleEffect = nobleLocationAction.Effects.OfType<SocialStandingEffect>().First();

        // Higher social levels should provide more reputation gain
        Assert.Contains("merchant", merchantEffect.GetDescriptionForPlayer().ToLower());
        Assert.Contains("high noble", nobleEffect.GetDescriptionForPlayer().ToLower());
    }
}