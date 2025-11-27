/// <summary>
/// Time blocks for the day. Each block contains 1-4 segments.
/// CRITICAL: Segments are RELATIVE to each block (1-4), NOT absolute day positions.
/// </summary>
public enum TimeBlocks
{
    Morning,    // 4 segments (1-4 within Morning) = day segments 1-4: 6-10 AM
    Midday,     // 4 segments (1-4 within Midday) = day segments 5-8: 10 AM - 2 PM
    Afternoon,  // 4 segments (1-4 within Afternoon) = day segments 9-12: 2-6 PM
    Evening     // 4 segments (1-4 within Evening) = day segments 13-16: 6-10 PM
                // Day ends at segment 16 (Evening segment 4), sleep/recovery happens automatically
}

/// <summary>
/// Day advancement options for Consequence time progression
/// Used by single-choice instant situations to control day transitions
/// </summary>
public enum DayAdvancement
{
    CurrentDay,  // Stay on current day
    NextDay      // Advance to next day
}

public static class TimeBlockSegments
{
    public static readonly Dictionary<TimeBlocks, int> SegmentsPerBlock = new()
{
    { TimeBlocks.Morning, 4 },
    { TimeBlocks.Midday, 4 },
    { TimeBlocks.Afternoon, 4 },
    { TimeBlocks.Evening, 4 }
};

    public const int TOTAL_SEGMENTS_PER_DAY = 16; // Morning(4) + Midday(4) + Afternoon(4) + Evening(4)

    public static int GetSegmentsForBlock(TimeBlocks block)
    {
        return SegmentsPerBlock.TryGetValue(block, out int segments) ? segments : 0;
    }
}
