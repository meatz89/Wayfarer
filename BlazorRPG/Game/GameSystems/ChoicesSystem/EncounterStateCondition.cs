public class EncounterStateCondition
{
    private Dictionary<ValueTypes, int> MinValues { get; }
    private Dictionary<ValueTypes, int> MaxValues { get; }

    public EncounterStateCondition(Dictionary<ValueTypes, int> minValues, Dictionary<ValueTypes, int> maxValues)
    {
        MinValues = minValues;
        MaxValues = maxValues;
    }

    public bool IsMet(EncounterStateValues state)
    {
        foreach ((ValueTypes valueType, int minValue) in MinValues)
        {
            int currentValue = GetValueFromState(state, valueType);
            if (currentValue < minValue) return false;
        }

        foreach ((ValueTypes valueType, int maxValue) in MaxValues)
        {
            int currentValue = GetValueFromState(state, valueType);
            if (currentValue > maxValue) return false;
        }

        return true;
    }

    private int GetValueFromState(EncounterStateValues state, ValueTypes type)
    {
        return type switch
        {
            ValueTypes.Advantage => state.Advantage,
            ValueTypes.Understanding => state.Understanding,
            ValueTypes.Connection => state.Connection,
            ValueTypes.Tension => state.Tension,
            _ => 0
        };
    }
}