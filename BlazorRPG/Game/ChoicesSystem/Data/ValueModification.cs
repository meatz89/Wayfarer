public class BaseValueChange
{
    public ValueTypes ValueType { get; }
    public int Amount { get; }

    public BaseValueChange(ValueTypes type, int amount)
    {
        ValueType = type;
        Amount = amount;
    }

    public override string ToString()
    {
        return $"{ValueType} {Amount}";
    }

    public static List<BaseValueChange> CombineBaseValueChanges(List<BaseValueChange> changes)
    {
        return changes
            .GroupBy(c => c.ValueType)
            .Where(g => g.Sum(c => c.Amount) != 0)
            .Select(g => new BaseValueChange(g.Key, g.Sum(c => c.Amount)))
            .ToList();
    }
}

// Modifications - track source and can be chained/combined
public abstract class ValueModification
{
    public int Amount { get; set; }
    public string Source { get; set; }
}

// Modifications - track source and can be chained/combined
public class EncounterValueModification : ValueModification
{
    public ValueTypes ValueType { get; }

    public EncounterValueModification(ValueTypes type, int amount, string source)
    {
        ValueType = type;
        Amount = amount;
        Source = source;
    }

    public override string ToString()
    {
        return $"{ValueType}: {Amount} (from {Source}";
    }
}

public class EnergyCostReduction : ValueModification
{
    public EnergyTypes EnergyType { get; set; }

    public EnergyCostReduction(EnergyTypes type, int amount, string source)
    {
        EnergyType = type;
        Amount = amount;
        Source = source;
    }

    public override string ToString()
    {
        return $"{EnergyType}: {Amount} (from {Source}";
    }
}
