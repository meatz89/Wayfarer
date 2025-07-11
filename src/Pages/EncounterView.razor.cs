using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

public class EncounterViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] public GameWorldManager GameWorldManager { get; set; }
    [Parameter] public EventCallback<BeatOutcome> OnEncounterCompleted { get; set; }
    [Parameter] public EncounterManager EncounterManager { get; set; }

    // State
    public GameWorldSnapshot currentSnapshot;

    // Tooltip state
    public EncounterChoice hoveredChoice;
    public bool showTooltip;
    public double tooltipX;
    public double tooltipY;

    protected override async Task OnInitializedAsync()
    {
        currentSnapshot = GameWorldManager.GetGameSnapshot();
        await GameWorldManager.NextEncounterBeat();
    }

    protected override async Task OnParametersSetAsync()
    {
        currentSnapshot = GameWorldManager.GetGameSnapshot();

        if (!currentSnapshot.IsStreaming &&
            !currentSnapshot.IsAwaitingAIResponse)
        {
            await GameWorldManager.NextEncounterBeat();
        }
    }

    public async Task MakeChoice(string choiceId)
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

    public void ShowTooltip(MouseEventArgs e, EncounterChoice choice)
    {
        hoveredChoice = choice;
        showTooltip = true;
        tooltipX = e.ClientX + 5;
        tooltipY = e.ClientY - 300;
    }

    public void HideTooltip()
    {
        if (showTooltip)
        {
            hoveredChoice = null;
            showTooltip = false;
        }
    }

}