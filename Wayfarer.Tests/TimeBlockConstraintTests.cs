using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Unit tests for Time Block constraint enforcement system.
    /// These tests validate the critical "5 time blocks per day" constraint
    /// that enables the  simulation's core resource management gameplay.
    /// 
    /// User Story: Daily Time Block Allocation (:16-32)
    /// Priority: CRITICAL - Foundation for all other systems
    /// </summary>
    public class TimeBlockConstraintTests
    {
        /// <summary>
        /// Test that TimeManager correctly tracks daily time block usage
        /// Acceptance Criteria: Player can see remaining time blocks for current day
        /// </summary>
        [Fact]
        public void TimeManager_Should_Track_Daily_Time_Block_Usage()
        {
            // Arrange
            Player player = CreateTestPlayer();
            WorldState worldState = CreateTestWorldState();
            TimeManager timeManager = new TimeManager(player, worldState);

            // Act & Assert
            Assert.Equal(5, TimeManager.MaxDailyTimeBlocks);
            Assert.Equal(0, timeManager.UsedTimeBlocks);
            Assert.Equal(5, timeManager.RemainingTimeBlocks);
            Assert.True(timeManager.CanPerformTimeBlockAction);
        }

        /// <summary>
        /// Test that time blocks are consumed when performing actions
        /// Acceptance Criteria: Each major action consumes exactly one time block
        /// </summary>
        [Fact]
        public void TimeManager_Should_Consume_Time_Blocks_When_Performing_Actions()
        {
            // Arrange
            Player player = CreateTestPlayer();
            WorldState worldState = CreateTestWorldState();
            TimeManager timeManager = new TimeManager(player, worldState);

            // Act
            timeManager.ConsumeTimeBlock(1);

            // Assert
            Assert.Equal(1, timeManager.UsedTimeBlocks);
            Assert.Equal(4, timeManager.RemainingTimeBlocks);
            Assert.True(timeManager.CanPerformTimeBlockAction);
        }

        /// <summary>
        /// Test that multiple time blocks can be consumed up to the daily limit
        /// Acceptance Criteria: Player can perform up to 5 major actions per day
        /// </summary>
        [Fact]
        public void TimeManager_Should_Allow_Multiple_Actions_Up_To_Daily_Limit()
        {
            // Arrange
            Player player = CreateTestPlayer();
            WorldState worldState = CreateTestWorldState();
            TimeManager timeManager = new TimeManager(player, worldState);

            // Act - Consume 4 time blocks
            timeManager.ConsumeTimeBlock(1);
            timeManager.ConsumeTimeBlock(1);
            timeManager.ConsumeTimeBlock(1);
            timeManager.ConsumeTimeBlock(1);

            // Assert
            Assert.Equal(4, timeManager.UsedTimeBlocks);
            Assert.Equal(1, timeManager.RemainingTimeBlocks);
            Assert.True(timeManager.CanPerformTimeBlockAction);
        }

        /// <summary>
        /// Test that the 5th action is allowed but exhausts daily time blocks
        /// Acceptance Criteria: Day automatically advances when all time blocks are consumed
        /// </summary>
        [Fact]
        public void TimeManager_Should_Allow_Fifth_Action_And_Exhaust_Daily_Blocks()
        {
            // Arrange
            Player player = CreateTestPlayer();
            WorldState worldState = CreateTestWorldState();
            TimeManager timeManager = new TimeManager(player, worldState);

            // Act - Consume all 5 time blocks
            for (int i = 0; i < 5; i++)
            {
                timeManager.ConsumeTimeBlock(1);
            }

            // Assert
            Assert.Equal(5, timeManager.UsedTimeBlocks);
            Assert.Equal(0, timeManager.RemainingTimeBlocks);
            Assert.False(timeManager.CanPerformTimeBlockAction);
        }

        /// <summary>
        /// Test that attempting to exceed the daily limit throws an exception
        /// Acceptance Criteria: Player cannot perform major actions when no time blocks remain
        /// </summary>
        [Fact]
        public void TimeManager_Should_Prevent_Actions_When_Daily_Limit_Exceeded()
        {
            // Arrange
            Player player = CreateTestPlayer();
            WorldState worldState = CreateTestWorldState();
            TimeManager timeManager = new TimeManager(player, worldState);

            // Act - Consume all 5 time blocks
            for (int i = 0; i < 5; i++)
            {
                timeManager.ConsumeTimeBlock(1);
            }

            // Assert - 6th action should throw exception
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => timeManager.ConsumeTimeBlock(1));

            Assert.Contains("Cannot exceed daily time block limit", exception.Message);
        }

        /// <summary>
        /// Test that time blocks reset to 5 at the start of a new day
        /// Acceptance Criteria: Time blocks reset to 5 at start of each new day
        /// </summary>
        [Fact]
        public void TimeManager_Should_Reset_Time_Blocks_On_New_Day()
        {
            // Arrange
            Player player = CreateTestPlayer();
            WorldState worldState = CreateTestWorldState();
            TimeManager timeManager = new TimeManager(player, worldState);

            // Act - Consume all time blocks then start new day
            for (int i = 0; i < 5; i++)
            {
                timeManager.ConsumeTimeBlock(1);
            }

            int dayBeforeReset = worldState.CurrentDay;
            timeManager.StartNewDay();

            // Assert
            Assert.Equal(dayBeforeReset + 1, worldState.CurrentDay);
            Assert.Equal(0, timeManager.UsedTimeBlocks);
            Assert.Equal(5, timeManager.RemainingTimeBlocks);
            Assert.True(timeManager.CanPerformTimeBlockAction);
        }

        /// <summary>
        /// Test that time blocks can be validated before consumption
        /// Acceptance Criteria: UI clearly shows which actions consume time blocks vs. free actions
        /// </summary>
        [Fact]
        public void TimeManager_Should_Validate_Time_Block_Actions_Before_Consumption()
        {
            // Arrange
            Player player = CreateTestPlayer();
            WorldState worldState = CreateTestWorldState();
            TimeManager timeManager = new TimeManager(player, worldState);

            // Consume 4 time blocks
            for (int i = 0; i < 4; i++)
            {
                timeManager.ConsumeTimeBlock(1);
            }

            // Act & Assert
            Assert.True(timeManager.ValidateTimeBlockAction(1)); // Can perform 1 more action
            Assert.False(timeManager.ValidateTimeBlockAction(2)); // Cannot perform 2 more actions
        }

        /// <summary>
        /// Test that consuming multiple time blocks at once works correctly
        /// This supports actions that might consume more than 1 time block
        /// </summary>
        [Fact]
        public void TimeManager_Should_Handle_Multi_Block_Actions()
        {
            // Arrange
            Player player = CreateTestPlayer();
            WorldState worldState = CreateTestWorldState();
            TimeManager timeManager = new TimeManager(player, worldState);

            // Act - Consume 2 time blocks at once
            timeManager.ConsumeTimeBlock(2);

            // Assert
            Assert.Equal(2, timeManager.UsedTimeBlocks);
            Assert.Equal(3, timeManager.RemainingTimeBlocks);
            Assert.True(timeManager.CanPerformTimeBlockAction);
        }

        /// <summary>
        /// Test that multi-block actions respect the daily limit
        /// </summary>
        [Fact]
        public void TimeManager_Should_Prevent_Multi_Block_Actions_That_Exceed_Limit()
        {
            // Arrange
            Player player = CreateTestPlayer();
            WorldState worldState = CreateTestWorldState();
            TimeManager timeManager = new TimeManager(player, worldState);

            // Act - Consume 3 time blocks, leaving 2 remaining
            timeManager.ConsumeTimeBlock(3);

            // Assert - Attempting to consume 3 more should fail
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => timeManager.ConsumeTimeBlock(3));

            Assert.Contains("Cannot exceed daily time block limit", exception.Message);
        }

        /// <summary>
        /// Test that the TimeManager integrates properly with existing time advancement
        /// Ensures backward compatibility with existing AdvanceTime functionality
        /// </summary>
        [Fact]
        public void TimeManager_Should_Integrate_With_Existing_Time_Advancement()
        {
            // Arrange
            Player player = CreateTestPlayer();
            WorldState worldState = CreateTestWorldState();
            TimeManager timeManager = new TimeManager(player, worldState);

            // Act - Use existing AdvanceTime method (should consume time blocks)
            timeManager.AdvanceTime(2); // This should consume 2 time blocks

            // Assert
            Assert.Equal(2, timeManager.UsedTimeBlocks);
            Assert.Equal(3, timeManager.RemainingTimeBlocks);
        }

        #region Helper Methods

        /// <summary>
        /// Creates a test player with standard starting configuration
        /// </summary>
        private static Player CreateTestPlayer()
        {
            Player player = new Player();
            player.Initialize("TestPlayer", Professions.Merchant, Genders.Male);
            return player;
        }

        /// <summary>
        /// Creates a test world state with standard starting configuration
        /// </summary>
        private static WorldState CreateTestWorldState()
        {
            WorldState worldState = new WorldState();
            worldState.CurrentDay = 1;
            worldState.CurrentTimeBlock = TimeBlocks.Dawn;
            return worldState;
        }

        #endregion
    }
}