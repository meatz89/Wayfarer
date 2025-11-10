/// <summary>
/// Result returned when an obligation situation is completed
/// Contains data for displaying the obligation progress modal
/// </summary>
public class ObligationProgressResult
{
public string ObligationId { get; set; }
public string ObligationName { get; set; }
public string CompletedSituationName { get; set; }
public string OutcomeNarrative { get; set; }
public List<NewLeadInfo> NewLeads { get; set; } = new List<NewLeadInfo>();
public int CompletedSituationCount { get; set; }
public int TotalSituationCount { get; set; }
}

/// <summary>
/// Information about a newly unlocked obligation situation
/// </summary>
public class NewLeadInfo
{
public string SituationName { get; set; }
public string LocationName { get; set; }
public string SpotName { get; set; }
}
