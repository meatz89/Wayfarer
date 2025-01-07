using Microsoft.AspNetCore.Components;

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
        if (choice.EncounterChoice.Requirement != null &&
            !choice.EncounterChoice.Requirement.IsSatisfied(GameState.Player))
        {
            return;
        }

        GameManager.ExecuteEncounterChoice(choice);
        OnEncounterCompleted.InvokeAsync();
    }

    public bool IsRequirementMet(UserEncounterChoiceOption choice)
    {
        return choice.EncounterChoice.Requirement?.IsSatisfied(GameState.Player) ?? true;
    }
}
