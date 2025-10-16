using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Evaluates which investigations can be discovered based on game state
/// STATELESS service - all state in GameWorld
/// Follows ARCHITECTURE.md principles: service operates on GameWorld, doesn't store state
/// </summary>
public class InvestigationDiscoveryEvaluator
{
    private readonly GameWorld _gameWorld;

    public InvestigationDiscoveryEvaluator(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Evaluate all Potential investigations and return those ready to be discovered
    /// Called on: Venue entry, knowledge gain, item acquisition, obligation acceptance
    /// </summary>
    public List<Investigation> EvaluateDiscoverableInvestigations(Player player)
    {
        List<Investigation> discoverable = new List<Investigation>();

        foreach (string investigationId in _gameWorld.InvestigationJournal.PotentialInvestigationIds)
        {
            Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
            if (investigation == null)
            {
                Console.WriteLine($"[InvestigationEvaluator] WARNING: Potential investigation '{investigationId}' not found in GameWorld");
                continue;
            }

            Console.WriteLine($"[InvestigationEvaluator] Evaluating investigation '{investigation.Name}' (ID: {investigation.Id})");

            // Skip if no intro action defined
            if (investigation.IntroAction == null)
            {
                Console.WriteLine($"[InvestigationEvaluator] Skipping '{investigation.Name}' - IntroAction is NULL");
                continue;
            }

            Console.WriteLine($"[InvestigationEvaluator] '{investigation.Name}' has trigger type: {investigation.IntroAction.TriggerType}");

            if (IsTriggerConditionMet(investigation, player))
            {
                Console.WriteLine($"[InvestigationEvaluator] ✓ Trigger condition MET for '{investigation.Name}'");
                discoverable.Add(investigation);
            }
            else
            {
                Console.WriteLine($"[InvestigationEvaluator] ✗ Trigger condition NOT met for '{investigation.Name}'");
            }
        }

        return discoverable;
    }

    /// <summary>
    /// Check if specific investigation's trigger condition is met
    /// </summary>
    private bool IsTriggerConditionMet(Investigation investigation, Player player)
    {
        InvestigationPrerequisites prereqs = investigation.IntroAction.TriggerPrerequisites;
        if (prereqs == null) return true; // No prerequisites = always available

        // Check prerequisites based on trigger type
        return investigation.IntroAction.TriggerType switch
        {
            DiscoveryTriggerType.ImmediateVisibility => CheckImmediateVisibility(prereqs, player),
            DiscoveryTriggerType.EnvironmentalObservation => CheckEnvironmentalObservation(prereqs, player),
            DiscoveryTriggerType.ConversationalDiscovery => CheckConversationalDiscovery(prereqs, player),
            DiscoveryTriggerType.ItemDiscovery => CheckItemDiscovery(prereqs, player),
            DiscoveryTriggerType.ObligationTriggered => CheckObligationTriggered(prereqs, player),
            DiscoveryTriggerType.GoalCompletionTrigger => CheckGoalCompletionTrigger(prereqs),
            _ => false
        };
    }

    /// <summary>
    /// ImmediateVisibility: Player is at required location
    /// Prerequisites: LocationId (globally unique)
    /// </summary>
    private bool CheckImmediateVisibility(InvestigationPrerequisites prereqs, Player player)
    {
        Console.WriteLine($"[InvestigationEvaluator] Checking ImmediateVisibility - Required LocationId: '{prereqs.LocationId ?? "NULL"}', Player LocationId: '{player.CurrentLocation?.Id ?? "NULL"}'");

        // Check if player is at required location (LocationId is globally unique)
        if (!string.IsNullOrEmpty(prereqs.LocationId) && player.CurrentLocation?.Id != prereqs.LocationId)
        {
            Console.WriteLine($"[InvestigationEvaluator] ImmediateVisibility FAILED - Player not at required location");
            return false;
        }

        Console.WriteLine($"[InvestigationEvaluator] ImmediateVisibility PASSED");
        return true;
    }

    /// <summary>
    /// EnvironmentalObservation: Player is at required location
    /// Prerequisites: LocationId (globally unique)
    /// </summary>
    private bool CheckEnvironmentalObservation(InvestigationPrerequisites prereqs, Player player)
    {
        // Check if player is at required location (LocationId is globally unique)
        if (!string.IsNullOrEmpty(prereqs.LocationId) && player.CurrentLocation?.Id != prereqs.LocationId)
            return false;

        return true;
    }

    /// <summary>
    /// ConversationalDiscovery: Investigation discovered through conversations
    /// KNOWLEDGE SYSTEM ELIMINATED: No more boolean gates
    /// Prerequisites: None (Knowledge system was deleted in Phase 2)
    /// </summary>
    private bool CheckConversationalDiscovery(InvestigationPrerequisites prereqs, Player player)
    {
        // Knowledge system eliminated - no prerequisites to check
        // ConversationalDiscovery now represents narrative triggers without gating
        return true;
    }

    /// <summary>
    /// ItemDiscovery: Investigation discovered through item-based narrative triggers
    /// PRINCIPLE 4: No boolean gates - investigations visible based on narrative context
    /// Prerequisites: None (RequiredItems system eliminated)
    /// </summary>
    private bool CheckItemDiscovery(InvestigationPrerequisites prereqs, Player player)
    {
        // RequiredItems system eliminated - no prerequisites to check
        // ItemDiscovery now represents narrative triggers without gating
        return true;
    }

    /// <summary>
    /// ObligationTriggered: Investigation discovered through obligation-based narrative triggers
    /// PRINCIPLE 4: No boolean gates - investigations visible based on narrative context
    /// Prerequisites: None (RequiredObligation system eliminated)
    /// </summary>
    private bool CheckObligationTriggered(InvestigationPrerequisites prereqs, Player player)
    {
        // RequiredObligation system eliminated - no prerequisites to check
        // ObligationTriggered now represents narrative triggers without gating
        return true;
    }

    /// <summary>
    /// GoalCompletionTrigger: Investigation discovered through goal-based narrative triggers
    /// PRINCIPLE 4: No boolean gates - investigations visible based on narrative context
    /// Prerequisites: None (CompletedGoalId system eliminated)
    /// </summary>
    private bool CheckGoalCompletionTrigger(InvestigationPrerequisites prereqs)
    {
        // CompletedGoalId system eliminated - no prerequisites to check
        // GoalCompletionTrigger now represents narrative triggers without gating
        return true;
    }
}
