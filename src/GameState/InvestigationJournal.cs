using System.Collections.Generic;

/// <summary>
/// Investigation Journal - tracks player's investigation progress
/// Investigations have four states: Potential → Discovered → Active → Completed
/// </summary>
public class InvestigationJournal
{
    /// <summary>
    /// Potential investigations - loaded from JSON but not yet discovered
    /// Awaiting discovery trigger evaluation
    /// NOT displayed in UI
    /// </summary>
    public List<string> PotentialInvestigationIds { get; set; } = new List<string>();

    /// <summary>
    /// Discovered investigations - trigger fired, intro action available
    /// Player can see these in journal but hasn't completed intro yet
    /// Displayed in journal's "Discovered" tab
    /// </summary>
    public List<string> DiscoveredInvestigationIds { get; set; } = new List<string>();

    /// <summary>
    /// Active investigations - intro completed, goals are being created and evaluated
    /// Displayed in journal's "Active" tab
    /// </summary>
    public List<ActiveInvestigation> ActiveInvestigations { get; set; } = new List<ActiveInvestigation>();

    /// <summary>
    /// Completed investigations - all required goals finished
    /// Displayed in journal's "Completed" tab
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
