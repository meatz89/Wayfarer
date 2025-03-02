using Microsoft.AspNetCore.Components;

public partial class EncounterChoiceTooltipBase : ComponentBase
{
    [Inject] public GameManager GameManager { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Parameter] public Encounter encounter { get; set; }
    [Parameter] public UserEncounterChoiceOption hoveredChoice { get; set; }
    [Parameter] public double mouseX { get; set; }
    [Parameter] public double mouseY { get; set; }

    public List<LocationPropertyChoiceEffect> GetLocationSpotEffects(Choice choice)
    {
        return GameManager.GetLocationEffects(encounter, choice);
    }

    public List<DetailedChange> GetValueChanges(Choice choice)
    {
        // Use the stored CalculationResult
        //if (choice.CalculationResult == null) return new List<DetailedChange>();
        //return ConvertDetailedChanges(choice.CalculationResult, choice.Approach);
        return new List<DetailedChange>();
    }

    public List<DetailedRequirement> GetDetailedRequirements(Choice choice)
    {
        //if (choice.CalculationResult == null) return new List<DetailedRequirement>();
        //return choice.GetDetailedRequirements(GameState);
        return new List<DetailedRequirement>();
    }

    public void AddDetailedChange(List<DetailedChange> combined, ValueTypes changeType, string source, int amount)
    {
        bool found = false;
        foreach (DetailedChange dc in combined)
        {
            if (dc.ChangeType == changeType)
            {
                dc.ChangeValues.TotalAmount += amount;
                dc.ChangeValues.Sources.Add($"{source}: {(amount >= 0 ? "+" : "")}{amount}");
                found = true;
                break;
            }
        }

        if (!found)
        {
            combined.Add(new DetailedChange
            {
                ChangeType = changeType,
                ChangeValues = new ChangeValues
                {
                    TotalAmount = amount,
                    Sources = new List<string> { $"{source}: {(amount >= 0 ? "+" : "")}{amount}" }
                }
            });
        }
    }

    public List<DetailedChange> ConvertDetailedChanges(ChoiceCalculationResult calculationResult, ChoiceApproaches approach)
    {
        List<DetailedChange> detailedChanges = new List<DetailedChange>();

        // Add base changes
        // Add modifications
        foreach (ValueModification change in calculationResult.ValueModifications)
        {
            if (change is MomentumModification evm)
            {
                AddDetailedChange(detailedChanges, ValueTypes.Momentum, change.Source, change.Amount);
            }
            if (change is PressureModification evp)
            {
                AddDetailedChange(detailedChanges, ValueTypes.Pressure, change.Source, change.Amount);
            }
            else if (change is EnergyCostReduction em)
            {
                AddDetailedChange(detailedChanges, ConvertEnergyTypeToChangeType(em.EnergyType), change.Source, em.Amount);
            }
        }

        // Sort the detailed changes
        detailedChanges = SortDetailedChanges(detailedChanges);

        return detailedChanges;
    }

    public List<DetailedChange> SortDetailedChanges(List<DetailedChange> changes)
    {
        // Define the order of ChangeTypes
        List<ValueTypes> order = new List<ValueTypes>()
        {
            ValueTypes.Momentum,
            ValueTypes.Pressure,
            ValueTypes.PhysicalEnergy,
            ValueTypes.Concentration,
            ValueTypes.Reputation
        };

        return changes.OrderBy(dc => order.IndexOf(dc.ChangeType)).ToList();
    }


    public MarkupString GetOutcomeIcon(Outcome outcome)
    {
        return new MarkupString("");
    }

    public MarkupString GetValueTypeIcon(ValueTypes valueType)
    {
        return valueType switch
        {
            ValueTypes.Momentum => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ValueTypes.Pressure => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ValueTypes.PhysicalEnergy => new MarkupString("<i class='value-icon physical-icon'>💪</i>"),
            ValueTypes.Concentration => new MarkupString("<i class='value-icon focus-icon'>🎯</i>"),
            ValueTypes.Reputation => new MarkupString("<i class='value-icon social-icon'>👥</i>"),
            _ => new MarkupString("")
        };
    }

    public MarkupString GetRequirementIcon(RequirementTypes requirementType)
    {
        return requirementType switch
        {
            RequirementTypes.Health => new MarkupString("<i class='requirement-icon health-icon'>❤️</i>"),
            RequirementTypes.Coins => new MarkupString("<i class='requirement-icon coins-icon'>💰</i>"),
            RequirementTypes.MaxPressure => new MarkupString("<i class='requirement-icon pressure-icon'>⚠</i>"),
            RequirementTypes.MinInsight => new MarkupString("<i class='requirement-icon insight-icon'>💡</i>"),
            RequirementTypes.PhysicalEnergy => new MarkupString("<i class='requirement-icon physical-icon'>💪</i>"),
            RequirementTypes.Concentration => new MarkupString("<i class='requirement-icon concentration-icon'>🌀</i>"),
            RequirementTypes.Strength => new MarkupString("<i class='requirement-icon strength-icon'>💪</i>"),
            RequirementTypes.Perception => new MarkupString("<i class='requirement-icon perception-icon'>👁️</i>"),
            RequirementTypes.Charisma => new MarkupString("<i class='requirement-icon charisma-icon'>💬</i>"),
            RequirementTypes.Tool => new MarkupString("<i class='requirement-icon tool-icon'>🔧</i>"),
            RequirementTypes.Wood => new MarkupString("<i class='requirement-icon wood-icon'>🌲</i>"),
            RequirementTypes.Metal => new MarkupString("<i class='requirement-icon metal-icon'>🔩</i>"),
            RequirementTypes.InventorySlots => new MarkupString("<i class='requirement-icon inventory-slots-icon'>🗄️</i>"),
            RequirementTypes.LocalHistory => new MarkupString("<i class='requirement-icon local-history-icon'>📜</i>"),
            RequirementTypes.Shunned => new MarkupString("<i class='requirement-icon reputation-icon'>👤</i>"),
            RequirementTypes.Untrustworthy => new MarkupString("<i class='requirement-icon reputation-icon'>👤</i>"),
            RequirementTypes.Neutral => new MarkupString("<i class='requirement-icon reputation-icon'>👤</i>"),
            RequirementTypes.Trusted => new MarkupString("<i class='requirement-icon reputation-icon'>👤</i>"),
            RequirementTypes.Respected => new MarkupString("<i class='requirement-icon reputation-icon'>👤</i>"),
            _ => new MarkupString("")
        };
    }

    public ValueTypes ConvertEnergyTypeToChangeType(EnergyTypes energyType)
    {
        return energyType switch
        {
            EnergyTypes.Physical => ValueTypes.PhysicalEnergy,
            EnergyTypes.Concentration => ValueTypes.Concentration,
            _ => throw new ArgumentException("Invalid EnergyType")
        };
    }

    public MarkupString GetEnergyTypeIcon(EnergyTypes energyType)
    {
        return energyType switch
        {
            EnergyTypes.Physical => new MarkupString("<i class='energy-icon physical-icon'>💪</i>"),
            EnergyTypes.Concentration => new MarkupString("<i class='energy-icon focus-icon'>🎯</i>"),
            _ => new MarkupString("")
        };
    }

}
