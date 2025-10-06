using System.Collections.Generic;

/// <summary>
/// Investigation Journal - tracks player's investigation progress
/// Investigations have three states: Pending, Active, Completed
/// </summary>
public class InvestigationJournal
{
    /// <summary>
    /// Pending investigations - loaded from JSON but not yet activated by player
    /// These are NOT displayed in UI until activated
    /// </summary>
    public List<string> PendingInvestigationIds { get; set; } = new List<string>();

    /// <summary>
    /// Active investigations - player has activated, goals are being created and evaluated
    /// </summary>
    public List<ActiveInvestigation> ActiveInvestigations { get; set; } = new List<ActiveInvestigation>();

    /// <summary>
    /// Completed investigations - all goals finished
    /// </summary>
    public List<string> CompletedInvestigationIds { get; set; } = new List<string>();
}

/// <summary>
/// Active investigation state - tracks which goals have been completed
/// </summary>
public class ActiveInvestigation
{
    public string InvestigationId { get; set; }
    
    /// <summary>
    /// IDs of goals that have been completed for this investigation
    /// Used to calculate progress (X/Y completed) for journal display
    /// </summary>
    public List<string> CompletedGoalIds { get; set; } = new List<string>();
}
