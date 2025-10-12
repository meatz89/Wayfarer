using System;
using System.Collections.Generic;

/// <summary>
/// Parser for converting ObstacleDTO to Obstacle domain model
/// </summary>
public static class ObstacleParser
{
    /// <summary>
    /// Convert ObstacleDTO to Obstacle entity with inline goals
    /// </summary>
    public static Obstacle ConvertDTOToObstacle(ObstacleDTO dto, string parentEntityId, GameWorld gameWorld)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"Obstacle in entity '{parentEntityId}' missing required 'Name' field");

        Obstacle obstacle = new Obstacle
        {
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            PhysicalDanger = dto.PhysicalDanger,
            MentalComplexity = dto.MentalComplexity,
            SocialDifficulty = dto.SocialDifficulty,
            StaminaCost = dto.StaminaCost,
            TimeCost = dto.TimeCost,
            IsPermanent = dto.IsPermanent,
            Goals = new List<Goal>()
        };

        // Parse inline goals (for investigation-spawned obstacles)
        if (dto.Goals != null && dto.Goals.Count > 0)
        {
            foreach (GoalDTO goalDto in dto.Goals)
            {
                Goal goal = GoalParser.ConvertDTOToGoal(goalDto, gameWorld);
                obstacle.Goals.Add(goal);

                // Register goal in GameWorld.Goals for facade lookups
                // DO NOT add to Location.ActiveGoals or NPC.ActiveGoals - obstacle-specific goals stay as children only
                gameWorld.Goals[goal.Id] = goal;
            }
            Console.WriteLine($"[ObstacleParser] Parsed obstacle '{obstacle.Name}' with {obstacle.Goals.Count} inline goals (registered in GameWorld.Goals)");
        }

        return obstacle;
    }

    /// <summary>
    /// Convert ObstaclePropertyReductionDTO to ObstaclePropertyReduction entity
    /// </summary>
    public static ObstaclePropertyReduction ConvertDTOToReduction(ObstaclePropertyReductionDTO dto)
    {
        if (dto == null)
            return null;

        ObstaclePropertyReduction reduction = new ObstaclePropertyReduction
        {
            ReducePhysicalDanger = dto.ReducePhysicalDanger,
            ReduceMentalComplexity = dto.ReduceMentalComplexity,
            ReduceSocialDifficulty = dto.ReduceSocialDifficulty,
            ReduceStaminaCost = dto.ReduceStaminaCost,
            ReduceTimeCost = dto.ReduceTimeCost
        };

        return reduction;
    }
}
