

public class LocationPropertiesBuilder
{
    private LocationProperties properties;

    public LocationPropertiesBuilder(LocationProperties existingProperties = null)
    {
        this.properties = existingProperties ?? new LocationProperties();
    }

    public LocationPropertiesBuilder WithScale(AccessibilityTypes scale)
    {
        properties.SetProperty(LocationPropertyTypes.Exposure, scale);
        return this;
    }

    public LocationPropertiesBuilder WithExposure(ExposureConditionTypes exposure)
    {
        properties.SetProperty(LocationPropertyTypes.Accessibility, exposure);
        return this;
    }

    public LocationPropertiesBuilder WithLegality(SupervisionTypes legality)
    {
        properties.SetProperty(LocationPropertyTypes.Supervision, legality);
        return this;
    }

    public LocationPropertiesBuilder WithSupervision(SupervisionTypes pressure)
    {
        properties.SetProperty(LocationPropertyTypes.Atmosphere, pressure);
        return this;
    }

    public LocationPropertiesBuilder WithComplexity(AtmosphereTypes complexity)
    {
        properties.SetProperty(LocationPropertyTypes.Space, complexity);
        return this;
    }

    public LocationPropertiesBuilder WithResource(ResourceTypes resource)
    {
        properties.SetProperty(LocationPropertyTypes.Resource, resource);
        return this;
    }

    public LocationPropertiesBuilder WithAnyResource()
    {
        properties.SetProperty(LocationPropertyTypes.Resource, ResourceTypes.Any);
        return this;
    }

    public LocationPropertiesBuilder WithActivityLevel(ActivityLevelTypes crowdLevel)
    {
        properties.SetProperty(LocationPropertyTypes.ActivityLevel, crowdLevel);
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
}