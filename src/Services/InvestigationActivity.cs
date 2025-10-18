using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.GameState.Enums;

/// <summary>
/// Investigation service - provides operations for investigation lifecycle
/// STATE-LESS: All state lives in GameWorld.InvestigationJournal
/// Does NOT spawn tactical sessions - creates LocationGoals that existing goal system evaluates
/// </summary>
public class InvestigationActivity
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;

    private InvestigationDiscoveryResult _pendingDiscoveryResult;
    private InvestigationActivationResult _pendingActivationResult;
    private InvestigationProgressResult _pendingProgressResult;
    private InvestigationCompleteResult _pendingCompleteResult;
    private InvestigationIntroResult _pendingIntroResult;

    public InvestigationActivity(
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
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == investigation.IntroAction.LocationId);
        Venue venue = location != null
            ? _gameWorld.Venues.FirstOrDefault(v => v.Id == location.VenueId)
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
            SpotName = location?.Name ?? investigation.IntroAction.LocationId
        };
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
            InvestigationId = investigationId
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

        // Grant Understanding from phase completion rewards (0-10 max)
        if (completedPhase.CompletionReward != null && completedPhase.CompletionReward.UnderstandingReward > 0)
        {
            Player player = _gameWorld.GetPlayer();
            int newUnderstanding = Math.Min(10, player.Understanding + completedPhase.CompletionReward.UnderstandingReward);
            player.Understanding = newUnderstanding;

            _messageSystem.AddSystemMessage(
                $"Understanding increased by {completedPhase.CompletionReward.UnderstandingReward} (now {newUnderstanding}/10)",
                SystemMessageTypes.Success);
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

        // NOTE: Obstacles no longer have InvestigationId property - investigations tracked via Understanding resource
        // Progress now measured by Understanding accumulated, not obstacle counts
        int resolvedObstacleCount = 0; // Legacy - UI needs redesign
        int totalObstacleCount = 1; // Legacy - UI needs redesign

        // Build result for UI modal
        InvestigationProgressResult result = new InvestigationProgressResult
        {
            InvestigationId = investigationId,
            InvestigationName = investigation.Name,
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

        // NOTE: Obstacles no longer have InvestigationId property - investigations tracked via Understanding resource
        // Completion now based on Understanding reaching requirement, not obstacle counts
        // TODO: Add UnderstandingRequired property to Investigation model
        int requiredUnderstanding = 10; // Default completion threshold
        if (activeInv.UnderstandingAccumulated < requiredUnderstanding)
        {
            return null; // Not yet complete - need more Understanding
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
            Rewards = new InvestigationRewards
            {
                Coins = investigation.CompletionRewardCoins,
                XPRewards = investigation.CompletionRewardXP ?? new List<StatXPReward>(),
                NPCReputation = new List<NPCReputationReward>() // Future: NPC reputation system
            }
            // ObservationCardRewards eliminated - observation system removed
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

        // Grant coins
        if (investigation.CompletionRewardCoins > 0)
        {
            player.Coins += investigation.CompletionRewardCoins;
        }

        // Grant items (equipment) - add equipment IDs to player inventory
        if (investigation.CompletionRewardItems != null && investigation.CompletionRewardItems.Count > 0)
        {
            foreach (string itemId in investigation.CompletionRewardItems)
            {
                player.Inventory.AddItem(itemId);
                _messageSystem.AddSystemMessage(
                    $"Received equipment: {itemId}",
                    SystemMessageTypes.Success);
            }
        }

        // Grant player stat XP rewards
        if (investigation.CompletionRewardXP != null && investigation.CompletionRewardXP.Count > 0)
        {
            foreach (StatXPReward xpReward in investigation.CompletionRewardXP)
            {
                player.Stats.AddXP(xpReward.Stat, xpReward.XPAmount);
                _messageSystem.AddSystemMessage(
                    $"Gained {xpReward.XPAmount} {xpReward.Stat} XP",
                    SystemMessageTypes.Success);
            }
        }

        // Spawn new investigations
        if (investigation.SpawnedInvestigationIds != null && investigation.SpawnedInvestigationIds.Count > 0)
        {
            foreach (string investigationId in investigation.SpawnedInvestigationIds)
            {
                Investigation spawnedInvestigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
                if (spawnedInvestigation != null)
                {
                    // Move to Discovered state (player must accept it via intro action)
                    _gameWorld.InvestigationJournal.DiscoveredInvestigationIds.Add(investigationId);
                    _messageSystem.AddSystemMessage(
                        $"New investigation available: {spawnedInvestigation.Name}",
                        SystemMessageTypes.Info);
                }
            }
        }

        // ObservationCardRewards system eliminated - replaced by transparent resource competition
    }

    /// <summary>
    /// Discover investigation - moves Potential → Discovered
    /// Simple RPG pattern: Just discovery, no goal spawning (intro action is NOT a goal)
    /// Sets pending discovery result for UI modal display
    /// </summary>
    public void DiscoverInvestigation(string investigationId)
    {
        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation == null)
            throw new ArgumentException($"Investigation '{investigationId}' not found");

        if (investigation.IntroAction == null)
            throw new InvalidOperationException($"Investigation '{investigationId}' has no intro action defined");

        // Move Potential → Discovered
        _gameWorld.InvestigationJournal.PotentialInvestigationIds.Remove(investigationId);
        _gameWorld.InvestigationJournal.DiscoveredInvestigationIds.Add(investigationId);// Derive venue from location (LocationId is globally unique)
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == investigation.IntroAction.LocationId);
        Venue venue = location != null
            ? _gameWorld.Venues.FirstOrDefault(v => v.Id == location.VenueId)
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

        // Grant Understanding from intro completion reward (0-10 max)
        if (investigation.IntroAction?.CompletionReward != null &&
            investigation.IntroAction.CompletionReward.UnderstandingReward > 0)
        {
            Player player = _gameWorld.GetPlayer();
            int newUnderstanding = Math.Min(10, player.Understanding + investigation.IntroAction.CompletionReward.UnderstandingReward);
            player.Understanding = newUnderstanding;

            _messageSystem.AddSystemMessage(
                $"Understanding increased by {investigation.IntroAction.CompletionReward.UnderstandingReward} (now {newUnderstanding}/10)",
                SystemMessageTypes.Success);
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
