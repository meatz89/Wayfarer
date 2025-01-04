public class StandardPatterns
{
    public static readonly ChoicePattern DirectProgressImmediate = new()
    {
        Position = PositionTypes.Direct,
        Intent = IntentTypes.Progress,
        Scope = ScopeTypes.Immediate,
        BaseCompletionPoints = 3,
        BaseEnergyCost = 2,
        StandardValueChanges = new()
        {
            new(ValueTypes.Momentum, 2),
            new(ValueTypes.Tension, 1)
        }
    };

    public static readonly ChoicePattern CarefulPositionInvested = new()
    {
        Position = PositionTypes.Careful,
        Intent = IntentTypes.Position,
        Scope = ScopeTypes.Invested,
        BaseCompletionPoints = 1,
        BaseEnergyCost = 0,
        StandardValueChanges = new()
        {
            new(ValueTypes.Understanding, 1),
            new(ValueTypes.Connection, 1),
            new(ValueTypes.Momentum, -1)
        }
    };

    public static readonly ChoicePattern TacticalOpportunityStrategic = new()
    {
        Position = PositionTypes.Tactical,
        Intent = IntentTypes.Opportunity,
        Scope = ScopeTypes.Strategic,
        BaseCompletionPoints = 0,
        BaseEnergyCost = 1,
        StandardValueChanges = new()
        {
            new(ValueTypes.Advantage, 2),
            new(ValueTypes.Understanding, 1)
        }
    };
}
