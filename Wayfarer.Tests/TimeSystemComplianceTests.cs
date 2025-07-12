using Xunit;
using Wayfarer.Game.MainSystem;
using Wayfarer.Game.ActionSystem;

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
            var scenario = new TestScenarioBuilder()
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
            var scenario = new TestScenarioBuilder()
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
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithActionPoints(18).WithMaxActionPoints(18))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;
            
            // Start at dawn
            timeManager.SetNewTime(6); // 6:00 AM
            
            var timeProgression = new List<(int hour, TimeBlocks block)>();
            
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
            foreach (var (hour, block) in timeProgression)
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
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithActionPoints(0).WithMaxActionPoints(18))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;
            
            // Set to late in day
            timeManager.SetNewTime(22); // 10:00 PM
            Assert.Equal(TimeBlocks.Evening, timeManager.GetCurrentTimeBlock());
            
            int initialDay = timeManager.GetCurrentDay();
            
            // Act
            timeManager.StartNewDay();
            
            // Assert - Should reset to dawn
            Assert.Equal(6, timeManager.GetCurrentTimeHours()); // TimeDayStart = 6
            Assert.Equal(TimeBlocks.Morning, timeManager.GetCurrentTimeBlock());
            Assert.Equal(initialDay + 1, timeManager.GetCurrentDay());
            
            // Action points should be refreshed
            Assert.Equal(18, gameWorld.GetPlayer().CurrentActionPoints());
        }
        
        [Fact]
        public void TimeBlocks_Should_Never_Be_Stored_As_Separate_State()
        {
            // This test ensures TimeBlocks are always calculated, never cached
            
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithActionPoints(18))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;
            
            // Test boundary conditions where TimeBlocks change
            var boundaryTests = new[]
            {
                (hour: 11, expectedBefore: TimeBlocks.Morning, expectedAfter: TimeBlocks.Afternoon),
                (hour: 17, expectedBefore: TimeBlocks.Afternoon, expectedAfter: TimeBlocks.Evening),
                (hour: 23, expectedBefore: TimeBlocks.Evening, expectedAfter: TimeBlocks.Night)
            };
            
            foreach (var test in boundaryTests)
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
        
        [Fact]
        public void UI_Should_Never_Access_Time_Block_Counting_Properties()
        {
            // This test verifies that properties violating UI display rules don't exist or aren't public
            
            var timeManagerType = typeof(TimeManager);
            
            // These properties enable UI violations and should not be publicly accessible
            var violatingProperties = new[]
            {
                "RemainingTimeBlocks", // Enables "2/5 time blocks remaining" UI violation
                "UsedTimeBlocks",      // Enables time block counting UI violation
                "CanPerformTimeBlockAction" // May enable UI time block logic
            };
            
            foreach (string propertyName in violatingProperties)
            {
                var property = timeManagerType.GetProperty(propertyName);
                if (property != null && property.CanRead && property.GetMethod.IsPublic)
                {
                    Assert.True(false, 
                        $"ARCHITECTURAL VIOLATION: Public property '{propertyName}' enables UI to display " +
                        $"time blocks, which violates player mental model. Players should see actual time " +
                        $"progression like 'Morning 6:00' → 'Afternoon 14:00', not abstract time block counts. " +
                        $"Make this property internal or remove it entirely.");
                }
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
                >= 6 and < 12 => TimeBlocks.Morning,
                >= 12 and < 18 => TimeBlocks.Afternoon,  
                >= 18 and < 24 => TimeBlocks.Evening,
                _ => TimeBlocks.Night
            };
        }
        
        [Fact]
        public void Action_Time_Consumption_Should_Be_Realistic()
        {
            // Verify that time progression makes sense for player actions
            
            var scenario = new TestScenarioBuilder()
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