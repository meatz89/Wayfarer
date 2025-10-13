using System;
using System.Linq;


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
