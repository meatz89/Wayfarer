public class SpacePropertiesBuilder
{
    private AccessibilityTypes scale = AccessibilityTypes.Restricted;
    private ExposureTypes exposure = ExposureTypes.Indoor;
    private ActivityLevelTypes crowdLevel = ActivityLevelTypes.Deserted;

    public SpacePropertiesBuilder WithScale(AccessibilityTypes scale)
    {
        this.scale = scale;
        return this;
    }

    public SpacePropertiesBuilder WithExposure(ExposureTypes exposure)
    {
        this.exposure = exposure;
        return this;
    }

    public SpacePropertiesBuilder WithCrowdLevel(ActivityLevelTypes crowdLevel)
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