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

    public List<(ValueTypes Type, string Label, int Total, List<string> Details)> GetValueChanges(EncounterChoice choice)
    {
        // Get all detailed changes
        Dictionary<ValueTypes, (int TotalAmount, List<string> Sources)> detailedChanges = choice.GetDetailedChanges();
        List<(ValueTypes Type, string Label, int Total, List<string> Details)> result = new List<(ValueTypes Type, string Label, int Total, List<string> Details)>();

        // For each value type, create a display entry
        foreach (KeyValuePair<ValueTypes, (int TotalAmount, List<string> Sources)> kvp in detailedChanges)
        {
            ValueTypes valueType = kvp.Key;
            (int totalAmount, List<string> sources) = kvp.Value;

            result.Add((
                valueType,                        // The value type
                valueType.ToString(),            // Label for display
                totalAmount,                     // Total combined change
                sources                          // List of detailed change descriptions
            ));
        }

        return result;
    }

    public string RenderValueModification(EncounterChoice choice, ValueModification change)
    {
        Dictionary<ValueTypes, (int TotalAmount, List<string> Sources)> detailedChanges = choice.GetDetailedChanges();
        if (detailedChanges.TryGetValue(change.ValueType, out (int TotalAmount, List<string> Sources) details))
        {
            return string.Join(", ", details.Sources);
        }
        return "";
    }

    public bool IsValueModified(EncounterChoice choice, ValueModification change)
    {
        // Check if there are any modifications for this value type
        return choice.ValueModifications.Any(m => m.ValueType == change.ValueType);
    }

    public EncounterStateValues GetNewStateValues(UserEncounterChoiceOption choice)
    {
        ChoiceCalculationResult result = GameManager.CalculateChoiceEffects(choice.EncounterChoice, choice.Encounter.Context);
        return result.NewStateValues;
    }

    public void HandleChoiceSelection(UserEncounterChoiceOption choice)
    {
        if (IsChoiceDisabled(choice))
        {
            return;
        }

        GameManager.ExecuteEncounterChoice(choice);
        OnEncounterCompleted.InvokeAsync();
        HideTooltip();
    }

    public List<LocationPropertyChoiceEffect> GetLocationEffects(EncounterChoice choice)
    {
        return GameManager.GetLocationEffects(choice);
    }

    public string RenderEnergyCostModification(EncounterChoice choice)
    {
        // TODO
        return "";
    }

    public bool IsEnergyCostModified(EncounterChoice choice)
    {
        return choice.EnergyCost != choice.EnergyCost;
    }

    public string GetValueChangeClass(ValueModification change)
    {
        return change.Amount > 0 ? "positive-change" : change.Amount < 0 ? "negative-change" : "neutral-change";
    }

    public bool IsChoiceDisabled(UserEncounterChoiceOption choice)
    {
        // Use the ModifiedRequirements for the disabled check
        return choice.EncounterChoice.ModifiedRequirements.Any(req => !req.IsSatisfied(GameState.Player));
    }
}

public class Dimensions
{
    public int WindowHeight { get; set; }
    public int TooltipHeight { get; set; }
}