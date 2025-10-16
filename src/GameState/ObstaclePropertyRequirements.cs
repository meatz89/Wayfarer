/// <summary>
/// Defines property threshold requirements for goal visibility
/// Goals become visible when obstacle properties meet (are less than or equal to) these thresholds
/// Pure numerical comparison - no string matching
/// Value of -1 means no requirement for that property
/// </summary>
public class ObstaclePropertyRequirements
{
    public int MaxIntensity { get; set; } = -1;

    /// <summary>
    /// Check if obstacle meets all requirements (AND logic)
    /// Returns true if all non-default requirements are satisfied
    /// </summary>
    public bool MeetsRequirements(Obstacle obstacle)
    {
        if (obstacle == null)
            return false;

        // Check requirement - if set (!= -1), obstacle intensity must be <= threshold
        if (MaxIntensity != -1 && obstacle.Intensity > MaxIntensity)
            return false;

        return true;
    }
}
