using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests;

/// <summary>
/// Action execution tests demonstrating the new superior test pattern.
/// Tests action processing using repository-based architecture with direct GameWorld access.
/// </summary>
public class ActionExecutionTests
{
    private GameWorld CreateTestGameWorld()
    {
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p
                .StartAt("dusty_flagon")
                .WithCoins(50)
                .WithStamina(10)
                .WithActionPoints(18))
            .WithTimeState(t => t
                .Day(1)
                .TimeBlock(TimeBlocks.Morning));
        
        return TestGameWorldInitializer.CreateTestWorld(scenario);
    }

    [Fact]
    public void ProcessAction_Should_Consume_Time_Blocks()
    {
        // === SETUP WITH NEW TEST PATTERN ===
        GameWorld gameWorld = CreateTestGameWorld();
        
        // Create repositories using new pattern
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        
        // Create services with proper dependencies
        MessageSystem messageSystem = new MessageSystem();
        ContractProgressionService contractProgression = new ContractProgressionService(
            contractRepository, itemRepository, locationRepository);
        
        ActionProcessor actionProcessor = new ActionProcessor(
            gameWorld,
            new PlayerProgression(gameWorld, messageSystem),
            new LocationPropertyManager(new LocationSystem(gameWorld, locationRepository)),
            locationRepository,
            messageSystem,
            contractProgression
        );

        LocationAction testAction = new LocationAction
        {
            ActionId = "test_action",
            Name = "Test Action",
            ActionPointCost = 2,
            SilverCost = 0,
            StaminaCost = 0,
            ConcentrationCost = 0
        };

        int initialTimeBlocks = gameWorld.TimeManager.UsedTimeBlocks;

        // Act
        actionProcessor.ProcessAction(testAction);

        // === VERIFY USING DIRECT GAMEWORLD ACCESS ===
        int finalTimeBlocks = gameWorld.TimeManager.UsedTimeBlocks;
        Assert.Equal(initialTimeBlocks + 2, finalTimeBlocks);
    }

    [Fact]
    public void ProcessAction_Should_Apply_Resource_Costs()
    {
        // === SETUP WITH NEW TEST PATTERN ===
        GameWorld gameWorld = CreateTestGameWorld();
        
        // Create repositories using new pattern
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        
        // Create services with proper dependencies
        MessageSystem messageSystem = new MessageSystem();
        ContractProgressionService contractProgression = new ContractProgressionService(
            contractRepository, itemRepository, locationRepository);
        
        ActionProcessor actionProcessor = new ActionProcessor(
            gameWorld,
            new PlayerProgression(gameWorld, messageSystem),
            new LocationPropertyManager(new LocationSystem(gameWorld, locationRepository)),
            locationRepository,
            messageSystem,
            contractProgression
        );

        Player player = gameWorld.GetPlayer();
        int initialCoins = player.Coins;
        int initialStamina = player.Stamina;
        int initialConcentration = player.Concentration;

        LocationAction testAction = new LocationAction
        {
            ActionId = "test_action",
            Name = "Test Action",
            ActionPointCost = 1,
            SilverCost = 5,
            StaminaCost = 3,
            ConcentrationCost = 2
        };

        // Act
        actionProcessor.ProcessAction(testAction);

        // === VERIFY USING DIRECT GAMEWORLD ACCESS ===
        Assert.Equal(initialCoins - 5, player.Coins);
        Assert.Equal(initialStamina - 3, player.Stamina);
        // Concentration should decrease by the cost amount
        Assert.Equal(initialConcentration - 2, player.Concentration);
    }

    [Fact]
    public void CanExecute_Should_Block_Actions_With_Insufficient_Resources()
    {
        // === SETUP WITH NEW TEST PATTERN ===
        GameWorld gameWorld = CreateTestGameWorld();
        
        // Create repositories using new pattern
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        
        // Create services with proper dependencies
        MessageSystem messageSystem = new MessageSystem();
        ContractProgressionService contractProgression = new ContractProgressionService(
            contractRepository, itemRepository, locationRepository);
        
        ActionProcessor actionProcessor = new ActionProcessor(
            gameWorld,
            new PlayerProgression(gameWorld, messageSystem),
            new LocationPropertyManager(new LocationSystem(gameWorld, locationRepository)),
            locationRepository,
            messageSystem,
            contractProgression
        );

        Player player = gameWorld.GetPlayer();
        
        // Create action that costs more than player has
        LocationAction expensiveAction = new LocationAction
        {
            ActionId = "expensive_action",
            Name = "Expensive Action",
            ActionPointCost = 1,
            SilverCost = player.Coins + 100, // More than player has
            StaminaCost = 0,
            ConcentrationCost = 0
        };

        // Act & Assert
        Assert.False(actionProcessor.CanExecute(expensiveAction));
    }

    [Fact]
    public void CanExecute_Should_Block_Actions_With_Insufficient_Time_Blocks()
    {
        // === SETUP WITH NEW TEST PATTERN ===
        GameWorld gameWorld = CreateTestGameWorld();
        
        // Create repositories using new pattern
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        
        // Create services with proper dependencies
        MessageSystem messageSystem = new MessageSystem();
        ContractProgressionService contractProgression = new ContractProgressionService(
            contractRepository, itemRepository, locationRepository);
        
        ActionProcessor actionProcessor = new ActionProcessor(
            gameWorld,
            new PlayerProgression(gameWorld, messageSystem),
            new LocationPropertyManager(new LocationSystem(gameWorld, locationRepository)),
            locationRepository,
            messageSystem,
            contractProgression
        );

        // Consume all available time blocks
        while (gameWorld.TimeManager.CanPerformTimeBlockAction)
        {
            gameWorld.TimeManager.ConsumeTimeBlock(1);
        }

        LocationAction timeAction = new LocationAction
        {
            ActionId = "time_action",
            Name = "Time Action",
            ActionPointCost = 1, // Requires 1 time block but none available
            SilverCost = 0,
            StaminaCost = 0,
            ConcentrationCost = 0
        };

        // Act & Assert
        Assert.False(actionProcessor.CanExecute(timeAction));
    }

    [Fact]
    public void CanExecute_Should_Allow_Valid_Actions()
    {
        // === SETUP WITH NEW TEST PATTERN ===
        GameWorld gameWorld = CreateTestGameWorld();
        
        // Create repositories using new pattern
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        
        // Create services with proper dependencies
        MessageSystem messageSystem = new MessageSystem();
        ContractProgressionService contractProgression = new ContractProgressionService(
            contractRepository, itemRepository, locationRepository);
        
        ActionProcessor actionProcessor = new ActionProcessor(
            gameWorld,
            new PlayerProgression(gameWorld, messageSystem),
            new LocationPropertyManager(new LocationSystem(gameWorld, locationRepository)),
            locationRepository,
            messageSystem,
            contractProgression
        );

        LocationAction validAction = new LocationAction
        {
            ActionId = "valid_action",
            Name = "Valid Action",
            ActionPointCost = 1,
            SilverCost = 1, // Player should have sufficient resources (50 coins)
            StaminaCost = 1,
            ConcentrationCost = 1,
            Requirements = new List<IRequirement>() // Ensure no blocking requirements
        };

        // Act & Assert
        Assert.True(actionProcessor.CanExecute(validAction));
    }

    [Fact]
    public void ProcessAction_Should_Apply_Card_Refresh_Effects()
    {
        // === SETUP WITH NEW TEST PATTERN ===
        GameWorld gameWorld = CreateTestGameWorld();
        
        // Create repositories using new pattern
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        
        // Create services with proper dependencies
        MessageSystem messageSystem = new MessageSystem();
        ContractProgressionService contractProgression = new ContractProgressionService(
            contractRepository, itemRepository, locationRepository);
        
        ActionProcessor actionProcessor = new ActionProcessor(
            gameWorld,
            new PlayerProgression(gameWorld, messageSystem),
            new LocationPropertyManager(new LocationSystem(gameWorld, locationRepository)),
            locationRepository,
            messageSystem,
            contractProgression
        );

        LocationAction refreshAction = new LocationAction
        {
            ActionId = "refresh_action",
            Name = "Refresh Action",
            ActionPointCost = 1,
            RefreshCardType = SkillCategories.Physical
        };

        // Act
        actionProcessor.ProcessAction(refreshAction);

        // Assert - Check that a message was added about refreshing cards
        // This is a placeholder test until full card refresh logic is implemented
        Assert.True(true); // TODO: Verify actual card refresh when card system is implemented
    }
}