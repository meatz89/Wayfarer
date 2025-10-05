using System;
using System.Threading.Tasks;

/// <summary>
/// Strategic orchestrator for investigations - NOT a fourth tactical system
/// Spawns tactical sessions (Social, Mental, Physical) across multiple phases
/// </summary>
public class InvestigationActivity
{
    private readonly GameWorld _gameWorld;
    private readonly ConversationFacade _conversationFacade;
    private readonly MentalFacade _mentalFacade;
    private readonly PhysicalFacade _physicalFacade;
    private readonly MessageSystem _messageSystem;

    // Current investigation state
    private InvestigationTemplate _investigation;
    private InvestigationProgress _progress;
    private string _currentSessionId;
    private string _locationId;  // Location where investigation is taking place

    public InvestigationActivity(
        GameWorld gameWorld,
        ConversationFacade conversationFacade,
        MentalFacade mentalFacade,
        PhysicalFacade physicalFacade,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _conversationFacade = conversationFacade ?? throw new ArgumentNullException(nameof(conversationFacade));
        _mentalFacade = mentalFacade ?? throw new ArgumentNullException(nameof(mentalFacade));
        _physicalFacade = physicalFacade ?? throw new ArgumentNullException(nameof(physicalFacade));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }

    /// <summary>
    /// Start investigation - begins first phase
    /// </summary>
    public async Task StartInvestigation(string investigationId, string locationId)
    {
        // Load investigation template from GameWorld
        _investigation = _gameWorld.InvestigationTemplates.TryGetValue(investigationId, out InvestigationTemplate template)
            ? template
            : null;

        if (_investigation == null)
        {
            throw new ArgumentException($"Investigation template '{investigationId}' not found in GameWorld");
        }

        _locationId = locationId;

        // Initialize progress tracker
        _progress = new InvestigationProgress
        {
            InvestigationId = investigationId,
            CurrentPhaseIndex = 0,
            CompletedPhases = new System.Collections.Generic.List<int>(),
            Discoveries = new System.Collections.Generic.Dictionary<DiscoveryType, System.Collections.Generic.List<string>>()
        };

        // Start first phase
        await StartCurrentPhase();

        _messageSystem.AddSystemMessage(
            $"Investigation started: {_investigation.Name}",
            SystemMessageTypes.Info);
    }

