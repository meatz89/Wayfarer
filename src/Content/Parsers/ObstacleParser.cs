using System;
using System.Collections.Generic;
using Wayfarer.GameState.Enums;

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

        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException($"Obstacle in entity '{parentEntityId}' missing required 'Id' field");

        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"Obstacle '{dto.Id}' in entity '{parentEntityId}' missing required 'Name' field");

        // Parse contexts from JSON strings to enum
        List<ObstacleContext> contexts = new List<ObstacleContext>();
        if (dto.Contexts != null)
        {
            foreach (string contextString in dto.Contexts)
            {
                if (Enum.TryParse<ObstacleContext>(contextString, ignoreCase: true, out ObstacleContext context))
                {
                    contexts.Add(context);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Invalid context '{contextString}' in obstacle '{dto.Id}' (entity '{parentEntityId}'). " +
                        $"Must be one of: {string.Join(", ", Enum.GetNames<ObstacleContext>())}");
                }
            }
        }

        Obstacle obstacle = new Obstacle
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            Intensity = dto.Intensity,
            Contexts = contexts,
            IsPermanent = dto.IsPermanent,
            GoalIds = new List<string>()
        };

        // Parse inline goals (for investigation-spawned obstacles)
        if (dto.Goals != null && dto.Goals.Count > 0)
        {
            foreach (GoalDTO goalDto in dto.Goals)
            {
                Goal goal = GoalParser.ConvertDTOToGoal(goalDto, gameWorld);

                // Register goal in GameWorld.Goals (single source of truth)
                gameWorld.Goals.Add(goal);

                // Store goal ID reference in obstacle
                obstacle.GoalIds.Add(goal.Id);
            }}

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
            ReduceIntensity = dto.ReduceIntensity
        };

        return reduction;
    }
}
