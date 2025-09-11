using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wayfarer.Subsystems.TimeSubsystem
{
    /// <summary>
    /// Public facade for all time-related operations.
    /// Single entry point for time management, progression, and display.
    /// </summary>
    public class TimeFacade
    {
        private readonly TimeManager _timeManager;
        private readonly TimeBlockCalculator _timeBlockCalculator;
        private readonly TimeProgressionManager _timeProgressionManager;
        private readonly TimeDisplayFormatter _timeDisplayFormatter;
        private readonly GameWorld _gameWorld;

        public TimeFacade(
            TimeManager timeManager,
            TimeBlockCalculator timeBlockCalculator,
            TimeProgressionManager timeProgressionManager,
            TimeDisplayFormatter timeDisplayFormatter,
            GameWorld gameWorld)
        {
            _timeManager = timeManager;
            _timeBlockCalculator = timeBlockCalculator;
            _timeProgressionManager = timeProgressionManager;
            _timeDisplayFormatter = timeDisplayFormatter;
            _gameWorld = gameWorld;
        }

        // ========== TIME STATE ==========

        public int GetCurrentDay()
        {
            return _gameWorld.CurrentDay;
        }

        public int GetCurrentSegment()
        {
            return _timeManager.CurrentSegment;
        }

        public int GetSegmentsInCurrentPeriod()
        {
            return _timeManager.TimeModel.CurrentState.SegmentsInCurrentBlock;
        }

        public TimeBlocks GetCurrentTimeBlock()
        {
            return _timeManager.CurrentTimeBlock;
        }

        public int GetSegmentsRemainingInDay()
        {
            return _timeManager.SegmentsRemainingInDay;
        }

        public TimeInfo GetTimeInfo()
        {
            return new TimeInfo(
                GetCurrentTimeBlock(),
                GetSegmentsRemainingInDay(),
                GetCurrentDay(),
                _timeManager.GetSegmentDisplay());
        }

        // ========== TIME PROGRESSION ==========

        public TimeBlocks AdvanceSegments(int segments)
        {
            return _timeProgressionManager.AdvanceSegments(segments);
        }

        public TimeBlocks JumpToNextPeriod()
        {
            return _timeProgressionManager.JumpToNextPeriod();
        }

        public int WaitUntilNextTimeBlock()
        {
            TimeBlocks current = GetCurrentTimeBlock();
            TimeBlocks next = GetNextTimeBlock(current);
            return _timeProgressionManager.WaitUntilTimeBlock(next, _timeBlockCalculator);
        }

        public async Task<bool> SpendSegments(int segments, string description)
        {
            return await _timeManager.SpendSegments(segments, description);
        }

        public bool CanPerformAction(int segmentsRequired)
        {
            return _timeProgressionManager.CanPerformAction(segmentsRequired);
        }

        // ========== TIME BLOCK CALCULATIONS ==========

        public int GetTimeBlockStartSegment(TimeBlocks timeBlock)
        {
            return _timeBlockCalculator.GetTimeBlockStartSegment(timeBlock);
        }

        public int GetTimeBlockEndSegment(TimeBlocks timeBlock)
        {
            return _timeBlockCalculator.GetTimeBlockEndSegment(timeBlock);
        }

        public int CalculateSegmentsUntilTimeBlock(TimeBlocks target)
        {
            return _timeBlockCalculator.CalculateSegmentsUntilTimeBlock(
                GetCurrentTimeBlock(),
                target,
                GetCurrentSegment());
        }

        public TimeBlocks? GetNextAvailableTimeBlock(List<TimeBlocks> availableTimes)
        {
            return _timeBlockCalculator.GetNextAvailableTimeBlock(
                GetCurrentTimeBlock(),
                availableTimes);
        }

        public string GetTimeBlockDisplayName(TimeBlocks timeBlock)
        {
            return _timeBlockCalculator.GetTimeBlockDisplayName(timeBlock);
        }

        public string GetWaitingNarrative(TimeBlocks targetTime)
        {
            return _timeBlockCalculator.GetWaitingNarrative(targetTime);
        }

        private TimeBlocks GetNextTimeBlock(TimeBlocks current)
        {
            return current switch
            {
                TimeBlocks.Dawn => TimeBlocks.Midday,
                TimeBlocks.Midday => TimeBlocks.Afternoon,
                TimeBlocks.Afternoon => TimeBlocks.Evening,
                TimeBlocks.Evening => TimeBlocks.Night,
                TimeBlocks.Night => TimeBlocks.DeepNight,
                TimeBlocks.DeepNight => TimeBlocks.Dawn,
                _ => TimeBlocks.Dawn
            };
        }

        // ========== TIME DISPLAY ==========

        public string GetFormattedTimeDisplay()
        {
            return _timeDisplayFormatter.GetFormattedTimeDisplay();
        }

        public string GetTimeString()
        {
            return _timeDisplayFormatter.GetTimeString();
        }

        public string GetTimeDescription()
        {
            return _timeDisplayFormatter.GetTimeDescription();
        }

        public string FormatDuration(int segments)
        {
            return _timeDisplayFormatter.FormatDuration(segments);
        }

        public string FormatSegments(int segments)
        {
            return _timeDisplayFormatter.FormatSegments(segments);
        }

        public string GetDayName(int day)
        {
            return _timeDisplayFormatter.GetDayName(day);
        }

        public string GetShortDayName(int day)
        {
            return _timeDisplayFormatter.GetShortDayName(day);
        }

        // ========== AVAILABILITY WINDOWS ==========

        public string GetNextAvailableTimeDisplay(List<TimeBlocks> availableTimes)
        {
            TimeBlocks? nextTime = GetNextAvailableTimeBlock(availableTimes);
            if (!nextTime.HasValue) return "Not available today";

            int segmentsUntil = CalculateSegmentsUntilTimeBlock(nextTime.Value);
            string timeBlockName = GetTimeBlockDisplayName(nextTime.Value);

            if (segmentsUntil == 0)
            {
                return $"Available now during {timeBlockName}";
            }
            else if (segmentsUntil == 1)
            {
                return $"Available in 1 segment at {timeBlockName}";
            }
            else
            {
                return $"Available in {segmentsUntil} segments at {timeBlockName}";
            }
        }

        public bool IsTimeBlockAvailable(TimeBlocks timeBlock, List<TimeBlocks> availableTimes)
        {
            return availableTimes.Contains(timeBlock);
        }

        public bool IsCurrentlyAvailable(List<TimeBlocks> availableTimes)
        {
            return IsTimeBlockAvailable(GetCurrentTimeBlock(), availableTimes);
        }

        // ========== LEGACY COMPATIBILITY METHODS ==========
    }
}