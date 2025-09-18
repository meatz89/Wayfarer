using System.Collections.Generic;

public enum TimeBlocks
{
    Dawn,       // 2-6 AM: 4 segments
    Morning,    // 6-10 AM: 4 segments
    Midday,     // 10 AM - 2 PM: 4 segments
    Afternoon,  // 2-6 PM: 4 segments
    Evening,    // 6-10 PM: 4 segments
    Night       // 10 PM - 2 AM: 4 segments
}

public static class TimeBlockSegments
{
    public static readonly Dictionary<TimeBlocks, int> SegmentsPerBlock = new()
    {
        { TimeBlocks.Dawn, 4 },
        { TimeBlocks.Morning, 4 },
        { TimeBlocks.Midday, 4 },
        { TimeBlocks.Afternoon, 4 },
        { TimeBlocks.Evening, 4 },
        { TimeBlocks.Night, 4 }
    };

    public const int TOTAL_SEGMENTS_PER_DAY = 24; // Dawn(4) + Morning(4) + Midday(4) + Afternoon(4) + Evening(4) + Night(4)

    public static int GetSegmentsForBlock(TimeBlocks block)
    {
        return SegmentsPerBlock.TryGetValue(block, out int segments) ? segments : 0;
    }
}
