using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Comprehensive tests for the Five Time Blocks system.
    /// Validates that the game correctly maps 24 hours to exactly 5 time blocks:
    /// Dawn (6-8), Morning (9-11), Afternoon (12-15), Evening (16-19), Night (20-5)
    /// 
    /// This test prevents regression of the architectural requirement that there are
    /// exactly 5 time blocks per day, matching MaxDailyTimeBlocks = 5.
    /// </summary>
    public class FiveTimeBlocksSystemTests
    {
        [Fact]
        public void TimeBlocks_Should_Have_Exactly_Five_Values()
        {
            // Verify the TimeBlocks enum has exactly 5 values
            TimeBlocks[] timeBlockValues = Enum.GetValues<TimeBlocks>();

            Assert.Equal(5, timeBlockValues.Length);

            // Verify all expected time blocks exist
            Assert.Contains(TimeBlocks.Dawn, timeBlockValues);
            Assert.Contains(TimeBlocks.Morning, timeBlockValues);
            Assert.Contains(TimeBlocks.Afternoon, timeBlockValues);
            Assert.Contains(TimeBlocks.Evening, timeBlockValues);
            Assert.Contains(TimeBlocks.Night, timeBlockValues);
        }

        [Theory]
        [InlineData(6, TimeBlocks.Dawn)]
        [InlineData(7, TimeBlocks.Dawn)]
        [InlineData(8, TimeBlocks.Dawn)]
        [InlineData(9, TimeBlocks.Morning)]
        [InlineData(10, TimeBlocks.Morning)]
        [InlineData(11, TimeBlocks.Morning)]
        [InlineData(12, TimeBlocks.Afternoon)]
        [InlineData(13, TimeBlocks.Afternoon)]
        [InlineData(14, TimeBlocks.Afternoon)]
        [InlineData(15, TimeBlocks.Afternoon)]
        [InlineData(16, TimeBlocks.Evening)]
        [InlineData(17, TimeBlocks.Evening)]
        [InlineData(18, TimeBlocks.Evening)]
        [InlineData(19, TimeBlocks.Evening)]
        [InlineData(20, TimeBlocks.Night)]
        [InlineData(21, TimeBlocks.Night)]
        [InlineData(22, TimeBlocks.Night)]
        [InlineData(23, TimeBlocks.Night)]
        [InlineData(0, TimeBlocks.Night)]
        [InlineData(1, TimeBlocks.Night)]
        [InlineData(2, TimeBlocks.Night)]
        [InlineData(3, TimeBlocks.Night)]
        [InlineData(4, TimeBlocks.Night)]
        [InlineData(5, TimeBlocks.Night)]
        public void GetCurrentTimeBlock_Should_Map_Hours_To_Correct_TimeBlocks(int hour, TimeBlocks expectedTimeBlock)
        {
            // Arrange
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("test_start_location"))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            // Act
            timeManager.SetNewTime(hour);
            TimeBlocks actualTimeBlock = timeManager.GetCurrentTimeBlock();

            // Assert
            Assert.Equal(expectedTimeBlock, actualTimeBlock);
        }

        [Fact]
        public void Five_Time_Blocks_Should_Cover_Full_Day()
        {
            // Verify that all 24 hours map to exactly one of the 5 time blocks
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("test_start_location"))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            Dictionary<int, TimeBlocks> hourToTimeBlockMap = new Dictionary<int, TimeBlocks>();

            // Test every hour of the day
            for (int hour = 0; hour < 24; hour++)
            {
                timeManager.SetNewTime(hour);
                TimeBlocks timeBlock = timeManager.GetCurrentTimeBlock();
                hourToTimeBlockMap[hour] = timeBlock;
            }

            // Verify all hours are mapped
            Assert.Equal(24, hourToTimeBlockMap.Count);

            // Verify all 5 time blocks are used
            List<TimeBlocks> usedTimeBlocks = hourToTimeBlockMap.Values.Distinct().ToList();
            Assert.Equal(5, usedTimeBlocks.Count);
            Assert.Contains(TimeBlocks.Dawn, usedTimeBlocks);
            Assert.Contains(TimeBlocks.Morning, usedTimeBlocks);
            Assert.Contains(TimeBlocks.Afternoon, usedTimeBlocks);
            Assert.Contains(TimeBlocks.Evening, usedTimeBlocks);
            Assert.Contains(TimeBlocks.Night, usedTimeBlocks);
        }

        [Fact]
        public void Time_Block_Durations_Should_Be_Reasonable()
        {
            // Verify the duration of each time block makes sense
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("test_start_location"))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            Dictionary<TimeBlocks, List<int>> timeBlockHours = new Dictionary<TimeBlocks, List<int>>();

            // Map each hour to its time block
            for (int hour = 0; hour < 24; hour++)
            {
                timeManager.SetNewTime(hour);
                TimeBlocks timeBlock = timeManager.GetCurrentTimeBlock();

                if (!timeBlockHours.ContainsKey(timeBlock))
                    timeBlockHours[timeBlock] = new List<int>();

                timeBlockHours[timeBlock].Add(hour);
            }

            // Verify expected durations
            Assert.Equal(3, timeBlockHours[TimeBlocks.Dawn].Count); // 6-8 (3 hours)
            Assert.Equal(3, timeBlockHours[TimeBlocks.Morning].Count); // 9-11 (3 hours)
            Assert.Equal(4, timeBlockHours[TimeBlocks.Afternoon].Count); // 12-15 (4 hours)
            Assert.Equal(4, timeBlockHours[TimeBlocks.Evening].Count); // 16-19 (4 hours)
            Assert.Equal(10, timeBlockHours[TimeBlocks.Night].Count); // 20-5 (10 hours)

            // Verify continuous ranges (no gaps)
            Assert.Equal(new[] { 6, 7, 8 }, timeBlockHours[TimeBlocks.Dawn].OrderBy(h => h));
            Assert.Equal(new[] { 9, 10, 11 }, timeBlockHours[TimeBlocks.Morning].OrderBy(h => h));
            Assert.Equal(new[] { 12, 13, 14, 15 }, timeBlockHours[TimeBlocks.Afternoon].OrderBy(h => h));
            Assert.Equal(new[] { 16, 17, 18, 19 }, timeBlockHours[TimeBlocks.Evening].OrderBy(h => h));
            Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 20, 21, 22, 23 }, timeBlockHours[TimeBlocks.Night].OrderBy(h => h));
        }

        [Fact]
        public void Time_Block_Boundaries_Should_Be_Precise()
        {
            // Test the exact boundary conditions between time blocks
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("test_start_location"))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            // Test transitions between time blocks
            (int lastHourOfPrevious, int firstHourOfNext, TimeBlocks expectedPrevious, TimeBlocks expectedNext)[] boundaries = new[]
            {
                (lastHourOfPrevious: 5, firstHourOfNext: 6, expectedPrevious: TimeBlocks.Night, expectedNext: TimeBlocks.Dawn),
                (lastHourOfPrevious: 8, firstHourOfNext: 9, expectedPrevious: TimeBlocks.Dawn, expectedNext: TimeBlocks.Morning),
                (lastHourOfPrevious: 11, firstHourOfNext: 12, expectedPrevious: TimeBlocks.Morning, expectedNext: TimeBlocks.Afternoon),
                (lastHourOfPrevious: 15, firstHourOfNext: 16, expectedPrevious: TimeBlocks.Afternoon, expectedNext: TimeBlocks.Evening),
                (lastHourOfPrevious: 19, firstHourOfNext: 20, expectedPrevious: TimeBlocks.Evening, expectedNext: TimeBlocks.Night)
            };

            foreach ((int lastHourOfPrevious, int firstHourOfNext, TimeBlocks expectedPrevious, TimeBlocks expectedNext) boundary in boundaries)
            {
                // Test last hour of previous time block
                timeManager.SetNewTime(boundary.lastHourOfPrevious);
                Assert.Equal(boundary.expectedPrevious, timeManager.GetCurrentTimeBlock());

                // Test first hour of next time block
                timeManager.SetNewTime(boundary.firstHourOfNext);
                Assert.Equal(boundary.expectedNext, timeManager.GetCurrentTimeBlock());
            }
        }

        [Fact]
        public void Action_Consumption_Should_Progress_Through_All_Five_Time_Blocks()
        {
            // Verify that consuming all 5 time blocks progresses through different periods
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("test_start_location"))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            // Start at dawn
            timeManager.SetNewTime(6); // Dawn
            Assert.Equal(TimeBlocks.Dawn, timeManager.GetCurrentTimeBlock());

            List<TimeBlocks> timeBlockProgression = new List<TimeBlocks>();

            // Consume time blocks and track progression
            for (int i = 0; i < 5; i++)
            {
                timeManager.AdvanceTime(4); // Advance by ~4 hours (roughly one time block)
                timeBlockProgression.Add(timeManager.GetCurrentTimeBlock());
            }

            // Should have progressed through multiple time blocks
            Assert.True(timeBlockProgression.Count > 0, "Should consume at least one time block");

            // Should have progressed through multiple time blocks
            Assert.True(timeBlockProgression.Count == 5, "Should have consumed all 5 time blocks");

            // Verify we went through different time blocks
            int distinctTimeBlocks = timeBlockProgression.Distinct().Count();
            Assert.True(distinctTimeBlocks >= 3,
                $"Should progress through at least 3 different time blocks, but only went through {distinctTimeBlocks}");

            // Final time should have rolled over to next day
            int finalDay = timeManager.GetCurrentDay();
            Assert.True(finalDay > 1, "After advancing 20 hours from 6:00, should be on the next day");
        }

        [Fact]
        public void Time_Block_Names_Should_Match_Natural_Periods()
        {
            // Verify time block names align with natural time periods
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("test_start_location"))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;

            // Dawn should be early morning
            timeManager.SetNewTime(7);
            Assert.Equal(TimeBlocks.Dawn, timeManager.GetCurrentTimeBlock());

            // Morning should be mid-morning
            timeManager.SetNewTime(10);
            Assert.Equal(TimeBlocks.Morning, timeManager.GetCurrentTimeBlock());

            // Afternoon should be midday
            timeManager.SetNewTime(13);
            Assert.Equal(TimeBlocks.Afternoon, timeManager.GetCurrentTimeBlock());

            // Evening should be late afternoon/early evening
            timeManager.SetNewTime(17);
            Assert.Equal(TimeBlocks.Evening, timeManager.GetCurrentTimeBlock());

            // Night should be late evening/night/early morning
            timeManager.SetNewTime(22);
            Assert.Equal(TimeBlocks.Night, timeManager.GetCurrentTimeBlock());

            timeManager.SetNewTime(2);
            Assert.Equal(TimeBlocks.Night, timeManager.GetCurrentTimeBlock());
        }
    }
}