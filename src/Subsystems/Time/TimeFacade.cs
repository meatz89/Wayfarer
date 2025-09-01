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
        
        public int GetCurrentHour()
        {
            return _timeManager.CurrentHour;
        }
        
        public int GetCurrentMinutes()
        {
            return _timeManager.GetCurrentMinutes();
        }
        
        public TimeBlocks GetCurrentTimeBlock()
        {
            return _timeManager.CurrentTimeBlock;
        }
        
        public int GetHoursRemaining()
        {
            return _timeManager.HoursRemaining;
        }
        
        public TimeInfo GetTimeInfo()
        {
            return new TimeInfo(
                GetCurrentTimeBlock(),
                GetHoursRemaining(),
                GetCurrentDay());
        }
        
        // ========== TIME PROGRESSION ==========
        
        public TimeBlocks AdvanceTimeByHours(int hours)
        {
            return _timeProgressionManager.AdvanceTimeByHours(hours);
        }
        
        public TimeBlocks AdvanceTimeByMinutes(int minutes)
        {
            return _timeProgressionManager.AdvanceTimeByMinutes(minutes);
        }
        
        public int WaitUntilNextTimeBlock()
        {
            TimeBlocks current = GetCurrentTimeBlock();
            TimeBlocks next = GetNextTimeBlock(current);
            return _timeProgressionManager.WaitUntilTimeBlock(next, _timeBlockCalculator);
        }
        
        public async Task<bool> SpendTime(int hours, string description)
        {
            return await _timeManager.SpendTime(hours, description);
        }
        
        public bool CanPerformAction(int hoursRequired)
        {
            return _timeProgressionManager.CanPerformAction(hoursRequired);
        }
        
        // ========== TIME BLOCK CALCULATIONS ==========
        
        public int GetTimeBlockStartHour(TimeBlocks timeBlock)
        {
            return _timeBlockCalculator.GetTimeBlockStartHour(timeBlock);
        }
        
        public int GetTimeBlockEndHour(TimeBlocks timeBlock)
        {
            return _timeBlockCalculator.GetTimeBlockEndHour(timeBlock);
        }
        
        public int CalculateHoursUntilTimeBlock(TimeBlocks target)
        {
            return _timeBlockCalculator.CalculateHoursUntilTimeBlock(
                GetCurrentTimeBlock(), 
                target, 
                GetCurrentHour());
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
                TimeBlocks.Dawn => TimeBlocks.Morning,
                TimeBlocks.Morning => TimeBlocks.Afternoon,
                TimeBlocks.Afternoon => TimeBlocks.Evening,
                TimeBlocks.Evening => TimeBlocks.Night,
                TimeBlocks.Night => TimeBlocks.LateNight,
                TimeBlocks.LateNight => TimeBlocks.Dawn,
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
        
        public string FormatDuration(int hours)
        {
            return _timeDisplayFormatter.FormatDuration(hours);
        }
        
        public string FormatMinutes(int minutes)
        {
            return _timeDisplayFormatter.FormatMinutes(minutes);
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
            
            int hoursUntil = CalculateHoursUntilTimeBlock(nextTime.Value);
            string timeBlockName = GetTimeBlockDisplayName(nextTime.Value);
            
            if (hoursUntil == 0)
            {
                return $"Available now during {timeBlockName}";
            }
            else if (hoursUntil == 1)
            {
                return $"Available in 1 hour at {timeBlockName}";
            }
            else
            {
                return $"Available in {hoursUntil} hours at {timeBlockName}";
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
    }
}