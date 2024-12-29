public class LocationPropertiesBuilder
{
    private LocationNames location;
    private LocationTypes locationType;
    private ActivityTypes activityType;
    private List<TimeSlots> timeSlots = new();
    private BasicActionTypes primaryAction;

    internal LocationPropertiesBuilder ForLocation(LocationNames location)
    {
        this.location = location;
        return this;
    }

    internal LocationPropertiesBuilder SetLocationType(LocationTypes locationType)
    {
        this.locationType = locationType;
        return this;
    }

    internal LocationPropertiesBuilder SetActivityType(ActivityTypes mingle)
    {
        this.activityType = activityType;
        return this;
    }

    internal LocationPropertiesBuilder AddTimeSlot(TimeSlots timeSlot)
    {
        this.timeSlots.Add(timeSlot);
        return this;
    }

    internal LocationPropertiesBuilder SetAccessType(AccessTypes open)
    {
        return this;
    }

    internal LocationPropertiesBuilder SetShelterStatus(ShelterStates none)
    {
        return this;
    }

    internal LocationPropertiesBuilder SetDangerLevel(DangerLevels safe)
    {
        return this;
    }

    public LocationPropertiesBuilder AddPrimaryAction(BasicActionTypes basicActionType)
    {
        primaryAction = basicActionType;
        return this;
    }

    public LocationProperties Build()
    {
        return new LocationProperties
        {
            Location = location,
            LocationType = locationType,
            ActivityType = activityType,
            TimeSlots = timeSlots,
            PrimaryAction = primaryAction
        };
    }
}