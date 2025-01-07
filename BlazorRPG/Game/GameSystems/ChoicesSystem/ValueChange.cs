// Represents a specific value change in the encounter state
public class ValueChange
{
    public ValueTypes Type { get; }
    public int Amount { get; }

    public ValueChange(ValueTypes type, int amount)
    {
        Type = type;
        Amount = amount;
    }

}