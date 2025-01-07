public class SpacePropertiesBuilder
{
    private ScaleVariationTypes scale = ScaleVariationTypes.Medium;
    private ExposureConditionTypes exposure = ExposureConditionTypes.Indoor;
    private CrowdLevelTypes crowdLevel = CrowdLevelTypes.Empty;

    public SpacePropertiesBuilder WithScale(ScaleVariationTypes scale)
    {
        this.scale = scale;
        return this;
    }

    public SpacePropertiesBuilder WithExposure(ExposureConditionTypes exposure)
    {
        this.exposure = exposure;
        return this;
    }

    public SpacePropertiesBuilder WithCrowdLevel(CrowdLevelTypes crowdLevel)
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