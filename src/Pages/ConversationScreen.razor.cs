using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.ViewModels;
using Wayfarer.GameState.Interactions;

public class ConversationScreenBase : ComponentBase
{
    [Inject] protected GameFacade GameFacade { get; set; }
    [Inject] protected NavigationManager Navigation { get; set; }
    
    [Parameter] public string NpcId { get; set; }
    [Parameter] public Action OnConversationEnd { get; set; }
    [Parameter] public EventCallback<CurrentViews> OnNavigate { get; set; }
    
    protected ConversationViewModel Model { get; set; }
    protected List<IInteractiveChoice> UnifiedChoices { get; set; }
    protected int CurrentAttention { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await LoadConversation();
        RefreshAttentionState();
    }
    
    protected async Task LoadConversation()
    {
        try
        {
            // First try to get the current pending conversation
            Model = GameFacade.GetCurrentConversation();
            
            // If no pending conversation, start a new one
            if (Model == null && !string.IsNullOrEmpty(NpcId))
            {
                Console.WriteLine($"[ConversationScreen] No pending conversation found, starting new conversation with NPC: {NpcId}");
                Model = await GameFacade.GetConversationAsync(NpcId);
            }
            
            if (Model == null)
            {
                Console.WriteLine($"[ConversationScreen] ERROR: Could not load conversation model");
            }
            else
            {
                Console.WriteLine($"[ConversationScreen] Loaded conversation with NPC: {Model.NpcId}, Text: {Model.CurrentText?.Substring(0, Math.Min(50, Model.CurrentText?.Length ?? 0))}...");
                GenerateUnifiedChoices();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading conversation: {ex.Message}");
        }
    }
    
    protected void RefreshAttentionState()
    {
        var (current, max, timeBlock) = GameFacade.GetCurrentAttentionState();
        CurrentAttention = current;
    }
    
    protected void GenerateUnifiedChoices()
    {
        UnifiedChoices = new List<IInteractiveChoice>();
        
        // Convert existing choices to unified format
        if (Model?.Choices != null)
        {
            foreach (var choice in Model.Choices)
            {
                UnifiedChoices.Add(new ConversationChoice
                {
                    Id = choice.Id,
                    DisplayText = choice.Text,
                    AttentionCost = choice.AttentionCost,
                    IsAvailable = choice.IsAvailable,
                    LockReason = choice.UnavailableReason,
                    Type = DetermineInteractionType(choice),
                    Style = DetermineChoiceStyle(choice),
                    MechanicalPreviews = GeneratePreviews(choice)
                });
            }
        }
    }
    
    protected async Task HandleUnifiedChoice(IInteractiveChoice choice)
    {
        await SelectChoice(choice.Id);
        RefreshAttentionState();
    }
    
    protected async Task SelectChoice(string choiceId)
    {
        try
        {
            Console.WriteLine($"[ConversationScreen] SelectChoice called with: {choiceId}");
            
            // Process the choice through GameFacade
            var updatedModel = await GameFacade.ProcessConversationChoice(choiceId);
            
            if (updatedModel != null)
            {
                Model = updatedModel;
                GenerateUnifiedChoices();
                StateHasChanged();
                Console.WriteLine($"[ConversationScreen] Conversation updated after choice");
            }
            else
            {
                Console.WriteLine($"[ConversationScreen] Conversation ended after choice");
                // Notify parent that conversation has ended
                OnConversationEnd?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error selecting choice: {ex.Message}");
        }
    }
    
    protected IInteractiveChoice GetDefaultChoice()
    {
        return new ConversationChoice
        {
            Id = "default",
            DisplayText = "\"I understand. I'll see what I can do.\"",
            AttentionCost = 0,
            IsAvailable = true,
            Type = InteractionType.ConversationFree,
            Style = InteractionStyle.Default,
            MechanicalPreviews = new List<string> { "→ Continue conversation" }
        };
    }
    
    private InteractionType DetermineInteractionType(ConversationChoiceViewModel choice)
    {
        if (choice.AttentionCost == 0) return InteractionType.ConversationFree;
        if (choice.Text.Contains("negotiate") || choice.Text.Contains("prioritize")) 
            return InteractionType.ConversationNegotiate;
        if (choice.Text.Contains("investigate") || choice.Text.Contains("tell me"))
            return InteractionType.ConversationInvestigate;
        return InteractionType.ConversationHelp;
    }
    
    private InteractionStyle DetermineChoiceStyle(ConversationChoiceViewModel choice)
    {
        if (choice.Text.Contains("swear") || choice.Text.Contains("promise"))
            return InteractionStyle.Urgent;
        if (choice.Mechanics?.Any(m => m.Type == MechanicEffectType.Positive) == true)
            return InteractionStyle.Beneficial;
        return InteractionStyle.Default;
    }
    
    private List<string> GeneratePreviews(ConversationChoiceViewModel choice)
    {
        var previews = new List<string>();
        if (choice.Mechanics != null)
        {
            foreach (var mechanic in choice.Mechanics)
            {
                previews.Add($"{mechanic.Icon} {mechanic.Description}");
            }
        }
        return previews;
    }
    
    // Helper class for conversion
    private class ConversationChoice : IInteractiveChoice
    {
        public string Id { get; set; }
        public string DisplayText { get; set; }
        public int AttentionCost { get; set; }
        public int TimeCostMinutes { get; set; } = 0; // Conversations don't cost time directly
        public InteractionType Type { get; set; }
        public bool IsAvailable { get; set; }
        public string LockReason { get; set; }
        public InteractionStyle Style { get; set; }
        public List<string> MechanicalPreviews { get; set; }
        
        public InteractionResult Execute(GameWorld gameWorld, AttentionManager attention)
        {
            // This is handled by the screen's SelectChoice method
            throw new NotImplementedException();
        }
    }
}