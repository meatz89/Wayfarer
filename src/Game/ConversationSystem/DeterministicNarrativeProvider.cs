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
        
        // Default introduction based on conversation topic
        var introduction = context.ConversationTopic switch
        {
            "Action_GatherResources" => "You survey the area for resources to gather.",
            "Action_Work" => $"{context.TargetNPC?.Name ?? "Someone"} has work available.",
            "Action_Socialize" => $"You approach {context.TargetNPC?.Name ?? "someone"} for a conversation.",
            "Action_Deliver" => $"You prepare to deliver the letter to {context.TargetNPC?.Name ?? "the recipient"}.",
            _ => "You consider your options."
        };
        
        return Task.FromResult(introduction);
    }
    
    public Task<List<ConversationChoice>> GenerateChoices(
        ConversationContext context, 
        ConversationState state,
        List<ChoiceTemplate> availableTemplates)
    {
        var choices = new List<ConversationChoice>();
        
        // Convert templates to choices
        for (int i = 0; i < availableTemplates.Count; i++)
        {
            var template = availableTemplates[i];
            var choice = new ConversationChoice
            {
                ChoiceID = (i + 1).ToString(),
                NarrativeText = GetNarrativeTextForTemplate(template, context),
                FocusCost = template.InputMechanics?.FocusCost?.Amount ?? 0,
                IsAffordable = state.FocusPoints >= (template.InputMechanics?.FocusCost?.Amount ?? 0),
                TemplateUsed = template.TemplateName,
                TemplatePurpose = template.TemplatePurpose,
                RequiresSkillCheck = template.InputMechanics?.SkillCheckRequirement != null
            };
            
            choices.Add(choice);
        }
        
        // If no templates provided, generate default choices based on context
        if (choices.Count == 0)
        {
            choices = GenerateDefaultChoices(context, state);
        }
        
        return Task.FromResult(choices);
    }
    
    public Task<string> GenerateReaction(
        ConversationContext context,
        ConversationState state,
        ConversationChoice selectedChoice,
        bool success)
    {
        // Look for specific reaction in choice
        var reaction = success ? selectedChoice.SuccessNarrative : selectedChoice.FailureNarrative;
        
        if (!string.IsNullOrEmpty(reaction))
        {
            return Task.FromResult(reaction);
        }
        
        // Generate reaction based on template
        reaction = selectedChoice.TemplateUsed switch
        {
            "GatherSafely" => success ? "You carefully gather resources without incident." : "You struggle to find anything useful.",
            "GatherRiskily" => success ? "Your bold approach pays off with extra resources!" : "Your haste causes you to damage what you were gathering.",
            "WorkHard" => success ? "You put in solid effort and earn your full wages." : "The work proves too demanding.",
            "WorkLight" => success ? "You complete the light tasks efficiently." : "Even the simple work proves challenging today.",
            _ => success ? "You succeed in your action." : "Things don't go as planned."
        };
        
        return Task.FromResult(reaction);
    }
    
    public Task<string> GenerateConclusion(
        ConversationContext context,
        ConversationState state,
        ConversationChoice lastChoice)
    {
        var conclusion = context.ConversationTopic switch
        {
            "Action_GatherResources" => "You finish your gathering and prepare to move on.",
            "Action_Work" => "The work is complete and you collect your payment.",
            "Action_Socialize" => "The conversation comes to a natural end.",
            "Action_Deliver" => "The delivery is complete.",
            _ => "You've accomplished what you set out to do."
        };
        
        return Task.FromResult(conclusion);
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