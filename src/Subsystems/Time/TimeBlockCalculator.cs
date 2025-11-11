/// <summary>
/// Calculates time block transitions and availability windows.
/// </summary>
public class TimeBlockCalculator
{

    /// <summary>
    /// Calculate segments until a specific time block from current position.
    /// This is the primary method for time calculations.
    /// </summary>
    public int CalculateTimeUntilTimeBlock(TimeBlocks current, TimeBlocks target, int currentSegment)
    {
        return CalculateSegmentsUntilTimeBlock(current, target, currentSegment);
    }

    /// <summary>
    /// Get the next available time block from a list of available times.
    /// </summary>
    public TimeBlocks? GetNextAvailableTimeBlock(TimeBlocks current, List<TimeBlocks> availableTimes)
    {
        if (availableTimes.Count == 0) return null;

        TimeBlocks[] timeOrder = new[]
        {
        TimeBlocks.Morning,
        TimeBlocks.Midday,
        TimeBlocks.Afternoon,
        TimeBlocks.Evening
    };

        int currentIndex = Array.IndexOf(timeOrder, current);

        // Look for next available time after current
        for (int i = currentIndex + 1; i < timeOrder.Length; i++)
        {
            if (availableTimes.Contains(timeOrder[i]))
            {
                return timeOrder[i];
            }
        }

        // Wrap around to tomorrow
        for (int i = 0; i <= currentIndex; i++)
        {
            if (availableTimes.Contains(timeOrder[i]))
            {
                return timeOrder[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Get display name for a time block.
    /// </summary>
    public string GetTimeBlockDisplayName(TimeBlocks timeBlock)
    {
        return timeBlock switch
        {
            TimeBlocks.Morning => "Morning (6 AM-10 AM)",
            TimeBlocks.Midday => "Midday (10 AM-2 PM)",
            TimeBlocks.Afternoon => "Afternoon (2 PM-6 PM)",
            TimeBlocks.Evening => "Evening (6 PM-10 PM)",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get narrative description for waiting until a time block.
    /// </summary>
    public string GetWaitingNarrative(TimeBlocks targetTime)
    {
        return targetTime switch
        {
            TimeBlocks.Morning => "The morning sun climbs higher as time passes.",
            TimeBlocks.Midday => "The day wears on toward afternoon.",
            TimeBlocks.Afternoon => "Shadows lengthen as evening approaches.",
            TimeBlocks.Evening => "Darkness falls across the town.",
            _ => "Time passes..."
        };
    }

    // Segment-based methods for the new time system

    /// <summary>
    /// Get the starting segment for a time block within the day.
    /// </summary>
    public int GetTimeBlockStartSegment(TimeBlocks timeBlock)
    {
        return timeBlock switch
        {
            TimeBlocks.Morning => 1,
            TimeBlocks.Midday => 5,
            TimeBlocks.Afternoon => 9,
            TimeBlocks.Evening => 13,
            _ => 1
        };
    }

    /// <summary>
    /// Get the ending segment for a time block within the day.
    /// </summary>
    public int GetTimeBlockEndSegment(TimeBlocks timeBlock)
    {
        return timeBlock switch
        {
            TimeBlocks.Morning => 4,
            TimeBlocks.Midday => 8,
            TimeBlocks.Afternoon => 12,
            TimeBlocks.Evening => 16,
            _ => 16
        };
    }

    /// <summary>
    /// Calculate segments until a specific time block from current position.
    /// </summary>
    public int CalculateSegmentsUntilTimeBlock(TimeBlocks current, TimeBlocks target, int currentSegment)
    {
        int targetStartSegment = GetTimeBlockStartSegment(target);
        int currentBlockStart = GetTimeBlockStartSegment(current);

        // If target is later today
        if (targetStartSegment > currentSegment)
        {
            return targetStartSegment - currentSegment;
        }

        // Target is tomorrow - calculate remaining segments today + segments to target tomorrow
        int segmentsRemainingToday = 24 - currentSegment;
        return segmentsRemainingToday + targetStartSegment;
    }
}
