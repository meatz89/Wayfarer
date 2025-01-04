public class ChoicePattern
{
    public PositionTypes Position { get; init; }
    public IntentTypes Intent { get; init; }
    public ScopeTypes Scope { get; init; }

    public int BaseCompletionPoints { get; init; }
    public int BaseEnergyCost { get; init; }

    public List<ValueChange> StandardValueChanges { get; init; } = new();
    public List<Requirement> StandardRequirements { get; init; } = new();
    public List<Outcome> StandardOutcomes { get; init; } = new();

    public NarrativeChoice CreateChoice(ActionContext context, int index)
    {
        var builder = new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ConvertPositionToChoiceType())
            .WithName(GenerateDescription(context));

        // Apply standard costs
        if (BaseEnergyCost > 0)
        {
            builder.ExpendsEnergy(GetContextEnergy(context), BaseEnergyCost);
        }

        // Apply standard value changes
        foreach (var change in StandardValueChanges)
        {
            ApplyValueChange(builder, change);
        }

        // Apply standard requirements
        foreach (Requirement requirement in StandardRequirements)
        {
            builder.WithRequirement(requirement);
        }

        return builder.Build();
    }

    private ChoiceTypes ConvertPositionToChoiceType()
    {
        return Position switch
        {
            PositionTypes.Direct => ChoiceTypes.Aggressive,
            PositionTypes.Careful => ChoiceTypes.Careful,
            PositionTypes.Tactical => ChoiceTypes.Tactical,
            PositionTypes.Recovery => ChoiceTypes.Modified,
            _ => throw new ArgumentException("Invalid position type")
        };
    }

    private void ApplyValueChange(ChoiceBuilder builder, ValueChange change)
    {
        switch (change.Type)
        {
            case ValueTypes.Momentum:
                builder.WithMomentumChange(change.Amount);
                break;
            case ValueTypes.Advantage:
                builder.WithAdvantageChange(change.Amount);
                break;
            case ValueTypes.Understanding:
                builder.WithUnderstandingChange(change.Amount);
                break;
            case ValueTypes.Connection:
                builder.WithConnectionChange(change.Amount);
                break;
            case ValueTypes.Tension:
                builder.WithTensionChange(change.Amount);
                break;
        }
    }

    private string GenerateDescription(ActionContext context)
    {
        return $"{Position} {Intent} ({Scope})";
    }

    private EnergyTypes GetContextEnergy(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => EnergyTypes.Physical,
            ActionTypes.Trade => EnergyTypes.Social,
            ActionTypes.Investigate => EnergyTypes.Focus,
            ActionTypes.Mingle => EnergyTypes.Social,
            _ => EnergyTypes.Physical
        };
    }
}
