using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Interface for providing narrative content for conversations.
/// Implementations can be AI-driven or deterministic.
/// </summary>
public interface INarrativeProvider
{
    /// <summary>
    /// Generate the introduction narrative for a conversation
    /// </summary>
    Task<string> GenerateIntroduction(ConversationContext context, ConversationState state);

    /// <summary>
    /// Generate choices available to the player at this conversation beat
    /// </summary>
    Task<List<ConversationChoice>> GenerateChoices(
        ConversationContext context,
        ConversationState state,
        List<ChoiceTemplate> availableTemplates);

    /// <summary>
    /// Generate the reaction narrative based on player's choice
    /// </summary>
    Task<string> GenerateReaction(
        ConversationContext context,
        ConversationState state,
        ConversationChoice selectedChoice,
        bool success);

    /// <summary>
    /// Generate the conclusion narrative for the conversation
    /// </summary>
    Task<string> GenerateConclusion(
        ConversationContext context,
        ConversationState state,
        ConversationChoice lastChoice);

    /// <summary>
    /// Check if the provider is available (for AI providers that might have rate limits)
    /// </summary>
    Task<bool> IsAvailable();
}