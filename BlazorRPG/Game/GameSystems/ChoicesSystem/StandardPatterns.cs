
public class StandardPatterns
{
    public static readonly ChoicePattern DirectProgressImmediate = new()
    {
        Position = PositionTypes.Direct,
        Intent = IntentTypes.Progress,
        Scope = ScopeTypes.Immediate,
        BaseEnergyCost = 2, // Cost in appropriate energy
        StandardValueChanges = new()
        {
            new(ValueTypes.Advantage, 2),
            new(ValueTypes.Tension, 1)
        }
    };

    public static readonly ChoicePattern CarefulPositionInvested = new()
    {
        Position = PositionTypes.Careful,
        Intent = IntentTypes.Position,
        Scope = ScopeTypes.Invested,
        BaseEnergyCost = 1, // Cost in appropriate energy
        StandardValueChanges = new()
        {
            new(ValueTypes.Understanding, 2),
            new(ValueTypes.Tension, -1) // Reduce tension
        }
    };

    public static readonly ChoicePattern TacticalOpportunityStrategic = new()
    {
        Position = PositionTypes.Tactical,
        Intent = IntentTypes.Opportunity,
        Scope = ScopeTypes.Strategic,
        BaseEnergyCost = 1, // Cost in appropriate energy
        StandardValueChanges = new()
        {
            new(ValueTypes.Advantage, 1),
            new(ValueTypes.Connection, 1)
        },
        StandardOutcomes = new()
        {
            new ResourceOutcome(ResourceTypes.Food, 1) // Example: Gain a small amount of food
        }
    };
}