using Microsoft.AspNetCore.Components;

public partial class NarrativeViewBase : ComponentBase
{
    [Inject] public GameState GameState { get; set; }
    [Inject] public GameManager GameManager { get; set; }

    public UserNarrativeChoiceOption hoveredChoice;
    public bool showTooltip;
    public double mouseX;
    public double mouseY;

    public void ShowTooltip(UserNarrativeChoiceOption choice)
    {
        hoveredChoice = choice;
        showTooltip = true;
    }

    public void HideTooltip()
    {
        hoveredChoice = null;
        showTooltip = false;
    }

    public void HandleChoiceSelection(UserNarrativeChoiceOption choice)
    {
        if (choice.NarrativeChoice.Requirement != null &&
            !choice.NarrativeChoice.Requirement.IsSatisfied(GameState.Player))
        {
            return;
        }

        GameManager.ExecuteNarrativeChoice(choice);
    }

    public bool IsRequirementMet(UserNarrativeChoiceOption choice)
    {
        return choice.NarrativeChoice.Requirement?.IsSatisfied(GameState.Player) ?? true;
    }
}