

public class ValueChangeBuilder
{
    private List<ValueChange> changes = new();

    public ValueChangeBuilder WithAdvantage(int value)
    {
        changes.Add(new ValueChange(ValueTypes.Advantage, value));
        return this;
    }

    public ValueChangeBuilder WithUnderstanding(int value)
    {
        changes.Add(new ValueChange(ValueTypes.Understanding, value));
        return this;
    }

    public ValueChangeBuilder WithConnection(int value)
    {
        changes.Add(new ValueChange(ValueTypes.Connection, value));
        return this;
    }

    public ValueChangeBuilder WithTension(int value)
    {
        changes.Add(new ValueChange(ValueTypes.Tension, value));
        return this;
    }

    public List<ValueChange> Build()
    {
        return changes;
    }
}