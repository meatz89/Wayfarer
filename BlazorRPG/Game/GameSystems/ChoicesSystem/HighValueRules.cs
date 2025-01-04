
public class HighValueRules
{
    public static readonly List<ValueStateRule> AllRules = new()
    {
        new ValueStateRule
        {
            ValueType = ValueTypes.Momentum,
            Threshold = 8,
            Modification = new()
            {
                CompletionPointMultiplier = 2.0f,
                EnergyCostIncrease = 1,
                ValueChanges = new() { new(ValueTypes.Tension, 1) }
            }
        },
        new ValueStateRule
        {
            ValueType = ValueTypes.Advantage,
            Threshold = 8,
            Modification = new()
            {
                SkipRequirements = true,
                ValueChanges = new() { new(ValueTypes.Understanding, 1) }
            }
        },
        new ValueStateRule
        {
            ValueType = ValueTypes.Understanding,
            Threshold = 8,
            Modification = new()
            {
                UnlockedOptions = new() { "HIDDEN_OPTION" },
                ValueChanges = new() { new(ValueTypes.Connection, 1) }
            }
        },
        new ValueStateRule
        {
            ValueType = ValueTypes.Connection,
            Threshold = 8,
            Modification = new()
            {
                UnlockedOptions = new() { "SPECIAL_REQUEST" },
                ValueChanges = new() { new(ValueTypes.Advantage, 1) }
            }
        },
        new ValueStateRule
        {
            ValueType = ValueTypes.Tension,
            Threshold = 8,
            Modification = new()
            {
                CompletionPointMultiplier = 1.5f,
                ValueChanges = new() { new(ValueTypes.Momentum, 1) }
            }
        }
    };
}