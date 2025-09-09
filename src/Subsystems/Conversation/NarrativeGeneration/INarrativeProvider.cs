/// <summary>
/// Interface for narrative content generation providers.
/// Implements the backwards construction principle - analyzing available cards
/// to generate NPC dialogue that all options can meaningfully respond to.
/// </summary>
public interface INarrativeProvider
{
    /// <summary>
    /// Generates narrative content based on current conversation state.
    /// Uses backwards construction: examines active cards to create NPC dialogue
    /// that all cards can respond to meaningfully.
    /// </summary>
    /// <param name="state">Current mechanical state of the conversation</param>
    /// <param name="npcData">Simplified NPC information for narrative generation</param>
    /// <param name="activeCards">Cards currently available for the player to use</param>
    /// <returns>Generated narrative content including NPC dialogue and card narratives</returns>
    NarrativeOutput GenerateNarrativeContent(
        ConversationState state,
        NPCData npcData,
        CardCollection activeCards);
    
    /// <summary>
    /// Checks if this provider is currently available for use.
    /// PRINCIPLE: Always use async/await for I/O operations. Never block async code
    /// with .Wait() or .Result as it causes deadlocks in ASP.NET Core.
    /// </summary>
    /// <returns>True if provider can generate content, false otherwise</returns>
    Task<bool> IsAvailableAsync();
    
    /// <summary>
    /// Gets the display name of this provider for debugging and logging.
    /// </summary>
    /// <returns>Human-readable provider name</returns>
    string GetProviderName();
}