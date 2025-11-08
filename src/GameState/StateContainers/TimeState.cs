/// <summary>
/// Immutable state container for time tracking using segments instead of minutes/hours.
/// Provides tactical decision-making with clear opportunity costs through segment-based time management.
/// </summary>
public sealed class TimeState
{
    // Private fields for encapsulation
    private readonly int _currentDay;
    private readonly TimeBlocks _currentTimeBlock;
    private readonly int _currentSegment;
    private readonly int _totalSegmentsElapsed;

    // Public properties
    public int CurrentDay => _currentDay;
    public TimeBlocks CurrentTimeBlock => _currentTimeBlock;
    public int CurrentSegment => _currentSegment;
    public int TotalSegmentsElapsed => _totalSegmentsElapsed;

    /// <summary>
    /// Current segment within the current time block (1-based index).
    /// </summary>
    public int SegmentInCurrentBlock => _currentSegment;

    /// <summary>
    /// Total segments available in the current time block.
    /// </summary>
    public int SegmentsInCurrentBlock => TimeBlockSegments.GetSegmentsForBlock(_currentTimeBlock);

    /// <summary>
    /// Segments remaining in the current time block.
    /// </summary>
    public int SegmentsRemainingInBlock => Math.Max(0, SegmentsInCurrentBlock - _currentSegment);

    /// <summary>
    /// Total segments remaining in the current day (including current block).
    /// </summary>
    public int SegmentsRemainingInDay
    {
        get
        {
            int remaining = SegmentsRemainingInBlock;

            // Add segments from future blocks today
            int currentBlockIndex = (int)_currentTimeBlock;
            for (int i = currentBlockIndex + 1; i < 4; i++) // 4 blocks (Morning through Evening)
            {
                TimeBlocks futureBlock = (TimeBlocks)i;
                remaining += TimeBlockSegments.GetSegmentsForBlock(futureBlock);
            }

            return remaining;
        }
    }

    /// <summary>
    /// Active segments remaining in the current day
    /// </summary>
    public int ActiveSegmentsRemaining => IsActiveTime ? SegmentsRemainingInDay : 0;

    /// <summary>
    /// True if the current time allows active gameplay (always true in 4-block system)
    /// </summary>
    public bool IsActiveTime => true;

    /// <summary>
    /// True if this is the final segment of the current block.
    /// </summary>
    public bool IsLastSegmentInBlock => _currentSegment >= SegmentsInCurrentBlock;

    /// <summary>
    /// True if this is the final segment of the day 
    /// </summary>
    public bool IsLastSegmentInDay => _currentTimeBlock == TimeBlocks.Evening && IsLastSegmentInBlock;

    /// <summary>
    /// Creates a new TimeState starting at the beginning of a day.
    /// </summary>
    public TimeState(int day = 1) : this(day, TimeBlocks.Morning, 1, CalculateTotalSegments(day, TimeBlocks.Morning, 1))
    {
    }

    /// <summary>
    /// Creates a new TimeState with specific time block and segment.
    /// </summary>
    /// <param name="day">Day number (1-based)</param>
    /// <param name="timeBlock">Time block (Morning/Midday/Afternoon/Evening)</param>
    /// <param name="segment">Segment WITHIN the time block (1-4, relative to block start). NOT absolute segment of day.</param>
    /// <remarks>
    /// CRITICAL: Segment is RELATIVE to the time block, not absolute position in day.
    /// Example: Evening segment 1 = first segment of Evening (13th segment of full day)
    /// Example: Midday segment 3 = third segment of Midday (7th segment of full day)
    /// </remarks>
    public TimeState(int day, TimeBlocks timeBlock, int segment) : this(day, timeBlock, segment, CalculateTotalSegments(day, timeBlock, segment))
    {
    }

    /// <summary>
    /// Private constructor for complete state specification.
    /// </summary>
    private TimeState(int day, TimeBlocks timeBlock, int segment, int totalSegmentsElapsed)
    {
        if (day < 1)
            throw new ArgumentException("Day must be at least 1", nameof(day));

        // No time block restrictions in 4-block system (Morning, Midday, Afternoon, Evening)

        int maxSegments = TimeBlockSegments.GetSegmentsForBlock(timeBlock);
        if (segment < 1 || segment > maxSegments)
            throw new ArgumentException($"Segment must be between 1 and {maxSegments} for {timeBlock}", nameof(segment));

        _currentDay = day;
        _currentTimeBlock = timeBlock;
        _currentSegment = segment;
        _totalSegmentsElapsed = totalSegmentsElapsed;
    }

    /// <summary>
    /// Advances time by the specified number of segments.
    /// Handles time block and day transitions automatically.
    /// </summary>
    public TimeAdvancementResult AdvanceSegments(int segments)
    {
        if (segments <= 0)
            throw new ArgumentException("Segments to advance must be positive", nameof(segments));

        TimeState oldState = this;
        TimeBlocks oldTimeBlock = _currentTimeBlock;

        int remainingSegments = segments;
        int currentDay = _currentDay;
        TimeBlocks currentTimeBlock = _currentTimeBlock;
        int currentSegment = _currentSegment;
        int totalSegments = _totalSegmentsElapsed;

        while (remainingSegments > 0)
        {
            int segmentsInCurrentBlock = TimeBlockSegments.GetSegmentsForBlock(currentTimeBlock);
            int segmentsRemainingInCurrentBlock = segmentsInCurrentBlock - currentSegment + 1;

            if (remainingSegments < segmentsRemainingInCurrentBlock)
            {
                // Advancement stays within current block
                currentSegment += remainingSegments;
                totalSegments += remainingSegments;
                remainingSegments = 0;
            }
            else
            {
                // Need to advance to next block
                remainingSegments -= segmentsRemainingInCurrentBlock;
                totalSegments += segmentsRemainingInCurrentBlock;

                // Move to next time block
                if (currentTimeBlock == TimeBlocks.Evening)
                {
                    // Evening → sleep → Morning (next day)
                    currentDay++;
                    currentTimeBlock = TimeBlocks.Morning;
                    currentSegment = 1;
                }
                else
                {
                    currentTimeBlock = GetNextTimeBlock(currentTimeBlock);
                    currentSegment = 1;
                }
            }
        }

        TimeState newState = new TimeState(currentDay, currentTimeBlock, currentSegment, totalSegments);

        return new TimeAdvancementResult
        {
            OldState = oldState,
            NewState = newState,
            SegmentsAdvanced = segments,
            DaysAdvanced = newState.CurrentDay - oldState.CurrentDay,
            CrossedDayBoundary = newState.CurrentDay > oldState.CurrentDay,
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newState.CurrentTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newState.CurrentTimeBlock
        };
    }

