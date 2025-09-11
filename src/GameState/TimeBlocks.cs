using System.Collections.Generic;

public enum TimeBlocks
{
    Dawn,       // 6-10 AM: 3 segments
    Midday,     // 10 AM-2 PM: 4 segments (renamed from Morning for clarity)
    Afternoon,  // 2-6 PM: 4 segments
    Evening,    // 6-10 PM: 4 segments
    Night,      // 10 PM-12 AM: 1 segment
    DeepNight   // 12 AM-6 AM: Sleep only (renamed from LateNight)
}

public static class TimeBlockSegments
{
    public static readonly Dictionary<TimeBlocks, int> SegmentsPerBlock = new()
    {
        { TimeBlocks.Dawn, 3 },
        { TimeBlocks.Midday, 4 },
        { TimeBlocks.Afternoon, 4 },
        { TimeBlocks.Evening, 4 },
        { TimeBlocks.Night, 1 },
        { TimeBlocks.DeepNight, 0 } // No playable segments during sleep time
    };
    
    public const int TOTAL_SEGMENTS_PER_DAY = 16; // Dawn(3) + Midday(4) + Afternoon(4) + Evening(4) + Night(1)
    
    public static int GetSegmentsForBlock(TimeBlocks block)
    {
        return SegmentsPerBlock.TryGetValue(block, out var segments) ? segments : 0;
    }
}
