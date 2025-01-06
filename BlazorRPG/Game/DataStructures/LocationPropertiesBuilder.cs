public class LocationPropertiesBuilder
{
    private LocationProperties properties;

    public LocationPropertiesBuilder(LocationProperties existingProperties = null)
    {
        this.properties = existingProperties ?? new LocationProperties();
    }

    public LocationPropertiesBuilder WithScale(ScaleVariations scale)
    {
        properties.SetProperty(LocationPropertyType.Scale, scale);
        return this;
    }

    public LocationPropertiesBuilder WithExposure(ExposureConditions exposure)
    {
        properties.SetProperty(LocationPropertyType.Exposure, exposure);
        return this;
    }

    public LocationPropertiesBuilder WithLegality(LegalityTypes legality)
    {
        properties.SetProperty(LocationPropertyType.Legality, legality);
        return this;
    }

    public LocationPropertiesBuilder WithTension(TensionState tension)
    {
        properties.SetProperty(LocationPropertyType.Tension, tension);
        return this;
    }

    public LocationPropertiesBuilder WithComplexity(ComplexityTypes complexity)
    {
        properties.SetProperty(LocationPropertyType.Complexity, complexity);
        return this;
    }

    public LocationPropertiesBuilder WithResource(ResourceTypes resource)
    {
        properties.SetProperty(LocationPropertyType.Resource, resource);
        return this;
    }

    public LocationPropertiesBuilder WithCrowdLevel(CrowdLevel crowdLevel)
    {
        properties.SetProperty(LocationPropertyType.CrowdLevel, crowdLevel);
        return this;
    }

    public LocationPropertiesBuilder WithArchetype(LocationArchetype archetype)
    {
        properties.SetProperty(LocationPropertyType.Archetype, archetype);
        return this;
    }

    public LocationProperties Build()
    {
        return properties;
    }
}