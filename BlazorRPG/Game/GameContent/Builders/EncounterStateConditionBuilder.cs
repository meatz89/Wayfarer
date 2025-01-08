public class EncounterStateConditionBuilder
{
    private Dictionary<ValueTypes, int> maxValues = new();
    private Dictionary<ValueTypes, int> minValues = new();

    public EncounterStateConditionBuilder WithMaxPressure(int value)
    {
        maxValues[ValueTypes.Pressure] = value;
        return this;
    }

    public EncounterStateConditionBuilder WithMinInsight(int value)
    {
        minValues[ValueTypes.Insight] = value;
        return this;
    }

    // Add more value type conditions as needed

    public EncounterStateCondition Build()
    {
        return new EncounterStateCondition(minValues, maxValues);
    }
}
