public class TimeWindows
{
    public static TimeWindows None = new TimeWindows() { Values = new List<TimeWindowTypes>() };
    public static TimeWindows All = new TimeWindows() { Values = new List<TimeWindowTypes>() { TimeWindowTypes.Morning, TimeWindowTypes.Afternoon, TimeWindowTypes.Evening, TimeWindowTypes.Night } };
    public static TimeWindows Morning = new TimeWindows() { Values = new List<TimeWindowTypes>() { TimeWindowTypes.Morning } };
    public static TimeWindows Afternoon = new TimeWindows() { Values = new List<TimeWindowTypes>() { TimeWindowTypes.Afternoon } };
    public static TimeWindows Evening = new TimeWindows() { Values = new List<TimeWindowTypes>() { TimeWindowTypes.Evening } };

    public List<TimeWindowTypes> Values { get; set; } = new List<TimeWindowTypes>();

    public TimeWindows()
    {
        Values = new List<TimeWindowTypes>();
    }

    public void Add(TimeWindowTypes window)
    {
        Values.Add(window);
    }

    public bool Contains(TimeWindowTypes window)
    {
        return Values.Contains(window);
    }
}

public enum TimeWindowTypes
{
    Night,
    Morning,
    Afternoon,
    Evening,
    None
}
