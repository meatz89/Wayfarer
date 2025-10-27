using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Evaluates which obligations can be discovered based on game state
/// STATELESS service - all state in GameWorld
/// Follows ARCHITECTURE.md principles: service operates on GameWorld, doesn't store state
/// </summary>
public class ObligationDiscoveryEvaluator
{
    private readonly GameWorld _gameWorld;

    public ObligationDiscoveryEvaluator(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Evaluate all Potential obligations and return those ready to be discovered
    /// Called on: Venue entry, knowledge gain, item acquisition, obligation acceptance
    /// </summary>
    public List<Obligation> EvaluateDiscoverableObligations(Player player)
    {
        List<Obligation> discoverable = new List<Obligation>();

        foreach (string obligationId in _gameWorld.ObligationJournal.PotentialObligationIds)
        {
            Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
            if (obligation == null)
            {
                continue;
            }

            // Skip if no intro action defined
            if (obligation.IntroAction == null)
            {
                continue;
            }

            if (IsTriggerConditionMet(obligation, player))
            {
                discoverable.Add(obligation);
            }
        }

        return discoverable;
    }

    /// <summary>
    /// Check if specific obligation's trigger condition is met
    /// </summary>
    private bool IsTriggerConditionMet(Obligation obligation, Player player)
    {
        ObligationPrerequisites prereqs = obligation.IntroAction.TriggerPrerequisites;
        if (prereqs == null) return true; // No prerequisites = always available

        // Check prerequisites based on trigger type
        return obligation.IntroAction.TriggerType switch
        {
            DiscoveryTriggerType.ImmediateVisibility => CheckImmediateVisibility(prereqs, player),
            DiscoveryTriggerType.EnvironmentalObservation => CheckEnvironmentalObservation(prereqs, player),
            DiscoveryTriggerType.ConversationalDiscovery => CheckConversationalDiscovery(prereqs, player),
            DiscoveryTriggerType.ItemDiscovery => CheckItemDiscovery(prereqs, player),
            DiscoveryTriggerType.ObligationTriggered => CheckObligationTriggered(prereqs, player),
            DiscoveryTriggerType.SituationCompletionTrigger => CheckSituationCompletionTrigger(prereqs),
            _ => false
        };
    }

    /// <summary>
    /// ImmediateVisibility: Player is at required location
    /// Prerequisites: LocationId (globally unique)
    /// </summary>
    private bool CheckImmediateVisibility(ObligationPrerequisites prereqs, Player player)
    {
        // Check if player is at required location (LocationId is globally unique)
        if (!string.IsNullOrEmpty(prereqs.LocationId) && player.CurrentLocation.Id != prereqs.LocationId)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// EnvironmentalObservation: Player is at required location
    /// Prerequisites: LocationId (globally unique)
    /// </summary>
    private bool CheckEnvironmentalObservation(ObligationPrerequisites prereqs, Player player)
    {
        // Check if player is at required location (LocationId is globally unique)
        if (!string.IsNullOrEmpty(prereqs.LocationId) && player.CurrentLocation.Id != prereqs.LocationId)
            return false;

        return true;
    }

    /// <summary>
    /// ConversationalDiscovery: Obligation discovered through conversations
    /// KNOWLEDGE SYSTEM ELIMINATED: No more boolean gates
    /// Prerequisites: None (Knowledge system was deleted in Phase 2)
    /// </summary>
    private bool CheckConversationalDiscovery(ObligationPrerequisites prereqs, Player player)
    {
        // Knowledge system eliminated - no prerequisites to check
        // ConversationalDiscovery now represents narrative triggers without gating
        return true;
    }

    /// <summary>
    /// ItemDiscovery: Obligation discovered through item-based narrative triggers
    /// PRINCIPLE 4: No boolean gates - obligations visible based on narrative context
    /// Prerequisites: None (RequiredItems system eliminated)
    /// </summary>
    private bool CheckItemDiscovery(ObligationPrerequisites prereqs, Player player)
    {
        // RequiredItems system eliminated - no prerequisites to check
        // ItemDiscovery now represents narrative triggers without gating
        return true;
    }

    /// <summary>
    /// ObligationTriggered: Obligation discovered through obligation-based narrative triggers
    /// PRINCIPLE 4: No boolean gates - obligations visible based on narrative context
    /// Prerequisites: None (RequiredObligation system eliminated)
    /// </summary>
    private bool CheckObligationTriggered(ObligationPrerequisites prereqs, Player player)
    {
        // RequiredObligation system eliminated - no prerequisites to check
        // ObligationTriggered now represents narrative triggers without gating
        return true;
    }

    /// <summary>
    /// SituationCompletionTrigger: Obligation discovered through situation-based narrative triggers
    /// PRINCIPLE 4: No boolean gates - obligations visible based on narrative context
    /// Prerequisites: None (CompletedSituationId system eliminated)
    /// </summary>
    private bool CheckSituationCompletionTrigger(ObligationPrerequisites prereqs)
    {
        // CompletedSituationId system eliminated - no prerequisites to check
        // SituationCompletionTrigger now represents narrative triggers without gating
        return true;
    }
}
