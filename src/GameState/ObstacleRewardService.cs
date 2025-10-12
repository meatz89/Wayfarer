using System;

/// <summary>
/// Domain service for applying obstacle property reductions from goal card rewards
/// Pure domain logic - no external dependencies
/// </summary>
public static class ObstacleRewardService
{
    /// <summary>
    /// Apply obstacle property reduction to an obstacle
    /// Properties are reduced by simple subtraction, floored at 0
    /// </summary>
    /// <param name="obstacle">The obstacle to reduce</param>
    /// <param name="reduction">The reduction amounts to apply</param>
    /// <returns>True if obstacle is cleared after reduction (all non-permanent obstacles at 0)</returns>
    public static bool ApplyPropertyReduction(Obstacle obstacle, ObstaclePropertyReduction reduction)
    {
        if (obstacle == null)
            throw new ArgumentNullException(nameof(obstacle));
        if (reduction == null)
            throw new ArgumentNullException(nameof(reduction));

        Console.WriteLine($"[ObstacleRewardService] Applying reduction to obstacle '{obstacle.Name}':");
        Console.WriteLine($"  Before: PhysicalDanger={obstacle.PhysicalDanger}, MentalComplexity={obstacle.MentalComplexity}, " +
            $"SocialDifficulty={obstacle.SocialDifficulty}");

        // Apply reductions with floor at 0
        obstacle.PhysicalDanger = Math.Max(0, obstacle.PhysicalDanger - reduction.ReducePhysicalDanger);
        obstacle.MentalComplexity = Math.Max(0, obstacle.MentalComplexity - reduction.ReduceMentalComplexity);
        obstacle.SocialDifficulty = Math.Max(0, obstacle.SocialDifficulty - reduction.ReduceSocialDifficulty);

        Console.WriteLine($"  After: PhysicalDanger={obstacle.PhysicalDanger}, MentalComplexity={obstacle.MentalComplexity}, " +
            $"SocialDifficulty={obstacle.SocialDifficulty}");

        // Check if obstacle is cleared
        bool isCleared = obstacle.IsCleared();
        if (isCleared)
        {
            Console.WriteLine($"[ObstacleRewardService] Obstacle '{obstacle.Name}' is now CLEARED (all properties at 0)");
        }

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
