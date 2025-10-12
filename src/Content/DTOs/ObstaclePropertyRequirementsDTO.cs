/// <summary>
/// DTO for obstacle property requirements
/// JSON field names MUST match property names EXACTLY (no JsonPropertyName workarounds)
/// </summary>
public class ObstaclePropertyRequirementsDTO
{
    public int MaxPhysicalDanger { get; set; } = -1;
    public int MaxMentalComplexity { get; set; } = -1;
    public int MaxSocialDifficulty { get; set; } = -1;
    public int MaxStaminaCost { get; set; } = -1;
    public int MaxTimeCost { get; set; } = -1;
}
