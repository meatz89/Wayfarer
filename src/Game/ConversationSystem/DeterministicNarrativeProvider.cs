using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Provides deterministic narrative content for conversations with narrative override support.
/// </summary>
public class DeterministicNarrativeProvider : INarrativeProvider
{
    private readonly NarrativeManager _narrativeManager;

    public DeterministicNarrativeProvider(NarrativeManager narrativeManager)
    {
        _narrativeManager = narrativeManager;
    }

    public Task<string> GenerateIntroduction(ConversationContext context)
    {
        // Check for narrative dialogue override
        if (context.TargetNPC != null)
        {
            var narrativeDialogue = _narrativeManager.GetNarrativeDialogue(context.TargetNPC.ID);
            if (!string.IsNullOrEmpty(narrativeDialogue))
            {
                return Task.FromResult(narrativeDialogue);
            }
        }

        // Default introduction
        return Task.FromResult("You begin a conversation.");
    }

    public Task<string> GenerateIntroduction(ConversationContext context, ConversationState state)
    {
        // Check for narrative dialogue override
        if (context.TargetNPC != null)
        {
            Console.WriteLine($"[DeterministicNarrativeProvider] Checking dialogue for NPC: {context.TargetNPC.ID}");
            var narrativeDialogue = _narrativeManager.GetNarrativeDialogue(context.TargetNPC.ID);
            Console.WriteLine($"[DeterministicNarrativeProvider] Got dialogue: {narrativeDialogue ?? "null"}");
            if (!string.IsNullOrEmpty(narrativeDialogue))
            {
                return Task.FromResult(narrativeDialogue);
            }
        }

        // Default introduction
        Console.WriteLine("[DeterministicNarrativeProvider] Using default introduction");
        return Task.FromResult("You begin a conversation.");
    }

    public Task<List<ConversationChoice>> GenerateChoices(ConversationContext context, ConversationState state, List<ChoiceTemplate> availableTemplates)
    {
        var choices = new List<ConversationChoice>();
        
        // Check if we have a narrative dialogue override - if so, provide a simple continue option
        if (context.TargetNPC != null)
        {
            var narrativeDialogue = _narrativeManager.GetNarrativeDialogue(context.TargetNPC.ID);
            if (!string.IsNullOrEmpty(narrativeDialogue))
            {
                // For tutorial/narrative dialogues, provide a simple continue option
                choices.Add(new ConversationChoice
                {
                    ChoiceID = "continue",
                    NarrativeText = "Continue",
                    FocusCost = 0,
                    IsAffordable = true
                });
                return Task.FromResult(choices);
            }
        }
        
        // If we have available templates, use them
        if (availableTemplates != null && availableTemplates.Count > 0)
        {
            foreach (var template in availableTemplates)
            {
                choices.Add(new ConversationChoice
                {
                    ChoiceID = template.TemplateName,
                    NarrativeText = template.Description ?? template.TemplateName,
                    FocusCost = template.FocusCost,
                    IsAffordable = true // In tutorial, choices are always affordable
                });
            }
        }
        
        // If no choices available, provide a default end conversation option
        if (choices.Count == 0)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "end",
                NarrativeText = "End conversation",
                FocusCost = 0,
                IsAffordable = true
            });
        }
        
        return Task.FromResult(choices);
    }

    public Task<string> GenerateReaction(ConversationContext context, ConversationState state, ConversationChoice selectedChoice, bool success)
    {
        // Special handling for "continue" choice in tutorial dialogues
        if (selectedChoice.ChoiceID == "continue")
        {
            // Mark conversation as complete after the continue choice
            state.IsConversationComplete = true;
            return Task.FromResult(""); // Empty response since conversation is ending
        }
        
        // Check for narrative dialogue override
        if (context.TargetNPC != null)
        {
            var narrativeDialogue = _narrativeManager.GetNarrativeDialogue(context.TargetNPC.ID);
            if (!string.IsNullOrEmpty(narrativeDialogue))
            {
                return Task.FromResult(narrativeDialogue);
            }
        }

        // Default reaction
        return Task.FromResult("They respond to your choice.");
    }

    public Task<string> GenerateConclusion(ConversationContext context, ConversationState state, ConversationChoice lastChoice)
    {
        // Check for narrative dialogue override
        if (context.TargetNPC != null)
        {
            var narrativeDialogue = _narrativeManager.GetNarrativeDialogue(context.TargetNPC.ID);
            if (!string.IsNullOrEmpty(narrativeDialogue))
            {
                return Task.FromResult(narrativeDialogue);
            }
        }

        // Default conclusion
        return Task.FromResult("The conversation ends.");
    }

    public Task<bool> IsAvailable()
    {
        // STUB: Always available
        return Task.FromResult(true);
    }
}

/// <summary>
/// STUB: Conversation context for command-based interactions
/// </summary>
public class CommandConversationContext : ConversationContext
{
    public string CommandType { get; set; }
    public IGameCommand Command { get; set; }
}