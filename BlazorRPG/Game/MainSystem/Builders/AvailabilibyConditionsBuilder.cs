public class AvailabilibyConditionsBuilder
{
    private LocationSpotProperties properties;

    public AvailabilibyConditionsBuilder(LocationSpotProperties existingProperties = null)
    {
        this.properties = existingProperties ?? new LocationSpotProperties();
    }

    public AvailabilibyConditionsBuilder WithAnyResource()
    {
        return this;
    }

    public AvailabilibyConditionsBuilder WithAccessibility(Accessibility accessability)
    {
        properties.SetProperty(LocationSpotPropertyTypes.Accessibility, accessability);
        return this;
    }

    public AvailabilibyConditionsBuilder WithEngagement(Engagement engagement)
    {
        properties.SetProperty(LocationSpotPropertyTypes.Engagement, engagement);
        return this;
    }

    public AvailabilibyConditionsBuilder WithAtmosphere(Atmosphere socialDynamics)
    {
        properties.SetProperty(LocationSpotPropertyTypes.Atmosphere, socialDynamics);
        return this;
    }

    public AvailabilibyConditionsBuilder WithRoomLayout(RoomLayout roomLayout)
    {
        properties.SetProperty(LocationSpotPropertyTypes.RoomLayout, roomLayout);
        return this;
    }

    public AvailabilibyConditionsBuilder WithTemperature(Temperature temperature)
    {
        properties.SetProperty(LocationSpotPropertyTypes.Temperature, temperature);
        return this;
    }

    public LocationSpotProperties Build()
    {
        return properties;
    }

}