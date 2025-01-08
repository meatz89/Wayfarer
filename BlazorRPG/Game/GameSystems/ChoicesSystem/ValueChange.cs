// Represents a specific value change in the encounter state
public class ValueChange
{
    public ValueTypes ValueType { get; }
    public int Change { get; }

    public ValueChange(ValueTypes type, int amount)
    {
        ValueType = type;
        Change = amount;
    }

}

public enum ValueTypes
{
    Momentum,
    Outcome,
    Insight,
    Resonance,
    Pressure
}
