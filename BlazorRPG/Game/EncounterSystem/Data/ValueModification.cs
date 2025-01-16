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
public class ValueModification
{
    public ValueTypes ValueType { get; }
    public int Amount { get; }
    public string Source { get; }

    public ValueModification(ValueTypes type, int amount, string source)
    {
        ValueType = type;
        Amount = amount;
        Source = source;
    }
}