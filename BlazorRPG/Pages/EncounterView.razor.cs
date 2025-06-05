using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

public class EncounterViewBase : ComponentBase
{
    [Inject] protected GameWorldManager GameWorldManager { get; set; }
    [Parameter] public EventCallback<BeatOutcome> OnEncounterCompleted { get; set; }
    [Inject] protected IJSRuntime JSRuntime { get; set; }
    [Parameter] public EncounterManager EncounterManager { get; set; }

    // State
    protected GameWorldSnapshot currentSnapshot;

    // Tooltip state
    protected EncounterChoice hoveredChoice;
    protected bool showTooltip;
    protected double tooltipX;
    protected double tooltipY;

    protected override async Task OnInitializedAsync()
    {
        await EncounterManager.ProcessNextBeat();

        currentSnapshot = GameWorldManager.GetGameSnapshot();
    }
    protected override void OnParametersSet()
    {
        currentSnapshot = GameWorldManager.GetGameSnapshot();
    }

    protected async Task MakeChoice(string choiceId)
    {
        HideTooltip();

        if (currentSnapshot?.CanSelectChoice == true)
        {
            EncounterChoice selectedChoice = currentSnapshot.AvailableChoices
                .FirstOrDefault(c => c.ChoiceID == choiceId);

            if (selectedChoice != null)
            {
                await GameWorldManager.ProcessPlayerChoice(new PlayerChoiceSelection
                {
                    Choice = selectedChoice
                });
            }
        }
    }

    protected void ShowTooltip(MouseEventArgs e, EncounterChoice choice)
    {
        hoveredChoice = choice;
        showTooltip = true;
        tooltipX = e.ClientX + 10;
        tooltipY = e.ClientY + 10;
    }

    protected void HideTooltip()
    {
        hoveredChoice = null;
        showTooltip = false;
    }

}