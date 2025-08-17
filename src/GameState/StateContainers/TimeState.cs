using System;


/// <summary>
/// Immutable state container for time tracking with validation and atomic operations.
/// Replaces distributed time tracking across GameWorld, WorldState, and TimeManager.
/// </summary>
public sealed class TimeState
{
    // Constants
    public const int HOURS_PER_DAY = 24;
    public const int ACTIVE_DAY_START = 6;
    public const int ACTIVE_DAY_END = 22;
    public const int ACTIVE_HOURS_PER_DAY = ACTIVE_DAY_END - ACTIVE_DAY_START;

    // Private fields for encapsulation
    private readonly int _currentHour;
    private readonly int _currentMinute;
    private readonly int _currentDay;

    // Public properties with validation
    public int CurrentHour => _currentHour;

    public int CurrentMinute => _currentMinute;

    public int CurrentDay => _currentDay;

    public TimeBlocks CurrentTimeBlock => CalculateTimeBlock(_currentHour);

    public int ActiveHoursRemaining => Math.Max(0, ACTIVE_DAY_END - _currentHour);

    public bool IsActiveTime => _currentHour >= ACTIVE_DAY_START && _currentHour < ACTIVE_DAY_END;

    /// <summary>
    /// Creates a new TimeState with the specified day, hour, and minute.
    /// </summary>
    public TimeState(int day = 1, int hour = ACTIVE_DAY_START, int minute = 0)
    {
        if (day < 1)
            throw new ArgumentException("Day must be at least 1", nameof(day));

        if (hour < 0 || hour >= HOURS_PER_DAY)
            throw new ArgumentException($"Hour must be between 0 and {HOURS_PER_DAY - 1}", nameof(hour));

        if (minute < 0 || minute >= 60)
            throw new ArgumentException("Minute must be between 0 and 59", nameof(minute));

        _currentDay = day;
        _currentHour = hour;
        _currentMinute = minute;
    }

    /// <summary>
    /// Creates a new TimeState by advancing time atomically.
    /// Returns a new instance with the updated time.
    /// </summary>
    public TimeAdvancementResult AdvanceTime(int hours)
    {
        if (hours <= 0)
            throw new ArgumentException("Hours to advance must be positive", nameof(hours));

        TimeBlocks oldTimeBlock = CurrentTimeBlock;
        int totalHours = _currentHour + hours;
        int daysAdvanced = totalHours / HOURS_PER_DAY;
        int newHour = totalHours % HOURS_PER_DAY;
        int newDay = _currentDay + daysAdvanced;

        TimeState newState = new TimeState(newDay, newHour, _currentMinute);

        return new TimeAdvancementResult
        {
            OldState = this,
            NewState = newState,
            HoursAdvanced = hours,
            DaysAdvanced = daysAdvanced,
            CrossedDayBoundary = daysAdvanced > 0,
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newState.CurrentTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newState.CurrentTimeBlock
        };
    }

    /// <summary>
    /// Creates a new TimeState by advancing time by minutes.
    /// Properly handles hour and day rollovers.
    /// </summary>
    public TimeAdvancementResult AdvanceTimeMinutes(int minutes)
    {
        if (minutes <= 0)
            throw new ArgumentException("Minutes to advance must be positive", nameof(minutes));

        TimeBlocks oldTimeBlock = CurrentTimeBlock;

        // Calculate new time
        int totalMinutes = _currentMinute + minutes;
        int additionalHours = totalMinutes / 60;
        int newMinute = totalMinutes % 60;

        int totalHours = _currentHour + additionalHours;
        int daysAdvanced = totalHours / HOURS_PER_DAY;
        int newHour = totalHours % HOURS_PER_DAY;
        int newDay = _currentDay + daysAdvanced;

        TimeState newState = new TimeState(newDay, newHour, newMinute);

        return new TimeAdvancementResult
        {
            OldState = this,
            NewState = newState,
            HoursAdvanced = additionalHours,
            MinutesAdvanced = minutes,
            DaysAdvanced = daysAdvanced,
            CrossedDayBoundary = daysAdvanced > 0,
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newState.CurrentTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newState.CurrentTimeBlock
        };
    }

    /// <summary>
    /// Checks if the specified number of hours can be spent without exceeding active time.
    /// </summary>
    public bool CanSpendActiveHours(int hours)
    {
        if (hours <= 0) return false;
        return _currentHour + hours <= ACTIVE_DAY_END;
    }

    /// <summary>
    /// Creates a new TimeState at the start of the next day.
    /// </summary>
    public TimeState AdvanceToNextDay()
    {
        return new TimeState(_currentDay + 1, ACTIVE_DAY_START, 0);
    }

    /// <summary>
    /// Gets a human-readable time string.
    /// </summary>
    public string GetTimeString()
    {
        string period = _currentHour >= 12 ? "PM" : "AM";
        int displayHour = _currentHour > 12 ? _currentHour - 12 : (_currentHour == 0 ? 12 : _currentHour);
        return $"{displayHour}:{_currentMinute:D2} {period}";
    }

    /// <summary>
    /// Calculates the time block for a given hour.
    /// </summary>
    private static TimeBlocks CalculateTimeBlock(int hour)
    {
        return hour switch
        {
            >= 6 and < 8 => TimeBlocks.Dawn,        // 6:00-7:59 (2 hours)
            >= 8 and < 12 => TimeBlocks.Morning,    // 8:00-11:59 (4 hours)
            >= 12 and < 16 => TimeBlocks.Afternoon, // 12:00-15:59 (4 hours)
            >= 16 and < 20 => TimeBlocks.Evening,   // 16:00-19:59 (4 hours)
            >= 20 and < 22 => TimeBlocks.Night,     // 20:00-21:59 (2 hours)
            _ => TimeBlocks.LateNight               // 22:00-5:59 (8 hours)
        };
    }

    /// <summary>
    /// Creates a copy of this TimeState (for serialization/deserialization scenarios).
    /// </summary>
    public TimeState Clone()
    {
        return new TimeState(_currentDay, _currentHour, _currentMinute);
    }

    public override string ToString()
    {
        return $"Day {_currentDay}, {_currentHour:00}:{_currentMinute:00} ({CurrentTimeBlock})";
    }

    public override bool Equals(object obj)
    {
        if (obj is not TimeState other) return false;
        return _currentDay == other._currentDay && _currentHour == other._currentHour && _currentMinute == other._currentMinute;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_currentDay, _currentHour, _currentMinute);
    }
}

/// <summary>
/// Result of a time advancement operation.
/// </summary>
public class TimeAdvancementResult
{
    public TimeState OldState { get; init; }
    public TimeState NewState { get; init; }
    public int HoursAdvanced { get; init; }
    public int MinutesAdvanced { get; init; }
    public int DaysAdvanced { get; init; }
    public bool CrossedDayBoundary { get; init; }
    public TimeBlocks OldTimeBlock { get; init; }
    public TimeBlocks NewTimeBlock { get; init; }
    public bool CrossedTimeBlock { get; init; }

    public bool IsSuccess => NewState != null;
}