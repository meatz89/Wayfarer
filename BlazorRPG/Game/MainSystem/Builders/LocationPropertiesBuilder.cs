public class LocationPropertiesBuilder
{
    private LocationSpotProperties properties;

    public LocationPropertiesBuilder(LocationSpotProperties existingProperties = null)
    {
        this.properties = existingProperties ?? new LocationSpotProperties();
    }

    public LocationPropertiesBuilder WithAnyResource()
    {
        return this;
    }

    public LocationPropertiesBuilder WithAccessability(Accessability accessability)
    {
        properties.SetProperty(LocationPropertyTypes.Accessibility, accessability);
        return this;
    }

    public LocationPropertiesBuilder WithEngagement(Engagement engagement)
    {
        properties.SetProperty(LocationPropertyTypes.Engagement, engagement);
        return this;
    }

    public LocationPropertiesBuilder WithAtmosphere(Atmosphere socialDynamics)
    {
        properties.SetProperty(LocationPropertyTypes.Atmosphere, socialDynamics);
        return this;
    }

    public LocationPropertiesBuilder WithRoomLayout(RoomLayout roomLayout)
    {
        properties.SetProperty(LocationPropertyTypes.RoomLayout, roomLayout);
        return this;
    }

    public LocationPropertiesBuilder WithTemperature(Temperature temperature)
    {
        properties.SetProperty(LocationPropertyTypes.Temperature, temperature);
        return this;
    }

    public LocationSpotProperties Build()
    {
        return properties;
    }

}