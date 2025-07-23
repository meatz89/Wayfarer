using Wayfarer.Core.Repositories;

namespace Wayfarer.Core.Repositories.Implementation
{
    /// <summary>
    /// Adapter that provides ITimeManager interface over TimeManager
    /// </summary>
    public class TimeManagerAdapter : ITimeManager
    {
        private readonly TimeManager _timeManager;

        public TimeManagerAdapter(TimeManager timeManager)
        {
            _timeManager = timeManager ?? throw new System.ArgumentNullException(nameof(timeManager));
        }

        public int GetCurrentTimeHours() => _timeManager.GetCurrentTimeHours();

        public int GetCurrentDay() => _timeManager.GetCurrentDay();

        public TimeBlocks GetCurrentTimeBlock() => _timeManager.GetCurrentTimeBlock();

        public bool CanPerformAction(int hoursRequired = 1) => _timeManager.CanPerformAction(hoursRequired);

        public int HoursRemaining => _timeManager.HoursRemaining;
    }
}