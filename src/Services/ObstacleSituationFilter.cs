using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service for aggregating situations from ambient and obstacle sources
/// PRINCIPLE 4: ALL SITUATIONS ALWAYS VISIBLE - difficulty varies based on DifficultyModifiers
/// No boolean gates - equipment/resources reduce difficulty, never hide content
/// </summary>
public class ObstacleSituationFilter
{
    private readonly GameWorld _gameWorld;

    public ObstacleSituationFilter(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new System.ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Get all visible situations for a location
    /// Aggregates: ambient situations + obstacle situations (all always visible)
    /// DISTRIBUTED INTERACTION: Obstacles stored in GameWorld.Obstacles, filtered by PlacementLocationId
    /// </summary>
    public List<Situation> GetVisibleLocationSituations(Location location, GameWorld gameWorld)
    {
        if (location == null)
            return new List<Situation>();

        List<Situation> visibleSituations = new List<Situation>();

        // Add ambient situations (always visible)
        foreach (string situationId in location.ActiveSituationIds)
        {
            Situation situation = gameWorld.Situations.FirstOrDefault(g => g.Id == situationId);
            if (situation != null)
            {
                visibleSituations.Add(situation);
            }
        }

        // Add filtered obstacle-specific situations (distributed interaction pattern)
        foreach (string obstacleId in location.ObstacleIds)
        {
            Obstacle obstacle = gameWorld.Obstacles.FirstOrDefault(o => o.Id == obstacleId);
            if (obstacle != null)
            {
                // Filter situations where PlacementLocationId matches this location
                foreach (string situationId in obstacle.SituationIds)
                {
                    Situation situation = gameWorld.Situations.FirstOrDefault(g => g.Id == situationId);
                    if (situation != null && situation.PlacementLocationId == location.Id)
                    {
                        // PRINCIPLE 4: All situations always visible, difficulty varies via DifficultyModifiers
                        visibleSituations.Add(situation);
                    }
                }
            }
        }

        return visibleSituations;
    }

    /// <summary>
    /// Get all visible situations for an NPC
    /// Aggregates: ambient situations + obstacle situations (all always visible)
    /// DISTRIBUTED INTERACTION: Obstacles stored in GameWorld.Obstacles, filtered by PlacementNpcId
    /// </summary>
    public List<Situation> GetVisibleNPCSituations(NPC npc, GameWorld gameWorld)
    {
        if (npc == null)
            return new List<Situation>();

        List<Situation> visibleSituations = new List<Situation>();

        // Add ambient situations (always visible)
        foreach (string situationId in npc.ActiveSituationIds)
        {
            Situation situation = gameWorld.Situations.FirstOrDefault(g => g.Id == situationId);
            if (situation != null)
            {
                visibleSituations.Add(situation);
            }
        }

        // Add filtered obstacle-specific situations (distributed interaction pattern)
        foreach (string obstacleId in npc.ObstacleIds)
        {
            Obstacle obstacle = gameWorld.Obstacles.FirstOrDefault(o => o.Id == obstacleId);
            if (obstacle != null)
            {
                // Filter situations where PlacementNpcId matches this NPC
                foreach (string situationId in obstacle.SituationIds)
                {
                    Situation situation = gameWorld.Situations.FirstOrDefault(g => g.Id == situationId);
                    if (situation != null && situation.PlacementNpcId == npc.ID)
                    {
                        // PRINCIPLE 4: All situations always visible, difficulty varies via DifficultyModifiers
                        visibleSituations.Add(situation);
                    }
                }
            }
        }

        return visibleSituations;
    }

    /// <summary>
    /// Get all visible situations for a route
    /// Routes only have obstacle-specific situations (all always visible, no ambient situations)
    /// </summary>
    public List<Situation> GetVisibleRouteSituations(RouteOption route)
    {
        if (route == null)
            return new List<Situation>();

        List<Situation> visibleSituations = new List<Situation>();

        // Routes only have obstacle-specific situations (no ambient situations)
        // DISTRIBUTED INTERACTION: Obstacles stored in GameWorld.Obstacles, referenced by route.ObstacleIds
        if (route.ObstacleIds != null)
        {
            foreach (string obstacleId in route.ObstacleIds)
            {
                Obstacle obstacle = _gameWorld.Obstacles.FirstOrDefault(o => o.Id == obstacleId);
                if (obstacle != null)
                {
                    visibleSituations.AddRange(GetVisibleSituationsFromObstacle(obstacle));
                }
            }
        }

        return visibleSituations;
    }

    /// <summary>
    /// Get visible situations from a single obstacle
    /// PRINCIPLE 4: All situations always visible, difficulty varies via DifficultyModifiers
    /// Equipment/resources reduce difficulty, never gate visibility
    /// </summary>
    private List<Situation> GetVisibleSituationsFromObstacle(Obstacle obstacle)
    {
        List<Situation> visibleSituations = new List<Situation>();

        if (obstacle == null)
            return visibleSituations;

        foreach (string situationId in obstacle.SituationIds)
        {
            Situation situation = _gameWorld.Situations.FirstOrDefault(g => g.Id == situationId);
            if (situation == null)
                continue;

            // PRINCIPLE 4: All situations always visible
            visibleSituations.Add(situation);
        }

        return visibleSituations;
    }

    /// <summary>
    /// Get all obstacles that have at least one visible situation
    /// Useful for UI display to show which obstacles player can currently interact with
    /// </summary>
    public List<Obstacle> GetObstaclesWithVisibleSituations(List<Obstacle> obstacles)
    {
        if (obstacles == null)
            return new List<Obstacle>();

        return obstacles
            .Where(o => GetVisibleSituationsFromObstacle(o).Count > 0)
            .ToList();
    }

    /// <summary>
    /// Count visible situations for an obstacle (useful for UI badges/indicators)
    /// </summary>
    public int CountVisibleSituations(Obstacle obstacle)
    {
        return GetVisibleSituationsFromObstacle(obstacle).Count;
    }
}
