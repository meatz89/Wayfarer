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

public abstract class ValueTransformation
{
}

public class ConvertValueTransformation : ValueTransformation
{
    public ValueTypes SourceValueType { get; set; }
    public ValueTypes TargetValueType { get; set; }
}

public class ChangeValueTransformation : ValueTransformation
{
    public ValueTypes ValueType { get; set; }
    public int ChangeInValue { get; set; }
}

public class CancelValueTransformation : ValueTransformation
{
    public ValueTypes ValueType { get; set; }
}

public class EnergyValueTransformation : ValueTransformation
{
    public EnergyTypes EnergyType { get; set; }
    public int ChangeInValue { get; set; }
}
