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
        int baseChange = choice.BaseValueChanges.FirstOrDefault(b => b.ValueType == finalChange.ValueType)?.Change ?? 0;
        List<string> modifications = new List<string>();

        if (baseChange != finalChange.Change)
        {
            modifications.Add($"Base: {baseChange}");
            foreach (LocationPropertyChoiceEffect effect in GetLocationEffects(choice))
            {
                if (effect.ValueTypeEffect is ValueModification mod && mod.ValueType == finalChange.ValueType)
                {
                    modifications.Add($"{effect.LocationProperty.GetPropertyType()}: {(mod.ModifierAmount > 0 ? "+" : "")}{mod.ModifierAmount}");
                }
            }
        }

        return modifications.Count > 0 ? $"({string.Join(", ", modifications)})" : string.Empty;
    }

    public List<LocationPropertyChoiceEffect> GetLocationEffects(EncounterChoice choice)
    {
        return GameManager.GetLocationEffects(choice);
    }

    public bool IsValueModified(EncounterChoice choice, ValueChange finalChange)
    {
        int baseChange = choice.BaseValueChanges.FirstOrDefault(b => b.ValueType == finalChange.ValueType)?.Change ?? 0;
        return baseChange != finalChange.Change;
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

}