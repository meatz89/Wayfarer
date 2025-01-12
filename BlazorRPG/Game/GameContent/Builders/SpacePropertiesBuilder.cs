public class SpacePropertiesBuilder
{
    private AccessibilityTypes scale = AccessibilityTypes.Restricted;
    private ExposureConditionTypes exposure = ExposureConditionTypes.Indoor;
    private ActivityLevelTypes crowdLevel = ActivityLevelTypes.Deserted;

    public SpacePropertiesBuilder WithScale(AccessibilityTypes scale)
    {
        this.scale = scale;
        return this;
    }

    public SpacePropertiesBuilder WithExposure(ExposureConditionTypes exposure)
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