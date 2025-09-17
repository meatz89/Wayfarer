using System.Collections.Generic;

public enum TimeBlocks
{
    Dawn,       // 6-10 AM: 3 segments
    Morning,    // 10 AM-2 PM: 4 segments
    Midday,  // 2-6 PM: 4 segments
    Afternoon,    // 6-10 PM: 4 segments
    Evening,      // 10 PM-12 AM: 1 segment
    Night   // 12 AM-6 AM: Sleep only
}

public static class TimeBlockSegments
{
    public static readonly Dictionary<TimeBlocks, int> SegmentsPerBlock = new()
    {
        { TimeBlocks.Dawn, 3 },
        { TimeBlocks.Morning, 4 },
        { TimeBlocks.Midday, 4 },
        { TimeBlocks.Afternoon, 4 },
        { TimeBlocks.Evening, 1 },
        { TimeBlocks.Night, 0 } // No playable segments during sleep time
    };

    public const int TOTAL_SEGMENTS_PER_DAY = 16; // Dawn(3) + Morning(4) + Afternoon(4) + Evening(4) + Night(1)

    public static int GetSegmentsForBlock(TimeBlocks block)
    {
        return SegmentsPerBlock.TryGetValue(block, out int segments) ? segments : 0;
    }
}
