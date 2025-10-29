/// <summary>
/// Time model that serves as the single source of truth for all time-related state.
/// Builds on TimeState to provide a complete segment-based time management system.
/// </summary>
public class TimeModel
{
    // Segment constants - single source of truth
    public const int TOTAL_SEGMENTS_PER_DAY = TimeBlockSegments.TOTAL_SEGMENTS_PER_DAY;

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

    public int CurrentDay => CurrentState.CurrentDay;

    public int CurrentSegment => CurrentState.CurrentSegment;

    public int SegmentInCurrentBlock => CurrentState.SegmentInCurrentBlock;

    public int SegmentsInCurrentBlock => CurrentState.SegmentsInCurrentBlock;

    public int SegmentsRemainingInBlock => CurrentState.SegmentsRemainingInBlock;

    public int SegmentsRemainingInDay => CurrentState.SegmentsRemainingInDay;

    public int ActiveSegmentsRemaining => CurrentState.ActiveSegmentsRemaining;

    public TimeBlocks CurrentTimeBlock => CurrentState.CurrentTimeBlock;

    public bool IsActiveTime => CurrentState.IsActiveTime;

    // Events removed per architecture guidelines - use return values instead

    public TimeModel(int startDay = 1)
    {
        _currentState = new TimeState(startDay);
    }

    /// <summary>
    /// Sets initial time state from package starting conditions
    /// MUST be called before any time advancement
    /// </summary>
    public void SetInitialState(int day, TimeBlocks timeBlock, int segment)
    {
        lock (_lock)
        {
            _currentState = new TimeState(day, timeBlock, segment);
        }
    }

    /// <summary>
    /// Validates that a segment transition is valid before applying it.
    /// </summary>
    public ValidationResult ValidateSegmentTransition(int segments)
    {
        if (segments <= 0)
            return ValidationResult.Failure("Segments must be positive");

        if (!IsActiveTime && segments > 0)
            return ValidationResult.Warning("Currently outside active time");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Advances time by segments atomically with full validation.
    /// </summary>
    public TimeAdvancementResult AdvanceSegments(int segments)
    {
        if (segments <= 0)
            throw new ArgumentException("Segments must be positive", nameof(segments));

        lock (_lock)
        {
            TimeAdvancementResult result = _currentState.AdvanceSegments(segments);
            _currentState = result.NewState;

            return result;
        }
    }

    /// <summary>
    /// Checks if the specified action can be performed within available segments.
    /// </summary>
    public bool CanPerformAction(int segmentsRequired)
    {
        return CurrentState.CanSpendSegments(segmentsRequired);
    }

    /// <summary>
    /// Advances to the next day starting at Dawn.
    /// Returns the time advancement result for the caller to handle.
    /// </summary>
    public TimeAdvancementResult AdvanceToNextDay()
    {
        lock (_lock)
        {
            TimeState oldState = _currentState;
            _currentState = _currentState.Sleep(); // Sleep automatically goes to next day at Dawn

            TimeAdvancementResult result = new TimeAdvancementResult
            {
                OldState = oldState,
                NewState = _currentState,
                SegmentsAdvanced = 0, // Sleep jump doesn't count as segment advancement
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
    /// Jumps to the next time period (like work or rest actions).
    /// Advances to the first segment of the next time block.
    /// </summary>
    public TimeAdvancementResult JumpToNextPeriod()
    {
        lock (_lock)
        {
            // Calculate segments needed to reach the next time block
            int segmentsToNextPeriod = SegmentsRemainingInBlock;
            if (segmentsToNextPeriod == 0)
                segmentsToNextPeriod = 1; // Already at end, move to next block

            return AdvanceSegments(segmentsToNextPeriod);
        }
    }

    /// <summary>
    /// Gets the segments until the next time block transition.
    /// </summary>
    public int SegmentsUntilNextTimeBlock()
    {
        return SegmentsRemainingInBlock;
    }

    /// <summary>
    /// Gets a human-readable segment display string.
    /// Format: "AFTERNOON ●●○○ [2/4]"
    /// </summary>
    public string GetSegmentDisplay()
    {
        return CurrentState.GetSegmentDisplay();
    }

    /// <summary>
    /// Gets a detailed time description using segments.
    /// </summary>
    public string GetTimeDescription()
    {
        return $"Day {CurrentDay}, {GetSegmentDisplay()}";
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
