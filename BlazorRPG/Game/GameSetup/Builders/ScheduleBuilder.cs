

public class ScheduleBuilder
{
    private TimeWindows timeWindow;
    private LocationSpotNames locationSpotType;
    private BasicActionTypes basicActionTypes;

    public ScheduleBuilder AtTime(TimeWindows timeWindow)
    {
        this.timeWindow = timeWindow;
        return this;
    }

    public ScheduleBuilder AtSpot(LocationSpotNames locationSpotType)
    {
        this.locationSpotType = locationSpotType;
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