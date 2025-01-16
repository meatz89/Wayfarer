public class ValueChange
{
    public ValueTypes ValueType { get; }
    public int Amount { get; set; }

    public ValueChange(ValueTypes type, int change)
    {
        ValueType = type;
        Amount = change;
    }

    public override string ToString()
    {
        return $"{Amount} {ValueType.ToString()}";
    }
}
