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
    public int Intensity { get; set; }
    public List<string> Contexts { get; set; } = new List<string>();
    public bool IsPermanent { get; set; }
    public List<SituationDTO> Situations { get; set; } = new List<SituationDTO>();
}
