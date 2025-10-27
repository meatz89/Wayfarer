using System;
using System.Collections.Generic;
using Wayfarer.GameState.Enums;

/// <summary>
/// Parser for converting ObstacleDTO to Obstacle domain model
/// </summary>
public static class ObstacleParser
{
    /// <summary>
    /// Convert ObstacleDTO to Obstacle entity with inline situations
    /// </summary>
    public static Obstacle ConvertDTOToObstacle(ObstacleDTO dto, string parentEntityId, GameWorld gameWorld)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException($"Obstacle in entity '{parentEntityId}' missing required 'Id' field");

        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"Obstacle '{dto.Id}' in entity '{parentEntityId}' missing required 'Name' field");

        if (string.IsNullOrEmpty(dto.Description))
            throw new InvalidOperationException($"Obstacle '{dto.Id}' in entity '{parentEntityId}' missing required 'Description' field");

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
            Description = dto.Description,
            Intensity = dto.Intensity,
            Contexts = contexts,
            IsPermanent = dto.IsPermanent,
            SituationIds = new List<string>()
        };

        // Parse inline situations (for obligation-spawned obstacles)
        if (dto.Situations != null && dto.Situations.Count > 0)
        {
            foreach (SituationDTO situationDto in dto.Situations)
            {
                Situation situation = SituationParser.ConvertDTOToSituation(situationDto, gameWorld);

                // Register situation in GameWorld.Situations (single source of truth)
                gameWorld.Situations.Add(situation);

                // Store situation ID reference in obstacle
                obstacle.SituationIds.Add(situation.Id);
            }
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
            ReduceIntensity = dto.ReduceIntensity
        };

        return reduction;
    }
}
