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
        
        // Start the conversation by showing introduction and getting first choices
        if (ConversationManager != null)
        {
            await ConversationManager.InitializeConversation();
            await ConversationManager.ProcessNextBeat();
            currentSnapshot = GameWorldManager.GetGameSnapshot();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        currentSnapshot = GameWorldManager.GetGameSnapshot();

        if (!currentSnapshot.IsStreaming &&
            !currentSnapshot.IsAwaitingAIResponse &&
            ConversationManager != null)
        {
            // Process next beat if conversation is active
            if (!currentSnapshot.IsConversationComplete)
            {
                await ConversationManager.ProcessNextBeat();
                currentSnapshot = GameWorldManager.GetGameSnapshot();
            }
        }
    }

    public async Task MakeChoice(string choiceId)
    {
        HideTooltip();

        if (ConversationManager != null && ConversationManager.Choices?.Any() == true)
        {
            ConversationChoice selectedChoice = ConversationManager.Choices
                .FirstOrDefault(c => c.ChoiceID == choiceId);

            if (selectedChoice != null)
            {
                // Process the player's choice
                var outcome = await ConversationManager.ProcessPlayerChoice(selectedChoice);
                
                // Update the game world with the last selected choice
                GameWorldManager.SetLastSelectedChoice(selectedChoice);
                
                // Check if conversation is complete
                if (outcome.IsConversationComplete)
                {
                    await OnConversationCompleted.InvokeAsync(outcome);
                }
                else
                {
                    // Continue conversation
                    await ConversationManager.ProcessNextBeat();
                    currentSnapshot = GameWorldManager.GetGameSnapshot();
                    StateHasChanged();
                }
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
    
    public async Task CompleteConversation()
    {
        // Create simple outcome for completion
        var outcome = new ConversationBeatOutcome
        {
            IsConversationComplete = true,
            NarrativeDescription = ConversationManager?.State?.CurrentNarrative ?? ""
        };
        
        await OnConversationCompleted.InvokeAsync(outcome);
    }

}