using System.Collections.Generic;

/// <summary>
/// DTO for observation data from JSON packages
/// </summary>
public class ObservationDTO
{
    public string Id { get; set; }
    public string DisplayText { get; set; }
    public string Category { get; set; }
    public List<string> RequiredTags { get; set; } = new List<string>();
    public List<string> ExcludedTags { get; set; } = new List<string>();
    public int Weight { get; set; } = 1;
    public string TriggerCondition { get; set; }
    public bool OneTimeOnly { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}