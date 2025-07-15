using Wayfarer.Game.MainSystem;
using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Tests for Period-Based Activity Planning user story implementation.
    /// Validates that:
    /// - Day divided into 5 periods: Dawn, Morning, Afternoon, Evening, Night
    /// - Each significant activity consumes 1 period
    /// - NPC availability tied to logical professional schedules
    /// - Transport schedules operate on period system
    /// - No action point regeneration or management mini-games
    /// </summary>
    public class PeriodBasedActivityPlanningTests
    {
        [Fact]
        public void All_Activities_Should_Consume_Exactly_One_Time_Period()
        {
            // Arrange
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon"))
                .WithTimeState(t => t.Hour(6)); // Dawn

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            RouteRepository routeRepository = new RouteRepository(gameWorld);

            // Act & Assert - All routes should consume exactly 1 time block
            List<RouteOption> allRoutes = routeRepository.GetAllRoutes();
            foreach (RouteOption route in allRoutes)
            {
                Assert.Equal(1, route.TimeBlockCost);
            }
        }

        [Fact]
        public void StartNewDay_Should_Reset_Time_Blocks()
        {
            // Arrange
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithTimeState(t => t.Day(1).Hour(6));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            // Consume some time blocks
            timeManager.ConsumeTimeBlock(2);
            int usedTimeBlocks = timeManager.UsedTimeBlocks;

            // Act - Start new day
            timeManager.StartNewDay();

            // Assert - Time blocks should be reset for new day
            Assert.Equal(0, timeManager.UsedTimeBlocks);
            Assert.Equal(2, gameWorld.WorldState.CurrentDay);
            Assert.Equal(6, timeManager.CurrentTimeHours); // Should reset to Dawn (6 AM)
        }

        [Fact]
        public void Five_Time_Periods_Should_Cover_Full_Day()
        {
            // Arrange
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithTimeState(t => t.Day(1));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            // Act & Assert - Test all 5 time periods map correctly
            timeManager.SetNewTime(6);  // Dawn: 6:00-8:59
            Assert.Equal(TimeBlocks.Dawn, timeManager.GetCurrentTimeBlock());

            timeManager.SetNewTime(9);  // Morning: 9:00-11:59
            Assert.Equal(TimeBlocks.Morning, timeManager.GetCurrentTimeBlock());

            timeManager.SetNewTime(12); // Afternoon: 12:00-15:59
            Assert.Equal(TimeBlocks.Afternoon, timeManager.GetCurrentTimeBlock());

            timeManager.SetNewTime(16); // Evening: 16:00-19:59
            Assert.Equal(TimeBlocks.Evening, timeManager.GetCurrentTimeBlock());

            timeManager.SetNewTime(20); // Night: 20:00-5:59
            Assert.Equal(TimeBlocks.Night, timeManager.GetCurrentTimeBlock());

            timeManager.SetNewTime(2);  // Still Night
            Assert.Equal(TimeBlocks.Night, timeManager.GetCurrentTimeBlock());
        }

        [Fact]
        public void ConsumeTimeBlock_Should_Advance_Clock_Time_Proportionally()
        {
            // Arrange
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithTimeState(t => t.Hour(6)); // Dawn 6:00

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            // Act - Consume 1 time block (should advance ~3.6 hours)
            timeManager.ConsumeTimeBlock(1);

            // Assert - Should advance to Morning (9:00+)
            Assert.True(timeManager.CurrentTimeHours >= 9);
            Assert.Equal(TimeBlocks.Morning, timeManager.GetCurrentTimeBlock());
        }

        [Theory]
        [InlineData(Schedule.Market_Hours, TimeBlocks.Morning, true)]
        [InlineData(Schedule.Market_Hours, TimeBlocks.Afternoon, true)]
        [InlineData(Schedule.Market_Hours, TimeBlocks.Evening, false)]
        [InlineData(Schedule.Workshop_Hours, TimeBlocks.Dawn, true)]
        [InlineData(Schedule.Workshop_Hours, TimeBlocks.Morning, true)]
        [InlineData(Schedule.Workshop_Hours, TimeBlocks.Afternoon, true)]
        [InlineData(Schedule.Workshop_Hours, TimeBlocks.Evening, false)]
        [InlineData(Schedule.Workshop_Hours, TimeBlocks.Night, false)]
        [InlineData(Schedule.Evening_Only, TimeBlocks.Evening, true)]
        [InlineData(Schedule.Evening_Only, TimeBlocks.Morning, false)]
        [InlineData(Schedule.Dawn_Only, TimeBlocks.Dawn, true)]
        [InlineData(Schedule.Dawn_Only, TimeBlocks.Morning, false)]
        [InlineData(Schedule.Night_Only, TimeBlocks.Night, true)]
        [InlineData(Schedule.Night_Only, TimeBlocks.Dawn, false)]
        public void NPC_Schedule_Should_Match_Professional_Logic(Schedule schedule, TimeBlocks timeBlock, bool expectedAvailable)
        {
            // Arrange
            NPC npc = new NPC
            {
                ID = "test_npc",
                Name = "Test NPC",
                AvailabilitySchedule = schedule
            };

            // Act
            bool isAvailable = npc.IsAvailable(timeBlock);

            // Assert
            Assert.Equal(expectedAvailable, isAvailable);
        }
    }
}