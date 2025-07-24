using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// STUB: Provides deterministic narrative content for conversations.
/// TODO: Implement full narrative provider system
/// </summary>
public class DeterministicNarrativeProvider : INarrativeProvider
{
    public DeterministicNarrativeProvider()
    {
        // STUB: Constructor
    }
    
    public Task<string> GenerateIntroduction(ConversationContext context)
    {
        // STUB: Return basic introduction
        return Task.FromResult("You begin a conversation.");
    }

    public Task<string> GenerateIntroduction(ConversationContext context, ConversationState state)
    {
        // STUB: Return basic introduction
        return Task.FromResult("You begin a conversation.");
    }

    public Task<List<ConversationChoice>> GenerateChoices(ConversationContext context, ConversationState state, List<ChoiceTemplate> availableTemplates)
    {
        // STUB: Return empty choices
        return Task.FromResult(new List<ConversationChoice>());
    }

    public Task<string> GenerateReaction(ConversationContext context, ConversationState state, ConversationChoice selectedChoice, bool success)
    {
        // STUB: Return basic reaction
        return Task.FromResult("They respond to your choice.");
    }

    public Task<string> GenerateConclusion(ConversationContext context, ConversationState state, ConversationChoice lastChoice)
    {
        // STUB: Return basic conclusion
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