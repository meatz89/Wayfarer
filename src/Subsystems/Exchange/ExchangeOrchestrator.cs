/// <summary>
/// Orchestrates exchange sessions and coordinates exchange operations.
/// Internal to the Exchange subsystem - not exposed publicly.
/// </summary>
public class ExchangeOrchestrator
{
    private readonly GameWorld _gameWorld;
    private readonly ExchangeValidator _validator;
    private readonly ExchangeProcessor _processor;
    private readonly MessageSystem _messageSystem;

    private List<ExchangeSession> _activeSessions;

    public ExchangeOrchestrator(
        GameWorld gameWorld,
        ExchangeValidator validator,
        ExchangeProcessor processor,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _activeSessions = new List<ExchangeSession>();
    }

    /// <summary>
    /// Create a new exchange session with an NPC
    /// </summary>
    public ExchangeSession CreateSession(NPC npc, List<ExchangeOption> availableExchanges)
    {
        // End any existing session with this NPC
        ExchangeSession existingSession = _activeSessions.FirstOrDefault(s => s.NpcId == npc.ID);
        if (existingSession != null)
        {
            EndSession(npc.ID);
        }

        // Create new session
        ExchangeSession session = new ExchangeSession
        {
            SessionId = Guid.NewGuid().ToString(),
            NpcId = npc.ID,
            AvailableExchanges = availableExchanges,
            StartTime = DateTime.Now,
            IsActive = true
        };

        _activeSessions.Add(session);

        _messageSystem.AddSystemMessage(
            $"Exchange session started with {npc.Name}",
            SystemMessageTypes.Info);

        return session;
    }

    /// <summary>
    /// End an exchange session
    /// </summary>
    public void EndSession(string npcId)
    {
        ExchangeSession session = _activeSessions.FirstOrDefault(s => s.NpcId == npcId);
        if (session != null)
        {
            session.IsActive = false;
            _activeSessions.Remove(session);

            NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == session.NpcId);
            string npcName = npc != null ? npc.Name : "NPC";
            _messageSystem.AddSystemMessage(
                $"Exchange session with {npcName} ended",
                SystemMessageTypes.Info);
        }
    }

    /// <summary>
    /// Get an active session by NPC ID
    /// </summary>
    public ExchangeSession GetActiveSession(string npcId)
    {
        return _activeSessions.FirstOrDefault(s => s.NpcId == npcId);
    }

    /// <summary>
    /// Check if an NPC has an active exchange session
    /// </summary>
    public bool HasActiveSession(string npcId)
    {
        return _activeSessions.Any(s => s.NpcId == npcId);
    }

    /// <summary>
    /// Check if exchange should trigger special events
    /// </summary>
    public void CheckExchangeTriggers(ExchangeCard exchange, NPC npc)
    {
        // Check for relationship milestones from token rewards
        if (exchange.Reward != null && exchange.Reward.Tokens != null)
        {
            foreach (TokenCount tokenReward in exchange.Reward.Tokens)
            {
                CheckRelationshipMilestone(npc.ID, tokenReward.Type, tokenReward.Count);
            }
        }

        // Check for special exchange chains
        if (!string.IsNullOrEmpty(exchange.UnlocksExchangeId))
        {
            UnlockExchange(npc.ID, exchange.UnlocksExchangeId);
        }

        // Check for story triggers
        if (!string.IsNullOrEmpty(exchange.TriggerEvent))
        {
            TriggerStoryEvent(exchange.TriggerEvent);
        }
    }

    /// <summary>
    /// Clear all active sessions (used when loading game or changing locations)
    /// </summary>
    public void ClearAllSessions()
    {
        List<string> npcIds = _activeSessions.Select(s => s.NpcId).ToList();
        foreach (string npcId in npcIds)
        {
            EndSession(npcId);
        }
    }

    // Helper methods

    private void CheckRelationshipMilestone(string npcId, ConnectionType tokenType, int amount)
    {
        // This would integrate with the Token subsystem to check milestones
    }

    private void UnlockExchange(string npcId, string exchangeId)
    {
        // This would integrate with ExchangeInventory to unlock new exchanges
    }

    private void TriggerStoryEvent(string eventId)
    {
        // This would integrate with a story/event system
    }
}
