public class EncounterStateConditionBuilder
{
    private Dictionary<ValueTypes, int> maxValues = new();
    private Dictionary<ValueTypes, int> minValues = new();

    public EncounterStateConditionBuilder WithMaxTension(int value)
    {
        maxValues[ValueTypes.Tension] = value;
        return this;
    }

    public EncounterStateConditionBuilder WithMinUnderstanding(int value)
    {
        minValues[ValueTypes.Understanding] = value;
        return this;
    }

    // Add more value type conditions as needed

    public EncounterStateCondition Build()
    {
        return new EncounterStateCondition(minValues, maxValues);
    }
}
