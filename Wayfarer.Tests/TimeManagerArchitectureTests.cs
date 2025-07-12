using Xunit;
using Wayfarer.Game.MainSystem;

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
            var scenario = new TestScenarioBuilder()
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
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100).WithActionPoints(18))
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
            var scenario = new TestScenarioBuilder()
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
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100).WithActionPoints(18))
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
            Assert.Equal(TimeBlocks.Morning, worldStateBlock); // StartNewDay should set to morning
        }
        
        [Fact]
        public void TimeManager_Time_Block_Progression_Should_Work()
        {
            // Create test world with player having full action points
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100).WithActionPoints(18).WithMaxActionPoints(18))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Verify we start at morning
            Assert.Equal(TimeBlocks.Morning, gameWorld.TimeManager.GetCurrentTimeBlock());
            
            // Consume some action points to advance time
            Player player = gameWorld.GetPlayer();
            int initialAP = player.CurrentActionPoints();
            
            // Consume action points (this should trigger time progression)
            player.ActionPoints = player.ActionPoints - 9; // Use half action points
            
            // Debug: Check current AP values
            int maxAP = player.MaxActionPoints;
            int currentAP = player.CurrentActionPoints();
            Assert.True(currentAP > 0, $"CurrentActionPoints should be > 0, was {currentAP}. MaxAP: {maxAP}");
            
            // Update time block calculation
            gameWorld.TimeManager.UpdateCurrentTimeBlock();
            
            // Debug: Check calculated hour
            int currentHour = gameWorld.TimeManager.CurrentTimeHours;
            Assert.True(currentHour >= 12 && currentHour < 18, $"Hour should be 12-17 for Afternoon, was {currentHour}. MaxAP: {maxAP}, CurrentAP: {currentAP}");
            
            // Verify time has progressed to afternoon
            TimeBlocks newTime = gameWorld.TimeManager.GetCurrentTimeBlock();
            Assert.Equal(TimeBlocks.Afternoon, newTime);
            
            // Verify synchronization
            Assert.Equal(gameWorld.TimeManager.GetCurrentTimeBlock(), newTime);
        }
        
        [Fact]
        public void TimeManager_When_ActionPoints_Zero_Should_Set_Night()
        {
            // Create test world
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100).WithActionPoints(18))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Exhaust all action points
            Player player = gameWorld.GetPlayer();
            player.ActionPoints = 0;
            
            // Update time block
            gameWorld.TimeManager.UpdateCurrentTimeBlock();
            
            // Should be night when action points are zero
            Assert.Equal(TimeBlocks.Night, gameWorld.TimeManager.GetCurrentTimeBlock());
            Assert.Equal(TimeBlocks.Night, gameWorld.TimeManager.GetCurrentTimeBlock());
            
            // Hour should also be reset to 0 (midnight)
            Assert.Equal(0, gameWorld.TimeManager.CurrentTimeHours);
        }
        
        [Fact]
        public void TimeManager_ConsumeTimeBlock_Should_Track_Usage()
        {
            // Create test world
            var scenario = new TestScenarioBuilder()
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
            var scenario = new TestScenarioBuilder()
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
            var scenario = new TestScenarioBuilder()
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