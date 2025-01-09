public class ValueChangeDetail
{
    public ValueTypes ValueType { get; set; }
    public int BaseChange { get; set; }
    public List<LocationArchetypeTransformation> Transformations { get; set; } = new();
    public int FinalChange => Transformations.Sum(t => t.Amount);
}

public class LocationArchetypeTransformation
{
    public LocationArchetypes Source { get; set; }
    public int Amount { get; set; }
    public string Explanation { get; set; }
}

public class ValueTransformation
{
    public ValueTypes SourceValue { get; set; }
    public ValueTypes TargetValue { get; set; }
    public TransformationType TransformationType { get; set; }
}