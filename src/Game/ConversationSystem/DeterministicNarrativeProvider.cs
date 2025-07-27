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
            var narrativeDialogue = _narrativeManager.GetNarrativeDialogue(context.TargetNPC.ID);
            if (!string.IsNullOrEmpty(narrativeDialogue))
            {
                return Task.FromResult(narrativeDialogue);
            }
        }

        // Default introduction
        return Task.FromResult("You begin a conversation.");
    }

    public Task<List<ConversationChoice>> GenerateChoices(ConversationContext context, ConversationState state, List<ChoiceTemplate> availableTemplates)
    {
        // STUB: Return empty choices
        return Task.FromResult(new List<ConversationChoice>());
    }

    public Task<string> GenerateReaction(ConversationContext context, ConversationState state, ConversationChoice selectedChoice, bool success)
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