using System.Collections.Generic;

/// <summary>
/// Result returned when an investigation goal is completed
/// Contains data for displaying the investigation progress modal
/// </summary>
public class InvestigationProgressResult
{
    public string InvestigationId { get; set; }
    public string InvestigationName { get; set; }
    public string CompletedGoalName { get; set; }
    public string OutcomeNarrative { get; set; }
    public List<NewLeadInfo> NewLeads { get; set; } = new List<NewLeadInfo>();
    public int CompletedGoalCount { get; set; }
    public int TotalGoalCount { get; set; }
}

/// <summary>
/// Information about a newly unlocked investigation goal
/// </summary>
public class NewLeadInfo
{
    public string GoalName { get; set; }
    public string LocationName { get; set; }
    public string SpotName { get; set; }
}
