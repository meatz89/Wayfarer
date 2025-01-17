using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

public partial class EncounterViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; } // Inject IJSRuntime
    [Parameter] public EventCallback<EncounterResults> OnEncounterCompleted { get; set; }
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

    public List<DetailedChange> GetValueChanges(EncounterChoice choice)
    {
        return choice.GetDetailedChanges();
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

        EncounterResults result = GameManager.ExecuteEncounterChoice(choice);
        OnEncounterCompleted.InvokeAsync(result);
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
        return choice.EncounterChoice.Requirements.Any(req => !req.IsSatisfied(GameState.Player));
    }
}

public class Dimensions
{
    public int WindowHeight { get; set; }
    public int TooltipHeight { get; set; }
}