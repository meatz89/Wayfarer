using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

public class ConversationViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] public GameWorldManager GameWorldManager { get; set; }
    [Parameter] public EventCallback<ConversationBeatOutcome> OnConversationCompleted { get; set; }
    [Parameter] public ConversationManager ConversationManager { get; set; }

    // State
    public GameWorldSnapshot currentSnapshot;

    // Tooltip state
    public ConversationChoice hoveredChoice;
    public bool showTooltip;
    public double tooltipX;
    public double tooltipY;

    protected override async Task OnInitializedAsync()
    {
        currentSnapshot = GameWorldManager.GetGameSnapshot();
        // TODO: Implement conversation beat logic
        // await GameWorldManager.NextConversationBeat();
    }

    protected override async Task OnParametersSetAsync()
    {
        currentSnapshot = GameWorldManager.GetGameSnapshot();

        if (!currentSnapshot.IsStreaming &&
            !currentSnapshot.IsAwaitingAIResponse)
        {
            // TODO: Implement conversation beat logic
        // await GameWorldManager.NextConversationBeat();
        }
    }

    public async Task MakeChoice(string choiceId)
    {
        HideTooltip();

        if (currentSnapshot?.CanSelectChoice == true)
        {
            ConversationChoice selectedChoice = currentSnapshot.AvailableChoices
                .FirstOrDefault(c => c.ChoiceID == choiceId);

            if (selectedChoice != null)
            {
                // TODO: Implement player choice processing
                // await GameWorldManager.ProcessPlayerChoice(new PlayerChoiceSelection
                // {
                //     Choice = selectedChoice
                // });
            }
        }
    }

    public void ShowTooltip(MouseEventArgs e, ConversationChoice choice)
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