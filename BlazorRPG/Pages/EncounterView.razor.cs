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

    public List<string> GetLocationTransformationRules(LocationArchetypes archetype)
    {
        var rules = new List<string>();
        var archetypeEffect = LocationArchetypeContent.Effects[archetype];

        foreach (var transformation in archetypeEffect.ValueTransformations)
        {
            foreach (var trans in transformation.Value)
            {
                switch (trans.TransformationType)
                {
                    case TransformationType.Convert:
                        rules.Add($"Each point of {trans.SourceValue} converts one point of {trans.SourceValue} gain into {trans.TargetValue}");
                        break;
                    case TransformationType.Reduce:
                    case TransformationType.ReduceCost:
                        rules.Add($"Each point of {trans.SourceValue} reduces one point of {trans.TargetValue} gain");
                        break;
                    case TransformationType.Increase:
                        rules.Add($"Each point of {trans.SourceValue} increases one point of {trans.TargetValue} gain");
                        break;
                    case TransformationType.Set:
                        rules.Add($"Each point of {trans.SourceValue} can be set to {trans.TargetValue} instead");
                        break;
                }
            }
        }

        foreach (var reduction in archetypeEffect.EnergyCostReductions)
        {
            rules.Add($"{reduction.Key} energy cost at this Location reduced by {reduction.Value}");
        }

        return rules;
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
        var finalCost = new FinalEnergyCost
        {
            FinalCost = choice.EnergyCost,
            Reduction = 0
        };

        // Apply location-based energy reduction
        var archetypeEffect = LocationArchetypeContent.Effects[GameState.Actions.CurrentEncounter.Context.LocationArchetype];

        if (archetypeEffect.EnergyCostReductions.TryGetValue(choice.EnergyType, out int reduction))
        {
            finalCost.Reduction += reduction;
        }

        finalCost.FinalCost = Math.Max(0, finalCost.FinalCost - finalCost.Reduction);
        return finalCost;
    }


    private string GetTransformationEffect(ChoiceModification modification)
    {
        if (modification.Type != ModificationType.ValueChange || modification.ValueChange.ValueTransformation == null)
        {
            return string.Empty;
        }

        var transformation = modification.ValueChange.ValueTransformation;
        switch (transformation.TransformationType)
        {
            case TransformationType.Convert:
                return $"Converted {modification.ValueChange.Amount} {transformation.SourceValue} to {modification.ValueChange.Amount} {transformation.TargetValue}";
            case TransformationType.Reduce:
            case TransformationType.ReduceCost:
                return $"Reduced {modification.ValueChange.Amount} {transformation.TargetValue} due to {modification.ValueChange.Amount} {transformation.SourceValue}";
            case TransformationType.Increase:
                return $"Increased {modification.ValueChange.Amount} {transformation.TargetValue} due to {modification.ValueChange.Amount} {transformation.SourceValue}";
            case TransformationType.Set:
                return $"Set {modification.ValueChange.Amount} {transformation.SourceValue} to {modification.ValueChange.Amount} {transformation.TargetValue}";
            default:
                return string.Empty;
        }
    }
}
