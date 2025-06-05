using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

public class EncounterViewBase : ComponentBase, IDisposable
{
    [Inject] protected GameWorldManager GameWorldManager { get; set; }
    [Parameter] public EventCallback<BeatOutcome> OnEncounterCompleted { get; set; }
    [Inject] protected IJSRuntime JSRuntime { get; set; }

    // State
    protected Timer _pollingTimer;
    protected GameWorldSnapshot currentSnapshot;

    // Tooltip state
    protected EncounterChoice hoveredChoice;
    protected bool showTooltip;
    protected double tooltipX;
    protected double tooltipY;

    protected override void OnInitialized()
    {
        // Set up polling timer - no events, just regular polling
        _pollingTimer = new Timer(_ =>
        {
            InvokeAsync(() =>
            {
                PollGameState();
                StateHasChanged();
            });
        }, null, 0, 100); // Poll every 100ms
    }

    protected void PollGameState()
    {
        // Poll for current game state
        currentSnapshot = GameWorldManager.GetGameSnapshot();

        // Check if encounter has completed and streaming is done
        if (currentSnapshot.HasActiveEncounter &&
            currentSnapshot.IsEncounterComplete &&
            !currentSnapshot.IsStreaming)
        {
            OnEncounterCompleted.InvokeAsync(new BeatOutcome
            {
                IsEncounterComplete = true,
                Outcome = currentSnapshot.SuccessfulOutcome ? BeatOutcomes.Success : BeatOutcomes.Failure
            });
        }
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

    public void Dispose()
    {
        _pollingTimer?.Dispose();
    }
}