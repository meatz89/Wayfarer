public class EncounterStateConditionBuilder
{
    private Dictionary<ValueTypes, int> maxValues = new();
    private Dictionary<ValueTypes, int> minValues = new();

    public EncounterStateConditionBuilder WithMaxMomentum(int value)
    {
        maxValues[ValueTypes.Momentum] = value;
        return this;
    }

    public EncounterStateConditionBuilder WithMaxPressure(int value)
    {
        maxValues[ValueTypes.Pressure] = value;
        return this;
    }

    public EncounterStateConditionBuilder WithMaxInsight(int value)
    {
        maxValues[ValueTypes.Insight] = value;
        return this;
    }

    public EncounterStateConditionBuilder WithMaxResonance(int value)
    {
        maxValues[ValueTypes.Resonance] = value;
        return this;
    }

    public EncounterStateConditionBuilder WithMaxOutcome(int value)
    {
        maxValues[ValueTypes.Outcome] = value;
        return this;
    }


    public EncounterStateConditionBuilder WithMinInsight(int value)
    {
        minValues[ValueTypes.Insight] = value;
        return this;
    }

    public EncounterStateConditionBuilder WithMinResonance(int value)
    {
        minValues[ValueTypes.Resonance] = value;
        return this;
    }

    public EncounterStateConditionBuilder WithMinOutcome(int value)
    {
        minValues[ValueTypes.Outcome] = value;
        return this;
    }

    public EncounterStateConditionBuilder WithMinPressure(int value)
    {
        minValues[ValueTypes.Pressure] = value;
        return this;
    }

    public EncounterStateCondition Build()
    {
        return new EncounterStateCondition(minValues, maxValues);
    }

}
