using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service for aggregating and filtering goals from ambient and obstacle sources
/// Implements 80 Days-style property-based goal visibility gating
/// Filters by both property requirements (obstacle properties) and access requirements (knowledge, equipment, stats)
/// </summary>
public class ObstacleGoalFilter
{
    private readonly GoalRequirementsChecker _requirementsChecker;
    private readonly GameWorld _gameWorld;

    public ObstacleGoalFilter(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new System.ArgumentNullException(nameof(gameWorld));
        _requirementsChecker = new GoalRequirementsChecker(gameWorld);
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
                            // Check property requirements and access requirements
                            bool propertyRequirementsMet = goal.PropertyRequirements == null ||
                                                          goal.PropertyRequirements.MeetsRequirements(obstacle);

                            bool accessRequirementsMet = _requirementsChecker.CheckGoalRequirements(goal);

                            if (propertyRequirementsMet && accessRequirementsMet)
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
                            // Check property requirements and access requirements
                            bool propertyRequirementsMet = goal.PropertyRequirements == null ||
                                                          goal.PropertyRequirements.MeetsRequirements(obstacle);

                            bool accessRequirementsMet = _requirementsChecker.CheckGoalRequirements(goal);

                            if (propertyRequirementsMet && accessRequirementsMet)
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
        if (route.Obstacles != null)
        {
            foreach (Obstacle obstacle in route.Obstacles)
            {
                visibleGoals.AddRange(GetVisibleGoalsFromObstacle(obstacle));
            }
        }

        return visibleGoals;
    }

    /// <summary>
    /// Get visible goals from a single obstacle (filtered by property requirements and access requirements)
    /// 80 Days pattern: Goals become visible as obstacle properties are reduced
    /// Access requirements: Goals require knowledge, equipment, stats, familiarity, completed goals
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

            // Goal visible if ALL conditions met:
            // 1. Property requirements met (obstacle properties <= thresholds)
            // 2. Access requirements met (knowledge, equipment, stats, familiarity, completed goals)
            bool propertyRequirementsMet = goal.PropertyRequirements == null ||
                                          goal.PropertyRequirements.MeetsRequirements(obstacle);

            bool accessRequirementsMet = _requirementsChecker.CheckGoalRequirements(goal);

            if (propertyRequirementsMet && accessRequirementsMet)
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
