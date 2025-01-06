
public class ScheduleBuilder
{
    private TimeSlots timeWindow;
    private LocationNames locationName;
    private BasicActionTypes basicActionTypes;

    public ScheduleBuilder AtTime(TimeSlots timeWindow)
    {
        this.timeWindow = timeWindow;
        return this;
    }

    public ScheduleBuilder AtLocation(LocationNames location)
    {
        this.locationName = location;
        return this;
    }

    public ScheduleBuilder WithAction(BasicActionTypes basicActionTypes)
    {
        this.basicActionTypes = basicActionTypes;
        return this;
    }

    public Schedule Build()
    {
        return new Schedule();
    }
}