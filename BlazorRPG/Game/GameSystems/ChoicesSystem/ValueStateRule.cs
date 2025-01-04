public class ValueStateRule
{
    public ValueTypes ValueType { get; init; }
    public int Threshold { get; init; }
    public ChoiceModification Modification { get; init; }

    public bool ShouldApply(NarrativeState state)
    {
        var value = ValueType switch
        {
            ValueTypes.Momentum => state.Momentum,
            ValueTypes.Advantage => state.Advantage,
            ValueTypes.Understanding => state.Understanding,
            ValueTypes.Connection => state.Connection,
            ValueTypes.Tension => state.Tension,
            _ => throw new ArgumentException("Invalid value type")
        };

        return value >= Threshold;
    }
}
