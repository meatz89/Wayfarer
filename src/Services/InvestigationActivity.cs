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
    private InvestigationIntroResult _pendingIntroResult;

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
    /// Get and clear pending intro result for UI modal display
    /// Returns null if no result pending
    /// </summary>
    public InvestigationIntroResult GetAndClearPendingIntroResult()
    {
        InvestigationIntroResult result = _pendingIntroResult;
        _pendingIntroResult = null;
        return result;
    }

    /// <summary>
    /// Set pending intro action - prepares quest acceptance modal but doesn't activate
    /// RPG Pattern: Button click → Modal → "Begin Investigation" → Activate
    /// </summary>
    public void SetPendingIntroAction(string investigationId)
    {
        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation == null)
            throw new ArgumentException($"Investigation '{investigationId}' not found");

        if (investigation.IntroAction == null)
            throw new InvalidOperationException($"Investigation '{investigationId}' has no intro action");

        // Derive venue from location
        LocationEntry spotEntry = _gameWorld.Locations.FirstOrDefault(s => s.LocationId == investigation.IntroAction.LocationId);
        Venue venue = spotEntry != null
            ? _gameWorld.WorldState.venues.FirstOrDefault(l => l.Id == spotEntry.location.VenueId)
            : null;

        // Create intro result for quest acceptance modal
        _pendingIntroResult = new InvestigationIntroResult
        {
            InvestigationId = investigationId,
            InvestigationName = investigation.Name,
            IntroNarrative = investigation.IntroAction.IntroNarrative,
            IntroActionText = investigation.IntroAction.ActionText,
            ColorCode = investigation.ColorCode,
            LocationName = venue?.Name ?? "Unknown Venue",
            SpotName = spotEntry?.location.Name ?? investigation.IntroAction.LocationId
        };

        Console.WriteLine($"[InvestigationActivity] Pending intro modal set for '{investigation.Name}'");
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

        // NOTE: Investigations no longer spawn goals directly
        // Goals are contained within obstacles as children (containment pattern)
        // Obstacles are spawned by investigation phase completion rewards

        _messageSystem.AddSystemMessage(
            $"Investigation activated: {investigation.Name}",
            SystemMessageTypes.Info);
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

        // Spawn obstacles from phase completion rewards
        if (completedPhase.CompletionReward?.ObstaclesSpawned != null &&
            completedPhase.CompletionReward.ObstaclesSpawned.Count > 0)
        {
            foreach (ObstacleSpawnInfo spawnInfo in completedPhase.CompletionReward.ObstaclesSpawned)
            {
                SpawnObstacle(spawnInfo);
            }
        }

        // NOTE: Goals are no longer spawned by investigations
        // Goals are contained within obstacles as children (containment pattern)
        // New leads come from obstacle-spawned goals, not phase-spawned goals
        List<NewLeadInfo> newLeads = new List<NewLeadInfo>();

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
    /// Discover investigation - moves Potential → Discovered
    /// Simple RPG pattern: Just discovery, no goal spawning (intro action is NOT a goal)
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
    /// Complete intro action - moves Discovered → Active, spawns Phase 1 obstacle
    /// Called when player clicks intro action button in discovery modal
    /// </summary>
    public void CompleteIntroAction(string investigationId)
    {
        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation == null)
        {
            throw new ArgumentException($"Investigation '{investigationId}' not found");
        }

        // Activate investigation (moves Discovered → Active)
        ActivateInvestigation(investigationId);

        // Spawn obstacles from intro completion reward
        if (investigation.IntroAction?.CompletionReward?.ObstaclesSpawned != null &&
            investigation.IntroAction.CompletionReward.ObstaclesSpawned.Count > 0)
        {
            foreach (ObstacleSpawnInfo spawnInfo in investigation.IntroAction.CompletionReward.ObstaclesSpawned)
            {
                SpawnObstacle(spawnInfo);
            }
        }

        // Grant knowledge from intro completion reward
        if (investigation.IntroAction?.CompletionReward?.KnowledgeGranted != null)
        {
            foreach (string knowledgeId in investigation.IntroAction.CompletionReward.KnowledgeGranted)
            {
                _knowledgeService.GrantKnowledge(knowledgeId);
            }
        }

        // Create activation result for UI modal
        _pendingActivationResult = new InvestigationActivationResult
        {
            InvestigationId = investigationId,
            InvestigationName = investigation.Name,
            IntroNarrative = investigation.IntroAction.IntroNarrative
        };

        _messageSystem.AddSystemMessage(
            $"Investigation activated: {investigation.Name}",
            SystemMessageTypes.Success);
    }

    /// <summary>
    /// Spawn an obstacle at the specified target entity as investigation phase reward
    /// </summary>
    private void SpawnObstacle(ObstacleSpawnInfo spawnInfo)
    {
        if (spawnInfo?.Obstacle == null)
            return;

        switch (spawnInfo.TargetType)
        {
            case ObstacleSpawnTargetType.Location:
                Location location = _gameWorld.GetLocation(spawnInfo.TargetEntityId);
                if (location == null)
                {
                    Console.WriteLine($"[InvestigationActivity] WARNING: Cannot spawn obstacle '{spawnInfo.Obstacle.Name}' - Location '{spawnInfo.TargetEntityId}' not found");
                    return;
                }
                // Duplicate ID protection - prevent data corruption
                if (!_gameWorld.Obstacles.Any(o => o.Id == spawnInfo.Obstacle.Id))
                {
                    _gameWorld.Obstacles.Add(spawnInfo.Obstacle);
                    location.ObstacleIds.Add(spawnInfo.Obstacle.Id);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Duplicate obstacle ID '{spawnInfo.Obstacle.Id}' found when spawning at Location '{location.Name}'. " +
                        $"Obstacle IDs must be globally unique across all packages.");
                }
                Console.WriteLine($"[InvestigationActivity] Spawned obstacle '{spawnInfo.Obstacle.Name}' at Location '{location.Name}'");
                _messageSystem.AddSystemMessage(
                    $"New obstacle appeared at {location.Name}: {spawnInfo.Obstacle.Name}",
                    SystemMessageTypes.Warning);
                break;

            case ObstacleSpawnTargetType.Route:
                // Find route in WorldState.Routes
                RouteOption route = _gameWorld.WorldState.Routes.FirstOrDefault(r => r.Id == spawnInfo.TargetEntityId);
                if (route == null)
                {
                    Console.WriteLine($"[InvestigationActivity] WARNING: Cannot spawn obstacle '{spawnInfo.Obstacle.Name}' - Route '{spawnInfo.TargetEntityId}' not found");
                    return;
                }
                route.Obstacles.Add(spawnInfo.Obstacle);
                Console.WriteLine($"[InvestigationActivity] Spawned obstacle '{spawnInfo.Obstacle.Name}' on Route '{route.Name}'");
                _messageSystem.AddSystemMessage(
                    $"New obstacle appeared on route to {route.Name}: {spawnInfo.Obstacle.Name}",
                    SystemMessageTypes.Warning);
                break;

            case ObstacleSpawnTargetType.NPC:
                NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == spawnInfo.TargetEntityId);
                if (npc == null)
                {
                    Console.WriteLine($"[InvestigationActivity] WARNING: Cannot spawn obstacle '{spawnInfo.Obstacle.Name}' - NPC '{spawnInfo.TargetEntityId}' not found");
                    return;
                }
                // Validate: NPCs can ONLY have SocialDifficulty obstacles
                if (spawnInfo.Obstacle.PhysicalDanger > 0 || spawnInfo.Obstacle.MentalComplexity > 0)
                {
                    Console.WriteLine($"[InvestigationActivity] ERROR: Cannot spawn obstacle '{spawnInfo.Obstacle.Name}' on NPC '{npc.Name}' - NPCs can only have SocialDifficulty obstacles");
                    return;
                }
                // Duplicate ID protection - prevent data corruption
                if (!_gameWorld.Obstacles.Any(o => o.Id == spawnInfo.Obstacle.Id))
                {
                    _gameWorld.Obstacles.Add(spawnInfo.Obstacle);
                    npc.ObstacleIds.Add(spawnInfo.Obstacle.Id);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Duplicate obstacle ID '{spawnInfo.Obstacle.Id}' found when spawning on NPC '{npc.Name}'. " +
                        $"Obstacle IDs must be globally unique across all packages.");
                }
                Console.WriteLine($"[InvestigationActivity] Spawned obstacle '{spawnInfo.Obstacle.Name}' on NPC '{npc.Name}'");
                _messageSystem.AddSystemMessage(
                    $"New social obstacle with {npc.Name}: {spawnInfo.Obstacle.Name}",
                    SystemMessageTypes.Warning);
                break;

            default:
                Console.WriteLine($"[InvestigationActivity] ERROR: Unknown ObstacleSpawnTargetType: {spawnInfo.TargetType}");
                break;
        }
    }

}