    /// <summary>
    /// Start current phase based on progress.CurrentPhaseIndex
    /// </summary>
    private async Task StartCurrentPhase()
    {
        if (_progress.CurrentPhaseIndex >= _investigation.Phases.Count)
        {
            throw new InvalidOperationException("No more phases to start - investigation should be complete");
        }

        InvestigationPhase phase = _investigation.Phases[_progress.CurrentPhaseIndex];

        _messageSystem.AddSystemMessage(
            $"Phase {_progress.CurrentPhaseIndex + 1}: {phase.Name}",
            SystemMessageTypes.Info);

        // Spawn appropriate tactical session based on phase system type
        switch (phase.SystemType)
        {
            case TacticalSystemType.Social:
                // THREE PARALLEL SYSTEMS: Social phases use NPC and Request
                if (string.IsNullOrEmpty(phase.NpcId) || string.IsNullOrEmpty(phase.RequestId))
                {
                    throw new InvalidOperationException($"[InvestigationActivity] Social phase '{phase.Id}' missing NpcId or RequestId");
                }

                // Get NPC from GameWorld
                NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == phase.NpcId);
                if (npc == null)
                {
                    throw new InvalidOperationException($"[InvestigationActivity] NPC '{phase.NpcId}' not found for Social phase '{phase.Id}'");
                }

                // Get Request from NPC
                NPCRequest request = npc.GetRequestById(phase.RequestId);
                if (request == null)
                {
                    throw new InvalidOperationException($"[InvestigationActivity] Request '{phase.RequestId}' not found on NPC '{phase.NpcId}' for phase '{phase.Id}'");
                }

                // Start conversation using ConversationFacade (parallel to Mental/Physical spawning)
                // Note: ConversationFacade.StartConversation handles building deck from engagement type
                ConversationSession socialSession = _conversationFacade.StartConversation(phase.NpcId, phase.RequestId);
                _currentSessionId = socialSession.SessionId;
                break;

            case TacticalSystemType.Mental:
                // THREE PARALLEL SYSTEMS: Look up Mental engagement type
                if (!_gameWorld.MentalEngagementTypes.TryGetValue(phase.EngagementTypeId, out MentalEngagementType mentalEngagement))
                {
                    throw new InvalidOperationException($"[InvestigationActivity] Mental engagement type '{phase.EngagementTypeId}' not found for phase '{phase.Id}'");
                }

                // Build deck from Mental engagement
                if (!_gameWorld.MentalEngagementDecks.TryGetValue(mentalEngagement.DeckId, out MentalEngagementDeck mentalDeck))
                {
                    throw new InvalidOperationException($"[InvestigationActivity] Mental deck '{mentalEngagement.DeckId}' not found");
                }

                List<CardInstance> mentalCards = mentalDeck.BuildCardInstances(_gameWorld);
                MentalSession mentalSession = _mentalFacade.StartSession(mentalEngagement, mentalCards, new List<CardInstance>(), _locationId);
                _currentSessionId = mentalSession.SessionId;
                break;

            case TacticalSystemType.Physical:
                // THREE PARALLEL SYSTEMS: Look up Physical engagement type
                if (!_gameWorld.PhysicalEngagementTypes.TryGetValue(phase.EngagementTypeId, out PhysicalEngagementType physicalEngagement))
                {
                    throw new InvalidOperationException($"[InvestigationActivity] Physical engagement type '{phase.EngagementTypeId}' not found for phase '{phase.Id}'");
                }

                // Build deck from Physical engagement
                if (!_gameWorld.PhysicalEngagementDecks.TryGetValue(physicalEngagement.DeckId, out PhysicalEngagementDeck physicalDeck))
                {
                    throw new InvalidOperationException($"[InvestigationActivity] Physical deck '{physicalEngagement.DeckId}' not found");
                }

                List<CardInstance> physicalCards = physicalDeck.BuildCardInstances(_gameWorld);
                PhysicalSession physicalSession = _physicalFacade.StartSession(physicalEngagement, physicalCards, new List<CardInstance>(), _locationId);
                _currentSessionId = physicalSession.SessionId;
                break;

            default:
                throw new InvalidOperationException($"Unknown tactical system type: {phase.SystemType}");
        }
    }

    /// <summary>
    /// Handle phase completion - called by UI or tactical facades
    /// </summary>
    public async Task HandlePhaseComplete(string sessionId)
    {
        if (_currentSessionId != sessionId)
        {
            throw new ArgumentException($"Session ID mismatch: expected {_currentSessionId}, got {sessionId}");
        }

        // Mark current phase as complete
        _progress.CompletedPhases.Add(_progress.CurrentPhaseIndex);

        InvestigationPhase completedPhase = _investigation.Phases[_progress.CurrentPhaseIndex];

        // Grant phase completion rewards
        if (completedPhase.CompletionReward != null)
        {
            GrantPhaseRewards(completedPhase.CompletionReward);
        }

        // Increment phase index
        _progress.CurrentPhaseIndex++;

        // Check if investigation complete (all phases done)
        if (_progress.CurrentPhaseIndex >= _investigation.Phases.Count)
        {
            await CompleteInvestigation();
        }
        else
        {
            // Start next phase
            await StartCurrentPhase();
        }
    }

    /// <summary>
    /// Complete investigation - award knowledge, discoveries
    /// </summary>
    private async Task CompleteInvestigation()
    {
        _messageSystem.AddSystemMessage(
            $"Investigation complete: {_investigation.Name}",
            SystemMessageTypes.Success);

        // Grant investigation-level rewards
        Player player = _gameWorld.GetPlayer();

        // Grant observation card rewards
        if (_investigation.ObservationCardRewards != null)
        {
            foreach (InvestigationObservationReward reward in _investigation.ObservationCardRewards)
            {
                // Create observation card and assign to NPC
                // ASSUMPTION: ObservationManager has method to create cards from IDs
                _messageSystem.AddSystemMessage(
                    $"New observation unlocked for {reward.NpcId}",
                    SystemMessageTypes.Success);
            }
        }

        // Clear investigation state
        _investigation = null;
        _progress = null;
        _currentSessionId = null;

        await Task.CompletedTask;
    }

    /// <summary>
    /// Grant phase completion rewards
    /// </summary>
    private void GrantPhaseRewards(PhaseCompletionReward reward)
    {
        Player player = _gameWorld.GetPlayer();

        // Grant discoveries
        if (reward.DiscoveriesGranted != null)
        {
            foreach (string discoveryId in reward.DiscoveriesGranted)
            {
                // Add to progress discoveries
                // ASSUMPTION: Discovery system tracks what player has discovered
                _messageSystem.AddSystemMessage(
                    $"Discovery: {discoveryId}",
                    SystemMessageTypes.Success);
            }
        }

        // Display narrative
        if (!string.IsNullOrEmpty(reward.Narrative))
        {
            _messageSystem.AddSystemMessage(
                reward.Narrative,
                SystemMessageTypes.Info);
        }
    }

    /// <summary>
    /// Get current phase context
    /// </summary>
    public InvestigationPhase GetCurrentPhase()
    {
        if (_investigation == null || _progress == null)
            return null;

        if (_progress.CurrentPhaseIndex >= _investigation.Phases.Count)
            return null;

        return _investigation.Phases[_progress.CurrentPhaseIndex];
    }

    /// <summary>
    /// Get current investigation progress
    /// </summary>
    public InvestigationProgress GetProgress()
    {
        return _progress;
    }

    /// <summary>
    /// Check if investigation is active
    /// </summary>
    public bool IsInvestigationActive()
    {
        return _investigation != null && _progress != null;
    }

    /// <summary>
    /// Get current session ID
    /// </summary>
    public string GetCurrentSessionId()
    {
        return _currentSessionId;
    }
}

/// <summary>
/// Player's progress through an investigation
/// </summary>
public class InvestigationProgress
{
    public string InvestigationId { get; set; }
    public int CurrentPhaseIndex { get; set; } // Current phase (0-based)
    public System.Collections.Generic.List<int> CompletedPhases { get; set; } = new System.Collections.Generic.List<int>();
    public System.Collections.Generic.Dictionary<DiscoveryType, System.Collections.Generic.List<string>> Discoveries { get; set; } = new System.Collections.Generic.Dictionary<DiscoveryType, System.Collections.Generic.List<string>>();
}
