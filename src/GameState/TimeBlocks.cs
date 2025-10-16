using System.Collections.Generic;

public enum TimeBlocks
{
    Morning,    // Segments 1-4: 6-10 AM
    Midday,     // Segments 5-8: 10 AM - 2 PM
    Afternoon,  // Segments 9-12: 2-6 PM
    Evening     // Segments 13-16: 6-10 PM
    // Day ends at segment 16, sleep/recovery happens automatically
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
