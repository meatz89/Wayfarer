/// <summary>
/// ObstaclePropertyReduction - Reward type for GoalCard completion
/// Specifies how much to reduce each property of a targeted obstacle
/// Part of GoalCard.Rewards structure
/// Design principle: Simple addition reduction, no formulas or conditional logic
/// </summary>
public class ObstaclePropertyReduction
{
    /// <summary>
    /// Amount to reduce PhysicalDanger property
    /// </summary>
    public int ReducePhysicalDanger { get; set; }

    /// <summary>
    /// Amount to reduce MentalComplexity property
    /// </summary>
    public int ReduceMentalComplexity { get; set; }

    /// <summary>
    /// Amount to reduce SocialDifficulty property
    /// </summary>
    public int ReduceSocialDifficulty { get; set; }

    /// <summary>
    /// Check if this reduction has any non-zero values
    /// </summary>
    public bool HasAnyReduction()
    {
        return ReducePhysicalDanger > 0 ||
               ReduceMentalComplexity > 0 ||
               ReduceSocialDifficulty > 0;
    }

    /// <summary>
    /// Get total reduction magnitude (sum of three core reductions)
    /// Useful for UI display
    /// </summary>
    public int GetTotalReduction()
    {
        return ReducePhysicalDanger + ReduceMentalComplexity + ReduceSocialDifficulty;
    }
}
