using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service for aggregating goals from ambient and obstacle sources
/// PRINCIPLE 4: ALL GOALS ALWAYS VISIBLE - difficulty varies based on DifficultyModifiers
/// No boolean gates - equipment/resources reduce difficulty, never hide content
/// </summary>
public class ObstacleGoalFilter
{
    private readonly GameWorld _gameWorld;

    public ObstacleGoalFilter(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new System.ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Get all visible goals for a location
    /// Aggregates: ambient goals + obstacle goals (all always visible)
    /// DISTRIBUTED INTERACTION: Obstacles stored in GameWorld.Obstacles, filtered by PlacementLocationId
    /// </summary>
    public List<Goal> GetVisibleLocationGoals(Location location, GameWorld gameWorld)
    {
        if (location == null)
            return new List<Goal>();

        List<Goal> visibleGoals = new List<Goal>();

        // Add ambient goals (always visible)
        foreach (string goalId in location.ActiveGoalIds)
        {
            Goal goal = gameWorld.Goals.FirstOrDefault(g => g.Id == goalId);
            if (goal != null)
            {
                visibleGoals.Add(goal);
            }
        }

        // Add filtered obstacle-specific goals (distributed interaction pattern)
        if (location.ObstacleIds != null && gameWorld.Obstacles != null)
        {
            foreach (string obstacleId in location.ObstacleIds)
            {
                Obstacle obstacle = gameWorld.Obstacles.FirstOrDefault(o => o.Id == obstacleId);
                if (obstacle != null)
                {
                    // Filter goals where PlacementLocationId matches this location
                    foreach (string goalId in obstacle.GoalIds)
                    {
                        Goal goal = gameWorld.Goals.FirstOrDefault(g => g.Id == goalId);
                        if (goal != null && goal.PlacementLocationId == location.Id)
                        {
                            // PRINCIPLE 4: All goals always visible, difficulty varies via DifficultyModifiers
                            visibleGoals.Add(goal);
                        }
                    }
                }
            }
        }

        return visibleGoals;
    }

    /// <summary>
    /// Get all visible goals for an NPC
    /// Aggregates: ambient goals + obstacle goals (all always visible)
    /// DISTRIBUTED INTERACTION: Obstacles stored in GameWorld.Obstacles, filtered by PlacementNpcId
    /// </summary>
    public List<Goal> GetVisibleNPCGoals(NPC npc, GameWorld gameWorld)
    {
        if (npc == null)
            return new List<Goal>();

        List<Goal> visibleGoals = new List<Goal>();

        // Add ambient goals (always visible)
        foreach (string goalId in npc.ActiveGoalIds)
        {
            Goal goal = gameWorld.Goals.FirstOrDefault(g => g.Id == goalId);
            if (goal != null)
            {
                visibleGoals.Add(goal);
            }
        }

        // Add filtered obstacle-specific goals (distributed interaction pattern)
        if (npc.ObstacleIds != null && gameWorld.Obstacles != null)
        {
            foreach (string obstacleId in npc.ObstacleIds)
            {
                Obstacle obstacle = gameWorld.Obstacles.FirstOrDefault(o => o.Id == obstacleId);
                if (obstacle != null)
                {
                    // Filter goals where PlacementNpcId matches this NPC
                    foreach (string goalId in obstacle.GoalIds)
                    {
                        Goal goal = gameWorld.Goals.FirstOrDefault(g => g.Id == goalId);
                        if (goal != null && goal.PlacementNpcId == npc.ID)
                        {
                            // PRINCIPLE 4: All goals always visible, difficulty varies via DifficultyModifiers
                            visibleGoals.Add(goal);
                        }
                    }
                }
            }
        }

        return visibleGoals;
    }

    /// <summary>
    /// Get all visible goals for a route
    /// Routes only have obstacle-specific goals (all always visible, no ambient goals)
    /// </summary>
    public List<Goal> GetVisibleRouteGoals(RouteOption route)
    {
        if (route == null)
            return new List<Goal>();

        List<Goal> visibleGoals = new List<Goal>();

        // Routes only have obstacle-specific goals (no ambient goals)
        // DISTRIBUTED INTERACTION: Obstacles stored in GameWorld.Obstacles, referenced by route.ObstacleIds
        if (route.ObstacleIds != null)
        {
            foreach (string obstacleId in route.ObstacleIds)
            {
                Obstacle obstacle = _gameWorld.Obstacles.FirstOrDefault(o => o.Id == obstacleId);
                if (obstacle != null)
                {
                    visibleGoals.AddRange(GetVisibleGoalsFromObstacle(obstacle));
                }
            }
        }

        return visibleGoals;
    }

    /// <summary>
    /// Get visible goals from a single obstacle
    /// PRINCIPLE 4: All goals always visible, difficulty varies via DifficultyModifiers
    /// Equipment/resources reduce difficulty, never gate visibility
    /// </summary>
    private List<Goal> GetVisibleGoalsFromObstacle(Obstacle obstacle)
    {
        List<Goal> visibleGoals = new List<Goal>();

        if (obstacle?.GoalIds == null)
            return visibleGoals;

        foreach (string goalId in obstacle.GoalIds)
        {
            Goal goal = _gameWorld.Goals.FirstOrDefault(g => g.Id == goalId);
            if (goal == null)
                continue;

            // PRINCIPLE 4: All goals always visible
            visibleGoals.Add(goal);
        }

        return visibleGoals;
    }

    /// <summary>
    /// Get all obstacles that have at least one visible goal
    /// Useful for UI display to show which obstacles player can currently interact with
    /// </summary>
    public List<Obstacle> GetObstaclesWithVisibleGoals(List<Obstacle> obstacles)
    {
        if (obstacles == null)
            return new List<Obstacle>();

        return obstacles
            .Where(o => GetVisibleGoalsFromObstacle(o).Count > 0)
            .ToList();
    }

    /// <summary>
    /// Count visible goals for an obstacle (useful for UI badges/indicators)
    /// </summary>
    public int CountVisibleGoals(Obstacle obstacle)
    {
        return GetVisibleGoalsFromObstacle(obstacle).Count;
    }
}
