/// <summary>
/// Defines property threshold requirements for goal visibility
/// Goals become visible when obstacle properties meet (are less than or equal to) these thresholds
/// Pure numerical comparison - no string matching
/// Value of -1 means no requirement for that property
/// </summary>
public class ObstaclePropertyRequirements
{
    public int MaxPhysicalDanger { get; set; } = -1;
    public int MaxMentalComplexity { get; set; } = -1;
    public int MaxSocialDifficulty { get; set; } = -1;

    /// <summary>
    /// Check if obstacle meets all requirements (AND logic)
    /// Returns true if all non-default requirements are satisfied
    /// </summary>
    public bool MeetsRequirements(Obstacle obstacle)
    {
        if (obstacle == null)
            return false;

        // Check each requirement - if set (!= -1), obstacle property must be <= threshold
        if (MaxPhysicalDanger != -1 && obstacle.PhysicalDanger > MaxPhysicalDanger)
            return false;

        if (MaxMentalComplexity != -1 && obstacle.MentalComplexity > MaxMentalComplexity)
            return false;

        if (MaxSocialDifficulty != -1 && obstacle.SocialDifficulty > MaxSocialDifficulty)
            return false;

        return true;
    }
}
