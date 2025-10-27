using System;

/// <summary>
/// Domain service for applying obstacle property reductions from situation card rewards
/// Pure domain logic - no external dependencies
/// </summary>
public static class ObstacleRewardService
{
    /// <summary>
    /// Apply obstacle property reduction to an obstacle
    /// Intensity is reduced by simple subtraction, floored at 0
    /// </summary>
    /// <param name="obstacle">The obstacle to reduce</param>
    /// <param name="reduction">The reduction amounts to apply</param>
    /// <returns>True if obstacle is cleared after reduction (intensity reaches 0)</returns>
    public static bool ApplyPropertyReduction(Obstacle obstacle, ObstaclePropertyReduction reduction)
    {
        if (obstacle == null)
            throw new ArgumentNullException(nameof(obstacle));
        if (reduction == null)
            throw new ArgumentNullException(nameof(reduction));// Apply reduction with floor at 0
        obstacle.Intensity = Math.Max(0, obstacle.Intensity - reduction.ReduceIntensity);// Check if obstacle is cleared
        bool isCleared = obstacle.IsCleared();
        if (isCleared)
        { }

        return isCleared;
    }

    /// <summary>
    /// Check if a reduction has any non-zero values
    /// </summary>
    public static bool HasAnyReduction(ObstaclePropertyReduction reduction)
    {
        if (reduction == null)
            return false;

        return reduction.HasAnyReduction();
    }
}
