using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service for aggregating and filtering goals from ambient and obstacle sources
/// Implements 80 Days-style property-based goal visibility gating
/// ALL GOALS ALWAYS VISIBLE - difficulty varies based on DifficultyModifiers
/// Boolean gate elimination: No more knowledge/equipment hiding goals
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
    /// Aggregates: ambient goals (always visible) + obstacle goals (filtered by property requirements)
    /// Filters by requirements (knowledge, equipment, stats, familiarity, completed goals)
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
            if (gameWorld.Goals.TryGetValue(goalId, out Goal goal))
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
                        if (gameWorld.Goals.TryGetValue(goalId, out Goal goal) &&
                            goal.PlacementLocationId == location.Id)
                        {
                            // Check property requirements only (boolean gates eliminated)
                            // Goals always visible - difficulty varies via DifficultyModifiers
                            bool propertyRequirementsMet = goal.PropertyRequirements == null ||
                                                          goal.PropertyRequirements.MeetsRequirements(obstacle);

                            if (propertyRequirementsMet)
                            {
                                visibleGoals.Add(goal);
                            }
                        }
                    }
                }
            }
        }

        return visibleGoals;
    }

    /// <summary>
    /// Get all visible goals for an NPC
    /// Aggregates: ambient goals (always visible) + obstacle goals (filtered by property requirements)
    /// Filters by requirements (knowledge, equipment, stats, familiarity, completed goals)
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
            if (gameWorld.Goals.TryGetValue(goalId, out Goal goal))
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
                        if (gameWorld.Goals.TryGetValue(goalId, out Goal goal) &&
                            goal.PlacementNpcId == npc.ID)
                        {
                            // Check property requirements only (boolean gates eliminated)
                            // Goals always visible - difficulty varies via DifficultyModifiers
                            bool propertyRequirementsMet = goal.PropertyRequirements == null ||
                                                          goal.PropertyRequirements.MeetsRequirements(obstacle);

                            if (propertyRequirementsMet)
                            {
                                visibleGoals.Add(goal);
                            }
                        }
                    }
                }
            }
        }

        return visibleGoals;
    }

    /// <summary>
    /// Get all visible goals for a route
    /// Routes only have obstacle-specific goals (no ambient goals)
    /// Filters by requirements (knowledge, equipment, stats, familiarity, completed goals)
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
    /// Get visible goals from a single obstacle (filtered by property requirements only)
    /// 80 Days pattern: Goals become visible as obstacle properties are reduced
    /// Boolean gate elimination: Goals always visible, difficulty varies via DifficultyModifiers
    /// </summary>
    private List<Goal> GetVisibleGoalsFromObstacle(Obstacle obstacle)
    {
        List<Goal> visibleGoals = new List<Goal>();

        if (obstacle?.GoalIds == null)
            return visibleGoals;

        foreach (string goalId in obstacle.GoalIds)
        {
            if (!_gameWorld.Goals.TryGetValue(goalId, out Goal goal))
                continue;

            // Property requirements only (boolean gates eliminated)
            // Goals always visible - difficulty varies via DifficultyModifiers
            bool propertyRequirementsMet = goal.PropertyRequirements == null ||
                                          goal.PropertyRequirements.MeetsRequirements(obstacle);

            if (propertyRequirementsMet)
            {
                visibleGoals.Add(goal);
            }
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