    /// <summary>
    /// Handles sleep transition from any time to Morning of the next day.
    /// Used when player sleeps or when Evening ends.
    /// </summary>
    public TimeState Sleep()
    {
        int nextDay = _currentTimeBlock == TimeBlocks.Evening && IsLastSegmentInBlock ? _currentDay + 1 : _currentDay + 1;
        return new TimeState(nextDay, TimeBlocks.Morning, 1, CalculateTotalSegments(nextDay, TimeBlocks.Morning, 1));
    }

    /// <summary>
    /// Checks if the specified number of segments can be spent within the current day.
    /// </summary>
    public bool CanSpendSegments(int segments)
    {
        if (segments <= 0) return false;
        return segments <= SegmentsRemainingInDay;
    }

    /// <summary>
    /// Checks if the specified number of segments can be spent within the current time block.
    /// </summary>
    public bool CanSpendSegmentsInCurrentBlock(int segments)
    {
        if (segments <= 0) return false;
        return segments <= SegmentsRemainingInBlock;
    }

    /// <summary>
    /// Gets a human-readable segment display string.
    /// Format: "AFTERNOON ●●○○ [2/4]"
    /// </summary>
    public string GetSegmentDisplay()
    {
        int totalSegments = SegmentsInCurrentBlock;
        string dots = "";

        for (int i = 1; i <= totalSegments; i++)
        {
            dots += i <= _currentSegment ? "●" : "○";
        }

        return $"{_currentTimeBlock.ToString().ToUpper()} {dots} [{_currentSegment}/{totalSegments}]";
    }

    /// <summary>
    /// Gets a compact segment display for UI.
    /// Format: "●●○○"
    /// </summary>
    public string GetCompactSegmentDisplay()
    {
        int totalSegments = SegmentsInCurrentBlock;
        string dots = "";

        for (int i = 1; i <= totalSegments; i++)
        {
            dots += i <= _currentSegment ? "●" : "○";
        }

        return dots;
    }

    /// <summary>
    /// Gets the next time block in sequence.
    /// </summary>
    private static TimeBlocks GetNextTimeBlock(TimeBlocks currentBlock)
    {
        return currentBlock switch
        {
            TimeBlocks.Morning => TimeBlocks.Midday,
            TimeBlocks.Midday => TimeBlocks.Afternoon,
            TimeBlocks.Afternoon => TimeBlocks.Evening,
            TimeBlocks.Evening => TimeBlocks.Morning, // Wraps to next day
            _ => throw new ArgumentException($"Unknown time block: {currentBlock}")
        };
    }

    /// <summary>
    /// Calculates total segments elapsed for a given day, block, and segment.
    /// </summary>
    private static int CalculateTotalSegments(int day, TimeBlocks timeBlock, int segment)
    {
        int total = (day - 1) * TimeBlockSegments.TOTAL_SEGMENTS_PER_DAY;

        // Add segments from completed blocks in current day
        for (TimeBlocks block = TimeBlocks.Morning; block < timeBlock; block++)
        {
            total += TimeBlockSegments.GetSegmentsForBlock(block);
        }

        // Add current segment within current block
        total += segment;

        return total;
    }

    /// <summary>
    /// Creates a copy of this TimeState.
    /// </summary>
    public TimeState Clone()
    {
        return new TimeState(_currentDay, _currentTimeBlock, _currentSegment, _totalSegmentsElapsed);
    }

    public override string ToString()
    {
        return $"Day {_currentDay}, {GetSegmentDisplay()}";
    }

    public override bool Equals(object obj)
    {
        if (obj is not TimeState other) return false;
        return _currentDay == other._currentDay &&
               _currentTimeBlock == other._currentTimeBlock &&
               _currentSegment == other._currentSegment;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_currentDay, _currentTimeBlock, _currentSegment);
    }

    /// <summary>
    /// Gets the total segments in the current time block.
    /// Method form for compatibility with callers expecting a method.
    /// </summary>
    public int GetSegmentsInCurrentBlock()
    {
        return SegmentsInCurrentBlock;
    }
}

/// <summary>
/// Result of a time advancement operation using segments.
/// </summary>
public class TimeAdvancementResult
{
    public TimeState OldState { get; init; }
    public TimeState NewState { get; init; }
    public int SegmentsAdvanced { get; init; }
    public int DaysAdvanced { get; init; }
    public bool CrossedDayBoundary { get; init; }
    public TimeBlocks OldTimeBlock { get; init; }
    public TimeBlocks NewTimeBlock { get; init; }
    public bool CrossedTimeBlock { get; init; }

    public bool IsSuccess => NewState != null;
}