public class ChoicePattern
{
    public PositionTypes Position { get; init; }
    public IntentTypes Intent { get; init; }
    public ScopeTypes Scope { get; init; }

    public int BaseEnergyCost { get; init; }

    public List<ValueChange> StandardValueChanges { get; init; } = new();
    public List<Requirement> StandardRequirements { get; init; } = new();
    public List<Outcome> StandardOutcomes { get; init; } = new();

    public EncounterChoice CreateChoice(EncounterActionContext context, int index)
    {
        ChoiceBuilder builder = new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ConvertPositionToChoiceType())
            .WithName(GenerateDescription(context));

        // Determine the energy type based on the context
        EnergyTypes energyType = GetContextEnergy(context);

        // Apply standard costs
        if (BaseEnergyCost > 0)
        {
            builder.ExpendsEnergy(energyType, BaseEnergyCost);
        }

        // Apply standard value changes
        foreach (ValueChange change in StandardValueChanges)
        {
            ApplyValueChange(builder, change);
        }

        // Apply standard requirements
        foreach (Requirement requirement in StandardRequirements)
        {
            builder.AddRequirement(requirement);
        }

        // Apply standard outcomes (rewards)
        foreach (Outcome outcome in StandardOutcomes)
        {
            if (outcome is EnergyOutcome)
            {
                // For EnergyOutcome, add it to Costs
                builder.AddCost(outcome);
            }
            else
            {
                // For other outcomes, add them to Rewards
                builder.AddReward(outcome);
            }
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

    private string GenerateDescription(EncounterActionContext context)
    {
        // This is where you'll implement the logic to generate dynamic descriptions
        // based on Position, Intent, Scope, and context.
        // For now, we'll use a placeholder:
        return $"{Position} {Intent} ({Scope})";
    }

    private EnergyTypes GetContextEnergy(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => EnergyTypes.Physical,
            BasicActionTypes.Craft => EnergyTypes.Physical,
            BasicActionTypes.Move => EnergyTypes.Physical,
            BasicActionTypes.Trade => EnergyTypes.Social,
            BasicActionTypes.Gather => EnergyTypes.Focus,
            BasicActionTypes.Investigate => EnergyTypes.Focus,
            BasicActionTypes.Study => EnergyTypes.Focus,
            BasicActionTypes.Plan => EnergyTypes.Focus,
            BasicActionTypes.Mingle => EnergyTypes.Social,
            BasicActionTypes.Perform => EnergyTypes.Social,
            BasicActionTypes.Persuade => EnergyTypes.Social,
            BasicActionTypes.Reflect => EnergyTypes.Focus,
            BasicActionTypes.Rest => EnergyTypes.Physical,
            _ => EnergyTypes.Physical
        };
    }
}