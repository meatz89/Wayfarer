using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Linq;

public class ConversationViewBase : ComponentBase
{
    [Inject] public IGameFacade GameFacade { get; set; }
    [Parameter] public EventCallback OnConversationCompleted { get; set; }
    [Parameter] public string NPCId { get; set; }

    // State
    public ConversationViewModel CurrentConversation { get; set; }
    
    // Streaming state - for now, disable streaming until we refactor it
    public bool IsStreaming => false;
    public string StreamingText => "";
    public int StreamProgress => 0;

    // Tooltip state
    public ConversationChoiceViewModel hoveredChoice;
    public bool showTooltip = false;
    public double tooltipX;
    public double tooltipY;

    protected override async Task OnInitializedAsync()
    {
        // Start the conversation if NPCId is provided
        if (!string.IsNullOrEmpty(NPCId))
        {
            CurrentConversation = await GameFacade.StartConversationAsync(NPCId);
        }
        else
        {
            // Get current active conversation
            CurrentConversation = GameFacade.GetCurrentConversation();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // Refresh conversation state
        if (!string.IsNullOrEmpty(NPCId))
        {
            CurrentConversation = await GameFacade.StartConversationAsync(NPCId);
        }
        else
        {
            CurrentConversation = GameFacade.GetCurrentConversation();
        }
    }

    public async Task MakeChoice(string choiceId)
    {
        HideTooltip();

        if (CurrentConversation?.Choices?.Any() == true)
        {
            var selectedChoice = CurrentConversation.Choices
                .FirstOrDefault(c => c.Id == choiceId);

            if (selectedChoice != null && selectedChoice.IsAvailable)
            {
                // Process the player's choice
                CurrentConversation = await GameFacade.ContinueConversationAsync(choiceId);

                // Check if conversation is complete
                if (CurrentConversation?.IsComplete == true)
                {
                    await OnConversationCompleted.InvokeAsync();
                }
                else
                {
                    StateHasChanged();
                }
            }
        }
    }

    public void ShowTooltip(MouseEventArgs e, ConversationChoiceViewModel choice)
    {
        hoveredChoice = choice;
        showTooltip = false;
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
        await OnConversationCompleted.InvokeAsync();
    }

    public string GetChoiceClass(ConversationChoiceViewModel choice)
    {
        // For now, return a default class. We can enhance this later
        // based on choice text patterns if needed
        return "choice-default";
    }

    public string GetChoiceIcon(ConversationChoiceViewModel choice)
    {
        // For now, return null. We can enhance this later
        // based on choice text patterns if needed
        return null;
    }

}