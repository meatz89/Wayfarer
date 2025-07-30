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
        Console.WriteLine($"[ConversationView.OnInitializedAsync] Called with NPCId: '{NPCId}'");
        
        // Always get the current conversation from facade first
        // The conversation should already be started by ConverseCommand
        Console.WriteLine($"[ConversationView.OnInitializedAsync] Getting current conversation from facade");
        CurrentConversation = GameFacade.GetCurrentConversation();
        Console.WriteLine($"[ConversationView.OnInitializedAsync] GetCurrentConversation returned: {CurrentConversation?.CurrentText ?? "null"}");
        
        // Only start a new conversation if there's no current one and we have an NPCId
        if (CurrentConversation == null && !string.IsNullOrEmpty(NPCId))
        {
            Console.WriteLine($"[ConversationView.OnInitializedAsync] No current conversation, starting new one with NPC: {NPCId}");
            CurrentConversation = await GameFacade.StartConversationAsync(NPCId);
            Console.WriteLine($"[ConversationView.OnInitializedAsync] StartConversationAsync returned: {CurrentConversation?.CurrentText ?? "null"}");
        }
        
        Console.WriteLine($"[ConversationView.OnInitializedAsync] CurrentConversation null? {CurrentConversation == null}");
        if (CurrentConversation != null)
        {
            Console.WriteLine($"[ConversationView.OnInitializedAsync] CurrentConversation.IsComplete: {CurrentConversation.IsComplete}");
            Console.WriteLine($"[ConversationView.OnInitializedAsync] CurrentConversation.Choices count: {CurrentConversation.Choices?.Count ?? 0}");
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        Console.WriteLine($"[ConversationView.OnParametersSetAsync] Called with NPCId: '{NPCId}'");
        
        // Always get the current conversation from facade first
        // The conversation should already be started by ConverseCommand
        Console.WriteLine($"[ConversationView.OnParametersSetAsync] Getting current conversation from facade");
        CurrentConversation = GameFacade.GetCurrentConversation();
        Console.WriteLine($"[ConversationView.OnParametersSetAsync] GetCurrentConversation returned: {CurrentConversation?.CurrentText ?? "null"}");
        
        // Only start a new conversation if there's no current one and we have an NPCId
        if (CurrentConversation == null && !string.IsNullOrEmpty(NPCId))
        {
            Console.WriteLine($"[ConversationView.OnParametersSetAsync] No current conversation, starting new one with NPC: {NPCId}");
            CurrentConversation = await GameFacade.StartConversationAsync(NPCId);
            Console.WriteLine($"[ConversationView.OnParametersSetAsync] StartConversationAsync returned: {CurrentConversation?.CurrentText ?? "null"}");
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