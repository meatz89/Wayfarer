public class ValueChange
{
    public ValueTypes ValueType { get; }
    public int Change { get; set; }

    public ValueChange(ValueTypes type, int change)
    {
        ValueType = type;
        Change = change;
    }
}
