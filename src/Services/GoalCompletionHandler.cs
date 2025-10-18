using System;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Handles goal completion lifecycle - marking complete, removing from ActiveGoals if DeleteOnSuccess, and investigation progress
/// </summary>
public class GoalCompletionHandler
{
    private readonly GameWorld _gameWorld;
    private readonly InvestigationActivity _investigationActivity;

    public GoalCompletionHandler(GameWorld gameWorld, InvestigationActivity investigationActivity)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _investigationActivity = investigationActivity ?? throw new ArgumentNullException(nameof(investigationActivity));
    }

    /// <summary>
    /// Complete a goal - mark as complete, remove from ActiveGoals if DeleteOnSuccess=true, and check investigation progress
    /// </summary>
    /// <param name="goal">Goal that was successfully completed</param>
    public void CompleteGoal(Goal goal)
    {
        if (goal == null)
            throw new ArgumentNullException(nameof(goal));

        // Mark goal as complete
        goal.Complete();

        // Apply rewards from all achieved goal cards (idempotent - only if not already achieved)
        ApplyGoalCardRewards(goal);

        // If DeleteOnSuccess, remove from ActiveGoals
        if (goal.DeleteOnSuccess)
        {
            RemoveGoalFromActiveGoals(goal);
        }
        else
        {
        }

        // Check for investigation progress (system-agnostic - works for Mental, Physical, Social)
        if (!string.IsNullOrEmpty(goal.InvestigationId))
        {
            CheckInvestigationProgress(goal.Id, goal.InvestigationId);
        }
    }

    /// <summary>
    /// Check for investigation progress when goal completes
    /// Handles both intro actions (Discovered → Active) and regular goals (phase progression)
    /// </summary>
    private void CheckInvestigationProgress(string goalId, string investigationId)
    {
        // Check if this is an intro action (Discovered → Active transition)
        Goal goal = _gameWorld.Goals.FirstOrDefault(g => g.Id == goalId);
        if (goal != null && goal.IsIntroAction)
        {
            // This is intro completion - activate investigation and spawn Phase 1
            _investigationActivity.CompleteIntroAction(investigationId);
            return;
        }

        // Regular goal completion
        InvestigationProgressResult progressResult = _investigationActivity.CompleteGoal(goalId, investigationId);

        // Log progress for UI modal display (UI will handle modal)

        // Check if investigation is now complete
        InvestigationCompleteResult completeResult = _investigationActivity.CheckInvestigationCompletion(investigationId);
        if (completeResult != null)
        {
            // Investigation complete - UI will display completion modal
        }
    }

    /// <summary>
    /// Handle goal failure - goal always remains in ActiveGoals for retry
    /// </summary>
    /// <param name="goal">Goal that failed</param>
    public void FailGoal(Goal goal)
    {
        if (goal == null)
            throw new ArgumentNullException(nameof(goal));

        // Goals remain in ActiveGoals on failure regardless of DeleteOnSuccess
        // Player can retry the goal
    }

    /// <summary>
    /// Apply rewards from all achieved goal cards (idempotent - only applies rewards once per card)
    /// </summary>
    private void ApplyGoalCardRewards(Goal goal)
    {
        if (goal.GoalCards == null || goal.GoalCards.Count == 0)
        {
            return;
        }

        Player player = _gameWorld.GetPlayer();

        foreach (GoalCard goalCard in goal.GoalCards)
        {
            // Only apply rewards if card is achieved and not already rewarded (idempotent)
            if (!goalCard.IsAchieved)
            {
                continue;
            }

            if (goalCard.Rewards == null)
            {
                continue;
            }

            GoalCardRewards rewards = goalCard.Rewards;

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

            // INVESTIGATION CUBES - grant to goal's placement Location (localized mastery)
            if (rewards.InvestigationCubes.HasValue && rewards.InvestigationCubes.Value > 0)
            {
                if (!string.IsNullOrEmpty(goal.PlacementLocationId))
                {
                    _gameWorld.GrantLocationCubes(goal.PlacementLocationId, rewards.InvestigationCubes.Value);
                    Location location = _gameWorld.GetLocation(goal.PlacementLocationId);
                    string locationName = location.Name;
                }
                else
                {
                }
            }

            // STORY CUBES - grant to goal's placement NPC (localized mastery)
            if (rewards.StoryCubes.HasValue && rewards.StoryCubes.Value > 0)
            {
                if (!string.IsNullOrEmpty(goal.PlacementNpcId))
                {
                    _gameWorld.GrantNPCCubes(goal.PlacementNpcId, rewards.StoryCubes.Value);
                    NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == goal.PlacementNpcId);
                    string npcName = npc.Name;
                }
                else
                {
                }
            }

            // EXPLORATION CUBES - grant to route (requires route context - currently not implemented)
            if (rewards.ExplorationCubes.HasValue && rewards.ExplorationCubes.Value > 0)
            {
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

            // OBLIGATION ID - spawn existing investigation (SelfDiscovered or NPCCommissioned)
            if (!string.IsNullOrEmpty(rewards.ObligationId))
            {
                Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == rewards.ObligationId);
                if (investigation != null)
                {
                    // Move investigation to Discovered state (player must accept via intro action)
                    if (!_gameWorld.InvestigationJournal.DiscoveredInvestigationIds.Contains(rewards.ObligationId))
                    {
                        _gameWorld.InvestigationJournal.PotentialInvestigationIds.Remove(rewards.ObligationId);
                        _gameWorld.InvestigationJournal.DiscoveredInvestigationIds.Add(rewards.ObligationId);
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }

            // ROUTE SEGMENT UNLOCK - reveal hidden path by reducing HiddenUntilExploration threshold
            if (rewards.RouteSegmentUnlock != null)
            {
                RouteSegmentUnlock unlock = rewards.RouteSegmentUnlock;
                RouteOption route = _gameWorld.Routes.FirstOrDefault(r => r.Id == unlock.RouteId);
                if (route != null)
                {
                    if (unlock.SegmentPosition >= 0 && unlock.SegmentPosition < route.Segments.Count)
                    {
                        RouteSegment segment = route.Segments[unlock.SegmentPosition];
                        RoutePath path = segment.AvailablePaths.FirstOrDefault(p => p.Id == unlock.PathId);
                        if (path != null)
                        {
                            // Reveal hidden path by setting HiddenUntilExploration to 0 (always visible)
                            int previousThreshold = path.HiddenUntilExploration;
                            path.HiddenUntilExploration = 0;
                        }
                        else
                        {
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

            // OBSTACLE PROPERTY REDUCTION - reduce intensity of parent obstacle (if goal has PropertyReduction)
            if (rewards.ObstacleReduction != null && rewards.ObstacleReduction.HasAnyReduction())
            {
                // Find parent obstacle by checking all obstacles for this goalId in their GoalIds list
                Obstacle parentObstacle = _gameWorld.Obstacles.FirstOrDefault(o => o.GoalIds.Contains(goal.Id));
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
    /// Remove goal from NPC.ActiveGoalIds or Location.ActiveGoalIds
    /// </summary>
    private void RemoveGoalFromActiveGoals(Goal goal)
    {
        if (!string.IsNullOrEmpty(goal.PlacementNpcId))
        {
            // Remove from NPC.ActiveGoalIds
            NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == goal.PlacementNpcId);
            if (npc != null)
            {
                if (npc.ActiveGoalIds.Contains(goal.Id))
                {
                    npc.ActiveGoalIds.Remove(goal.Id);
                }
            }
        }
        else if (!string.IsNullOrEmpty(goal.PlacementLocationId))
        {
            // Remove from Location.ActiveGoalIds
            Location location = _gameWorld.GetLocation(goal.PlacementLocationId);
            if (location != null)
            {
                if (location.ActiveGoalIds.Contains(goal.Id))
                {
                    location.ActiveGoalIds.Remove(goal.Id);
                }
            }
        }
    }
}
