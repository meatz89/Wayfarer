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
        if (choice.EncounterChoice.Requirements.Any(req => !req.IsSatisfied(GameState.Player)))
        {
            return; // Don't execute if requirements are not met
        }

        // Apply the choice to the encounter
        GameManager.ExecuteEncounterChoice(choice);

        // The GameManager should have already updated the encounter state,
        // so we can just trigger a re-render
        StateHasChanged();
        OnEncounterCompleted.InvokeAsync();
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
}