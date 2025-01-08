public class LocationPropertiesBuilder
{
    private LocationProperties properties;

    public LocationPropertiesBuilder(LocationProperties existingProperties = null)
    {
        this.properties = existingProperties ?? new LocationProperties();
    }

    public LocationPropertiesBuilder WithScale(ScaleVariationTypes scale)
    {
        properties.SetProperty(LocationPropertyTypes.Scale, scale);
        return this;
    }

    public LocationPropertiesBuilder WithExposure(ExposureConditionTypes exposure)
    {
        properties.SetProperty(LocationPropertyTypes.Exposure, exposure);
        return this;
    }

    public LocationPropertiesBuilder WithLegality(LegalityTypes legality)
    {
        properties.SetProperty(LocationPropertyTypes.Legality, legality);
        return this;
    }

    public LocationPropertiesBuilder WithTension(TensionStateTypes tension)
    {
        properties.SetProperty(LocationPropertyTypes.Tension, tension);
        return this;
    }

    public LocationPropertiesBuilder WithComplexity(ComplexityTypes complexity)
    {
        properties.SetProperty(LocationPropertyTypes.Complexity, complexity);
        return this;
    }

    public LocationPropertiesBuilder WithResource(ResourceTypes resource)
    {
        properties.SetProperty(LocationPropertyTypes.Resource, resource);
        return this;
    }

    public LocationPropertiesBuilder WithCrowdLevel(CrowdLevelTypes crowdLevel)
    {
        properties.SetProperty(LocationPropertyTypes.CrowdLevel, crowdLevel);
        return this;
    }

    public LocationPropertiesBuilder WithArchetype(LocationArchetypes archetype)
    {
        properties.SetProperty(LocationPropertyTypes.Archetype, archetype);
        return this;
    }

    public LocationProperties Build()
    {
        return properties;
    }
}