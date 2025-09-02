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
    
    // Updated to use new properties format - list of strings instead of dictionary
    public List<string> Properties { get; set; } = new List<string>();
    
    // Additional properties that may exist in observations
    public string Name { get; set; }
    public string Description { get; set; }
    public string UniqueEffect { get; set; }
    public int ExpirationHours { get; set; }
    public string TokenType { get; set; }
    public string Difficulty { get; set; }
    public object SuccessEffect { get; set; }
    public object FailureEffect { get; set; }
}