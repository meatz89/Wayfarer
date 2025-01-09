public class SpaceProperties
{
    public ScaleVariationTypes Scale { get; set; }
    public ExposureConditionTypes Exposure { get; set; }
    public CrowdLevelTypes CrowdLevel { get; set; }
}

public enum AccessTypes
{
    Public, Private, Restricted
}

public enum ScaleVariationTypes
{
    Intimate, Medium, Large
}

public enum ExposureConditionTypes
{
    Indoor, Outdoor,
}

public enum CrowdLevelTypes
{
    Empty, Sparse, Busy, Crowded
}

public enum LightingTypes
{
    Bright, Dim, Dark
}

public enum VisibilityTypes
{
    Open, Limited, Hidden
}

