using System;

/// <summary>
/// Parser for converting ObstacleDTO to Obstacle domain model
/// </summary>
public static class ObstacleParser
{
    /// <summary>
    /// Convert ObstacleDTO to Obstacle entity
    /// </summary>
    public static Obstacle ConvertDTOToObstacle(ObstacleDTO dto, string parentEntityId)
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
            IsPermanent = dto.IsPermanent
        };

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
