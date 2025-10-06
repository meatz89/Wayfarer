using System.Collections.Generic;

/// <summary>
/// DTO for Knowledge entity
/// Maps from JSON to Knowledge domain model
/// </summary>
public class KnowledgeDTO
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string InvestigationContext { get; set; }
    public List<string> UnlocksInvestigationIntros { get; set; } = new List<string>();
    public List<string> UnlocksInvestigationGoals { get; set; } = new List<string>();
}
