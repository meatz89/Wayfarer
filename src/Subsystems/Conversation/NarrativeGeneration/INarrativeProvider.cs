using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Interface for narrative content generation providers.
/// Implements the backwards construction principle - analyzing available cards
/// to generate NPC dialogue that all options can meaningfully respond to.
/// Now split into two phases for progressive UI updates.
/// </summary>
public interface INarrativeProvider
{
    /// <summary>
    /// Phase 1: Generates NPC dialogue and environmental narrative only.
    /// Analyzes active cards to create NPC dialogue that all cards can respond to.
    /// Returns immediately after NPC dialogue generation for quick UI update.
    /// </summary>
    /// <param name="state">Current mechanical state of the conversation</param>
    /// <param name="npcData">Simplified NPC information for narrative generation</param>
    /// <param name="activeCards">Cards currently available for the player to use</param>
    /// <returns>NarrativeOutput with NPCDialogue/NarrativeText filled, CardNarratives empty</returns>
    Task<NarrativeOutput> GenerateNPCDialogueAsync(
        ConversationState state,
        NPCData npcData,
        CardCollection activeCards);
    
    /// <summary>
    /// Phase 2: Generates card-specific narratives based on NPC dialogue.
    /// Uses the NPC dialogue from Phase 1 to create contextually appropriate card responses.
    /// Called separately after UI has been updated with NPC dialogue.
    /// </summary>
    /// <param name="state">Current mechanical state of the conversation</param>
    /// <param name="npcData">Simplified NPC information for narrative generation</param>
    /// <param name="activeCards">Cards currently available for the player to use</param>
    /// <param name="npcDialogue">The NPC dialogue generated in Phase 1</param>
    /// <returns>List of card narratives with their IDs and text</returns>
    Task<List<CardNarrative>> GenerateCardNarrativesAsync(
        ConversationState state,
        NPCData npcData,
        CardCollection activeCards,
        string npcDialogue);
    
    /// <summary>
    /// Checks if this provider is currently available for use.
    /// PRINCIPLE: Always use async/await for I/O operations. Never block async code
    /// with .Wait() or .Result as it causes deadlocks in ASP.NET Core.
    /// </summary>
    /// <returns>True if provider can generate content, false otherwise</returns>
    Task<bool> IsAvailableAsync();
    
    /// <summary>
    /// Gets the provider type for identifying this provider.
    /// </summary>
    /// <returns>Provider type enum value</returns>
    NarrativeProviderType GetProviderType();
}