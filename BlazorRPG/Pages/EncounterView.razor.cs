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


    // New Method to calculate the preview state
    public EncounterStateValues CalculatePreviewState(EncounterStateValues currentState, List<ValueChange> valueChanges)
    {
        EncounterStateValues previewState = new EncounterStateValues
        {
            Advantage = currentState.Advantage,
            Understanding = currentState.Understanding,
            Connection = currentState.Connection,
            Tension = currentState.Tension
        };

        foreach (var valueChange in valueChanges)
        {
            switch (valueChange.ValueType)
            {
                case ValueTypes.Advantage:
                    previewState.Advantage = Math.Clamp(previewState.Advantage + valueChange.Change, 0, 10);
                    break;
                case ValueTypes.Understanding:
                    previewState.Understanding = Math.Clamp(previewState.Understanding + valueChange.Change, 0, 10);
                    break;
                case ValueTypes.Connection:
                    previewState.Connection = Math.Clamp(previewState.Connection + valueChange.Change, 0, 10);
                    break;
                case ValueTypes.Tension:
                    previewState.Tension = Math.Clamp(previewState.Tension + valueChange.Change, 0, 10);
                    break;
            }
        }

        return previewState;
    }

    public void HandleChoiceSelection(UserEncounterChoiceOption choice)
    {
        if (choice.EncounterChoice.ChoiceRequirements.Any(req => !req.IsSatisfied(GameState.Player)))
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
        foreach (Requirement req in choice.EncounterChoice.ChoiceRequirements)
        {
            if (!req.IsSatisfied(GameState.Player)) return false;
        }
        return true;
    }


    // Method to determine the CSS class based on the change
    public string GetStateChangeClass(int currentValue, int previewValue)
    {
        if (previewValue > currentValue)
        {
            return "positive";
        }
        else if (previewValue < currentValue)
        {
            return "negative";
        }
        else
        {
            return "";
        }
    }
}