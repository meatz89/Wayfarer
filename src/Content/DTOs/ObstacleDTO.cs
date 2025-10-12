using System.Collections.Generic;

/// <summary>
/// DTO for deserializing Obstacle from JSON
/// JSON field names MUST match property names EXACTLY (no JsonPropertyName workarounds)
/// </summary>
public class ObstacleDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int PhysicalDanger { get; set; }
    public int MentalComplexity { get; set; }
    public int SocialDifficulty { get; set; }
    public bool IsPermanent { get; set; }
    public List<GoalDTO> Goals { get; set; } = new List<GoalDTO>();
}
