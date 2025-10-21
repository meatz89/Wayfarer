using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.GameState.Enums;

/// <summary>
/// Obligation service - provides operations for obligation lifecycle
/// STATE-LESS: All state lives in GameWorld.ObligationJournal
/// Does NOT spawn tactical sessions - creates LocationGoals that existing goal system evaluates
/// </summary>
public class ObligationActivity
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;

    private ObligationDiscoveryResult _pendingDiscoveryResult;
    private ObligationActivationResult _pendingActivationResult;
    private ObligationProgressResult _pendingProgressResult;
    private ObligationCompleteResult _pendingCompleteResult;
    private ObligationIntroResult _pendingIntroResult;

    public ObligationActivity(
        GameWorld gameWorld,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }

    /// <summary>
    /// Get and clear pending discovery result for UI modal display
    /// Returns null if no result pending
    /// </summary>
    public ObligationDiscoveryResult GetAndClearPendingDiscoveryResult()
    {
        ObligationDiscoveryResult result = _pendingDiscoveryResult;
        _pendingDiscoveryResult = null;
        return result;
    }

    /// <summary>
    /// Get and clear pending progress result for UI modal display
    /// Returns null if no result pending
    /// </summary>
    public ObligationProgressResult GetAndClearPendingProgressResult()
    {
        ObligationProgressResult result = _pendingProgressResult;
        _pendingProgressResult = null;
        return result;
    }

    /// <summary>
    /// Get and clear pending completion result for UI modal display
    /// Returns null if no result pending
    /// </summary>
    public ObligationCompleteResult GetAndClearPendingCompleteResult()
    {
        ObligationCompleteResult result = _pendingCompleteResult;
        _pendingCompleteResult = null;
        return result;
    }

    /// <summary>
    /// Get and clear pending activation result for UI modal display
    /// Returns null if no result pending
    /// </summary>
    public ObligationActivationResult GetAndClearPendingActivationResult()
    {
        ObligationActivationResult result = _pendingActivationResult;
        _pendingActivationResult = null;
        return result;
    }

    /// <summary>
    /// Get and clear pending intro result for UI modal display
    /// Returns null if no result pending
    /// </summary>
    public ObligationIntroResult GetAndClearPendingIntroResult()
    {
        ObligationIntroResult result = _pendingIntroResult;
        _pendingIntroResult = null;
        return result;
    }

    /// <summary>
    /// Set pending intro action - prepares quest acceptance modal but doesn't activate
    /// RPG Pattern: Button click → Modal → "Begin Obligation" → Activate
    /// </summary>
    public void SetPendingIntroAction(string obligationId)
    {
        Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
        if (obligation == null)
            throw new ArgumentException($"Obligation '{obligationId}' not found");

        if (obligation.IntroAction == null)
            throw new InvalidOperationException($"Obligation '{obligationId}' has no intro action");

        // Derive venue from location
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == obligation.IntroAction.LocationId);
        if (location == null)
            throw new InvalidOperationException($"Location '{obligation.IntroAction.LocationId}' not found for obligation intro action");

        Venue venue = _gameWorld.Venues.FirstOrDefault(v => v.Id == location.VenueId);
        if (venue == null)
            throw new InvalidOperationException($"Venue '{location.VenueId}' not found for location '{location.Id}'");

        // Create intro result for quest acceptance modal
        _pendingIntroResult = new ObligationIntroResult
        {
            ObligationId = obligationId,
            ObligationName = obligation.Name,
            IntroNarrative = obligation.IntroAction.IntroNarrative,
            IntroActionText = obligation.IntroAction.ActionText,
            ColorCode = obligation.ColorCode,
            LocationName = venue.Name,
            SpotName = location.Name
        };
    }

    /// <summary>
    /// Activate obligation - looks up goals and spawns them at locations/NPCs
    /// Moves obligation from Pending → Active in GameWorld.ObligationJournal
    /// </summary>
    public void ActivateObligation(string obligationId)
    {
        // Load obligation template from GameWorld
        Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
        if (obligation == null)
        {
            throw new ArgumentException($"Obligation '{obligationId}' not found in GameWorld");
        }

        // Remove from potential and discovered
        _gameWorld.ObligationJournal.PotentialObligationIds.Remove(obligationId);
        _gameWorld.ObligationJournal.DiscoveredObligationIds.Remove(obligationId);

        // Create active obligation
        ActiveObligation activeObligation = new ActiveObligation
        {
            ObligationId = obligationId
        };
        _gameWorld.ObligationJournal.ActiveObligations.Add(activeObligation);

        // NOTE: Obligations no longer spawn goals directly
        // Goals are contained within obstacles as children (containment pattern)
        // Obstacles are spawned by obligation phase completion rewards

        _messageSystem.AddSystemMessage(
            $"Obligation activated: {obligation.Name}",
            SystemMessageTypes.Info);
    }

    /// <summary>
    /// Mark goal complete - checks for obligation progress
    /// Returns ObligationProgressResult for UI modal display
    /// </summary>
    public ObligationProgressResult CompleteGoal(string goalId, string obligationId)
    {
        // Find active obligation
        ActiveObligation activeInv = _gameWorld.ObligationJournal.ActiveObligations
            .FirstOrDefault(inv => inv.ObligationId == obligationId);

        if (activeInv == null)
        {
            throw new ArgumentException($"Obligation '{obligationId}' is not active");
        }

        // Load obligation template
        Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
        if (obligation == null)
        {
            throw new ArgumentException($"Obligation '{obligationId}' not found");
        }

        // Find completed phase definition
        ObligationPhaseDefinition completedPhase = obligation.PhaseDefinitions
            .FirstOrDefault(p => p.Id == goalId);

        if (completedPhase == null)
        {
            throw new ArgumentException($"Phase '{goalId}' not found in obligation '{obligationId}'");
        }

        // Grant Understanding from phase completion rewards (0-10 max)
        if (completedPhase.CompletionReward.UnderstandingReward > 0)
        {
            Player player = _gameWorld.GetPlayer();
            int newUnderstanding = Math.Min(10, player.Understanding + completedPhase.CompletionReward.UnderstandingReward);
            player.Understanding = newUnderstanding;

            _messageSystem.AddSystemMessage(
                $"Understanding increased by {completedPhase.CompletionReward.UnderstandingReward} (now {newUnderstanding}/10)",
                SystemMessageTypes.Success);
        }

        // Spawn obstacles from phase completion rewards
        if (completedPhase.CompletionReward.ObstaclesSpawned.Count > 0)
        {
            foreach (ObstacleSpawnInfo spawnInfo in completedPhase.CompletionReward.ObstaclesSpawned)
            {
                SpawnObstacle(spawnInfo);
            }
        }

        // NOTE: Goals are no longer spawned by obligations
        // Goals are contained within obstacles as children (containment pattern)
        // New leads come from obstacle-spawned goals, not phase-spawned goals
        List<NewLeadInfo> newLeads = new List<NewLeadInfo>();

        // NOTE: Obstacles no longer have ObligationId property - obligations tracked via Understanding resource
        // Progress now measured by Understanding accumulated, not obstacle counts
        int resolvedObstacleCount = 0; // Legacy - UI needs redesign
        int totalObstacleCount = 1; // Legacy - UI needs redesign

        // Build result for UI modal
        ObligationProgressResult result = new ObligationProgressResult
        {
            ObligationId = obligationId,
            ObligationName = obligation.Name,
            CompletedGoalName = completedPhase.Name,
            OutcomeNarrative = completedPhase.OutcomeNarrative,
            NewLeads = newLeads,
            CompletedGoalCount = resolvedObstacleCount,
            TotalGoalCount = totalObstacleCount
        };

        _pendingProgressResult = result;

        return result;
    }

    /// <summary>
    /// Check if obligation is complete - all goals done
    /// Returns ObligationCompleteResult if complete, null otherwise
    /// </summary>
    public ObligationCompleteResult CheckObligationCompletion(string obligationId)
    {
        // Find active obligation
        ActiveObligation activeInv = _gameWorld.ObligationJournal.ActiveObligations
            .FirstOrDefault(inv => inv.ObligationId == obligationId);

        if (activeInv == null)
        {
            return null; // Not active
        }

        // Load obligation template
        Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
        if (obligation == null)
        {
            return null;
        }

        // NOTE: Obstacles no longer have ObligationId property - obligations tracked via Understanding resource
        // Completion now based on Understanding reaching requirement, not obstacle counts
        // TODO: Add UnderstandingRequired property to Obligation model
        int requiredUnderstanding = 10; // Default completion threshold
        if (activeInv.UnderstandingAccumulated < requiredUnderstanding)
        {
            return null; // Not yet complete - need more Understanding
        }

        // Move from Active → Completed
        _gameWorld.ObligationJournal.ActiveObligations.Remove(activeInv);
        _gameWorld.ObligationJournal.CompletedObligationIds.Add(obligationId);

        // Grant rewards
        GrantObligationRewards(obligation);

        // Build result for UI modal
        ObligationCompleteResult result = new ObligationCompleteResult
        {
            ObligationId = obligationId,
            ObligationName = obligation.Name,
            CompletionNarrative = obligation.CompletionNarrative,
            Rewards = new ObligationRewards
            {
                Coins = obligation.CompletionRewardCoins,
                XPRewards = obligation.CompletionRewardXP,
                NPCReputation = new List<NPCReputationReward>() // Future: NPC reputation system
            }
            // ObservationCardRewards eliminated - observation system removed
        };

        _pendingCompleteResult = result;

        _messageSystem.AddSystemMessage(
            $"Obligation complete: {obligation.Name}",
            SystemMessageTypes.Success);

        return result;
    }

    /// <summary>
    /// Grant obligation completion rewards
    /// </summary>
    private void GrantObligationRewards(Obligation obligation)
    {
        Player player = _gameWorld.GetPlayer();

        // Grant coins
        if (obligation.CompletionRewardCoins > 0)
        {
            player.Coins += obligation.CompletionRewardCoins;
        }

        // Grant items (equipment) - add equipment IDs to player inventory
        foreach (string itemId in obligation.CompletionRewardItems)
        {
            player.Inventory.AddItem(itemId);
            _messageSystem.AddSystemMessage(
                $"Received equipment: {itemId}",
                SystemMessageTypes.Success);
        }

        // Grant player stat XP rewards
        foreach (StatXPReward xpReward in obligation.CompletionRewardXP)
        {
            player.Stats.AddXP(xpReward.Stat, xpReward.XPAmount);
            _messageSystem.AddSystemMessage(
                $"Gained {xpReward.XPAmount} {xpReward.Stat} XP",
                SystemMessageTypes.Success);
        }

        // Spawn new obligations
        foreach (string obligationId in obligation.SpawnedObligationIds)
        {
            Obligation spawnedObligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
            if (spawnedObligation != null)
            {
                // Move to Discovered state (player must accept it via intro action)
                _gameWorld.ObligationJournal.DiscoveredObligationIds.Add(obligationId);
                _messageSystem.AddSystemMessage(
                    $"New obligation available: {spawnedObligation.Name}",
                    SystemMessageTypes.Info);
            }
        }

        // ObservationCardRewards system eliminated - replaced by transparent resource competition
    }

    /// <summary>
    /// Discover obligation - moves Potential → Active and immediately spawns intro obstacles
    /// DISCOVERED = ACTIVE: No intermediate state, discovery immediately activates obligation
    /// Sets pending discovery result for UI modal display
    /// </summary>
    public void DiscoverObligation(string obligationId)
    {
        Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
        if (obligation == null)
            throw new ArgumentException($"Obligation '{obligationId}' not found");

        if (obligation.IntroAction == null)
            throw new InvalidOperationException($"Obligation '{obligationId}' has no intro action defined");

        // Move Potential → Active (skip Discovered state entirely)
        _gameWorld.ObligationJournal.PotentialObligationIds.Remove(obligationId);

        // Create active obligation
        ActiveObligation activeObligation = new ActiveObligation
        {
            ObligationId = obligationId
        };
        _gameWorld.ObligationJournal.ActiveObligations.Add(activeObligation);

        // Spawn obstacles from intro completion reward IMMEDIATELY
        if (obligation.IntroAction.CompletionReward.ObstaclesSpawned.Count > 0)
        {
            foreach (ObstacleSpawnInfo spawnInfo in obligation.IntroAction.CompletionReward.ObstaclesSpawned)
            {
                SpawnObstacle(spawnInfo);
            }
        }

        // Grant Understanding from intro completion reward (0-10 max)
        if (obligation.IntroAction.CompletionReward.UnderstandingReward > 0)
        {
            Player player = _gameWorld.GetPlayer();
            int newUnderstanding = Math.Min(10, player.Understanding + obligation.IntroAction.CompletionReward.UnderstandingReward);
            player.Understanding = newUnderstanding;

            _messageSystem.AddSystemMessage(
                $"Understanding increased by {obligation.IntroAction.CompletionReward.UnderstandingReward} (now {newUnderstanding}/10)",
                SystemMessageTypes.Success);
        }

        // Derive venue from location (LocationId is globally unique)
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == obligation.IntroAction.LocationId);
        if (location == null)
            throw new InvalidOperationException($"Location '{obligation.IntroAction.LocationId}' not found for obligation discovery");

        Venue venue = _gameWorld.Venues.FirstOrDefault(v => v.Id == location.VenueId);
        if (venue == null)
            throw new InvalidOperationException($"Venue '{location.VenueId}' not found for location '{location.Id}'");

        // Create discovery result for UI modal (narrative only)
        ObligationDiscoveryResult discoveryResult = new ObligationDiscoveryResult
        {
            ObligationId = obligationId,
            ObligationName = obligation.Name,
            IntroNarrative = obligation.IntroAction.IntroNarrative,
            IntroActionText = obligation.IntroAction.ActionText,
            ColorCode = obligation.ColorCode,
            LocationName = venue.Name,
            SpotName = location.Name
        };
        _pendingDiscoveryResult = discoveryResult;

        _messageSystem.AddSystemMessage(
            $"Obligation discovered: {obligation.Name}",
            SystemMessageTypes.Info);
    }

    /// <summary>
    /// Complete intro action - LEGACY METHOD, now redundant
    /// DiscoverObligation() now immediately activates and spawns obstacles
    /// This method is safe to call but does nothing if obligation already active
    /// </summary>
    public void CompleteIntroAction(string obligationId)
    {
        // Check if already active - discovery now activates immediately
        ActiveObligation activeInv = _gameWorld.ObligationJournal.ActiveObligations
            .FirstOrDefault(inv => inv.ObligationId == obligationId);

        if (activeInv != null)
        {
            // Already active - discovery handled everything
            return;
        }

        // Legacy path - should not be reached with new flow
        Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
        if (obligation == null)
        {
            throw new ArgumentException($"Obligation '{obligationId}' not found");
        }

        // Activate obligation (moves Discovered → Active)
        ActivateObligation(obligationId);

        // Spawn obstacles from intro completion reward
        if (obligation.IntroAction.CompletionReward.ObstaclesSpawned.Count > 0)
        {
            foreach (ObstacleSpawnInfo spawnInfo in obligation.IntroAction.CompletionReward.ObstaclesSpawned)
            {
                SpawnObstacle(spawnInfo);
            }
        }

        // Grant Understanding from intro completion reward (0-10 max)
        if (obligation.IntroAction.CompletionReward.UnderstandingReward > 0)
        {
            Player player = _gameWorld.GetPlayer();
            int newUnderstanding = Math.Min(10, player.Understanding + obligation.IntroAction.CompletionReward.UnderstandingReward);
            player.Understanding = newUnderstanding;

            _messageSystem.AddSystemMessage(
                $"Understanding increased by {obligation.IntroAction.CompletionReward.UnderstandingReward} (now {newUnderstanding}/10)",
                SystemMessageTypes.Success);
        }

        // Create activation result for UI modal
        _pendingActivationResult = new ObligationActivationResult
        {
            ObligationId = obligationId,
            ObligationName = obligation.Name,
            IntroNarrative = obligation.IntroAction.IntroNarrative
        };

        _messageSystem.AddSystemMessage(
            $"Obligation activated: {obligation.Name}",
            SystemMessageTypes.Success);
    }

    /// <summary>
    /// Spawn an obstacle at the specified target entity as obligation phase reward
    /// </summary>
    private void SpawnObstacle(ObstacleSpawnInfo spawnInfo)
    {
        if (spawnInfo.Obstacle == null)
            return;

        switch (spawnInfo.TargetType)
        {
            case ObstacleSpawnTargetType.Location:
                Location location = _gameWorld.GetLocation(spawnInfo.TargetEntityId);
                if (location == null)
                {
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
                _messageSystem.AddSystemMessage(
                    $"New obstacle appeared at {location.Name}: {spawnInfo.Obstacle.Name}",
                    SystemMessageTypes.Warning);
                break;

            case ObstacleSpawnTargetType.Route:
                // Find route in GameWorld.Routes
                RouteOption route = _gameWorld.Routes.FirstOrDefault(r => r.Id == spawnInfo.TargetEntityId);
                if (route == null)
                {
                    return;
                }
                // Duplicate ID protection - prevent data corruption
                if (!_gameWorld.Obstacles.Any(o => o.Id == spawnInfo.Obstacle.Id))
                {
                    _gameWorld.Obstacles.Add(spawnInfo.Obstacle);
                    route.ObstacleIds.Add(spawnInfo.Obstacle.Id);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Duplicate obstacle ID '{spawnInfo.Obstacle.Id}' found when spawning on Route '{route.Name}'. " +
                        $"Obstacle IDs must be globally unique across all packages.");
                }
                _messageSystem.AddSystemMessage(
                    $"New obstacle appeared on route to {route.Name}: {spawnInfo.Obstacle.Name}",
                    SystemMessageTypes.Warning);
                break;

            case ObstacleSpawnTargetType.NPC:
                NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == spawnInfo.TargetEntityId);
                if (npc == null)
                {
                    return;
                }
                // Validate: NPCs can ONLY have Social context obstacles
                ObstacleContext[] socialContexts = { ObstacleContext.Authority, ObstacleContext.Deception, ObstacleContext.Persuasion, ObstacleContext.Intimidation, ObstacleContext.Empathy, ObstacleContext.Negotiation, ObstacleContext.Etiquette };
                if (!spawnInfo.Obstacle.Contexts.Any(c => socialContexts.Contains(c)))
                {
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
                _messageSystem.AddSystemMessage(
                    $"New social obstacle with {npc.Name}: {spawnInfo.Obstacle.Name}",
                    SystemMessageTypes.Warning);
                break;

            default: break;
        }
    }

}
