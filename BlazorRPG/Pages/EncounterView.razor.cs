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
        var locationPropertyEffects = LocationPropertyChoiceEffects.Effects;

        foreach (var effect in locationPropertyEffects)
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

    private bool IsLocationPropertyMatch(LocationPropertyTypeValue locationProperty, EncounterContext context)
{
    switch (locationProperty.GetPropertyType())
    {
        case LocationPropertyTypes.Scale:
            return context.LocationProperties.Scale == ((ScaleValue)locationProperty).ScaleVariation;
        case LocationPropertyTypes.Exposure:
            return context.LocationProperties.Exposure == ((ExposureValue)locationProperty).ExposureCondition;
        case LocationPropertyTypes.Legality:
            return context.LocationProperties.Legality == ((LegalityValue)locationProperty).Legality;
        case LocationPropertyTypes.Pressure:
            return context.LocationProperties.Pressure == ((PressureValue)locationProperty).PressureState;
        case LocationPropertyTypes.Complexity:
            return context.LocationProperties.Complexity == ((ComplexityValue)locationProperty).Complexity;
        case LocationPropertyTypes.Resource:
            return context.LocationProperties.Resource == ((ResourceValue)locationProperty).Resource;
        case LocationPropertyTypes.CrowdLevel:
            return context.LocationProperties.CrowdLevel == ((CrowdLevelValue)locationProperty).CrowdLevel;
        case LocationPropertyTypes.ReputationType:
            return context.LocationProperties.ReputationType == ((LocationReputationTypeValue)locationProperty).ReputationType;
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

public FinalEnergyCost CalculateFinalEnergyCost(EncounterChoice choice)
{
    FinalEnergyCost finalCost = new FinalEnergyCost
    {
        FinalCost = choice.EnergyCost,
        Reduction = 0
    };

    // Apply location-based energy reduction
    foreach (var effect in LocationPropertyChoiceEffects.Effects)
    {
        // Check if the effect's LocationProperty matches the current context
        if (IsLocationPropertyMatch(effect.LocationProperty, GameState.Actions.CurrentEncounter.Context))
        {
            if (effect.ValueTypeEffect is EnergyValueTransformation energyTransformation &&
                choice.EnergyType == energyTransformation.EnergyType)
            {
                finalCost.Reduction -= energyTransformation.ChangeInValue; // Assuming positive change reduces cost
            }
        }
    }

    finalCost.FinalCost = Math.Max(0, finalCost.FinalCost - finalCost.Reduction);
    return finalCost;
}

public string GetTransformationEffect(ChoiceModification modification)
{
    if (modification.Type != ModificationType.ValueChange || modification.ValueChange.ValueTransformation == null)
    {
        return string.Empty;
    }

    switch (modification.ValueChange.ValueTransformation)
    {
        case ConvertValueTransformation convertTransformation:
            return $"Converted {modification.ValueChange.Amount} {convertTransformation.SourceValueType} to {modification.ValueChange.Amount} {convertTransformation.TargetValueType}";
        case ChangeValueTransformation changeTransformation:
            if (changeTransformation.ChangeInValue > 0)
            {
                return $"Increased {changeTransformation.ValueType} by {changeTransformation.ChangeInValue}";
            }
            else if (changeTransformation.ChangeInValue < 0)
            {
                return $"Reduced {changeTransformation.ValueType} by {Math.Abs(changeTransformation.ChangeInValue)}";
            }
            else
            {
                return string.Empty; // No change
            }
        case CancelValueTransformation cancelTransformation:
            return $"Canceled {cancelTransformation.ValueType}";
        default:
            return string.Empty;
    }
}
}