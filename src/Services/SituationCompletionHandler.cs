using System;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Handles situation completion lifecycle - marking complete, removing from ActiveSituations if DeleteOnSuccess, and obligation progress
/// </summary>
public class SituationCompletionHandler
{
    private readonly GameWorld _gameWorld;
    private readonly ObligationActivity _obligationActivity;
    private readonly TimeManager _timeManager;
    private readonly ConsequenceFacade _consequenceFacade;

    public SituationCompletionHandler(
        GameWorld gameWorld,
        ObligationActivity obligationActivity,
        TimeManager timeManager,
        ConsequenceFacade consequenceFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _obligationActivity = obligationActivity ?? throw new ArgumentNullException(nameof(obligationActivity));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _consequenceFacade = consequenceFacade ?? throw new ArgumentNullException(nameof(consequenceFacade));
    }

    /// <summary>
    /// Complete a situation - mark as complete, remove from ActiveSituations if DeleteOnSuccess=true, and check obligation progress
    /// </summary>
    /// <param name="situation">Situation that was successfully completed</param>
    public void CompleteSituation(Situation situation)
    {
        if (situation == null)
            throw new ArgumentNullException(nameof(situation));

        // Mark situation as complete
        situation.Complete();

        // Scene-Situation Architecture: Apply ProjectedConsequences (bonds, scales, states)
        _consequenceFacade.ApplyConsequences(
            situation.ProjectedBondChanges,
            situation.ProjectedScaleShifts,
            situation.ProjectedStates
        );

        // Apply rewards from all achieved situation cards (idempotent - only if not already achieved)
        ApplySituationCardRewards(situation);

        // If DeleteOnSuccess, remove from ActiveSituations
        if (situation.DeleteOnSuccess)
        {
            RemoveSituationFromActiveSituations(situation);
        }

        // Check for simple obligation completion (Player.ActiveObligationIds system)
        CheckSimpleObligationCompletion(situation);

        // Check for obligation progress (phase-based ObligationJournal system)
        if (!string.IsNullOrEmpty(situation.Obligation?.Id))
        {
            CheckObligationProgress(situation.Id, situation.Obligation?.Id);
        }

        // TODO Phase D: Execute spawn rules via SpawnFacade
    }

    /// <summary>
    /// Check for obligation progress when situation completes
    /// Handles both intro actions (Discovered → Active) and regular situations (phase progression)
    /// </summary>
    private void CheckObligationProgress(string situationId, string obligationId)
    {
        // Check if this is an intro action (Discovered → Active transition)
        Situation situation = _gameWorld.Situations.FirstOrDefault(g => g.Id == situationId);
        if (situation != null && situation.IsIntroAction)
        {
            // This is intro completion - activate obligation and spawn Phase 1
            _obligationActivity.CompleteIntroAction(obligationId);
            return;
        }

        // Regular situation completion
        ObligationProgressResult progressResult = _obligationActivity.CompleteSituation(situationId, obligationId);

        // Log progress for UI modal display (UI will handle modal)

        // Check if obligation is now complete
        ObligationCompleteResult completeResult = _obligationActivity.CheckObligationCompletion(obligationId);
        if (completeResult != null)
        {
            // Obligation complete - UI will display completion modal
        }
    }

    /// <summary>
    /// Handle situation failure - situation always remains in ActiveSituations for retry
    /// </summary>
    /// <param name="situation">Situation that failed</param>
    public void FailSituation(Situation situation)
    {
        if (situation == null)
            throw new ArgumentNullException(nameof(situation));

        // Situations remain in ActiveSituations on failure regardless of DeleteOnSuccess
        // Player can retry the situation
    }

