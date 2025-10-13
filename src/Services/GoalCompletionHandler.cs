using System;
using System.Linq;

namespace Wayfarer.Services
{
    /// <summary>
    /// Handles goal completion lifecycle - marking complete and removing from ActiveGoals if DeleteOnSuccess
    /// </summary>
    public class GoalCompletionHandler
    {
        private readonly GameWorld _gameWorld;

        public GoalCompletionHandler(GameWorld gameWorld)
        {
            _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        }

        /// <summary>
        /// Complete a goal - mark as complete, remove from ActiveGoals if DeleteOnSuccess=true
        /// </summary>
        /// <param name="goal">Goal that was successfully completed</param>
        public void CompleteGoal(Goal goal)
        {
            if (goal == null)
                throw new ArgumentNullException(nameof(goal));

            // Mark goal as complete
            goal.Complete();
            Console.WriteLine($"[GoalCompletion] Goal '{goal.Name}' marked complete");

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
}
