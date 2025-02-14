public class EncounterStateConditionBuilder
{
    int? maxValue;
    int? minValue;

    public EncounterStateConditionBuilder WithMaxOutcome(int value)
    {
        maxValue = value;
        return this;
    }

    public EncounterStateConditionBuilder WithMinOutcome(int value)
    {
        minValue = value;
        return this;
    }

    public EncounterStateCondition Build()
    {
        return new EncounterStateCondition()
        {
            MaxValue = maxValue,
            MinValue = minValue,
        };
    }
}
