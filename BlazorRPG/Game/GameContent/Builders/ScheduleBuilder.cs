

public class ScheduleBuilder
{
    private TimeSlots timeWindow;
    private LocationSpotNames locationSpotType;
    private ActionTypes basicActionTypes;

    public ScheduleBuilder AtTime(TimeSlots timeWindow)
    {
        this.timeWindow = timeWindow;
        return this;
    }

    public ScheduleBuilder AtSpot(LocationSpotNames locationSpotType)
    {
        this.locationSpotType = locationSpotType;
        return this;
    }

    public ScheduleBuilder WithAction(ActionTypes basicActionTypes)
    {
        this.basicActionTypes = basicActionTypes;
        return this;
    }

    public Schedule Build()
    {
        return new Schedule();
    }

}