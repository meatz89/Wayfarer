public class SpacePropertiesBuilder
{
    private Accessability scale = Accessability.Communal;
    private Temperature exposure = Temperature.Warm;
    private CrowdDensity crowdLevel = CrowdDensity.Deserted;

    public SpacePropertiesBuilder WithScale(Accessability scale)
    {
        this.scale = scale;
        return this;
    }

    public SpacePropertiesBuilder WithExposure(Temperature exposure)
    {
        this.exposure = exposure;
        return this;
    }

    public SpacePropertiesBuilder WithCrowdLevel(CrowdDensity crowdLevel)
    {
        this.crowdLevel = crowdLevel;
        return this;
    }

    public SpaceProperties Build()
    {
        return new SpaceProperties
        {
            Scale = scale,
            Exposure = exposure,
            CrowdLevel = crowdLevel,
        };
    }
}