    /// <summary>
    /// Apply rewards from all achieved situation cards (idempotent - only applies rewards once per card)
    /// </summary>
    private void ApplySituationCardRewards(Situation situation)
    {
        Player player = _gameWorld.GetPlayer();

        foreach (SituationCard situationCard in situation.SituationCards)
        {
            // Only apply rewards if card is achieved and not already rewarded (idempotent)
            if (!situationCard.IsAchieved)
            {
                continue;
            }

            SituationCardRewards rewards = situationCard.Rewards;

            // COINS - direct player currency
            if (rewards.Coins.HasValue && rewards.Coins.Value > 0)
            {
                player.AddCoins(rewards.Coins.Value);
            }

            // EQUIPMENT - add to player inventory by ID
            if (!string.IsNullOrEmpty(rewards.EquipmentId))
            {
                player.Inventory.AddItem(rewards.EquipmentId);
            }

            // OBLIGATION CUBES - grant to situation's placement Location (localized mastery)
            if (rewards.InvestigationCubes.HasValue && rewards.InvestigationCubes.Value > 0)
            {
                if (!string.IsNullOrEmpty(situation.PlacementLocation?.Id))
                {
                    _gameWorld.GrantLocationCubes(situation.PlacementLocation?.Id, rewards.InvestigationCubes.Value);
                    Location location = _gameWorld.GetLocation(situation.PlacementLocation?.Id);
                    string locationName = location.Name;
                }
                else
                {
                }
            }

            // STORY CUBES - grant to situation's placement NPC (localized mastery)
            if (rewards.StoryCubes.HasValue && rewards.StoryCubes.Value > 0)
            {
                if (!string.IsNullOrEmpty(situation.PlacementNpc?.ID))
                {
                    _gameWorld.GrantNPCCubes(situation.PlacementNpc?.ID, rewards.StoryCubes.Value);
                    NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == situation.PlacementNpc?.ID);
                    string npcName = npc.Name;
                }
                else
                {
                }
            }

            // EXPLORATION CUBES - grant to route (requires route context from situation)
            if (rewards.ExplorationCubes.HasValue && rewards.ExplorationCubes.Value > 0)
            {
                if (!string.IsNullOrEmpty(situation.PlacementRouteId))
                {
                    _gameWorld.GrantRouteCubes(situation.PlacementRouteId, rewards.ExplorationCubes.Value);
                    RouteOption route = _gameWorld.Routes.FirstOrDefault(r => r.Id == situation.PlacementRouteId);
                    string routeName = route?.Name ?? "Unknown Route";
                }
                else
                {
                    // Situation without route context - exploration cubes can't be granted
                    // This is expected for non-route situations
                }
            }

            // CREATE OBLIGATION - grant StoryCubes to patron NPC (RESOURCE-BASED PATTERN)
            // PRINCIPLE 4: No boolean gates - visibility based on resource thresholds
            if (rewards.CreateObligationData != null)
            {
                CreateObligationReward data = rewards.CreateObligationData;

                NPC patron = _gameWorld.NPCs.FirstOrDefault(n => n.ID == data.PatronNpcId);
                if (patron == null)
                {
                }
                else
                {
                    // Grant StoryCubes to patron (max 10)
                    int previousCubes = patron.StoryCubes;
                    patron.StoryCubes = Math.Min(10, patron.StoryCubes + data.StoryCubesGranted);
                }
            }

            // OBLIGATION ID - activate existing obligation (SelfDiscovered or NPCCommissioned)
            // Adds to Player.ActiveObligationIds and sets deadline if NPCCommissioned
            if (!string.IsNullOrEmpty(rewards.ObligationId))
            {
                Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == rewards.ObligationId);
                if (obligation != null)
                {
                    // Activate obligation (adds to Player.ActiveObligationIds, sets deadline if NPCCommissioned)
                    _gameWorld.ActivateObligation(rewards.ObligationId, _timeManager);
                }
            }

