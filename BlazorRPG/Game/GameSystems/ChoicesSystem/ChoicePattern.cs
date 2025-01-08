public class ChoicePattern
{
    public PositionTypes Position { get; init; }
    public IntentTypes Intent { get; init; }
    public ScopeTypes Scope { get; init; }

    public int BaseEnergyCost { get; init; }

    public List<EncounterValueChange> StandardValueChanges { get; init; } = new();
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
            builder.RequiresEnergy(energyType, BaseEnergyCost);
        }

        // Apply standard value changes using WithValueChange
        foreach (EncounterValueChange change in StandardValueChanges)
        {
            builder.WithValueChange(change.ValueType, change.Change);
        }

        // Apply standard requirements
        foreach (Requirement requirement in StandardRequirements)
        {
            builder.AddRequirement(requirement);
        }

        // Apply standard outcomes (rewards)
        foreach (Outcome outcome in StandardOutcomes)
        {
            // Apply outcomes based on type
            if (outcome is EnergyOutcome energyOutcome)
            {
                builder.AddCost(energyOutcome);
            }
            else if (outcome is ItemOutcome itemOutcome)
            {
                builder.AddReward(itemOutcome);
            }
            else if (outcome is KnowledgeOutcome knowledgeOutcome)
            {
                builder.AddReward(knowledgeOutcome);
            }
            else if (outcome is ReputationOutcome reputationOutcome)
            {
                builder.AddReward(reputationOutcome);
            }
            else if (outcome is CoinsOutcome coinsOutcome)
            {
                builder.AddReward(coinsOutcome);
            }
            else if (outcome is ResourceOutcome resourceOutcome)
            {
                builder.AddReward(resourceOutcome);
            }
            // Add other outcome types as needed
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

    private void ApplyValueChange(ChoiceBuilder builder, EncounterValueChange change)
    {
        switch (change.ValueType)
        {
            case EncounterValues.Advantage:
                builder.WithAdvantageChange(change.Change);
                break;
            case EncounterValues.Understanding:
                builder.WithUnderstandingChange(change.Change);
                break;
            case EncounterValues.Connection:
                builder.WithConnectionChange(change.Change);
                break;
            case EncounterValues.Tension:
                builder.WithTensionChange(change.Change);
                break;
        }
    }

    private string GenerateDescription(EncounterActionContext context)
    {
        string position = Position.ToString();
        string intent = Intent.ToString();
        string scope = Scope.ToString();

        switch (context.ActionType)
        {
            case BasicActionTypes.Labor:
                position = GetLaborPositionDescription();
                intent = GetLaborIntentDescription();
                scope = GetLaborScopeDescription();
                break;
            case BasicActionTypes.Trade:
                position = GetTradePositionDescription();
                intent = GetTradeIntentDescription();
                scope = GetTradeScopeDescription();
                break;
            case BasicActionTypes.Mingle:
                position = GetMinglePositionDescription();
                intent = GetMingleIntentDescription();
                scope = GetMingleScopeDescription();
                break;
            case BasicActionTypes.Investigate:
                position = GetInvestigatePositionDescription();
                intent = GetInvestigateIntentDescription();
                scope = GetInvestigateScopeDescription();
                break;
                // Add cases for other BasicActionTypes
        }

        return $"{position} {intent} ({scope})";
    }

    // Helper methods for generating descriptions based on ActionType
    private string GetLaborPositionDescription()
    {
        return Position switch
        {
            PositionTypes.Direct => "Forceful",
            PositionTypes.Careful => "Methodical",
            PositionTypes.Tactical => "Efficient",
            PositionTypes.Recovery => "Restorative",
            _ => Position.ToString()
        };
    }
    private string GetTradePositionDescription()
    {
        return Position switch
        {
            PositionTypes.Direct => "Assertive",
            PositionTypes.Careful => "Calculated",
            PositionTypes.Tactical => "Strategic",
            PositionTypes.Recovery => "Bargaining",
            _ => Position.ToString()
        };
    }

    private string GetMinglePositionDescription()
    {
        return Position switch
        {
            PositionTypes.Direct => "Charismatic",
            PositionTypes.Careful => "Observant",
            PositionTypes.Tactical => "Influential",
            PositionTypes.Recovery => "Amiable",
            _ => Position.ToString()
        };
    }

    private string GetInvestigatePositionDescription()
    {
        return Position switch
        {
            PositionTypes.Direct => "Confrontational",
            PositionTypes.Careful => "Thorough",
            PositionTypes.Tactical => "Insightful",
            PositionTypes.Recovery => "Reflective",
            _ => Position.ToString()
        };
    }

    private string GetLaborIntentDescription()
    {
        return Intent switch
        {
            IntentTypes.Progress => "Work",
            IntentTypes.Position => "Approach",
            IntentTypes.Opportunity => "Scheme",
            _ => Intent.ToString()
        };
    }

    private string GetTradeIntentDescription()
    {
        return Intent switch
        {
            IntentTypes.Progress => "Negotiation",
            IntentTypes.Position => "Haggling",
            IntentTypes.Opportunity => "Deal",
            _ => Intent.ToString()
        };
    }

    private string GetMingleIntentDescription()
    {
        return Intent switch
        {
            IntentTypes.Progress => "Interaction",
            IntentTypes.Position => "Impression",
            IntentTypes.Opportunity => "Connection",
            _ => Intent.ToString()
        };
    }

    private string GetInvestigateIntentDescription()
    {
        return Intent switch
        {
            IntentTypes.Progress => "Inquiry",
            IntentTypes.Position => "Deduction",
            IntentTypes.Opportunity => "Revelation",
            _ => Intent.ToString()
        };
    }

    private string GetLaborScopeDescription()
    {
        return Scope switch
        {
            ScopeTypes.Immediate => "Immediate Results",
            ScopeTypes.Invested => "Long-Term Gains",
            ScopeTypes.Strategic => "Overall Progress",
            _ => Scope.ToString()
        };
    }
    private string GetTradeScopeDescription()
    {
        return Scope switch
        {
            ScopeTypes.Immediate => "Quick Deal",
            ScopeTypes.Invested => "Favorable Terms",
            ScopeTypes.Strategic => "Market Advantage",
            _ => Scope.ToString()
        };
    }

    private string GetMingleScopeDescription()
    {
        return Scope switch
        {
            ScopeTypes.Immediate => "Social Interaction",
            ScopeTypes.Invested => "Building Rapport",
            ScopeTypes.Strategic => "Social Network",
            _ => Scope.ToString()
        };
    }

    private string GetInvestigateScopeDescription()
    {
        return Scope switch
        {
            ScopeTypes.Immediate => "Uncover Clue",
            ScopeTypes.Invested => "Gather Information",
            ScopeTypes.Strategic => "Solve Mystery",
            _ => Scope.ToString()
        };
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