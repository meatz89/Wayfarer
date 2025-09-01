public class TimeInfo
{
    public TimeBlocks TimeBlock { get; init; }
    public int HoursRemaining { get; init; }
    public int CurrentDay { get; init; }
    
    public TimeInfo(TimeBlocks timeBlock, int hoursRemaining, int currentDay)
    {
        TimeBlock = timeBlock;
        HoursRemaining = hoursRemaining;
        CurrentDay = currentDay;
    }
}