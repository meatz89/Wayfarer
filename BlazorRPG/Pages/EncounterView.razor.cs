using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;

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

    public bool IsChoiceDisabled(UserEncounterChoiceOption choice)
    {
        // Use the ModifiedRequirements for the disabled check
        return choice.EncounterChoice.ModifiedRequirements.Any(req => !req.IsSatisfied(GameState.Player));
    }

    public void OnMouseMove(MouseEventArgs e)
    {
        mouseX = e.ClientX + 10;
        mouseY = e.ClientY + 10;
    }

    public bool IsRequirementMet(UserEncounterChoiceOption choice)
    {
        foreach (Requirement req in choice.EncounterChoice.Requirements)
        {
            if (!req.IsSatisfied(GameState.Player)) return false;
        }
        return true;
    }

    public List<string> GetLocationTransformationRules()
    {
        List<string> rules = new List<string>();
        List<LocationPropertyChoiceEffect> locationPropertyEffects = LocationPropertyChoiceEffects.Effects;

        foreach (LocationPropertyChoiceEffect effect in locationPropertyEffects)
        {
            // Check if the effect's LocationProperty matches the current context
            if (IsLocationPropertyMatch(effect.LocationProperty, GameState.Actions.CurrentEncounter.Context))
            {
                // Get the rule description directly from the effect
                if (!string.IsNullOrEmpty(effect.RuleDescription))
                {
                    rules.Add(effect.RuleDescription);
                }
            }
        }

        return rules;
    }

    public bool IsLocationPropertyMatch(LocationPropertyTypeValue locationProperty, EncounterContext context)
    {
        switch (locationProperty.GetPropertyType())
        {
            case LocationPropertyTypes.Scale:
                return context.LocationProperties.Scale == ((ScaleValue)locationProperty).ScaleVariation;
            case LocationPropertyTypes.Exposure:
                return context.LocationProperties.Exposure == ((ExposureValue)locationProperty).Exposure;
            case LocationPropertyTypes.Legality:
                return context.LocationProperties.Legality == ((LegalityValue)locationProperty).Legality;
            case LocationPropertyTypes.Pressure:
                return context.LocationProperties.Pressure == ((PressureStateValue)locationProperty).PressureState;
            case LocationPropertyTypes.Complexity:
                return context.LocationProperties.Complexity == ((ComplexityValue)locationProperty).Complexity;
            case LocationPropertyTypes.Resource:
                return context.LocationProperties.Resource == ((ResourceValue)locationProperty).Resource;
            case LocationPropertyTypes.CrowdLevel:
                return context.LocationProperties.CrowdLevel == ((CrowdLevelValue)locationProperty).CrowdLevel;
            case LocationPropertyTypes.ReputationType:
                return context.LocationProperties.LocationReputationType == ((LocationReputationTypeValue)locationProperty).ReputationType;
            default:
                return false;
        }
    }

    public MarkupString GetRequirementDescription(Requirement req, PlayerState player)
    {
        bool isMet = req.IsSatisfied(player);
        string iconHtml = isMet
            ? "<span class='green-checkmark'>✓</span>"
            : "<span class='red-x'>✗</span>";

        return new MarkupString($"{iconHtml} {req.GetDescription()}");
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

    public string GetChoiceArchetypeIcon(ChoiceArchetypes archetype)
    {
        return archetype switch
        {
            ChoiceArchetypes.Physical => "💪",
            ChoiceArchetypes.Focus => "🎯",
            ChoiceArchetypes.Social => "👥",
            _ => ""
        };
    }

    public string GetChoiceApproachIcon(ChoiceApproaches approach)
    {
        return approach switch
        {
            ChoiceApproaches.Aggressive => "⚔️",
            ChoiceApproaches.Careful => "🛡️",
            ChoiceApproaches.Strategic => "📋",
            ChoiceApproaches.Desperate => "⚠️",
            _ => ""
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
}