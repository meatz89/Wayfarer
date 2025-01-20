public class LocationPropertiesBuilder
{
    private LocationProperties properties;

    public LocationPropertiesBuilder(LocationProperties existingProperties = null)
    {
        this.properties = existingProperties ?? new LocationProperties();
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

    public LocationProperties Build()
    {
        return properties;
    }

    // ONLY FOR ACTIONS TO LOCATION MAPPING
    // NOT FOR USE IN CHOICESETS
    public LocationPropertiesBuilder WithArchetype(LocationArchetypes archetype)
    {
        properties.SetProperty(LocationPropertyTypes.Archetype, archetype);
        return this;
    }

    public LocationPropertiesBuilder WithResource(ResourceTypes resource)
    {
        properties.SetProperty(LocationPropertyTypes.Resource, resource);
        return this;
    }

    public LocationPropertiesBuilder WithCrowdDensity(CrowdDensity crowdDensity)
    {
        properties.SetProperty(LocationPropertyTypes.CrowdDensity, crowdDensity);
        return this;
    }

    public LocationPropertiesBuilder WithLocationScale(LocationScale locationScale)
    {
        properties.SetProperty(LocationPropertyTypes.LocationScale, locationScale);
        return this;
    }

}