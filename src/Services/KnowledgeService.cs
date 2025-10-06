using System;
using System.Linq;

/// <summary>
/// Knowledge Service - Domain Service for knowledge management
/// Operates on GameWorld, doesn't store state
/// Follows ARCHITECTURE.md principles: services operate on GameWorld single source of truth
/// </summary>
public class KnowledgeService
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;

    public KnowledgeService(GameWorld gameWorld, MessageSystem messageSystem)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }

    /// <summary>
    /// Grant knowledge to player
    /// Adds knowledge to player's knowledge collection if not already known
    /// Triggers discovery evaluation for ConversationalDiscovery unlocks
    /// </summary>
    public void GrantKnowledge(string knowledgeId)
    {
        if (string.IsNullOrEmpty(knowledgeId))
            return;

        Player player = _gameWorld.GetPlayer();

        // Check if already known
        if (player.Knowledge.HasKnowledge(knowledgeId))
        {
            Console.WriteLine($"[KnowledgeService] Player already has knowledge '{knowledgeId}'");
            return;
        }

        // Grant knowledge
        player.Knowledge.AddKnowledge(knowledgeId);

        // Get knowledge display info from GameWorld
        Knowledge knowledgeData = _gameWorld.Knowledge.GetValueOrDefault(knowledgeId);
        string displayName = knowledgeData?.DisplayName ?? knowledgeId;

        _messageSystem.AddSystemMessage(
            $"Knowledge gained: {displayName}",
            SystemMessageTypes.Info);

        Console.WriteLine($"[KnowledgeService] Granted knowledge '{knowledgeId}' to player");

        // Knowledge may unlock investigation discovery (ConversationalDiscovery trigger)
        // Discovery evaluation happens externally in GameFacade after knowledge granted
    }

    /// <summary>
    /// Check if player has specific knowledge
    /// </summary>
    public bool HasKnowledge(string knowledgeId)
    {
        return _gameWorld.GetPlayer().Knowledge.HasKnowledge(knowledgeId);
    }

    /// <summary>
    /// Get all knowledge IDs player has acquired
    /// </summary>
    public System.Collections.Generic.List<string> GetAllKnowledge()
    {
        return _gameWorld.GetPlayer().Knowledge.GetAllKnowledge();
    }
}
