using System;
using System.Threading.Tasks;

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
        /// Advance time by a specified number of segments.
        /// </summary>
        public TimeBlocks AdvanceSegments(int segments)
        {
            TimeBlocks oldBlock = _timeManager.CurrentTimeBlock;
            _timeManager.AdvanceSegments(segments);
            return _timeManager.CurrentTimeBlock;
        }

        /// <summary>
        /// Jump to the next period (4 segments).
        /// </summary>
        public TimeBlocks JumpToNextPeriod()
        {
            TimeBlocks oldBlock = _timeManager.CurrentTimeBlock;
            _timeManager.JumpToNextPeriod();
            return _timeManager.CurrentTimeBlock;
        }

        /// <summary>
        /// Wait until a specific time block.
        /// </summary>
        public int WaitUntilTimeBlock(TimeBlocks targetTime, TimeBlockCalculator calculator)
        {
            TimeBlocks currentTime = _timeManager.CurrentTimeBlock;
            int currentSegment = _timeManager.CurrentSegment;
            int segmentsInCurrentPeriod = _timeManager.TimeModel.CurrentState.SegmentsInCurrentBlock;

            // Calculate segments to wait based on time progression design:
            // Dawn: 3 segments, Midday: 4 segments, Afternoon: 4 segments, Evening: 4 segments, Night: 1 segment
            int segmentsToWait = currentTime switch
            {
                TimeBlocks.Dawn => 3 - segmentsInCurrentPeriod,        // Wait until end of Dawn (3 segments)
                TimeBlocks.Midday => 4 - segmentsInCurrentPeriod,      // Wait until end of Midday (4 segments) 
                TimeBlocks.Afternoon => 4 - segmentsInCurrentPeriod,   // Wait until end of Afternoon (4 segments)
                TimeBlocks.Evening => 4 - segmentsInCurrentPeriod,     // Wait until end of Evening (4 segments)
                TimeBlocks.Night => 1 - segmentsInCurrentPeriod,       // Wait until end of Night (1 segment)
                TimeBlocks.DeepNight => _timeManager.SegmentsRemainingInDay, // Jump to next day
                _ => 0
            };

            if (segmentsToWait > 0)
            {
                // Get narrative description
                string narrative = calculator.GetWaitingNarrative(targetTime);
                _messageSystem.AddSystemMessage(narrative, SystemMessageTypes.Info);

                // Advance time
                AdvanceSegments(segmentsToWait);
            }

            return segmentsToWait;
        }

        /// <summary>
        /// Check if player can perform an action requiring specified segments.
        /// </summary>
        public bool CanPerformAction(int segmentsRequired)
        {
            return _timeManager.CanPerformAction(segmentsRequired);
        }

        /// <summary>
        /// Spend segments for an action.
        /// </summary>
        public async Task<bool> SpendSegmentsForActionAsync(int segments, string actionDescription)
        {
            return await _timeManager.SpendSegments(segments, actionDescription);
        }

        /// <summary>
        /// Get segments remaining in current day.
        /// </summary>
        public int GetSegmentsRemainingInDay()
        {
            return _timeManager.SegmentsRemainingInDay;
        }
    }
}