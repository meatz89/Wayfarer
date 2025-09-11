public class CurrentTimeBlocks
{
    public static CurrentTimeBlocks None = new CurrentTimeBlocks() { Values = new List<TimeBlocks>() };
    public static CurrentTimeBlocks All = new CurrentTimeBlocks() { Values = new List<TimeBlocks>() { TimeBlocks.Midday, TimeBlocks.Afternoon, TimeBlocks.Evening, TimeBlocks.Night } };
    public static CurrentTimeBlocks Morning = new CurrentTimeBlocks() { Values = new List<TimeBlocks>() { TimeBlocks.Midday } };
    public static CurrentTimeBlocks Afternoon = new CurrentTimeBlocks() { Values = new List<TimeBlocks>() { TimeBlocks.Afternoon } };
    public static CurrentTimeBlocks Evening = new CurrentTimeBlocks() { Values = new List<TimeBlocks>() { TimeBlocks.Evening } };

    public List<TimeBlocks> Values { get; set; } = new List<TimeBlocks>();

    public CurrentTimeBlocks()
    {
        Values = new List<TimeBlocks>();
    }

    public void Add(TimeBlocks window)
    {
        Values.Add(window);
    }

    public bool Contains(TimeBlocks window)
    {
        return Values.Contains(window);
    }
}
