using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Public API for all narrative operations.
/// Centralizes narrative generation, messaging, and observation handling.
/// </summary>
public class NarrativeFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly NarrativeRenderer _narrativeRenderer;
    private readonly NarrativeService _narrativeService;
    private readonly LocationNarrativeGenerator _locationNarrativeGenerator;

    public NarrativeFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        NarrativeRenderer narrativeRenderer,
        NarrativeService narrativeService,
        LocationNarrativeGenerator locationNarrativeGenerator)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _narrativeRenderer = narrativeRenderer;
        _narrativeService = narrativeService;
        _locationNarrativeGenerator = locationNarrativeGenerator;
    }

    // ========== MESSAGE OPERATIONS ==========

    /// <summary>
    /// Add a system message with narrative flair
    /// </summary>
    public void AddSystemMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        _messageSystem.AddSystemMessage(message, type);
    }

    /// <summary>
    /// Add a narrative event message
    /// </summary>
    public void AddNarrativeMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        _messageSystem.AddSystemMessage(message, type);
    }

    // ========== NARRATIVE GENERATION ==========

    /// <summary>
    /// Generate token gain narrative
    /// </summary>
    public TokenNarrativeResult GenerateTokenGainNarrative(ConnectionType type, int count, string npcId)
    {
        return _narrativeService.GenerateTokenGainNarrative(type, count, npcId);
    }

    /// <summary>
    /// Generate relationship milestone narrative
    /// </summary>
    public MilestoneNarrativeResult GenerateRelationshipMilestone(string npcId, int totalTokens)
    {
        return _narrativeService.GenerateRelationshipMilestone(npcId, totalTokens);
    }

    /// <summary>
    /// Generate relationship damage narrative
    /// </summary>
    public NarrativeResult GenerateRelationshipDamageNarrative(string npcId, ConnectionType type, int remainingTokens)
    {
        return _narrativeService.GenerateRelationshipDamageNarrative(npcId, type, remainingTokens);
    }

    /// <summary>
    /// Generate queue reorganization narrative
    /// </summary>
    public string[] GenerateQueueReorganizationNarrative(int removedPosition, int lettersShifted)
    {
        return _narrativeService.GenerateQueueReorganizationNarrative(removedPosition, lettersShifted);
    }

    /// <summary>
    /// Generate morning letter narrative
    /// </summary>
    public MorningNarrativeResult GenerateMorningLetterNarrative(int lettersGenerated, bool queueFull)
    {
        return _narrativeService.GenerateMorningLetterNarrative(lettersGenerated, queueFull);
    }

    /// <summary>
    /// Generate time transition narrative
    /// </summary>
    public TransitionNarrativeResult GenerateTimeTransitionNarrative(TimeBlocks from, TimeBlocks to, string actionDescription)
    {
        return _narrativeService.GenerateTimeTransitionNarrative(from, to, actionDescription);
    }

    /// <summary>
    /// Generate obligation warning narrative
    /// </summary>
    public NarrativeResult GenerateObligationWarning(StandingObligation obligation, int daysUntilForced)
    {
        return _narrativeService.GenerateObligationWarning(obligation, daysUntilForced);
    }

    /// <summary>
    /// Generate token spending narrative
    /// </summary>
    public string GenerateTokenSpendingNarrative(ConnectionType type, int amount, string action)
    {
        return _narrativeService.GenerateTokenSpendingNarrative(type, amount, action);
    }

    /// <summary>
    /// Generate obligation acceptance narrative
    /// </summary>
    public string GenerateObligationAcceptanceNarrative(StandingObligation obligation)
    {
        return _narrativeService.GenerateObligationAcceptanceNarrative(obligation);
    }

    /// <summary>
    /// Generate obligation conflict narrative
    /// </summary>
    public string GenerateObligationConflictNarrative(string newObligation, List<string> conflicts)
    {
        return _narrativeService.GenerateObligationConflictNarrative(newObligation, conflicts);
    }

    /// <summary>
    /// Generate obligation removal narrative
    /// </summary>
    public string GenerateObligationRemovalNarrative(StandingObligation obligation, bool isVoluntary)
    {
        return _narrativeService.GenerateObligationRemovalNarrative(obligation, isVoluntary);
    }

    /// <summary>
    /// Generate obligation breaking narrative
    /// </summary>
    public string GenerateObligationBreakingNarrative(StandingObligation obligation, int tokenLoss)
    {
        return _narrativeService.GenerateObligationBreakingNarrative(obligation, tokenLoss);
    }

    // ========== TEMPLATE RENDERING ==========

    /// <summary>
    /// Render a categorical template to human-readable text
    /// </summary>
    public string RenderTemplate(string template)
    {
        return _narrativeRenderer.RenderTemplate(template);
    }

    /// <summary>
    /// Generate venue arrival narrative
    /// </summary>
    public string GenerateArrivalText(Venue venue, Location entrySpot)
    {
        return _locationNarrativeGenerator.GenerateArrivalText(venue, entrySpot);
    }

    /// <summary>
    /// Generate venue departure narrative
    /// </summary>
    public string GenerateDepartureText(Venue venue, Location exitSpot)
    {
        return _locationNarrativeGenerator.GenerateDepartureText(venue, exitSpot);
    }

    /// <summary>
    /// Generate movement between Locations narrative
    /// </summary>
    public string GenerateMovementText(Location fromSpot, Location toSpot)
    {
        return _locationNarrativeGenerator.GenerateMovementText(fromSpot, toSpot);
    }
}
