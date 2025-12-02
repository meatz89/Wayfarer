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

/// <summary>
/// Segment counts per time block.
/// DOMAIN COLLECTION PRINCIPLE: Explicit properties for fixed enum (TimeBlocks).
/// All blocks have 4 segments in current design.
/// </summary>
public static class TimeBlockSegments
{
    public static int MorningSegments => 4;
    public static int MiddaySegments => 4;
    public static int AfternoonSegments => 4;
    public static int EveningSegments => 4;

    public const int TOTAL_SEGMENTS_PER_DAY = 16; // Morning(4) + Midday(4) + Afternoon(4) + Evening(4)

    public static int GetSegmentsForBlock(TimeBlocks block)
    {
        return block switch
        {
            TimeBlocks.Morning => MorningSegments,
            TimeBlocks.Midday => MiddaySegments,
            TimeBlocks.Afternoon => AfternoonSegments,
            TimeBlocks.Evening => EveningSegments,
            _ => throw new ArgumentOutOfRangeException(nameof(block), $"Unknown time block: {block}")
        };
    }
}
