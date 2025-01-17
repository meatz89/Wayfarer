// Base values - simple and straightforward
public class BaseValueChange
{
    public ValueTypes ValueType { get; }
    public int Amount { get; }

    public BaseValueChange(ValueTypes type, int amount)
    {
        ValueType = type;
        Amount = amount;
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
}

public class EnergyModification : ValueModification
{
    public EnergyTypes EnergyType { get; set; }

    public EnergyModification(EnergyTypes type, int amount, string source)
    {
        EnergyType = type;
        Amount = amount;
        Source = source;
    }
}
