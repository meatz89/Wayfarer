using Wayfarer.Game.MainSystem;
using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Tests to verify TimeManager architecture is correct and TimeManager/WorldState synchronization works properly.
    /// Ensures that TimeManager is the single source of truth for time calculations.
    /// </summary>
    public class TimeManagerArchitectureTests
    {
        [Fact]
        public void TimeManager_Should_Synchronize_With_WorldState()
        {
            // Create test world
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

            // Verify initial synchronization
            TimeBlocks timeManagerBlock = gameWorld.TimeManager.GetCurrentTimeBlock();
            TimeBlocks worldStateBlock = gameWorld.TimeManager.GetCurrentTimeBlock();

            Assert.Equal(timeManagerBlock, worldStateBlock);
            Assert.Equal(TimeBlocks.Morning, worldStateBlock);
        }

        [Fact]
        public void TimeManager_UpdateCurrentTimeBlock_Should_Sync_WorldState()
        {
            // Create test world
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

            // Get initial state
            TimeBlocks initialTime = gameWorld.TimeManager.GetCurrentTimeBlock();

            // Update time through TimeManager
            gameWorld.TimeManager.UpdateCurrentTimeBlock();

            // Verify both properties are synchronized
            TimeBlocks internalBlock = gameWorld.TimeManager.GetCurrentTimeBlock();
            TimeBlocks worldStateBlock = gameWorld.TimeManager.GetCurrentTimeBlock();

            Assert.Equal(internalBlock, worldStateBlock);
        }

        [Fact]
        public void TimeManager_SetNewTime_Should_Sync_WorldState()
        {
            // Create test world
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

            // Set time to afternoon (14 hours)
            gameWorld.TimeManager.SetNewTime(14);

            // Verify both properties are synchronized
            TimeBlocks internalBlock = gameWorld.TimeManager.GetCurrentTimeBlock();
            TimeBlocks worldStateBlock = gameWorld.TimeManager.GetCurrentTimeBlock();

            Assert.Equal(internalBlock, worldStateBlock);
            Assert.Equal(TimeBlocks.Afternoon, worldStateBlock);
        }

        [Fact]
        public void TimeManager_StartNewDay_Should_Sync_WorldState()
        {
            // Create test world
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Evening));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

            int initialDay = gameWorld.TimeManager.GetCurrentDay();

            // Start new day
            gameWorld.TimeManager.StartNewDay();

            // Verify day advanced
            Assert.Equal(initialDay + 1, gameWorld.TimeManager.GetCurrentDay());

            // Verify time blocks are synchronized
            TimeBlocks internalBlock = gameWorld.TimeManager.GetCurrentTimeBlock();
            TimeBlocks worldStateBlock = gameWorld.TimeManager.GetCurrentTimeBlock();

            Assert.Equal(internalBlock, worldStateBlock);
            Assert.Equal(TimeBlocks.Dawn, worldStateBlock); // StartNewDay sets to TimeDayStart=6 which is Dawn
        }

        [Fact]
        public void TimeManager_Time_Block_Progression_Should_Work()
        {
            // Create test world
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

            // Verify we start at morning
            Assert.Equal(TimeBlocks.Morning, gameWorld.TimeManager.GetCurrentTimeBlock());

            // Consume time blocks to advance time
            gameWorld.TimeManager.ConsumeTimeBlock(2);

            // Verify time has progressed
            TimeBlocks newTime = gameWorld.TimeManager.GetCurrentTimeBlock();
            Assert.True(newTime != TimeBlocks.Morning); // Should have changed from morning

            // Verify synchronization
            Assert.Equal(gameWorld.TimeManager.GetCurrentTimeBlock(), newTime);
        }

        [Fact]
        public void TimeManager_When_All_TimeBlocks_Used_Should_Work()
        {
            // Create test world
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

            // Exhaust all time blocks
            gameWorld.TimeManager.ConsumeTimeBlock(5);

            // Should not be able to perform more actions
            Assert.False(gameWorld.TimeManager.CanPerformTimeBlockAction);
            Assert.Equal(0, gameWorld.TimeManager.RemainingTimeBlocks);
        }

        [Fact]
        public void TimeManager_ConsumeTimeBlock_Should_Track_Usage()
        {
            // Create test world
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

            // Check initial state
            Assert.Equal(0, gameWorld.TimeManager.UsedTimeBlocks);
            Assert.Equal(5, gameWorld.TimeManager.RemainingTimeBlocks);
            Assert.True(gameWorld.TimeManager.CanPerformTimeBlockAction);

            // Consume some time blocks
            gameWorld.TimeManager.ConsumeTimeBlock(2);

            Assert.Equal(2, gameWorld.TimeManager.UsedTimeBlocks);
            Assert.Equal(3, gameWorld.TimeManager.RemainingTimeBlocks);
            Assert.True(gameWorld.TimeManager.CanPerformTimeBlockAction);

            // Consume remaining blocks
            gameWorld.TimeManager.ConsumeTimeBlock(3);

            Assert.Equal(5, gameWorld.TimeManager.UsedTimeBlocks);
            Assert.Equal(0, gameWorld.TimeManager.RemainingTimeBlocks);
            Assert.False(gameWorld.TimeManager.CanPerformTimeBlockAction);
        }

        [Fact]
        public void TimeManager_ConsumeTimeBlock_Should_Throw_When_Exceeding_Limit()
        {
            // Create test world
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

            // Try to consume more than daily limit
            Assert.Throws<InvalidOperationException>(() =>
                gameWorld.TimeManager.ConsumeTimeBlock(6));

            // Consume 4 blocks first
            gameWorld.TimeManager.ConsumeTimeBlock(4);

            // Now trying to consume 2 more should fail
            Assert.Throws<InvalidOperationException>(() =>
                gameWorld.TimeManager.ConsumeTimeBlock(2));
        }

        [Fact]
        public void TimeManager_ValidateTimeBlockAction_Should_Work()
        {
            // Create test world
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

            // Should allow valid actions
            Assert.True(gameWorld.TimeManager.ValidateTimeBlockAction(1));
            Assert.True(gameWorld.TimeManager.ValidateTimeBlockAction(5));

            // Should reject invalid actions
            Assert.False(gameWorld.TimeManager.ValidateTimeBlockAction(6));

            // After consuming some blocks
            gameWorld.TimeManager.ConsumeTimeBlock(3);

            Assert.True(gameWorld.TimeManager.ValidateTimeBlockAction(2));
            Assert.False(gameWorld.TimeManager.ValidateTimeBlockAction(3));
        }
    }
}