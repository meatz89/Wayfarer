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
    /// ADR-007: Store NPC object reference (not ID)
    /// </summary>
    public ExchangeSession CreateSession(NPC npc, List<ExchangeOption> availableExchanges)
    {
        // ADR-007: Find existing session by NPC object reference (not ID comparison)
        ExchangeSession existingSession = _activeSessions.FirstOrDefault(s => s.Npc == npc);
        if (existingSession != null)
        {
            EndSession(npc);
        }

        // ADR-007: Create session with object references (SessionId and NpcId deleted)
        ExchangeSession session = new ExchangeSession
        {
            Npc = npc,
            Location = npc.Location, // Capture location where exchange happening
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
    /// ADR-007: Accept NPC object (not ID)
    /// </summary>
    public void EndSession(NPC npc)
    {
        // ADR-007: Find session by NPC object reference (not ID)
        ExchangeSession session = _activeSessions.FirstOrDefault(s => s.Npc == npc);
        if (session != null)
        {
            session.IsActive = false;
            _activeSessions.Remove(session);

            // ADR-007: No ID lookup needed - session already has NPC object
            _messageSystem.AddSystemMessage(
                $"Exchange session with {session.Npc!.Name} ended",
                SystemMessageTypes.Info);
        }
    }

    /// <summary>
    /// Get an active session by NPC
    /// ADR-007: Accept NPC object (not ID)
    /// </summary>
    public ExchangeSession GetActiveSession(NPC npc)
    {
        // ADR-007: Find session by NPC object reference (not ID)
        return _activeSessions.FirstOrDefault(s => s.Npc == npc);
    }

    /// <summary>
    /// Check if an NPC has an active exchange session
    /// ADR-007: Accept NPC object (not ID)
    /// </summary>
    public bool HasActiveSession(NPC npc)
    {
        // ADR-007: Find session by NPC object reference (not ID)
        return _activeSessions.Any(s => s.Npc == npc);
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
                // HIGHLANDER: Pass NPC object directly, not npc.ID
                CheckRelationshipMilestone(npc, tokenReward.Type, tokenReward.Count);
            }
        }

        // Check for special exchange chains
        // HIGHLANDER: UnlocksExchange is object reference, not ID string
        if (exchange.UnlocksExchange != null)
        {
            // HIGHLANDER: Pass NPC and ExchangeCard objects directly
            UnlockExchange(npc, exchange.UnlocksExchange);
        }

        // Check for story triggers
        if (!string.IsNullOrEmpty(exchange.TriggerEvent))
        {
            TriggerStoryEvent(exchange.TriggerEvent);
        }
    }

    /// <summary>
    /// Clear all active sessions (used when loading game or changing locations)
    /// ADR-007: Work with NPC objects (not IDs)
    /// </summary>
    public void ClearAllSessions()
    {
        // ADR-007: Get NPC objects from sessions (not IDs)
        List<NPC> npcs = _activeSessions.Select(s => s.Npc).ToList();
        foreach (NPC npc in npcs)
        {
            EndSession(npc);
        }
    }

    // Helper methods

    /// <summary>
    /// Check relationship milestones after token rewards
    /// HIGHLANDER: Accepts NPC object, not string ID
    /// </summary>
    private void CheckRelationshipMilestone(NPC npc, ConnectionType tokenType, int amount)
    {
        // This would integrate with the Token subsystem to check milestones
    }

    /// <summary>
    /// Unlock new exchange for NPC
    /// HIGHLANDER: Accepts NPC and ExchangeCard objects, not IDs
    /// </summary>
    private void UnlockExchange(NPC npc, ExchangeCard exchange)
    {
        // This would integrate with ExchangeInventory to unlock new exchanges
        // Passes ExchangeCard object directly for inventory tracking
    }

    private void TriggerStoryEvent(string eventId)
    {
        // This would integrate with a story/event system
    }
}