            // ROUTE SEGMENT UNLOCK - reveal hidden PathCard by setting ExplorationThreshold to 0
            if (rewards.RouteSegmentUnlock != null)
            {
                RouteSegmentUnlock unlock = rewards.RouteSegmentUnlock;
                RouteOption route = _gameWorld.Routes.FirstOrDefault(r => r.Id == unlock.RouteId);
                if (route != null)
                {
                    if (unlock.SegmentPosition >= 0 && unlock.SegmentPosition < route.Segments.Count)
                    {
                        RouteSegment segment = route.Segments[unlock.SegmentPosition];

                        // Get PathCard collection for this segment
                        if (!string.IsNullOrEmpty(segment.PathCollectionId))
                        {
                            PathCardCollectionDTO collection = _gameWorld.AllPathCollections.GetCollection(segment.PathCollectionId);
                            if (collection != null)
                            {
                                PathCardDTO pathCard = collection.PathCards.FirstOrDefault(p => p.Id == unlock.PathId);
                                if (pathCard != null)
                                {
                                    // Reveal hidden PathCard by setting ExplorationThreshold to 0 (always visible)
                                    int previousThreshold = pathCard.ExplorationThreshold;
                                    pathCard.ExplorationThreshold = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }

            // OBSTACLE PROPERTY REDUCTION - reduce intensity of parent obstacle (if situation has PropertyReduction)
            if (rewards.ObstacleReduction != null && rewards.ObstacleReduction.HasAnyReduction())
            {
                // Find parent obstacle by checking all obstacles for this situationId in their SituationIds list
                Obstacle parentObstacle = _gameWorld.Obstacles.FirstOrDefault(o => o.SituationIds.Contains(situation.Id));
                if (parentObstacle != null)
                {
                    int previousIntensity = parentObstacle.Intensity;
                    parentObstacle.Intensity = Math.Max(0, parentObstacle.Intensity - rewards.ObstacleReduction.ReduceIntensity);

                    // If obstacle is now cleared, mark as resolved
                    if (parentObstacle.IsCleared() && parentObstacle.State == ObstacleState.Active)
                    {
                        parentObstacle.State = ObstacleState.Resolved;
                    }
                }
                else
                {
                }
            }
        }
    }

    /// <summary>
    /// Remove situation from NPC.ActiveSituationIds or Location.ActiveSituationIds
    /// </summary>
    private void RemoveSituationFromActiveSituations(Situation situation)
    {
        if (!string.IsNullOrEmpty(situation.PlacementNpc?.ID))
        {
            // Remove from NPC.ActiveSituationIds
            NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == situation.PlacementNpc?.ID);
            if (npc != null)
            {
                if (npc.ActiveSituationIds.Contains(situation.Id))
                {
                    npc.ActiveSituationIds.Remove(situation.Id);
                }
            }
        }
        else if (!string.IsNullOrEmpty(situation.PlacementLocation?.Id))
        {
            // Remove from Location.ActiveSituationIds
            Location location = _gameWorld.GetLocation(situation.PlacementLocation?.Id);
            if (location != null)
            {
                if (location.ActiveSituationIds.Contains(situation.Id))
                {
                    location.ActiveSituationIds.Remove(situation.Id);
                }
            }
        }
    }

    /// <summary>
    /// Check if completing this situation completes an active obligation (simple Player.ActiveObligationIds system)
    /// Query all situations with matching ObligationId to determine completion
    /// </summary>
    private void CheckSimpleObligationCompletion(Situation completedSituation)
    {
        // Only check if situation is part of an obligation
        if (string.IsNullOrEmpty(completedSituation.Obligation?.Id))
            return;

        // Only check if obligation is in Player.ActiveObligationIds
        Player player = _gameWorld.GetPlayer();
        if (!player.ActiveObligationIds.Contains(completedSituation.Obligation?.Id))
            return;

        // Find all situations for this obligation
        List<Situation> obligationSituations = _gameWorld.Situations
            .Where(g => g.Obligation?.Id == completedSituation.Obligation?.Id)
            .ToList();

        // Check if ALL situations are complete
        bool allSituationsComplete = obligationSituations.All(g => g.IsCompleted);

        if (allSituationsComplete)
        {
            // Complete the obligation (grants rewards, removes from ActiveObligationIds)
            _gameWorld.CompleteObligation(completedSituation.Obligation?.Id, _timeManager);
        }
    }
}
