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

public class LocationPropertyChoiceEffect
{
    public LocationPropertyTypeValue LocationProperty { get; set; }
    public ValueTransformation ValueTypeEffect { get; set; }
    public string RuleDescription { get; set; }
}

// Base class for all value transformations
public abstract class ValueTransformation
{
}

// Adds/subtracts from a value
public class ValueModification : ValueTransformation
{
    public ValueTypes ValueType { get; set; }
    public int ModifierAmount { get; set; }
}

// Completely converts one value type to another
public class ValueConversion : ValueTransformation
{
    public ValueTypes SourceValueType { get; set; }
    public ValueTypes TargetValueType { get; set; }
}

// Converts part of one value to another
public class PartialValueConversion : ValueTransformation
{
    public ValueTypes SourceValueType { get; set; }
    public ValueTypes TargetValueType { get; set; }
    public int ConversionAmount { get; set; }
    public ChoiceArchetypes TargetArchetype { get; set; }
}

// Modifies energy costs for specific choice types
public class EnergyModification : ValueTransformation
{
    public ChoiceArchetypes TargetArchetype { get; set; }
    public int EnergyCostModifier { get; set; }
}

// Adds bonus to specific value type for specific choice archetype
public class ValueBonus : ValueTransformation
{
    public ChoiceArchetypes ChoiceArchetype { get; set; }
    public ValueTypes ValueType { get; set; }
    public int BonusAmount { get; set; }
}
