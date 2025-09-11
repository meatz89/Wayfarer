public class TimeInfo
{
    public TimeBlocks TimeBlock { get; init; }
    public TimeBlocks CurrentTimeBlock { get; init; }
    public int SegmentsRemaining { get; init; }
    public int CurrentDay { get; init; }
    public string SegmentDisplay { get; init; }

    public TimeInfo(TimeBlocks timeBlock, int segmentsRemaining, int currentDay, string segmentDisplay)
    {
        TimeBlock = timeBlock;
        CurrentTimeBlock = timeBlock;
        SegmentsRemaining = segmentsRemaining;
        CurrentDay = currentDay;
        SegmentDisplay = segmentDisplay;
    }

}