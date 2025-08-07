using System;

/// <summary>
/// Time model that serves as the single source of truth for all time-related state.
/// Builds on TimeState to provide a complete time management system.
/// </summary>
public class TimeModel
{
    // Time constants - single source of truth
    public const int HOURS_PER_DAY = 24;
    public const int ACTIVE_DAY_START = 6;   // 6 AM
    public const int ACTIVE_DAY_END = 22;    // 10 PM
    public const int ACTIVE_HOURS_PER_DAY = ACTIVE_DAY_END - ACTIVE_DAY_START;

    // Time block boundaries
    public const int DAWN_START = 6;
    public const int DAWN_END = 8;
    public const int MORNING_START = 8;
    public const int MORNING_END = 12;
    public const int AFTERNOON_START = 12;
    public const int AFTERNOON_END = 16;
    public const int EVENING_START = 16;
    public const int EVENING_END = 20;
    public const int NIGHT_START = 20;
    public const int NIGHT_END = 22;
    public const int LATE_NIGHT_START = 22;
    public const int LATE_NIGHT_END = 6;

    private TimeState _currentState;
    private readonly object _lock = new object();

    public TimeState CurrentState
    {
        get
        {
            lock (_lock)
            {
                return _currentState;
            }
        }
    }

    public int CurrentHour => CurrentState.CurrentHour;

    public int CurrentDay => CurrentState.CurrentDay;

    public TimeBlocks CurrentTimeBlock => CurrentState.CurrentTimeBlock;

    public int ActiveHoursRemaining => CurrentState.ActiveHoursRemaining;

    public bool IsActiveTime => CurrentState.IsActiveTime;

    // Events removed per architecture guidelines - use return values instead

    public TimeModel(int startDay = 1, int startHour = ACTIVE_DAY_START, int startMinute = 0)
    {
        _currentState = new TimeState(startDay, startHour, startMinute);
    }

    /// <summary>
    /// Validates that a time transition is valid before applying it.
    /// </summary>
    public ValidationResult ValidateTimeTransition(int hours)
    {
        if (hours <= 0)
            return ValidationResult.Failure("Hours must be positive");

        if (!IsActiveTime && hours > 0)
            return ValidationResult.Warning("Currently outside active hours");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Advances time atomically with full validation and event notification.
    /// </summary>
    public TimeAdvancementResult AdvanceTime(int hours)
    {
        if (hours <= 0)
            throw new ArgumentException("Hours must be positive", nameof(hours));

        lock (_lock)
        {
            TimeAdvancementResult result = _currentState.AdvanceTime(hours);
            _currentState = result.NewState;

            return result;
        }
    }
    
    /// <summary>
    /// Advances time by minutes with full validation.
    /// </summary>
    public TimeAdvancementResult AdvanceTimeMinutes(int minutes)
    {
        if (minutes <= 0)
            throw new ArgumentException("Minutes must be positive", nameof(minutes));

        lock (_lock)
        {
            TimeAdvancementResult result = _currentState.AdvanceTimeMinutes(minutes);
            _currentState = result.NewState;

            return result;
        }
    }

    /// <summary>
    /// Checks if the specified action can be performed within active hours.
    /// </summary>
    public bool CanPerformAction(int hoursRequired)
    {
        return CurrentState.CanSpendActiveHours(hoursRequired);
    }

    /// <summary>
    /// Forces a new day to start at dawn.
    /// Returns the time advancement result for the caller to handle.
    /// </summary>
    public TimeAdvancementResult AdvanceToNextDay()
    {
        lock (_lock)
        {
            TimeState oldState = _currentState;
            _currentState = _currentState.AdvanceToNextDay();

            TimeAdvancementResult result = new TimeAdvancementResult
            {
                OldState = oldState,
                NewState = _currentState,
                HoursAdvanced = 0,
                DaysAdvanced = 1,
                CrossedDayBoundary = true,
                OldTimeBlock = oldState.CurrentTimeBlock,
                NewTimeBlock = _currentState.CurrentTimeBlock,
                CrossedTimeBlock = true
            };

            return result;
        }
    }

    /// <summary>
    /// Gets the hours until the next time block transition.
    /// </summary>
    public int HoursUntilNextTimeBlock()
    {
        int currentHour = CurrentHour;
        TimeBlocks currentBlock = CurrentTimeBlock;

        return currentBlock switch
        {
            TimeBlocks.Dawn => DAWN_END - currentHour,
            TimeBlocks.Morning => MORNING_END - currentHour,
            TimeBlocks.Afternoon => AFTERNOON_END - currentHour,
            TimeBlocks.Evening => EVENING_END - currentHour,
            TimeBlocks.Night => NIGHT_END - currentHour,
            TimeBlocks.LateNight => (LATE_NIGHT_END + 24 - currentHour) % 24,
            _ => 1
        };
    }

    /// <summary>
    /// Gets a human-readable time string.
    /// </summary>
    public string GetTimeString()
    {
        int hour = CurrentHour;
        string period = hour >= 12 ? "PM" : "AM";
        int displayHour = hour > 12 ? hour - 12 : (hour == 0 ? 12 : hour);
        return $"{displayHour}:00 {period}";
    }

    /// <summary>
    /// Gets a detailed time description.
    /// </summary>
    public string GetTimeDescription()
    {
        return $"Day {CurrentDay}, {GetTimeString()} ({CurrentTimeBlock})";
    }
}

/// <summary>
/// Result data for new day transitions.
/// </summary>
public class NewDayResult
{
    public int OldDay { get; init; }
    public int NewDay { get; init; }
    public int StartingHour { get; init; }
}
