using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

public partial class EncounterViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; } // Inject IJSRuntime

    [Parameter] public EventCallback OnEncounterCompleted { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Inject] public GameManager GameManager { get; set; }

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
        var dimensions = await JSRuntime.InvokeAsync<Dimensions>("getDimensions");

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

    public void HandleChoiceSelection(UserEncounterChoiceOption choice)
    {
        if (IsChoiceDisabled(choice))
        {
            return;
        }

        GameManager.ExecuteEncounterChoice(choice);
        OnEncounterCompleted.InvokeAsync();
    }

    public void OnMouseMove(MouseEventArgs e)
    {
        mouseX = e.ClientX + 10;
        mouseY = e.ClientY + 10;
    }

    public MarkupString GetValueTypeIcon(ValueTypes valueType)
    {
        return valueType switch
        {
            ValueTypes.Outcome => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ValueTypes.Momentum => new MarkupString("<i class='value-icon momentum-icon'>⚡</i>"),
            ValueTypes.Insight => new MarkupString("<i class='value-icon insight-icon'>💡</i>"),
            ValueTypes.Resonance => new MarkupString("<i class='value-icon resonance-icon'>🤝</i>"),
            ValueTypes.Pressure => new MarkupString("<i class='value-icon pressure-icon'>⚠</i>"),
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

    public string RenderLocationEffect(LocationPropertyChoiceEffect effect)
    {
        return effect.ValueTypeEffect switch
        {
            ValueModification mod => $"{effect.LocationProperty.GetPropertyType()}: {mod.ValueType} {(mod.ModifierAmount > 0 ? "+" : "")}{mod.ModifierAmount}",
            ValueConversion conv => $"{effect.LocationProperty.GetPropertyType()}: Convert {conv.SourceValueType} to {conv.TargetValueType}",
            PartialValueConversion pConv => $"{effect.LocationProperty.GetPropertyType()}: Convert {pConv.ConversionAmount} {pConv.SourceValueType} to {pConv.TargetValueType}",
            EnergyModification eMod => $"{effect.LocationProperty.GetPropertyType()}: {eMod.TargetArchetype} Energy {(eMod.EnergyCostModifier > 0 ? "+" : "")}{eMod.EnergyCostModifier}",
            ValueBonus bonus => $"{effect.LocationProperty.GetPropertyType()}: {bonus.ChoiceArchetype} gains {bonus.ValueType} {(bonus.BonusAmount > 0 ? "+" : "")}{bonus.BonusAmount}",
            _ => effect.RuleDescription
        };
    }
    public string RenderValueModification(EncounterChoice choice, ValueChange finalChange)
    {
        List<string> modifications = new();

        // Get the true base value from the original value in the first modification
        ChoiceModification? firstMod = choice.Modifications
            .FirstOrDefault(m => m.Type == ModificationType.ValueChange &&
                                m.ValueChange?.ValueType == finalChange.ValueType);

        int baseValue = firstMod?.ValueChange?.OriginalValue ?? 0;

        // If we have a base value and modifications, add it first
        if (baseValue != 0)
        {
            modifications.Add($"Base: {(baseValue > 0 ? "+" : "")}{baseValue}");
        }

        // Add each modification's contribution
        foreach (ChoiceModification? modification in choice.Modifications
            .Where(m => m.Type == ModificationType.ValueChange &&
                        m.ValueChange?.ValueType == finalChange.ValueType)
            .OrderBy(m => m.Source == ModificationSource.LocationProperty ? 0 : 1))
        {
            if (modification.Source == ModificationSource.LocationProperty)
            {
                ValueChangeModification valueChange = modification.ValueChange;
                int contribution = valueChange.ConversionAmount; // This is the actual modification amount
                modifications.Add($"{modification.SourceDetails}: {(contribution > 0 ? "+" : "")}{contribution}");
            }
        }

        return modifications.Count > 0 ? $"({string.Join(", ", modifications)})" : string.Empty;
    }

    public bool IsValueModified(EncounterChoice choice, ValueChange finalChange)
    {
        // Check if there are *any* modifications for this value type
        return choice.Modifications.Any(m =>
            m.Type == ModificationType.ValueChange &&
            m.ValueChange != null &&
            m.ValueChange.ValueType == finalChange.ValueType);
    }

    public List<LocationPropertyChoiceEffect> GetLocationEffects(EncounterChoice choice)
    {
        return GameManager.GetLocationEffects(choice);
    }

    public string RenderEnergyCostModification(EncounterChoice choice)
    {
        IEnumerable<string> modifications = GetLocationEffects(choice)
            .Where(e => e.ValueTypeEffect is EnergyModification)
            .Select(e => $"{e.LocationProperty.GetPropertyType()}: {((EnergyModification)e.ValueTypeEffect).EnergyCostModifier}");

        return modifications.Any() ? $"({string.Join(", ", modifications)})" : string.Empty;
    }

    public bool IsEnergyCostModified(EncounterChoice choice)
    {
        return choice.EnergyCost != choice.ModifiedEnergyCost;
    }

    public string GetValueChangeClass(ValueChange change)
    {
        return change.Change > 0 ? "positive-change" : change.Change < 0 ? "negative-change" : "neutral-change";
    }

    public bool IsChoiceDisabled(UserEncounterChoiceOption choice)
    {
        // Use the ModifiedRequirements for the disabled check
        return choice.EncounterChoice.ModifiedRequirements.Any(req => !req.IsSatisfied(GameState.Player));
    }

    public List<ValueChange> GetModifiedValue(UserEncounterChoiceOption hoveredChoice)
    {
        EncounterChoice encounterChoice = hoveredChoice.EncounterChoice;

        // Create a copy of BaseValueChanges to modify
        List<ValueChange> modifiedValueChanges = encounterChoice.BaseEncounterValueChanges
            .Select(vc => new ValueChange(vc.ValueType, vc.Change))
            .ToList();

        // Apply modifications to the copied list
        foreach (ChoiceModification modification in encounterChoice.Modifications)
        {
            if (modification.Type == ModificationType.ValueChange && modification.ValueChange != null)
            {
                ValueChangeModification valueChangeMod = modification.ValueChange;

                // Handle direct value changes
                ValueChange? existingChange = modifiedValueChanges
                    .FirstOrDefault(vc => vc.ValueType == valueChangeMod.ValueType);

                if (existingChange != null)
                {
                    existingChange.Change = valueChangeMod.TargetValue; // Update the change with the modified value
                }
            }
        }

        return modifiedValueChanges;
    }

}

public class Dimensions
{
    public int WindowHeight { get; set; }
    public int TooltipHeight { get; set; }
}