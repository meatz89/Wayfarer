/// <summary>
/// ObstaclePropertyReduction - Reward type for SituationCard completion
/// Specifies how much to reduce intensity of a targeted obstacle
/// Part of SituationCard.Rewards structure
/// Design principle: Simple addition reduction, no formulas or conditional logic
/// </summary>
public class ObstaclePropertyReduction
{
    /// <summary>
    /// Amount to reduce Intensity property
    /// </summary>
    public int ReduceIntensity { get; set; }

    /// <summary>
    /// Check if this reduction has any non-zero values
    /// </summary>
    public bool HasAnyReduction()
    {
        return ReduceIntensity > 0;
    }

    /// <summary>
    /// Get total reduction magnitude
    /// Useful for UI display
    /// </summary>
    public int GetTotalReduction()
    {
        return ReduceIntensity;
    }
}
