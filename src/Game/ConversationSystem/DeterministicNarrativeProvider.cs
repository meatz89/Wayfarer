using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Deterministic narrative provider for non-AI conversations.
/// Used for action conversations with pre-defined choices and outcomes.
/// </summary>
public class DeterministicNarrativeProvider : INarrativeProvider
{
    private readonly GameWorld _gameWorld;
    
    public DeterministicNarrativeProvider(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }
    
    public Task<string> GenerateIntroduction(ConversationContext context, ConversationState state)
    {
        // Look for initial narrative in context
        if (context is ActionConversationContext actionContext && !string.IsNullOrEmpty(actionContext.InitialNarrative))
        {
            return Task.FromResult(actionContext.InitialNarrative);
        }
        
        // Simple one-sentence narratives for thin layer
        var introduction = context.ConversationTopic switch
        {
            "Action_GatherResources" => "You carefully search the area for edible berries.",
            "Action_Browse" => "You examine the market stalls, noting prices and goods.",
            "Action_Observe" => "You blend into the crowd, listening to local gossip.",
            "Action_Work" => $"You begin your work for {context.TargetNPC?.Name ?? "your employer"}.",
            "Action_Socialize" => $"You engage {context.TargetNPC?.Name ?? "someone"} in friendly conversation.",
            "Action_Converse" => $"You approach {context.TargetNPC?.Name ?? "someone"} to talk.",
            "Action_Deliver" => $"You hand the letter to {context.TargetNPC?.Name ?? "the recipient"}.",
            "Action_Trade" => $"You browse {context.TargetNPC?.Name ?? "the merchant"}'s wares.",
            "Action_Rest" => "You find a comfortable spot to rest.",
            _ => "You proceed with your action."
        };
        
        return Task.FromResult(introduction);
    }
    
    public Task<List<ConversationChoice>> GenerateChoices(
        ConversationContext context, 
        ConversationState state,
        List<ChoiceTemplate> availableTemplates)
    {
        // For thin narrative layer, always return a single "Continue" button
        var choices = new List<ConversationChoice>
        {
            new ConversationChoice
            {
                ChoiceID = "1",
                NarrativeText = "Continue",
                FocusCost = 0,
                IsAffordable = true,
                TemplateUsed = "Continue",
                TemplatePurpose = "Proceed with the action"
            }
        };
        
        return Task.FromResult(choices);
    }
    
    public Task<string> GenerateReaction(
        ConversationContext context,
        ConversationState state,
        ConversationChoice selectedChoice,
        bool success)
    {
        // For thin narrative layer, no reaction needed - action will execute immediately
        // Return empty string to signal completion
        return Task.FromResult("");
    }
    
    public Task<string> GenerateConclusion(
        ConversationContext context,
        ConversationState state,
        ConversationChoice lastChoice)
    {
        // For thin narrative layer, no conclusion needed - action completes immediately
        return Task.FromResult("");
    }
    
    public Task<bool> IsAvailable()
    {
        // Deterministic provider is always available
        return Task.FromResult(true);
    }
    
    private string GetNarrativeTextForTemplate(ChoiceTemplate template, ConversationContext context)
    {
        // Generate appropriate narrative text based on template name
        return template.TemplateName switch
        {
            "GatherSafely" => "Take time to gather carefully",
            "GatherRiskily" => "Rush to gather more quickly",
            "WorkHard" => "Put in maximum effort (2 stamina)",
            "WorkLight" => "Do lighter tasks (1 stamina)",
            "DeliverForTokens" => "Accept token payment",
            "DeliverForCoins" => "Request coin payment",
            "SocializePersonal" => "Discuss personal matters",
            "SocializeBusiness" => "Talk about business",
            _ => template.TemplatePurpose ?? "Continue"
        };
    }
    
    private List<ConversationChoice> GenerateDefaultChoices(ConversationContext context, ConversationState state)
    {
        // Provide simple default choice to continue
        return new List<ConversationChoice>
        {
            new ConversationChoice
            {
                ChoiceID = "1",
                NarrativeText = "Continue",
                FocusCost = 0,
                IsAffordable = true,
                TemplateUsed = "Default",
                TemplatePurpose = "Continue the conversation"
            }
        };
    }
}

/// <summary>
/// Extended conversation context for action-based conversations
/// </summary>
public class ActionConversationContext : ConversationContext
{
    public ActionOption SourceAction { get; set; }
    public string InitialNarrative { get; set; }
    public List<ChoiceTemplate> AvailableTemplates { get; set; }
}