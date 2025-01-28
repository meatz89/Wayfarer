public class LocationSpotConditionBuilder
{
    private LocationSpotAvailabilityConditions properties;

    public LocationSpotConditionBuilder(LocationSpotAvailabilityConditions existingProperties = null)
    {
        this.properties = existingProperties ?? new LocationSpotAvailabilityConditions();
    }

    public LocationSpotConditionBuilder WithAnyResource()
    {
        return this;
    }

    public LocationSpotConditionBuilder WithAccessibility(Accessibility accessability)
    {
        properties.SetProperty(LocationSpotPropertyTypes.Accessibility, accessability);
        return this;
    }

    public LocationSpotConditionBuilder WithEngagement(Engagement engagement)
    {
        properties.SetProperty(LocationSpotPropertyTypes.Engagement, engagement);
        return this;
    }

    public LocationSpotConditionBuilder WithAtmosphere(Atmosphere socialDynamics)
    {
        properties.SetProperty(LocationSpotPropertyTypes.Atmosphere, socialDynamics);
        return this;
    }

    public LocationSpotConditionBuilder WithRoomLayout(RoomLayout roomLayout)
    {
        properties.SetProperty(LocationSpotPropertyTypes.RoomLayout, roomLayout);
        return this;
    }

    public LocationSpotConditionBuilder WithTemperature(Temperature temperature)
    {
        properties.SetProperty(LocationSpotPropertyTypes.Temperature, temperature);
        return this;
    }

    public LocationSpotAvailabilityConditions Build()
    {
        return properties;
    }

}