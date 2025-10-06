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

    private InvestigationProgressResult _pendingProgressResult;
    private InvestigationCompleteResult _pendingCompleteResult;

    public InvestigationActivity(
        GameWorld gameWorld,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
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
    /// Activate investigation - creates LocationGoals from PhaseDefinitions
    /// Moves investigation from Pending → Active in GameWorld.InvestigationJournal
    /// </summary>
    public List<LocationGoal> ActivateInvestigation(string investigationId)
    {
        // Load investigation template from GameWorld
        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation == null)
        {
            throw new ArgumentException($"Investigation '{investigationId}' not found in GameWorld");
        }

        // Remove from pending
        _gameWorld.InvestigationJournal.PendingInvestigationIds.Remove(investigationId);

        // Add to active
        ActiveInvestigation activeInvestigation = new ActiveInvestigation
        {
            InvestigationId = investigationId,
            CompletedGoalIds = new List<string>()
        };
        _gameWorld.InvestigationJournal.ActiveInvestigations.Add(activeInvestigation);

        // Create goals from phase definitions
        List<LocationGoal> createdGoals = new List<LocationGoal>();
        foreach (InvestigationPhaseDefinition phaseDef in investigation.PhaseDefinitions)
        {
            // Check if prerequisites met (initially only phases with no requirements)
            if (ArePrerequisitesMet(phaseDef.Requirements))
            {
                LocationGoal goal = CreateGoalFromPhaseDefinition(phaseDef, investigationId);
                createdGoals.Add(goal);
            }
        }

        _messageSystem.AddSystemMessage(
            $"Investigation activated: {investigation.Name}",
            SystemMessageTypes.Info);

        return createdGoals;
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

        // Check for newly unlocked goals
        List<NewLeadInfo> newLeads = new List<NewLeadInfo>();
        foreach (InvestigationPhaseDefinition phaseDef in investigation.PhaseDefinitions)
        {
            // Skip if already completed or already has goal created
            if (activeInv.CompletedGoalIds.Contains(phaseDef.Id))
                continue;

            // Check if this phase's prerequisites are now met
            if (ArePrerequisitesMet(phaseDef.Requirements, activeInv.CompletedGoalIds))
            {
                // Create new goal for newly unlocked phase
                LocationGoal newGoal = CreateGoalFromPhaseDefinition(phaseDef, investigationId);

                // Add to GameWorld locations
                Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == phaseDef.LocationId);
                if (location != null)
                {
                    newLeads.Add(new NewLeadInfo
                    {
                        GoalName = phaseDef.Name,
                        LocationName = location.Name,
                        SpotName = phaseDef.SpotId
                    });
                }
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

            // Grant XP (TODO: Implement XP system when ready)
            // Grant reputation (TODO: Implement reputation system when ready)
        }

        // Grant observation cards (TODO: Implement observation card creation when ready)
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
    /// Create LocationGoal from InvestigationPhaseDefinition
    /// </summary>
    private LocationGoal CreateGoalFromPhaseDefinition(InvestigationPhaseDefinition phaseDef, string investigationId)
    {
        LocationGoal goal = new LocationGoal
        {
            Id = phaseDef.Id,
            Name = phaseDef.Name,
            Description = phaseDef.Description,
            SystemType = phaseDef.SystemType,
            EngagementTypeId = phaseDef.EngagementTypeId,
            SpotId = phaseDef.SpotId,
            InvestigationId = investigationId,
            Requirements = phaseDef.Requirements,
            IsAvailable = true,
            IsCompleted = false
        };

        return goal;
    }

    /// <summary>
    /// Check if prerequisites are met for a phase
    /// </summary>
    private bool ArePrerequisitesMet(GoalRequirements requirements, List<string> completedGoalIds = null)
    {
        if (requirements == null)
            return true;

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

        // TODO: Check other prerequisites when systems are ready
        // - RequiredKnowledge
        // - RequiredEquipment
        // - RequiredStats
        // - MinimumLocationFamiliarity

        return true;
    }
}
