public class TimeWindows
{
    public static TimeWindows None = new TimeWindows() { Values = new List<TimeBlocks>() };
    public static TimeWindows All = new TimeWindows() { Values = new List<TimeBlocks>() { TimeBlocks.Morning, TimeBlocks.Afternoon, TimeBlocks.Evening, TimeBlocks.Night } };
    public static TimeWindows Morning = new TimeWindows() { Values = new List<TimeBlocks>() { TimeBlocks.Morning } };
    public static TimeWindows Afternoon = new TimeWindows() { Values = new List<TimeBlocks>() { TimeBlocks.Afternoon } };
    public static TimeWindows Evening = new TimeWindows() { Values = new List<TimeBlocks>() { TimeBlocks.Evening } };

    public List<TimeBlocks> Values { get; set; } = new List<TimeBlocks>();

    public TimeWindows()
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
