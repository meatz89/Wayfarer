

public class ValueChangeBuilder
{
    private List<ValueChange> changes = new();

    public ValueChangeBuilder WithOutcome(int value)
    {
        changes.Add(new ValueChange(ValueTypes.Outcome, value));
        return this;
    }

    public ValueChangeBuilder WithInsight(int value)
    {
        changes.Add(new ValueChange(ValueTypes.Insight, value));
        return this;
    }

    public ValueChangeBuilder WithResonance(int value)
    {
        changes.Add(new ValueChange(ValueTypes.Resonance, value));
        return this;
    }

    public ValueChangeBuilder WithPressure(int value)
    {
        changes.Add(new ValueChange(ValueTypes.Pressure, value));
        return this;
    }

    public List<ValueChange> Build()
    {
        return changes;
    }
}