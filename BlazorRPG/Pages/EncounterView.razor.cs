using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

public partial class EncounterViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; } // Inject IJSRuntime
    [Parameter] public EventCallback<EncounterResults> OnEncounterCompleted { get; set; }
    [Parameter] public Encounter Encounter { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Inject] public GameManager GameManager { get; set; }

    private const string BaseValueChangeLabel = "Base";

    public UserEncounterChoiceOption hoveredChoice;
    public bool showTooltip;
    public double mouseX;
    public double mouseY;

    public async Task ShowTooltip(UserEncounterChoiceOption choice, MouseEventArgs e)
    {
        hoveredChoice = choice;
        showTooltip = true;
        mouseX = e.ClientX + 10;
        mouseY = e.ClientY + 10;

        // Get dimensions using JavaScript interop
        Dimensions dimensions = await JSRuntime.InvokeAsync<Dimensions>("getDimensions");

        // Adjust mouseY if the tooltip would overflow
        if (mouseY + dimensions.TooltipHeight > dimensions.WindowHeight)
        {
            mouseY = e.ClientY - dimensions.TooltipHeight - 10; // Position above, with offset
        }
    }

    public void HideTooltip()
    {
        hoveredChoice = null;
        showTooltip = false;
    }


    public void OnMouseMove(MouseEventArgs e)
    {
        mouseX = e.ClientX + 10;
        mouseY = e.ClientY + 10;
    }

    public string GetProjectedValue(ChangeTypes changeType)
    {
        if (hoveredChoice == null) return "";

        int currentValue = GetCurrentValue(changeType);
        int projectedChange = GetProjectedChange(changeType);
        int projectedValue = currentValue + projectedChange;

        // Only show the change if it's not zero, and add class for styling
        if (projectedChange == 0)
        {
            return "";
        }
        else
        {
            string sign = projectedChange > 0 ? "+" : "";
            string projectedValueString = $"{sign}{projectedChange}";
            return projectedValueString;
        }
    }

    public int GetCurrentValue(ChangeTypes changeType)
    {
        EncounterValues currentValues = GameState.Actions.CurrentEncounter.Context.CurrentValues;
        return changeType switch
        {
            ChangeTypes.Outcome => currentValues.Outcome,
            ChangeTypes.Pressure => currentValues.Pressure,
            ChangeTypes.Momentum => currentValues.Momentum,
            ChangeTypes.Insight => currentValues.Insight,
            ChangeTypes.Resonance => currentValues.Resonance,
            _ => 0
        };
    }

    public int GetProjectedChange(ChangeTypes changeType)
    {
        if (hoveredChoice == null || hoveredChoice.EncounterChoice.CalculationResult == null) return 0;

        int projectedChange = 0;
        foreach (DetailedChange detailedChange in GetValueChanges(hoveredChoice.EncounterChoice))
        {
            if (detailedChange.ChangeType == changeType)
            {
                projectedChange += detailedChange.ChangeValues.TotalAmount;
            }
        }
        return projectedChange;
    }

    public MarkupString GetOutcomeIcon(Outcome outcome)
    {
        return new MarkupString("");
    }

    public MarkupString GetValueTypeIcon(ChangeTypes valueType)
    {
        return valueType switch
        {
            ChangeTypes.Outcome => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ChangeTypes.Momentum => new MarkupString("<i class='value-icon momentum-icon'>⚡</i>"),
            ChangeTypes.Insight => new MarkupString("<i class='value-icon insight-icon'>💡</i>"),
            ChangeTypes.Resonance => new MarkupString("<i class='value-icon resonance-icon'>🤝</i>"),
            ChangeTypes.Pressure => new MarkupString("<i class='value-icon pressure-icon'>⚠</i>"),
            ChangeTypes.PhysicalEnergy => new MarkupString("<i class='value-icon physical-icon'>💪</i>"),
            ChangeTypes.FocusEnergy => new MarkupString("<i class='value-icon focus-icon'>🎯</i>"),
            ChangeTypes.SocialEnergy => new MarkupString("<i class='value-icon social-icon'>👥</i>"),
            _ => new MarkupString("")
        };
    }

    public MarkupString GetEnergyTypeIcon(EnergyTypes energyType)
    {
        return energyType switch
        {
            EnergyTypes.Physical => new MarkupString("<i class='energy-icon physical-icon'>💪</i>"),
            EnergyTypes.Focus => new MarkupString("<i class='energy-icon focus-icon'>🎯</i>"),
            EnergyTypes.Social => new MarkupString("<i class='energy-icon social-icon'>👥</i>"),
            _ => new MarkupString("")
        };
    }

    public string GetEnergyDisplay(EncounterChoice choice)
    {
        int energyCost = choice.EnergyCost;

        // Calculate alternative costs if not enough energy
        switch (choice.EnergyType)
        {
            case EnergyTypes.Physical when GameState.Player.PhysicalEnergy < energyCost:
                int healthLoss = energyCost - GameState.Player.PhysicalEnergy;
                return $"-{healthLoss} Health";

            case EnergyTypes.Focus when GameState.Player.FocusEnergy < energyCost:
                int concentrationLoss = energyCost - GameState.Player.FocusEnergy;
                return $"-{concentrationLoss} Concentration";

            case EnergyTypes.Social when GameState.Player.SocialEnergy < energyCost:
                int reputationLoss = energyCost - GameState.Player.SocialEnergy;
                return $"{reputationLoss} Reputation";

            default:
                return $"{energyCost} {choice.EnergyType}";
        }
    }
    public List<CombinedValue> GetCombinedValues(EncounterChoice choice)
    {
        // Use the stored CalculationResult
        if (choice.CalculationResult == null) return new List<CombinedValue>();

        return ConvertCombinedValues(choice.CalculationResult.GetCombinedValues());
    }
    public List<CombinedValue> ConvertCombinedValues(Dictionary<ChangeTypes, int> combinedValuesDict)
    {
        List<CombinedValue> combinedValuesList = new List<CombinedValue>();
        foreach (KeyValuePair<ChangeTypes, int> kvp in combinedValuesDict)
        {
            combinedValuesList.Add(new CombinedValue { ChangeType = kvp.Key, Amount = kvp.Value });
        }
        return combinedValuesList;
    }

    public List<DetailedChange> GetValueChanges(EncounterChoice choice)
    {
        // Use the stored CalculationResult
        if (choice.CalculationResult == null) return new List<DetailedChange>();
        return ConvertDetailedChanges(choice.CalculationResult);
    }

    public List<DetailedChange> ConvertDetailedChanges(ChoiceCalculationResult calculationResult)
    {
        List<DetailedChange> detailedChanges = new List<DetailedChange>();

        // Add base changes
        foreach (BaseValueChange change in calculationResult.BaseValueChanges)
        {
            AddDetailedChange(detailedChanges, ConvertValueTypeToChangeType(change.ValueType), BaseValueChangeLabel, change.Amount);
        }

        // Add modifications
        foreach (ValueModification change in calculationResult.ValueModifications)
        {
            if (change is EncounterValueModification evm)
            {
                AddDetailedChange(detailedChanges, ConvertValueTypeToChangeType(evm.ValueType), change.Source, change.Amount);
            }
            else if (change is EnergyCostReduction em)
            {
                AddDetailedChange(detailedChanges, ConvertEnergyTypeToChangeType(em.EnergyType), change.Source, em.Amount);
            }
        }

        // Add Energy Cost as a negative modification
        if (calculationResult.EnergyCost > 0)
            AddDetailedChange(detailedChanges, ConvertEnergyTypeToChangeType(calculationResult.EnergyType), BaseValueChangeLabel, -calculationResult.EnergyCost);

        // Sort the detailed changes
        detailedChanges = SortDetailedChanges(detailedChanges);

        return detailedChanges;
    }

    public List<DetailedChange> SortDetailedChanges(List<DetailedChange> changes)
    {
        // Define the order of ChangeTypes
        List<ChangeTypes> order = new List<ChangeTypes>()
        {
            ChangeTypes.Momentum,
            ChangeTypes.Insight,
            ChangeTypes.Resonance,
            ChangeTypes.Outcome,
            ChangeTypes.Pressure,
            ChangeTypes.PhysicalEnergy,
            ChangeTypes.FocusEnergy,
            ChangeTypes.SocialEnergy
        };

        return changes.OrderBy(dc => order.IndexOf(dc.ChangeType)).ToList();
    }

    public void AddDetailedChange(List<DetailedChange> combined, ChangeTypes changeType, string source, int amount)
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

    public List<DetailedRequirement> GetDetailedRequirements(EncounterChoice choice)
    {
        if (choice.CalculationResult == null) return new List<DetailedRequirement>();
        return choice.GetDetailedRequirements(GameState.Player);
    }

    public ChangeTypes ConvertValueTypeToChangeType(ValueTypes valueType)
    {
        return valueType switch
        {
            ValueTypes.Outcome => ChangeTypes.Outcome,
            ValueTypes.Momentum => ChangeTypes.Momentum,
            ValueTypes.Insight => ChangeTypes.Insight,
            ValueTypes.Resonance => ChangeTypes.Resonance,
            ValueTypes.Pressure => ChangeTypes.Pressure,
            _ => throw new ArgumentException("Invalid ValueType")
        };
    }

    public ChangeTypes ConvertEnergyTypeToChangeType(EnergyTypes energyType)
    {
        return energyType switch
        {
            EnergyTypes.Physical => ChangeTypes.PhysicalEnergy,
            EnergyTypes.Focus => ChangeTypes.FocusEnergy,
            EnergyTypes.Social => ChangeTypes.SocialEnergy,
            _ => throw new ArgumentException("Invalid EnergyType")
        };
    }

    public void HandleChoiceSelection(UserEncounterChoiceOption choice)
    {
        if (IsChoiceDisabled(choice))
        {
            return;
        }

        EncounterResults result = GameManager.ExecuteEncounterChoice(choice);

        OnEncounterCompleted.InvokeAsync(result);
        HideTooltip();
    }

    public List<LocationPropertyChoiceEffect> GetLocationSpotEffects(EncounterChoice choice)
    {
        return GameManager.GetLocationEffects(Encounter, choice);
    }

    public bool IsChoiceDisabled(UserEncounterChoiceOption choice)
    {
        // Use the ModifiedRequirements for the disabled check
        return choice.EncounterChoice.Requirements.Any(req => !req.IsSatisfied(GameState.Player));
    }

    public MarkupString GetRequirementIcon(RequirementTypes requirementType)
    {
        return requirementType switch
        {
            RequirementTypes.MaxPressure => new MarkupString("<i class='requirement-icon pressure-icon'>⚠</i>"),
            RequirementTypes.MinInsight => new MarkupString("<i class='requirement-icon insight-icon'>💡</i>"),
            RequirementTypes.PhysicalEnergy => new MarkupString("<i class='requirement-icon physical-icon'>💪</i>"),
            RequirementTypes.FocusEnergy => new MarkupString("<i class='requirement-icon focus-icon'>🎯</i>"),
            RequirementTypes.SocialEnergy => new MarkupString("<i class='requirement-icon social-icon'>👥</i>"),
            RequirementTypes.Health => new MarkupString("<i class='requirement-icon health-icon'>❤️</i>"),
            RequirementTypes.Concentration => new MarkupString("<i class='requirement-icon concentration-icon'>🌀</i>"),
            RequirementTypes.Reputation => new MarkupString("<i class='requirement-icon reputation-icon'>👤</i>"),
            RequirementTypes.Coins => new MarkupString("<i class='requirement-icon coins-icon'>💰</i>"),
            RequirementTypes.Strength => new MarkupString("<i class='requirement-icon strength-icon'>💪</i>"),
            RequirementTypes.Perception => new MarkupString("<i class='requirement-icon perception-icon'>👁️</i>"),
            RequirementTypes.Charisma => new MarkupString("<i class='requirement-icon charisma-icon'>💬</i>"),
            RequirementTypes.Tool => new MarkupString("<i class='requirement-icon tool-icon'>🔧</i>"),
            RequirementTypes.Wood => new MarkupString("<i class='requirement-icon wood-icon'>🌲</i>"),
            RequirementTypes.Metal => new MarkupString("<i class='requirement-icon metal-icon'>🔩</i>"),
            RequirementTypes.InventorySlots => new MarkupString("<i class='requirement-icon inventory-slots-icon'>🗄️</i>"),
            RequirementTypes.LocalHistory => new MarkupString("<i class='requirement-icon local-history-icon'>📜</i>"),
            _ => new MarkupString("")
        };
    }
}

public class Dimensions
{
    public int WindowHeight { get; set; }
    public int TooltipHeight { get; set; }
}