/// <summary>
/// DTO for deserializing ObstaclePropertyReduction from JSON
/// Used in GoalCardRewards to specify obstacle property reductions
/// JSON field names MUST match property names EXACTLY (no JsonPropertyName workarounds)
/// </summary>
public class ObstaclePropertyReductionDTO
{
    public int ReduceIntensity { get; set; }
}
