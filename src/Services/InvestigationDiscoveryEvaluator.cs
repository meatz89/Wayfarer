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
    /// ConversationalDiscovery: Player has required knowledge from conversations
    /// Prerequisites: requiredKnowledge list
    /// </summary>
    private bool CheckConversationalDiscovery(InvestigationPrerequisites prereqs, Player player)
    {
        // Check if player has all required knowledge
        if (prereqs.RequiredKnowledge != null && prereqs.RequiredKnowledge.Any())
        {
            foreach (string knowledgeId in prereqs.RequiredKnowledge)
            {
                if (!player.Knowledge.HasKnowledge(knowledgeId))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// ItemDiscovery: Player has acquired required item
    /// Prerequisites: requiredItems list
    /// </summary>
    private bool CheckItemDiscovery(InvestigationPrerequisites prereqs, Player player)
    {
        // Check if player has all required items
        if (prereqs.RequiredItems != null && prereqs.RequiredItems.Any())
        {
            foreach (string itemId in prereqs.RequiredItems)
            {
                // TODO: Implement item checking when inventory system exists
                // For now, return false if items required
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// ObligationTriggered: Player has accepted required obligation
    /// Prerequisites: requiredObligation
    /// </summary>
    private bool CheckObligationTriggered(InvestigationPrerequisites prereqs, Player player)
    {
        // Check if player has accepted required obligation
        if (!string.IsNullOrEmpty(prereqs.RequiredObligation))
        {
            bool hasObligation = player.StandingObligations
                .Any(o => o.ID == prereqs.RequiredObligation);

            if (!hasObligation)
                return false;
        }

        return true;
    }
}
