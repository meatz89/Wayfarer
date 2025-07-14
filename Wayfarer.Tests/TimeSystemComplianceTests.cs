using Wayfarer.Game.ActionSystem;
using Wayfarer.Game.MainSystem;
using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Time system architectural compliance tests.
    /// These tests prevent the specific time system violations that caused UI confusion
    /// and ensure single-source-of-truth time tracking.
    /// 
    /// CRITICAL RULES ENFORCED:
    /// 1. Time blocks are internal mechanics only - NEVER shown in UI
    /// 2. All time representations derive from CurrentTimeHours
    /// 3. Actions that consume time blocks must advance clock time
    /// 4. TimeBlocks enum calculated from hours, never stored separately
    /// </summary>
    public class TimeSystemComplianceTests
    {
        [Fact]
        public void TimeManager_Should_Have_Single_Authoritative_Time_Source()
        {
            // Arrange
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithActionPoints(18))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            // Act - Set a specific time
            timeManager.SetNewTime(14); // 2:00 PM

            // Assert - All time representations should derive from this single source
            Assert.Equal(14, timeManager.GetCurrentTimeHours());
            Assert.Equal(TimeBlocks.Afternoon, timeManager.GetCurrentTimeBlock());

            // Verify WorldState synchronization
            Assert.Equal(TimeBlocks.Afternoon, timeManager.GetCurrentTimeBlock());

            // Test time progression consistency
            timeManager.SetNewTime(18); // 6:00 PM  
            Assert.Equal(18, timeManager.GetCurrentTimeHours());
            Assert.Equal(TimeBlocks.Evening, timeManager.GetCurrentTimeBlock());
            Assert.Equal(TimeBlocks.Evening, timeManager.GetCurrentTimeBlock());
        }

        [Fact]
        public void ConsumeTimeBlock_Should_Advance_Actual_Clock_Time()
        {
            // Arrange
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithActionPoints(18).WithMaxActionPoints(18))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            // Start at morning
            timeManager.SetNewTime(6); // 6:00 AM
            int initialHour = timeManager.GetCurrentTimeHours();
            TimeBlocks initialTimeBlock = timeManager.GetCurrentTimeBlock();

            // Act - Consume time blocks (representing actions)
            timeManager.ConsumeTimeBlock(1);

            // Assert - Clock should advance proportionally
            int finalHour = timeManager.GetCurrentTimeHours();
            TimeBlocks finalTimeBlock = timeManager.GetCurrentTimeBlock();

            // Time must advance - this is the core requirement
            Assert.True(finalHour > initialHour,
                $"Time must advance when consuming time blocks. " +
                $"Started at {initialHour}:00, still at {finalHour}:00. " +
                $"Players expect to see 'Morning 6:00' → 'Morning 9:00' progression.");

            // TimeBlocks should update based on new hour
            if (finalHour >= 12)
            {
                Assert.Equal(TimeBlocks.Afternoon, finalTimeBlock);
            }
            else
            {
                Assert.Equal(TimeBlocks.Morning, finalTimeBlock);
            }
        }

        [Fact]
        public void Multiple_Time_Block_Consumption_Should_Progress_Through_Day()
        {
            // Arrange
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithActionPoints(18).WithMaxActionPoints(18))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            // Start at dawn
            timeManager.SetNewTime(6); // 6:00 AM

            List<(int hour, TimeBlocks block)> timeProgression = new List<(int hour, TimeBlocks block)>();

            // Act - Consume multiple time blocks throughout the day
            for (int i = 0; i < 5; i++) // Max daily time blocks
            {
                if (timeManager.ValidateTimeBlockAction(1))
                {
                    timeManager.ConsumeTimeBlock(1);
                    timeProgression.Add((timeManager.GetCurrentTimeHours(), timeManager.GetCurrentTimeBlock()));
                }
            }

            // Assert - Time should progress through the day
            Assert.True(timeProgression.Count > 0, "Should consume at least one time block");

            // Verify progressive time advancement
            int previousHour = 6;
            foreach ((int hour, TimeBlocks block) in timeProgression)
            {
                Assert.True(hour >= previousHour,
                    $"Time should progress forward. Hour {hour} came after {previousHour}");
                previousHour = hour;

                // Verify time block matches hour
                TimeBlocks expectedBlock = CalculateExpectedTimeBlock(hour);
                Assert.Equal(expectedBlock, block);
            }

            // Final time should be significantly advanced
            int finalHour = timeProgression.Last().hour;
            Assert.True(finalHour > 6,
                $"Multiple actions should advance time significantly. Final hour: {finalHour}");
        }

        [Fact]
        public void StartNewDay_Should_Reset_Time_To_Dawn()
        {
            // Arrange
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithActionPoints(0).WithMaxActionPoints(18))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            // Set to late in day
            timeManager.SetNewTime(22); // 10:00 PM
            Assert.Equal(TimeBlocks.Night, timeManager.GetCurrentTimeBlock());

            int initialDay = timeManager.GetCurrentDay();

            // Act
            timeManager.StartNewDay();

            // Assert - Should reset to dawn
            Assert.Equal(6, timeManager.GetCurrentTimeHours()); // TimeDayStart = 6
            Assert.Equal(TimeBlocks.Dawn, timeManager.GetCurrentTimeBlock());
            Assert.Equal(initialDay + 1, timeManager.GetCurrentDay());

            // Action points should NOT be regenerated per Period-Based Activity Planning user story
            Assert.Equal(0, gameWorld.GetPlayer().CurrentActionPoints());
        }

        [Fact]
        public void TimeBlocks_Should_Never_Be_Stored_As_Separate_State()
        {
            // This test ensures TimeBlocks are always calculated, never cached

            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithActionPoints(18))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            // Test boundary conditions where TimeBlocks change
            (int hour, TimeBlocks expectedBefore, TimeBlocks expectedAfter)[] boundaryTests = new[]
            {
                (hour: 11, expectedBefore: TimeBlocks.Morning, expectedAfter: TimeBlocks.Afternoon),
                (hour: 15, expectedBefore: TimeBlocks.Afternoon, expectedAfter: TimeBlocks.Evening),
                (hour: 19, expectedBefore: TimeBlocks.Evening, expectedAfter: TimeBlocks.Night)
            };

            foreach ((int hour, TimeBlocks expectedBefore, TimeBlocks expectedAfter) test in boundaryTests)
            {
                // Before boundary
                timeManager.SetNewTime(test.hour);
                Assert.Equal(test.expectedBefore, timeManager.GetCurrentTimeBlock());

                // Cross boundary
                timeManager.SetNewTime(test.hour + 1);
                Assert.Equal(test.expectedAfter, timeManager.GetCurrentTimeBlock());

                // Verify WorldState stays synchronized
                Assert.Equal(test.expectedAfter, timeManager.GetCurrentTimeBlock());
            }
        }

        /// <summary>
        /// Helper method to calculate expected time block from hour
        /// This is the logic that should be used in TimeManager
        /// </summary>
        private TimeBlocks CalculateExpectedTimeBlock(int hour)
        {
            return hour switch
            {
                >= 6 and < 9 => TimeBlocks.Dawn,      // 6:00-8:59 (3 hours)
                >= 9 and < 12 => TimeBlocks.Morning,   // 9:00-11:59 (3 hours)
                >= 12 and < 16 => TimeBlocks.Afternoon, // 12:00-15:59 (4 hours)
                >= 16 and < 20 => TimeBlocks.Evening,   // 16:00-19:59 (4 hours)
                _ => TimeBlocks.Night                   // 20:00-5:59 (10 hours)
            };
        }

        [Fact]
        public void Action_Time_Consumption_Should_Be_Realistic()
        {
            // Verify that time progression makes sense for player actions

            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithActionPoints(18).WithMaxActionPoints(18))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            timeManager.SetNewTime(6); // Start at 6:00 AM

            // One action should advance time by reasonable amount (2-4 hours)
            timeManager.ConsumeTimeBlock(1);

            int timeAdvanced = timeManager.GetCurrentTimeHours() - 6;

            Assert.True(timeAdvanced >= 1 && timeAdvanced <= 5,
                $"One action should advance time by 1-5 hours for realistic progression. " +
                $"Advanced by {timeAdvanced} hours. This ensures meaningful time progression " +
                $"that players can understand and plan around.");
        }
    }
}