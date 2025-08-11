using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.ViewModels;
using Wayfarer.GameState.Interactions;
using Wayfarer.Models;
using Wayfarer.GameState;

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
    protected Dictionary<ConnectionType, int> NpcTokens { get; set; }
    
    // Transparent mechanics properties
    protected EmotionalState? CurrentEmotionalState { get; set; }
    protected StakeType? CurrentStakes { get; set; }
    protected TimeSpan? TimeToDeadline { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await LoadConversation();
        RefreshAttentionState();
        LoadTokenData();
        LoadEmotionalStateData();
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
        // SelectChoice already updates the Model and calls StateHasChanged
        // Now load the fresh token data AFTER the choice has been fully processed
        LoadTokenData(); // Load fresh token data after effects applied
        RefreshAttentionState(); // Update attention state
        StateHasChanged(); // Ensure UI refreshes with new data
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
                LoadTokenData(); // Reload tokens after processing choice
                RefreshAttentionState(); // Update attention 
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
    
    protected async Task HandleExitConversation()
    {
        try
        {
            Console.WriteLine($"[ConversationScreen] HandleExitConversation - manually exiting conversation");
            
            // End the conversation through GameFacade
            await GameFacade.EndConversationAsync();
            
            // Notify parent that conversation has ended
            OnConversationEnd?.Invoke();
            
            // Also trigger navigation if we have the callback
            if (OnNavigate.HasDelegate)
            {
                await OnNavigate.InvokeAsync(CurrentViews.LocationScreen);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exiting conversation: {ex.Message}");
        }
    }
    
    protected void LoadTokenData()
    {
        if (!string.IsNullOrEmpty(NpcId))
        {
            var tokenBalance = GameFacade.GetTokensWithNPC(NpcId);
            NpcTokens = new Dictionary<ConnectionType, int>();
            
            // Convert NPCTokenBalance to Dictionary
            if (tokenBalance != null && tokenBalance.Balances != null)
            {
                foreach (var balance in tokenBalance.Balances)
                {
                    NpcTokens[balance.TokenType] = balance.Amount;
                }
            }
            
            // Ensure all token types are represented
            foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
            {
                if (!NpcTokens.ContainsKey(tokenType))
                {
                    NpcTokens[tokenType] = 0;
                }
            }
        }
    }
    
    protected string GetCurrentConversationContext()
    {
        // Determine context from the current choices or conversation state
        if (UnifiedChoices != null && UnifiedChoices.Any())
        {
            // Check what types of choices are available
            var hasHelp = UnifiedChoices.Any(c => c.Type == InteractionType.ConversationHelp);
            var hasNegotiate = UnifiedChoices.Any(c => c.Type == InteractionType.ConversationNegotiate);
            var hasInvestigate = UnifiedChoices.Any(c => c.Type == InteractionType.ConversationInvestigate);
            
            // Return the most relevant context
            if (hasNegotiate) return "negotiate";
            if (hasInvestigate) return "investigate";
            if (hasHelp) return "help";
        }
        
        // Default based on conversation state (check CharacterState text)
        if (Model?.CharacterState?.Contains("desperate") == true || 
            Model?.CharacterState?.Contains("urgent") == true) return "help";
        if (Model?.CharacterState?.Contains("calculating") == true || 
            Model?.CharacterState?.Contains("thoughtful") == true) return "negotiate";
        
        return "help"; // Default context
    }
    
    protected string GetNpcRole()
    {
        // Determine NPC role from their ID or other context
        // This would ideally come from NPC data, but we can infer from name/ID
        if (string.IsNullOrEmpty(NpcId)) return null;
        
        var lowerNpcId = NpcId.ToLower();
        
        if (lowerNpcId.Contains("merchant") || lowerNpcId.Contains("trader") || lowerNpcId.Contains("shop"))
            return "merchant";
        if (lowerNpcId.Contains("noble") || lowerNpcId.Contains("lord") || lowerNpcId.Contains("lady"))
            return "noble";
        if (lowerNpcId.Contains("guard") || lowerNpcId.Contains("captain") || lowerNpcId.Contains("soldier"))
            return "guard";
        if (lowerNpcId.Contains("friend") || lowerNpcId == "elena" || lowerNpcId == "tam")
            return "friend";
        if (lowerNpcId.Contains("shadow") || lowerNpcId.Contains("informant"))
            return "informant";
            
        // Check NPC name if available
        if (Model?.NpcName != null)
        {
            var lowerName = Model.NpcName.ToLower();
            if (lowerName.Contains("merchant") || lowerName.Contains("trader"))
                return "merchant";
            if (lowerName.Contains("lord") || lowerName.Contains("lady"))
                return "noble";
            if (lowerName == "elena" || lowerName == "tam")
                return "friend";
        }
        
        return null; // Unknown role
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
                // Replace problematic Unicode in the description with Font Awesome icons
                var description = mechanic.Description
                    .Replace("⛓", "<i class='fas fa-link'></i>")
                    .Replace("♥", "<i class='fas fa-heart'></i>")
                    .Replace("⏱", "<i class='fas fa-clock'></i>")
                    .Replace("ℹ", "<i class='fas fa-info-circle'></i>")
                    .Replace("✓", "<i class='fas fa-check'></i>")
                    .Replace("⚠", "<i class='fas fa-exclamation-triangle'></i>")
                    .Replace("→", "<i class='fas fa-arrow-right'></i>")
                    .Replace("🪙", "<i class='fas fa-coins'></i>")
                    .Replace("🔍", "<i class='fas fa-search'></i>");
                    
                // Just add the description without icon - frontend will handle icon mapping
                previews.Add(description);
            }
        }
        return previews;
    }
    
    protected void LoadEmotionalStateData()
    {
        if (string.IsNullOrEmpty(NpcId)) return;
        
        // For now, derive emotional state from character description in Model
        // This would ideally come from NPC state tracking
        DeriveEmotionalStateFromModel();
        
        // Check the letter queue for any letters from this NPC
        var queue = GameFacade.GetLetterQueue();
        if (queue?.QueueSlots != null)
        {
            var npcLetterSlot = queue.QueueSlots.FirstOrDefault(s => s.Letter?.SenderName == NpcId);
            if (npcLetterSlot?.Letter != null)
            {
                var npcLetter = npcLetterSlot.Letter;
                // Infer stakes from the letter type/urgency
                CurrentStakes = DetermineStakesFromLetter(npcLetter);
                
                // Use the DeadlineInHours property directly
                TimeToDeadline = TimeSpan.FromHours(npcLetter.DeadlineInHours);
            }
        }
    }
    
    private void DeriveEmotionalStateFromModel()
    {
        if (Model?.CharacterState == null) return;
        
        var state = Model.CharacterState.ToLower();
        if (state.Contains("anxious") || state.Contains("worried") || state.Contains("nervous"))
            CurrentEmotionalState = EmotionalState.Anxious;
        else if (state.Contains("hostile") || state.Contains("angry") || state.Contains("furious"))
            CurrentEmotionalState = EmotionalState.Hostile;
        else if (state.Contains("closed") || state.Contains("withdrawn") || state.Contains("distant"))
            CurrentEmotionalState = EmotionalState.Closed;
        else
            CurrentEmotionalState = EmotionalState.Neutral; // Default
    }
    
    private StakeType DetermineStakesFromLetter(LetterViewModel letter)
    {
        // Infer stakes from letter properties
        // Since we don't have subject/content, use what we have
        
        // Check deadline urgency
        if (letter.DeadlineInHours < 4)
            return StakeType.SAFETY; // Very urgent = safety at stake
        
        // Check payment amount
        if (letter.Payment > 50)
            return StakeType.WEALTH; // High payment = financial stakes
        
        // Check if special letter
        if (letter.IsSpecial)
            return StakeType.SECRET; // Special letters often carry secrets
        
        // Default to reputation
        return StakeType.REPUTATION;
    }
    
    protected string GetStakesDescription()
    {
        if (CurrentStakes == null) return "No urgent letter";
        
        // Get the letter from the queue if it exists
        var queue = GameFacade.GetLetterQueue();
        var npcLetter = queue?.QueueSlots?.FirstOrDefault(s => s.Letter?.SenderName == NpcId)?.Letter;
        var subject = npcLetter?.SenderName ?? "matter";
        
        return CurrentStakes switch
        {
            StakeType.REPUTATION => $"Reputation at stake ({subject})",
            StakeType.WEALTH => $"Financial matter ({subject})",
            StakeType.SAFETY => $"Safety concerns ({subject})",
            StakeType.SECRET => $"Sensitive information ({subject})",
            _ => "Unknown stakes"
        };
    }
    
    protected TimeSpan? GetTimeRemaining()
    {
        return TimeToDeadline;
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