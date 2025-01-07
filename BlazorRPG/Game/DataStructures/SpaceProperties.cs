public class SpaceProperties
{
    public ScaleVariationTypes Scale { get; set; }
    public ExposureConditionTypes Exposure { get; set; }
    public CrowdLevelTypes CrowdLevel { get; internal set; }
}

public enum ScaleVariationTypes
{
    Medium,
    Intimate,
    Large
}

public enum ExposureConditionTypes
{
    Indoor,
    Outdoor,
}

public enum CrowdLevelTypes
{
    Empty,
    Sparse,
    Populated,
    Busy
}

