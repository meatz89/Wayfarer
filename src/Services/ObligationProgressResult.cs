using System.Collections.Generic;

/// <summary>
/// Result returned when an obligation goal is completed
/// Contains data for displaying the obligation progress modal
/// </summary>
public class ObligationProgressResult
{
    public string ObligationId { get; set; }
    public string ObligationName { get; set; }
    public string CompletedGoalName { get; set; }
    public string OutcomeNarrative { get; set; }
    public List<NewLeadInfo> NewLeads { get; set; } = new List<NewLeadInfo>();
    public int CompletedGoalCount { get; set; }
    public int TotalGoalCount { get; set; }
}

/// <summary>
/// Information about a newly unlocked obligation goal
/// </summary>
public class NewLeadInfo
{
    public string GoalName { get; set; }
    public string LocationName { get; set; }
    public string SpotName { get; set; }
}
