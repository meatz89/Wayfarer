
public class SpacePropertiesBuilder
{
    private ScaleVariations scale = ScaleVariations.Medium;
    private ExposureConditions exposure = ExposureConditions.Indoor;
    private CrowdLevel crowdLevel = CrowdLevel.Empty;

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

    public SpacePropertiesBuilder WithCrowdLevel(CrowdLevel crowdLevel)
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