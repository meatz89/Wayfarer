using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Investigation service - provides operations for investigation lifecycle
/// STATE-LESS: All state lives in GameWorld.InvestigationJournal
/// Does NOT spawn tactical sessions - creates LocationGoals that existing goal system evaluates
/// </summary>
public class InvestigationActivity
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly KnowledgeService _knowledgeService;

    private InvestigationDiscoveryResult _pendingDiscoveryResult;
    private InvestigationActivationResult _pendingActivationResult;
    private InvestigationProgressResult _pendingProgressResult;
    private InvestigationCompleteResult _pendingCompleteResult;

    public InvestigationActivity(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        KnowledgeService knowledgeService)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _knowledgeService = knowledgeService ?? throw new ArgumentNullException(nameof(knowledgeService));
    }

    /// <summary>
    /// Get and clear pending discovery result for UI modal display
    /// Returns null if no result pending
    /// </summary>
    public InvestigationDiscoveryResult GetAndClearPendingDiscoveryResult()
    {
        InvestigationDiscoveryResult result = _pendingDiscoveryResult;
        _pendingDiscoveryResult = null;
        return result;
    }

    /// <summary>
    /// Get and clear pending progress result for UI modal display
    /// Returns null if no result pending
    /// </summary>
    public InvestigationProgressResult GetAndClearPendingProgressResult()
    {
        InvestigationProgressResult result = _pendingProgressResult;
        _pendingProgressResult = null;
        return result;
    }

    /// <summary>
    /// Get and clear pending completion result for UI modal display
    /// Returns null if no result pending
    /// </summary>
    public InvestigationCompleteResult GetAndClearPendingCompleteResult()
    {
        InvestigationCompleteResult result = _pendingCompleteResult;
        _pendingCompleteResult = null;
        return result;
    }

    /// <summary>
    /// Get and clear pending activation result for UI modal display
    /// Returns null if no result pending
    /// </summary>
    public InvestigationActivationResult GetAndClearPendingActivationResult()
    {
        InvestigationActivationResult result = _pendingActivationResult;
        _pendingActivationResult = null;
        return result;
    }

    /// <summary>
    /// Activate investigation - looks up goals and spawns them at locations/NPCs
    /// Moves investigation from Pending → Active in GameWorld.InvestigationJournal
    /// </summary>
    public void ActivateInvestigation(string investigationId)
    {
        // Load investigation template from GameWorld
        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation == null)
        {
            throw new ArgumentException($"Investigation '{investigationId}' not found in GameWorld");
        }

        // Remove from potential and discovered
        _gameWorld.InvestigationJournal.PotentialInvestigationIds.Remove(investigationId);
        _gameWorld.InvestigationJournal.DiscoveredInvestigationIds.Remove(investigationId);

        // Create active investigation
        ActiveInvestigation activeInvestigation = new ActiveInvestigation
        {
            InvestigationId = investigationId,
            CompletedGoalIds = new List<string>()
        };
        _gameWorld.InvestigationJournal.ActiveInvestigations.Add(activeInvestigation);

        // Spawn initial goals (prerequisites met)
        foreach (InvestigationPhaseDefinition phaseDef in investigation.PhaseDefinitions)
        {
            if (ArePrerequisitesMet(phaseDef.Requirements, new List<string>()))
            {
                SpawnGoalForPhase(phaseDef, investigationId);
            }
        }

        _messageSystem.AddSystemMessage(
            $"Investigation activated: {investigation.Name}",
            SystemMessageTypes.Info);
    }

    /// <summary>
    /// Spawn goal for phase - looks up goal and adds to ActiveGoals
    /// </summary>
    private void SpawnGoalForPhase(InvestigationPhaseDefinition phaseDef, string investigationId)
    {
        // Look up goal from GameWorld.Goals dictionary
        if (!_gameWorld.Goals.TryGetValue(phaseDef.GoalId, out Goal goal))
        {
            throw new InvalidOperationException(
                $"Phase '{phaseDef.Id}' references goal '{phaseDef.GoalId}' which doesn't exist in GameWorld.Goals");
        }

        // Set investigation-specific properties
        goal.InvestigationId = investigationId;
        goal.Requirements = phaseDef.Requirements;
        goal.IsAvailable = true;

        // Add to appropriate ActiveGoals list
        if (!string.IsNullOrEmpty(goal.NpcId))
        {
            // Social goal → NPC.ActiveGoals
            NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == goal.NpcId);
            if (npc == null)
                throw new InvalidOperationException($"Goal '{goal.Id}' references NPC '{goal.NpcId}' which doesn't exist");

            if (!npc.ActiveGoals.Any(g => g.Id == goal.Id))
            {
                npc.ActiveGoals.Add(goal);
                Console.WriteLine($"[Investigation] Spawned Social goal '{goal.Name}' at NPC '{npc.Name}'");
            }
        }
        else if (!string.IsNullOrEmpty(goal.LocationId))
        {
            // Mental/Physical goal → Location.ActiveGoals
            Location location = _gameWorld.GetLocation(goal.LocationId);
            if (location == null)
                throw new InvalidOperationException($"Goal '{goal.Id}' references location '{goal.LocationId}' which doesn't exist");

            if (!location.ActiveGoals.Any(g => g.Id == goal.Id))
            {
                location.ActiveGoals.Add(goal);
                Console.WriteLine($"[Investigation] Spawned {goal.SystemType} goal '{goal.Name}' at location '{location.Name}'");
            }
        }
        else
        {
            throw new InvalidOperationException($"Goal '{goal.Id}' has neither NpcId nor LocationId");
        }
    }

    /// <summary>
    /// Mark goal complete - checks for investigation progress
    /// Returns InvestigationProgressResult for UI modal display
    /// </summary>
    public InvestigationProgressResult CompleteGoal(string goalId, string investigationId)
    {
        // Find active investigation
        ActiveInvestigation activeInv = _gameWorld.InvestigationJournal.ActiveInvestigations
            .FirstOrDefault(inv => inv.InvestigationId == investigationId);

        if (activeInv == null)
        {
            throw new ArgumentException($"Investigation '{investigationId}' is not active");
        }

        // Load investigation template
        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation == null)
        {
            throw new ArgumentException($"Investigation '{investigationId}' not found");
        }

        // Find completed phase definition
        InvestigationPhaseDefinition completedPhase = investigation.PhaseDefinitions
            .FirstOrDefault(p => p.Id == goalId);

        if (completedPhase == null)
        {
            throw new ArgumentException($"Phase '{goalId}' not found in investigation '{investigationId}'");
        }

        // Mark goal as complete
        if (!activeInv.CompletedGoalIds.Contains(goalId))
        {
            activeInv.CompletedGoalIds.Add(goalId);
        }

        // Grant knowledge from phase completion rewards
        if (completedPhase.CompletionReward?.KnowledgeGranted != null)
        {
            foreach (string knowledgeId in completedPhase.CompletionReward.KnowledgeGranted)
            {
                _knowledgeService.GrantKnowledge(knowledgeId);
            }
        }

        // Check for newly unlocked goals
        List<NewLeadInfo> newLeads = new List<NewLeadInfo>();
        foreach (InvestigationPhaseDefinition phaseDef in investigation.PhaseDefinitions)
        {
            // Skip if already completed
            if (activeInv.CompletedGoalIds.Contains(phaseDef.Id))
                continue;

            // Look up goal to check if already spawned
            if (!_gameWorld.Goals.TryGetValue(phaseDef.GoalId, out Goal goal))
                continue;

            // Check if already spawned (in ActiveGoals)
            bool alreadySpawned = false;
            if (!string.IsNullOrEmpty(goal.NpcId))
            {
                NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == goal.NpcId);
                alreadySpawned = npc?.ActiveGoals.Any(g => g.Id == goal.Id) ?? false;
            }
            else if (!string.IsNullOrEmpty(goal.LocationId))
            {
                Location location = _gameWorld.GetLocation(goal.LocationId);
                alreadySpawned = location?.ActiveGoals.Any(g => g.Id == goal.Id) ?? false;
            }

            if (alreadySpawned)
                continue;

            // Check if prerequisites now met
            if (ArePrerequisitesMet(phaseDef.Requirements, activeInv.CompletedGoalIds))
            {
                SpawnGoalForPhase(phaseDef, investigationId);

                // Build new lead info for UI
                NewLeadInfo newLead = new NewLeadInfo
                {
                    GoalName = phaseDef.Name,
                    LocationName = "", // Will be populated below
                    SpotName = ""      // Will be populated below
                };

                // Derive location info for UI display
                if (!string.IsNullOrEmpty(goal.LocationId))
                {
                    LocationEntry spotEntry = _gameWorld.Locations.FirstOrDefault(s => s.LocationId == goal.LocationId);
                    Venue venue = spotEntry != null
                        ? _gameWorld.WorldState.venues.FirstOrDefault(l => l.Id == spotEntry.location.VenueId)
                        : null;

                    newLead.LocationName = venue?.Name ?? "Unknown";
                    newLead.SpotName = spotEntry?.location.Name ?? goal.LocationId;
                }
                else if (!string.IsNullOrEmpty(goal.NpcId))
                {
                    NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == goal.NpcId);
                    newLead.LocationName = npc?.Name ?? "Unknown NPC";
                    newLead.SpotName = "";
                }

                newLeads.Add(newLead);
            }
        }

        // Build result for UI modal
        InvestigationProgressResult result = new InvestigationProgressResult
        {
            InvestigationId = investigationId,
            InvestigationName = investigation.Name,
            CompletedGoalName = completedPhase.Name,
            OutcomeNarrative = completedPhase.OutcomeNarrative,
            NewLeads = newLeads,
            CompletedGoalCount = activeInv.CompletedGoalIds.Count,
            TotalGoalCount = investigation.PhaseDefinitions.Count
        };

        _pendingProgressResult = result;

        return result;
    }

    /// <summary>
    /// Check if investigation is complete - all goals done
    /// Returns InvestigationCompleteResult if complete, null otherwise
    /// </summary>
    public InvestigationCompleteResult CheckInvestigationCompletion(string investigationId)
    {
        // Find active investigation
        ActiveInvestigation activeInv = _gameWorld.InvestigationJournal.ActiveInvestigations
            .FirstOrDefault(inv => inv.InvestigationId == investigationId);

        if (activeInv == null)
        {
            return null; // Not active
        }

        // Load investigation template
        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation == null)
        {
            return null;
        }

        // Check if all phases complete
        if (activeInv.CompletedGoalIds.Count < investigation.PhaseDefinitions.Count)
        {
            return null; // Not yet complete
        }

        // Move from Active → Completed
        _gameWorld.InvestigationJournal.ActiveInvestigations.Remove(activeInv);
        _gameWorld.InvestigationJournal.CompletedInvestigationIds.Add(investigationId);

        // Grant rewards
        GrantInvestigationRewards(investigation);

        // Build result for UI modal
        InvestigationCompleteResult result = new InvestigationCompleteResult
        {
            InvestigationId = investigationId,
            InvestigationName = investigation.Name,
            CompletionNarrative = investigation.CompletionNarrative,
            Rewards = investigation.CompletionRewards,
            ObservationCards = investigation.ObservationCardRewards
        };

        _pendingCompleteResult = result;

        _messageSystem.AddSystemMessage(
            $"Investigation complete: {investigation.Name}",
            SystemMessageTypes.Success);

        return result;
    }

    /// <summary>
    /// Grant investigation completion rewards
    /// </summary>
    private void GrantInvestigationRewards(Investigation investigation)
    {
        Player player = _gameWorld.GetPlayer();

        if (investigation.CompletionRewards != null)
        {
            // Grant coins
            if (investigation.CompletionRewards.Coins > 0)
            {
                player.Coins += investigation.CompletionRewards.Coins;
            }

            // XP and reputation systems not yet implemented
        }

        // Observation card creation deferred until ObservationManager supports it
        if (investigation.ObservationCardRewards != null)
        {
            foreach (ObservationCardReward reward in investigation.ObservationCardRewards)
            {
                _messageSystem.AddSystemMessage(
                    $"New observation unlocked for {reward.TargetNpcId}",
                    SystemMessageTypes.Success);
            }
        }
    }

    /// <summary>
    /// Check if prerequisites are met for a phase
    /// </summary>
    private bool ArePrerequisitesMet(GoalRequirements requirements, List<string> completedGoalIds)
    {
        if (requirements == null)
            return true;

        Player player = _gameWorld.GetPlayer();

        // Check completed goals prerequisite
        if (requirements.CompletedGoals != null && requirements.CompletedGoals.Count > 0)
        {
            if (completedGoalIds == null)
                return false;

            foreach (string requiredGoalId in requirements.CompletedGoals)
            {
                if (!completedGoalIds.Contains(requiredGoalId))
                    return false;
            }
        }

        // Check knowledge prerequisites
        if (requirements.RequiredKnowledge != null && requirements.RequiredKnowledge.Count > 0)
        {
            foreach (string knowledgeId in requirements.RequiredKnowledge)
            {
                if (!player.Knowledge.HasKnowledge(knowledgeId))
                    return false;
            }
        }

        // Future: Check other prerequisites when systems are ready
        // - RequiredEquipment (when inventory exists)
        // - RequiredStats
        // - MinimumLocationFamiliarity

        return true;
    }

    /// <summary>
    /// Discover investigation - moves Potential → Discovered, spawns intro goal
    /// Sets pending discovery result for UI modal display
    /// </summary>
    public void DiscoverInvestigation(string investigationId)
    {
        Console.WriteLine($"[InvestigationActivity] DiscoverInvestigation called for '{investigationId}'");

        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation == null)
            throw new ArgumentException($"Investigation '{investigationId}' not found");

        if (investigation.IntroAction == null)
            throw new InvalidOperationException($"Investigation '{investigationId}' has no intro action defined");

        // Move Potential → Discovered
        _gameWorld.InvestigationJournal.PotentialInvestigationIds.Remove(investigationId);
        _gameWorld.InvestigationJournal.DiscoveredInvestigationIds.Add(investigationId);
        Console.WriteLine($"[InvestigationActivity] Moved '{investigation.Name}' from Potential → Discovered");

        // Spawn intro goal (look up and add to ActiveGoals)
        SpawnIntroGoalForInvestigation(investigation);

        // Derive venue from location (LocationId is globally unique)
        LocationEntry spotEntry = _gameWorld.Locations.FirstOrDefault(s => s.LocationId == investigation.IntroAction.LocationId);
        Location location = spotEntry?.location;
        Venue venue = spotEntry != null
            ? _gameWorld.WorldState.venues.FirstOrDefault(l => l.Id == spotEntry.location.VenueId)
            : null;

        // Create discovery result for UI modal
        InvestigationDiscoveryResult discoveryResult = new InvestigationDiscoveryResult
        {
            InvestigationId = investigationId,
            InvestigationName = investigation.Name,
            IntroNarrative = investigation.IntroAction.IntroNarrative,
            IntroActionText = investigation.IntroAction.ActionText,
            ColorCode = investigation.ColorCode,
            LocationName = venue?.Name ?? "Unknown Venue",
            SpotName = location?.Name ?? investigation.IntroAction.LocationId
        };
        _pendingDiscoveryResult = discoveryResult;

        _messageSystem.AddSystemMessage(
            $"Investigation discovered: {investigation.Name}",
            SystemMessageTypes.Info);
    }

    /// <summary>
    /// Spawn intro goal - finds intro goal by investigationId and IsIntroAction flag
    /// </summary>
    private void SpawnIntroGoalForInvestigation(Investigation investigation)
    {
        // Find intro goal in GameWorld.Goals
        Goal introGoal = _gameWorld.Goals.Values
            .FirstOrDefault(g => g.InvestigationId == investigation.Id && g.IsIntroAction);

        if (introGoal == null)
        {
            throw new InvalidOperationException(
                $"Investigation '{investigation.Id}' has no intro goal. " +
                $"Create Goal with InvestigationId='{investigation.Id}' and IsIntroAction=true in 05_goals.json");
        }

        // Add to appropriate ActiveGoals
        if (!string.IsNullOrEmpty(introGoal.LocationId))
        {
            Location location = _gameWorld.GetLocation(introGoal.LocationId);
            if (location == null)
                throw new InvalidOperationException($"Intro goal '{introGoal.Id}' references location '{introGoal.LocationId}' which doesn't exist");

            if (!location.ActiveGoals.Any(g => g.Id == introGoal.Id))
            {
                location.ActiveGoals.Add(introGoal);
                Console.WriteLine($"[Investigation] Spawned intro goal '{introGoal.Name}' at location '{location.Name}'");
            }
        }
        else if (!string.IsNullOrEmpty(introGoal.NpcId))
        {
            NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == introGoal.NpcId);
            if (npc == null)
                throw new InvalidOperationException($"Intro goal '{introGoal.Id}' references NPC '{introGoal.NpcId}' which doesn't exist");

            if (!npc.ActiveGoals.Any(g => g.Id == introGoal.Id))
            {
                npc.ActiveGoals.Add(introGoal);
                Console.WriteLine($"[Investigation] Spawned intro goal '{introGoal.Name}' at NPC '{npc.Name}'");
            }
        }
    }

    /// <summary>
    /// Complete intro action - moves Discovered → Active, spawns first goals
    /// Called from tactical session completion (Mental/Physical/SocialFacade)
    /// </summary>
    public void CompleteIntroAction(string investigationId)
    {
        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);

        // Call ActivateInvestigation to spawn first goals
        ActivateInvestigation(investigationId);

        // Create activation result for UI modal
        _pendingActivationResult = new InvestigationActivationResult
        {
            InvestigationId = investigationId,
            InvestigationName = investigation?.Name ?? investigationId,
            IntroNarrative = investigation?.IntroAction?.IntroNarrative ?? "Investigation activated."
        };

        _messageSystem.AddSystemMessage(
            $"Investigation activated: {investigation?.Name}",
            SystemMessageTypes.Success);
    }

}
