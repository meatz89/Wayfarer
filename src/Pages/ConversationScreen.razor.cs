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
    protected List<ConversationChoice> Choices { get; set; }
    protected int CurrentPatience { get; set; }
    protected int MaxPatience { get; set; }
    protected int CurrentComfort { get; set; }
    protected Dictionary<ConnectionType, int> NpcTokens { get; set; }
    protected ConversationChoice DefaultChoice => GetDefaultChoice();
    
    // Transparent mechanics properties
    protected NPCEmotionalState? CurrentEmotionalState { get; set; }
    protected StakeType? CurrentStakes { get; set; }
    protected TimeSpan? TimeToDeadline { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await LoadConversation();
        RefreshConversationState();
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
                await GenerateChoicesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading conversation: {ex.Message}");
        }
    }
    
    protected void RefreshConversationState()
    {
        // Get NPC patience and comfort for conversation UI display from GameFacade
        var conversationManager = GameFacade.GetCurrentConversationManager();
        var conversationState = conversationManager?.State;
        if (conversationState != null)
        {
            // Map FocusPoints to Patience for UI display (these represent NPC patience in conversation)
            CurrentPatience = conversationState.FocusPoints;
            MaxPatience = conversationState.MaxFocusPoints;
            // For now, use a simple comfort calculation based on emotional state
            CurrentComfort = Model?.EmotionalState switch
            {
                NPCEmotionalState.DESPERATE => -1,
                NPCEmotionalState.ANXIOUS => -1,
                NPCEmotionalState.CALCULATING => 0,
                NPCEmotionalState.WITHDRAWN => -2,
                NPCEmotionalState.HOSTILE => -2,
                _ => 0
            };
        }
        else
        {
            // Default values if conversation state not available
            CurrentPatience = 3;
            MaxPatience = 3;
            CurrentComfort = 0;
        }
    }
    
    protected async Task GenerateChoicesAsync()
    {
        // Use card-based choices exclusively - no fallbacks
        Choices = await GenerateCardBasedChoicesAsync();
    }
    
    protected async Task HandleChoice(ConversationChoice choice)
    {
        await SelectChoice(choice.ChoiceID);
        // SelectChoice already updates the Model and calls StateHasChanged
        // Now load the fresh token data AFTER the choice has been fully processed
        LoadTokenData(); // Load fresh token data after effects applied
        RefreshConversationState(); // Update patience and comfort state
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
                await GenerateChoicesAsync();
                LoadTokenData(); // Reload tokens after processing choice
                RefreshConversationState(); // Update conversation state 
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
    
    protected ConversationChoice GetDefaultChoice()
    {
        return new ConversationChoice
        {
            ChoiceID = "default",
            NarrativeText = "I understand. I'll see what I can do.",
            PatienceCost = 0,
            IsAvailable = true,
            MechanicalDescription = "→ Maintains current state",
            MechanicalEffects = new List<IMechanicalEffect>()
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
        // Determine context from NPC emotional state - no string matching
        if (CurrentEmotionalState.HasValue)
        {
            return CurrentEmotionalState.Value switch
            {
                NPCEmotionalState.DESPERATE => "help",      // Desperate NPCs need assistance
                NPCEmotionalState.ANXIOUS => "help",        // Anxious NPCs need comfort
                NPCEmotionalState.CALCULATING => "negotiate", // Calculating NPCs prefer deals
                NPCEmotionalState.HOSTILE => "negotiate",   // Hostile NPCs require negotiation
                NPCEmotionalState.WITHDRAWN => "investigate", // Withdrawn NPCs need probing
                _ => "help"
            };
        }
        
        // Fallback: Default context based on categorical mechanics
        return "help"; // Default context when no emotional state available
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
    
    
    protected void LoadEmotionalStateData()
    {
        if (Model == null) return;
        
        // Use the emotional state from the ViewModel (calculated by NPCStateResolver)
        if (Model.EmotionalState.HasValue)
        {
            CurrentEmotionalState = Model.EmotionalState.Value;
            CurrentStakes = Model.CurrentStakes;
            
            if (Model.HoursToDeadline.HasValue)
            {
                TimeToDeadline = TimeSpan.FromHours(Model.HoursToDeadline.Value);
            }
        }
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
    
    /// <summary>
    /// Generate enhanced conversation choices using the card deck system
    /// This integrates with the backend ConversationChoiceGenerator
    /// </summary>
    protected async Task<List<ConversationChoice>> GenerateCardBasedChoicesAsync()
    {
        try
        {
            // HIGHLANDER PRINCIPLE: Model.NpcId is the ONLY source of truth
            if (string.IsNullOrEmpty(Model?.NpcId))
            {
                throw new InvalidOperationException("Cannot generate conversation choices: Model.NpcId is required");
            }
            
            // Get real card-based choices from GameFacade
            var cardChoices = await GameFacade.GetConversationChoicesFromDeckAsync(Model.NpcId);
            
            if (cardChoices?.Any() == true)
            {
                Console.WriteLine($"[ConversationScreen] Generated {cardChoices.Count} card-based choices for {Model.NpcId}");
                return cardChoices;
            }
            else
            {
                throw new InvalidOperationException($"NPC deck failed to generate choices for {Model.NpcId} - conversation system error");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error generating card-based choices: {ex.Message}");
            throw; // Let the exception bubble up - no fallbacks allowed
        }
    }
    
    
    // UI Helper methods for conversation choices
    protected string GetCostDisplay(int cost)
    {
        // Return just the number - CSS will handle beautiful diamond icons
        return cost.ToString();
    }

    protected List<string> ParseMechanicalDescription(string description)
    {
        // Split by pipe (|) for multiple effects and add relationship impact previews
        return description.Split('|', StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim())
                         .Select(effect => AddRelationshipImpactPreview(effect))
                         .ToList();
    }

    /// <summary>
    /// Add relationship impact preview to mechanical effects
    /// Frontend UI translates mechanical effects to human relationship context
    /// </summary>
    protected string AddRelationshipImpactPreview(string mechanicalEffect)
    {
        var lower = mechanicalEffect.ToLowerInvariant();
        
        // Add human relationship context to token effects while preserving mechanical display
        if (lower.Contains("trust") && lower.Contains("+"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>({Model?.NpcName} will remember this kindness)</span>";
        }
        else if (lower.Contains("trust") && lower.Contains("-"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>({Model?.NpcName} will feel betrayed)</span>";
        }
        else if (lower.Contains("commerce") && lower.Contains("+"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(Improved business relationship)</span>";
        }
        else if (lower.Contains("commerce") && lower.Contains("-"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(You owe {Model?.NpcName} for this favor)</span>";
        }
        else if (lower.Contains("status") && lower.Contains("+"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(Enhanced social standing)</span>";
        }
        else if (lower.Contains("status") && lower.Contains("-"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(Loss of respect and standing)</span>";
        }
        else if (lower.Contains("shadow") && lower.Contains("+"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(Deeper into secretive circles)</span>";
        }
        else if (lower.Contains("shadow") && lower.Contains("-"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(Information networks distance themselves)</span>";
        }
        else if (lower.Contains("binding obligation"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(Creates ongoing commitment)</span>";
        }
        
        // Return original effect if no relationship context applies
        return mechanicalEffect;
    }

    protected string GetEffectIcon(string description)
    {
        // Use beautiful CSS-based icons instead of ugly Unicode symbols
        var lowerDesc = description.ToLowerInvariant();
        
        // Positive outcomes - return CSS class for green plus icon
        if (lowerDesc.Contains("gain") || lowerDesc.Contains("unlock") || 
            lowerDesc.Contains("open") || lowerDesc.Contains("+") ||
            lowerDesc.Contains("comfort") || lowerDesc.Contains("trust"))
            return "icon icon-positive";
            
        // Negative/warning outcomes - return CSS class for red minus icon
        if (lowerDesc.Contains("lose") || lowerDesc.Contains("burn") || 
            lowerDesc.Contains("cost") || lowerDesc.Contains("obligation") ||
            lowerDesc.Contains("binding") || lowerDesc.Contains("-"))
            return "icon icon-negative";
            
        // Arrow/progression - return CSS class for arrow
        if (lowerDesc.Contains("maintain") || lowerDesc.Contains("negotiation"))
            return "icon icon-arrow";
            
        // Informational/neutral - return CSS class for blue info icon
        return "icon icon-neutral";
    }

    protected string GetMechanicClass(string preview)
    {
        // Style effects based on their type - matching mockup colors
        var lowerPreview = preview.ToLowerInvariant();
        
        // Positive effects (green)
        if (preview.Contains("✓") || lowerPreview.Contains("gain") || 
            lowerPreview.Contains("opens") || lowerPreview.Contains("+") ||
            lowerPreview.Contains("unlock"))
            return "mechanic-positive";
            
        // Negative effects (red)
        if (preview.Contains("⚠") || lowerPreview.Contains("lose") || 
            lowerPreview.Contains("burn") || lowerPreview.Contains("cost") ||
            lowerPreview.Contains("-") || lowerPreview.Contains("obligation") ||
            lowerPreview.Contains("binding"))
            return "mechanic-negative";
            
        // Informational effects (blue)
        if (preview.Contains("ℹ") || preview.Contains("→") || 
            lowerPreview.Contains("learn") || lowerPreview.Contains("maintain") ||
            lowerPreview.Contains("time") || lowerPreview.Contains("minute") ||
            lowerPreview.Contains("investigate") || lowerPreview.Contains("discover"))
            return "mechanic-neutral";
            
        return "mechanic-neutral";
    }
    
    protected string GetChoiceClass(ConversationChoice choice)
    {
        var classes = new List<string> { "choice-option" };
        
        if (!choice.IsAvailable || choice.PatienceCost > CurrentPatience)
        {
            classes.Add("locked");
        }
        
        return string.Join(" ", classes);
    }
    
    protected string GetPatienceOrbLabel(int index)
    {
        return index < CurrentPatience 
            ? $"Patience point {index + 1} available" 
            : $"Patience point {index + 1} spent";
    }
    
    protected string GetComfortClass()
    {
        return CurrentComfort switch
        {
            <= -2 => "comfort-hostile",
            -1 => "comfort-uncomfortable", 
            0 => "comfort-neutral",
            1 => "comfort-at-ease",
            _ => "comfort-relaxed"  // 2 and above
        };
    }
    
    protected string GetComfortDescription()
    {
        return CurrentComfort switch
        {
            <= -2 => "Hostile",
            -1 => "Uncomfortable", 
            0 => "Neutral",
            1 => "At Ease",
            _ => "Relaxed"  // 2 and above
        };
    }
}