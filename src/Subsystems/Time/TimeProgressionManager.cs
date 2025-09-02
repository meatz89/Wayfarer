using System;

namespace Wayfarer.Subsystems.TimeSubsystem
{
    /// <summary>
    /// Manages time progression and advancement mechanics.
    /// </summary>
    public class TimeProgressionManager
    {
        private readonly TimeManager _timeManager;
        private readonly MessageSystem _messageSystem;

        public TimeProgressionManager(TimeManager timeManager, MessageSystem messageSystem)
        {
            _timeManager = timeManager;
            _messageSystem = messageSystem;
        }

        /// <summary>
        /// Advance time by a specified number of hours.
        /// </summary>
        public TimeBlocks AdvanceTimeByHours(int hours)
        {
            TimeBlocks oldBlock = _timeManager.CurrentTimeBlock;
            _timeManager.AdvanceTime(hours);
            return _timeManager.CurrentTimeBlock;
        }

        /// <summary>
        /// Advance time by a specified number of minutes.
        /// </summary>
        public TimeBlocks AdvanceTimeByMinutes(int minutes)
        {
            TimeBlocks oldBlock = _timeManager.CurrentTimeBlock;
            _timeManager.AdvanceTimeMinutes(minutes);
            return _timeManager.CurrentTimeBlock;
        }

        /// <summary>
        /// Wait until a specific time block.
        /// </summary>
        public int WaitUntilTimeBlock(TimeBlocks targetTime, TimeBlockCalculator calculator)
        {
            TimeBlocks currentTime = _timeManager.CurrentTimeBlock;
            int currentHour = _timeManager.CurrentHour;

            // Calculate hours to wait
            int hoursToWait = currentTime switch
            {
                TimeBlocks.Dawn => 8 - currentHour,      // Dawn (6-8) -> Morning (8)
                TimeBlocks.Morning => 12 - currentHour,   // Morning (8-12) -> Afternoon (12)
                TimeBlocks.Afternoon => 17 - currentHour, // Afternoon (12-17) -> Evening (17)
                TimeBlocks.Evening => 20 - currentHour,   // Evening (17-20) -> Night (20)
                TimeBlocks.Night => 22 - currentHour,     // Night (20-22) -> Late Night (22)
                TimeBlocks.LateNight => 30 - currentHour, // Late Night (22-6) -> Next Dawn (+6)
                _ => 0
            };

            if (hoursToWait > 0)
            {
                // Get narrative description
                string narrative = calculator.GetWaitingNarrative(targetTime);
                _messageSystem.AddSystemMessage(narrative, SystemMessageTypes.Info);

                // Advance time
                AdvanceTimeByHours(hoursToWait);
            }

            return hoursToWait;
        }

        /// <summary>
        /// Check if player can perform an action requiring specified hours.
        /// </summary>
        public bool CanPerformAction(int hoursRequired)
        {
            return _timeManager.CanPerformAction(hoursRequired);
        }

        /// <summary>
        /// Spend time for an action.
        /// </summary>
        public async Task<bool> SpendTimeForActionAsync(int hours, string actionDescription)
        {
            return await _timeManager.SpendTime(hours, actionDescription);
        }

        /// <summary>
        /// Get hours remaining in current day.
        /// </summary>
        public int GetHoursRemaining()
        {
            return _timeManager.HoursRemaining;
        }
    }
}