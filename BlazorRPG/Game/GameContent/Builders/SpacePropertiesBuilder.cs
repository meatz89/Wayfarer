public class SpacePropertiesBuilder
{
    private AccessTypes access;
    private ScaleVariations scale = ScaleVariations.Medium;
    private ExposureConditions exposure = ExposureConditions.Indoor;
    private PopulationDensity population = PopulationDensity.Sparse;

    public SpacePropertiesBuilder WithAccess(AccessTypes access)
    {
        this.access = access;
        return this;
    }

    public SpacePropertiesBuilder WithScale(ScaleVariations scale)
    {
        this.scale = scale;
        return this;
    }

    public SpacePropertiesBuilder WithExposure(ExposureConditions exposure)
    {
        this.exposure = exposure;
        return this;
    }

    public SpacePropertiesBuilder WithPopulation(PopulationDensity population)
    {
        this.population = population;
        return this;
    }

    public SpaceProperties Build()
    {
        return new SpaceProperties
        {
            Access = access,
            Scale = scale,
            Exposure = exposure,
            Population = population
        };
    }
}