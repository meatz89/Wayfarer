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

        // Remove from potential (if still there, might already be in Discovered)
        _gameWorld.InvestigationJournal.PotentialInvestigationIds.Remove(investigationId);

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
            // Skip if already completed or already has goal created
            if (activeInv.CompletedGoalIds.Contains(phaseDef.Id))
                continue;

            // Check if this phase's prerequisites are now met
            if (ArePrerequisitesMet(phaseDef.Requirements, activeInv.CompletedGoalIds))
            {
                // Create new goal for newly unlocked phase
                LocationGoal newGoal = CreateGoalFromPhaseDefinition(phaseDef, investigationId);

                // Derive location from spot (SpotId is globally unique)
                LocationSpotEntry spotEntry = _gameWorld.Spots.FirstOrDefault(s => s.Spot.SpotID == phaseDef.SpotId);
                Location location = spotEntry != null
                    ? _gameWorld.Locations.FirstOrDefault(l => l.Id == spotEntry.Spot.LocationId)
                    : null;

                if (location != null)
                {
                    newLeads.Add(new NewLeadInfo
                    {
                        GoalName = phaseDef.Name,
                        LocationName = location.Name,
                        SpotName = spotEntry.Spot.Name
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
            ChallengeTypeId = phaseDef.ChallengeTypeId,
            SpotId = phaseDef.SpotId,
            NpcId = phaseDef.NpcId,
            RequestId = phaseDef.RequestId,
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
    /// Discover investigation - moves Potential → Discovered, spawns intro action
    /// Returns LocationGoal for intro action to be added to location
    /// Sets pending discovery result for UI modal display
    /// </summary>
    public LocationGoal DiscoverInvestigation(string investigationId)
    {
        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation == null)
            throw new ArgumentException($"Investigation '{investigationId}' not found");

        if (investigation.IntroAction == null)
            throw new InvalidOperationException($"Investigation '{investigationId}' has no intro action defined");

        // Move Potential → Discovered
        _gameWorld.InvestigationJournal.PotentialInvestigationIds.Remove(investigationId);
        _gameWorld.InvestigationJournal.DiscoveredInvestigationIds.Add(investigationId);

        // Create intro action as LocationGoal
        LocationGoal introGoal = CreateIntroGoalFromInvestigation(investigation);

        // Derive location from spot (SpotId is globally unique)
        LocationSpotEntry spotEntry = _gameWorld.Spots.FirstOrDefault(s => s.Spot.SpotID == investigation.IntroAction.SpotId);
        LocationSpot spot = spotEntry?.Spot;
        Location location = spotEntry != null
            ? _gameWorld.Locations.FirstOrDefault(l => l.Id == spotEntry.Spot.LocationId)
            : null;

        // Create discovery result for UI modal
        InvestigationDiscoveryResult discoveryResult = new InvestigationDiscoveryResult
        {
            InvestigationId = investigationId,
            InvestigationName = investigation.Name,
            IntroNarrative = investigation.IntroAction.IntroNarrative,
            IntroActionText = investigation.IntroAction.ActionText,
            ColorCode = investigation.ColorCode,
            LocationName = location?.Name ?? "Unknown Location",
            SpotName = spot?.Name ?? investigation.IntroAction.SpotId
        };
        _pendingDiscoveryResult = discoveryResult;

        _messageSystem.AddSystemMessage(
            $"Investigation discovered: {investigation.Name}",
            SystemMessageTypes.Info);

        return introGoal;
    }

    /// <summary>
    /// Complete intro action - moves Discovered → Active, spawns first goals
    /// Called from tactical session completion (Mental/Physical/SocialFacade)
    /// </summary>
    public List<LocationGoal> CompleteIntroAction(string investigationId)
    {
        // Move Discovered → Active
        _gameWorld.InvestigationJournal.DiscoveredInvestigationIds.Remove(investigationId);

        // Call existing ActivateInvestigation to create first goals
        List<LocationGoal> firstGoals = ActivateInvestigation(investigationId);

        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);

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

        return firstGoals;
    }

    /// <summary>
    /// Create intro action goal from investigation
    /// </summary>
    private LocationGoal CreateIntroGoalFromInvestigation(Investigation investigation)
    {
        InvestigationIntroAction intro = investigation.IntroAction;

        return new LocationGoal
        {
            Id = $"{investigation.Id}_intro",
            Name = intro.ActionText,
            Description = $"Begin investigation: {investigation.Name}",
            SystemType = intro.SystemType,
            ChallengeTypeId = intro.ChallengeTypeId,
            SpotId = intro.SpotId,
            NpcId = intro.NpcId,
            RequestId = intro.RequestId,
            InvestigationId = investigation.Id,
            IsIntroAction = true,  // Flag to identify intro actions
            IsAvailable = true,
            IsCompleted = false
        };
    }
}
