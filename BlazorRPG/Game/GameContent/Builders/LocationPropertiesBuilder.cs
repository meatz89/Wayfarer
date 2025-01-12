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

    public LocationPropertiesBuilder WithResource(ResourceTypes resource)
    {
        properties.SetProperty(LocationPropertyTypes.Resource, resource);
        return this;
    }

    public LocationPropertiesBuilder WithActivityLevel(ActivityLevelTypes activityLevel)
    {
        properties.SetProperty(LocationPropertyTypes.ActivityLevel, activityLevel);
        return this;
    }

    public LocationPropertiesBuilder WithAccessibility(AccessibilityTypes accessibility)
    {
        properties.SetProperty(LocationPropertyTypes.Accessibility, accessibility);
        return this;
    }

    public LocationPropertiesBuilder WithSupervision(SupervisionTypes supervision)
    {
        properties.SetProperty(LocationPropertyTypes.Supervision, supervision);
        return this;
    }

    public LocationPropertiesBuilder WithAtmosphere(AtmosphereTypes pressure)
    {
        properties.SetProperty(LocationPropertyTypes.Atmosphere, pressure);
        return this;
    }

    public LocationPropertiesBuilder WithSpace(SpaceTypes space)
    {
        properties.SetProperty(LocationPropertyTypes.Space, space);
        return this;
    }

    public LocationPropertiesBuilder WithLighting(LightingTypes lighting)
    {
        properties.SetProperty(LocationPropertyTypes.Lighting, lighting);
        return this;
    }

    public LocationPropertiesBuilder WithExposure(ExposureConditionTypes exposure)
    {
        properties.SetProperty(LocationPropertyTypes.Exposure, exposure);
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