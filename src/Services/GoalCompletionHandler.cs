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
        Console.WriteLine($"[GoalCompletion] Goal '{goal.Name}' marked complete");

        // Apply rewards from all achieved goal cards (idempotent - only if not already achieved)
        ApplyGoalCardRewards(goal);

        // If DeleteOnSuccess, remove from ActiveGoals
        if (goal.DeleteOnSuccess)
        {
            RemoveGoalFromActiveGoals(goal);
            Console.WriteLine($"[GoalCompletion] Goal '{goal.Name}' removed from ActiveGoals (DeleteOnSuccess=true)");
        }
        else
        {
            Console.WriteLine($"[GoalCompletion] Goal '{goal.Name}' remains in ActiveGoals (DeleteOnSuccess=false, repeatable)");
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
        if (_gameWorld.Goals.TryGetValue(goalId, out Goal goal) && goal.IsIntroAction)
        {
            // This is intro completion - activate investigation and spawn Phase 1
            _investigationActivity.CompleteIntroAction(investigationId);

            Console.WriteLine($"[GoalCompletion] Intro action complete - Investigation '{investigationId}' ACTIVATED");
            return;
        }

        // Regular goal completion
        InvestigationProgressResult progressResult = _investigationActivity.CompleteGoal(goalId, investigationId);

        // Log progress for UI modal display (UI will handle modal)
        Console.WriteLine($"[GoalCompletion] Investigation progress: {progressResult.CompletedGoalCount}/{progressResult.TotalGoalCount} goals complete");

        // Check if investigation is now complete
        InvestigationCompleteResult completeResult = _investigationActivity.CheckInvestigationCompletion(investigationId);
        if (completeResult != null)
        {
            // Investigation complete - UI will display completion modal
            Console.WriteLine($"[GoalCompletion] Investigation '{completeResult.InvestigationName}' COMPLETE!");
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
        Console.WriteLine($"[GoalCompletion] Goal '{goal.Name}' failed - remains in ActiveGoals for retry");
    }

    /// <summary>
    /// Apply rewards from all achieved goal cards (idempotent - only applies rewards once per card)
    /// </summary>
    private void ApplyGoalCardRewards(Goal goal)
    {
        if (goal.GoalCards == null || goal.GoalCards.Count == 0)
        {
            Console.WriteLine($"[GoalCompletion] No goal cards to process rewards for '{goal.Name}'");
            return;
        }

        Player player = _gameWorld.GetPlayer();

        foreach (GoalCard goalCard in goal.GoalCards)
        {
            // Only apply rewards if card is achieved and not already rewarded (idempotent)
            if (!goalCard.IsAchieved)
            {
                Console.WriteLine($"[GoalCompletion] GoalCard '{goalCard.Name}' not achieved, skipping rewards");
                continue;
            }

            if (goalCard.Rewards == null)
            {
                Console.WriteLine($"[GoalCompletion] GoalCard '{goalCard.Name}' has no rewards");
                continue;
            }

            GoalCardRewards rewards = goalCard.Rewards;
            Console.WriteLine($"[GoalCompletion] Applying rewards from GoalCard '{goalCard.Name}'");

            // COINS - direct player currency
            if (rewards.Coins.HasValue && rewards.Coins.Value > 0)
            {
                player.AddCoins(rewards.Coins.Value);
                Console.WriteLine($"[GoalCompletion] Granted {rewards.Coins.Value} coins (player total: {player.Coins})");
            }

            // EQUIPMENT - add to player inventory by ID
            if (!string.IsNullOrEmpty(rewards.EquipmentId))
            {
                player.Inventory.AddItem(rewards.EquipmentId);
                Console.WriteLine($"[GoalCompletion] Granted equipment '{rewards.EquipmentId}' to player inventory");
            }

            // INVESTIGATION CUBES - grant to goal's placement Location (localized mastery)
            if (rewards.InvestigationCubes.HasValue && rewards.InvestigationCubes.Value > 0)
            {
                if (!string.IsNullOrEmpty(goal.PlacementLocationId))
                {
                    _gameWorld.GrantLocationCubes(goal.PlacementLocationId, rewards.InvestigationCubes.Value);
                    Location location = _gameWorld.GetLocation(goal.PlacementLocationId);
                    string locationName = location?.Name ?? goal.PlacementLocationId;
                    Console.WriteLine($"[GoalCompletion] Granted {rewards.InvestigationCubes.Value} InvestigationCubes to Location '{locationName}' (total: {location?.InvestigationCubes ?? 0}/10)");
                }
                else
                {
                    Console.WriteLine($"[GoalCompletion] WARNING: Cannot grant InvestigationCubes - goal has no PlacementLocationId");
                }
            }

            // STORY CUBES - grant to goal's placement NPC (localized mastery)
            if (rewards.StoryCubes.HasValue && rewards.StoryCubes.Value > 0)
            {
                if (!string.IsNullOrEmpty(goal.PlacementNpcId))
                {
                    _gameWorld.GrantNPCCubes(goal.PlacementNpcId, rewards.StoryCubes.Value);
                    NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == goal.PlacementNpcId);
                    string npcName = npc?.Name ?? goal.PlacementNpcId;
                    Console.WriteLine($"[GoalCompletion] Granted {rewards.StoryCubes.Value} StoryCubes to NPC '{npcName}' (total: {npc?.StoryCubes ?? 0}/10)");
                }
                else
                {
                    Console.WriteLine($"[GoalCompletion] WARNING: Cannot grant StoryCubes - goal has no PlacementNpcId");
                }
            }

            // EXPLORATION CUBES - grant to route (requires route context - currently not implemented)
            if (rewards.ExplorationCubes.HasValue && rewards.ExplorationCubes.Value > 0)
            {
                Console.WriteLine($"[GoalCompletion] ExplorationCubes reward specified ({rewards.ExplorationCubes.Value}) but route context not available - requires goal to specify RouteId");
            }

            // CREATE OBLIGATION - grant StoryCubes to patron NPC (RESOURCE-BASED PATTERN)
            // PRINCIPLE 4: No boolean gates - visibility based on resource thresholds
            if (rewards.CreateObligationData != null)
            {
                CreateObligationReward data = rewards.CreateObligationData;

                NPC patron = _gameWorld.NPCs.FirstOrDefault(n => n.ID == data.PatronNpcId);
                if (patron == null)
                {
                    Console.WriteLine($"[GoalCompletion] WARNING: Patron NPC '{data.PatronNpcId}' not found");
                }
                else
                {
                    // Grant StoryCubes to patron (max 10)
                    int previousCubes = patron.StoryCubes;
                    patron.StoryCubes = Math.Min(10, patron.StoryCubes + data.StoryCubesGranted);

                    Console.WriteLine($"[GoalCompletion] Granted {data.StoryCubesGranted} StoryCubes to '{patron.Name}' ({previousCubes} → {patron.StoryCubes}/10)");
                    Console.WriteLine($"[GoalCompletion] Generic delivery goals now visible where NPC.StoryCubes >= threshold");
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
                        Console.WriteLine($"[GoalCompletion] Discovered investigation '{investigation.Name}' (moved to Discovered state)");
                    }
                    else
                    {
                        Console.WriteLine($"[GoalCompletion] Investigation '{investigation.Name}' already discovered");
                    }
                }
                else
                {
                    Console.WriteLine($"[GoalCompletion] WARNING: ObligationId '{rewards.ObligationId}' not found in GameWorld.Investigations");
                }
            }

            // ROUTE SEGMENT UNLOCK - reveal hidden path by reducing HiddenUntilExploration threshold
            if (rewards.RouteSegmentUnlock != null)
            {
                RouteSegmentUnlock unlock = rewards.RouteSegmentUnlock;
                RouteOption route = _gameWorld.WorldState.Routes.FirstOrDefault(r => r.Id == unlock.RouteId);
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
                            Console.WriteLine($"[GoalCompletion] Unlocked hidden path '{path.Description}' on route '{route.Name}' segment {unlock.SegmentPosition} (was hidden until {previousThreshold} ExplorationCubes)");
                        }
                        else
                        {
                            Console.WriteLine($"[GoalCompletion] WARNING: PathId '{unlock.PathId}' not found in segment {unlock.SegmentPosition} of route '{route.Name}'");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[GoalCompletion] WARNING: SegmentPosition {unlock.SegmentPosition} out of range for route '{route.Name}' (has {route.Segments.Count} segments)");
                    }
                }
                else
                {
                    Console.WriteLine($"[GoalCompletion] WARNING: RouteId '{unlock.RouteId}' not found in GameWorld.WorldState.Routes");
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
                    Console.WriteLine($"[GoalCompletion] Reduced obstacle '{parentObstacle.Name}' intensity by {rewards.ObstacleReduction.ReduceIntensity} (was {previousIntensity}, now {parentObstacle.Intensity})");

                    // If obstacle is now cleared, mark as resolved
                    if (parentObstacle.IsCleared() && parentObstacle.State == ObstacleState.Active)
                    {
                        parentObstacle.State = ObstacleState.Resolved;
                        Console.WriteLine($"[GoalCompletion] Obstacle '{parentObstacle.Name}' CLEARED (intensity reached 0)");
                    }
                }
                else
                {
                    Console.WriteLine($"[GoalCompletion] WARNING: ObstacleReduction specified but no parent obstacle found for goal '{goal.Name}'");
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
                    Console.WriteLine($"[GoalCompletion] Removed goal '{goal.Name}' from NPC '{npc.Name}' ActiveGoalIds");
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
                    Console.WriteLine($"[GoalCompletion] Removed goal '{goal.Name}' from location '{location.Name}' ActiveGoalIds");
                }
            }
        }
    }
}
