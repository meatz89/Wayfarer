public class EncounterStateCondition
{
    private Dictionary<ValueTypes, int> MinValues { get; }
    private Dictionary<ValueTypes, int> MaxValues { get; }

    public EncounterStateCondition(Dictionary<ValueTypes, int> minValues, Dictionary<ValueTypes, int> maxValues)
    {
        MinValues = minValues;
        MaxValues = maxValues;
    }

    public bool IsMet(EncounterValues state)
    {
        foreach ((ValueTypes valueType, int minValue) in MinValues)
        {
            int currentValue = GetValueFromState(state, valueType);
            if (currentValue <= minValue) return false;
        }

        foreach ((ValueTypes valueType, int maxValue) in MaxValues)
        {
            int currentValue = GetValueFromState(state, valueType);
            if (currentValue >= maxValue) return false;
        }

        return true;
    }

    private int GetValueFromState(EncounterValues state, ValueTypes type)
    {
        return type switch
        {
            ValueTypes.Momentum => state.Momentum,
            ValueTypes.Insight => state.Insight,
            ValueTypes.Resonance => state.Resonance,
            ValueTypes.Outcome => state.Outcome,
            ValueTypes.Pressure => state.Pressure,
            _ => 0
        };
    }

    public override string ToString()
    {
        string explanation = string.Empty;

        foreach (KeyValuePair<ValueTypes, int> minValue in MinValues)
        {
            if (!string.IsNullOrWhiteSpace(explanation)) explanation += Environment.NewLine;
            explanation += $">{minValue.Value} {minValue.Key}";
        }
        foreach (KeyValuePair<ValueTypes, int> maxValue in MaxValues)
        {
            if (!string.IsNullOrWhiteSpace(explanation)) explanation += Environment.NewLine;
            explanation += $"<{maxValue.Value} {maxValue.Key}";
        }

        return explanation;
    }
}