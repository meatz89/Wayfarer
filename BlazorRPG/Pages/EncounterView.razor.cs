using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

public partial class EncounterViewBase : ComponentBase
{
    [Parameter] public EventCallback OnEncounterCompleted { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Inject] public GameManager GameManager { get; set; }

    public UserEncounterChoiceOption hoveredChoice;
    public bool showTooltip;
    public double mouseX;
    public double mouseY;

    public void ShowTooltip(UserEncounterChoiceOption choice)
    {
        hoveredChoice = choice;
        showTooltip = true;
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
            ValueTypes.Insight => new MarkupString("<i class='value-icon insight-icon'>💡</i>"),
            ValueTypes.Resonance => new MarkupString("<i class='value-icon resonance-icon'>🤝</i>"),
            ValueTypes.Pressure => new MarkupString("<i class='value-icon pressure-icon'>⚡</i>"),
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
        var relevantModifications = choice.Modifications
            .Where(m => m.Type == ModificationType.ValueChange &&
                        (m.ValueChange.ValueType == finalChange.ValueType || m.ValueChange.TargetValueType == finalChange.ValueType))
            .OrderBy(m => m.Source == ModificationSource.LocationProperty ? 0 : 1); // Prioritize location property modifications

        foreach (var modification in relevantModifications)
        {
            string modDesc = "";
            if (modification.Source == ModificationSource.LocationProperty)
            {
                modDesc += $"{modification.SourceDetails}: ";
            }

            if (modification.ValueChange.TargetValueType == null) // Direct modification
            {
                if (modification.ValueChange.ValueType == finalChange.ValueType)
                {
                    modDesc += $"{(modification.ValueChange.NewSourceValue > 0 ? "+" : "")}{modification.ValueChange.NewSourceValue - modification.ValueChange.OriginalSourceValue}";
                }
            }
            else // Conversion
            {
                if (modification.ValueChange.ValueType == finalChange.ValueType) // Source of conversion
                {
                    modDesc += $"{(modification.ValueChange.ConversionAmount > 0 ? "-" : "")}{modification.ValueChange.ConversionAmount} (to {modification.ValueChange.TargetValueType})";
                }
                else if (modification.ValueChange.TargetValueType == finalChange.ValueType) // Target of conversion
                {
                    modDesc += $"{(modification.ValueChange.ConversionAmount > 0 ? "+" : "")}{modification.ValueChange.ConversionAmount} (from {modification.ValueChange.ValueType})";
                }
            }

            modifications.Add(modDesc);
        }

        return modifications.Count > 0 ? $"({string.Join(", ", modifications)})" : string.Empty;
    }

    public bool IsValueModified(EncounterChoice choice, ValueChange finalChange)
    {
        // Check if there are *any* modifications for this value type
        return choice.Modifications.Any(m =>
            m.Type == ModificationType.ValueChange &&
            (m.ValueChange.ValueType == finalChange.ValueType ||
             (m.ValueChange.TargetValueType.HasValue && m.ValueChange.TargetValueType.Value == finalChange.ValueType)));
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
        List<ValueChange> modifiedValueChanges = encounterChoice.BaseValueChanges
            .Select(vc => new ValueChange(vc.ValueType, vc.Change))
            .ToList();

        // Apply modifications to the copied list
        foreach (var modification in encounterChoice.Modifications)
        {
            if (modification.Type == ModificationType.ValueChange && modification.ValueChange != null)
            {
                var valueChangeMod = modification.ValueChange;

                // Handle direct value changes and conversions
                if (valueChangeMod.TargetValueType == null) // Direct change
                {
                    var existingChange = modifiedValueChanges
                        .FirstOrDefault(vc => vc.ValueType == valueChangeMod.ValueType);

                    if (existingChange != null)
                    {
                        existingChange.Change = valueChangeMod.NewSourceValue;
                    }
                    else
                    {
                        modifiedValueChanges.Add(new ValueChange(valueChangeMod.ValueType, valueChangeMod.NewSourceValue));
                    }
                }
                else // Conversion
                {
                    var sourceChange = modifiedValueChanges
                        .FirstOrDefault(vc => vc.ValueType == valueChangeMod.ValueType);
                    var targetChange = modifiedValueChanges
                        .FirstOrDefault(vc => vc.ValueType == valueChangeMod.TargetValueType);

                    if (sourceChange != null)
                    {
                        sourceChange.Change = valueChangeMod.NewSourceValue;
                    }

                    if (targetChange != null)
                    {
                        targetChange.Change = valueChangeMod.NewTargetValue;
                    }
                    else
                    {
                        modifiedValueChanges.Add(new ValueChange(valueChangeMod.TargetValueType.Value, valueChangeMod.NewTargetValue));
                    }
                }
            }
        }

        return modifiedValueChanges;
    }


}