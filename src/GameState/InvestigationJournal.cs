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
/// Active investigation state - tracks understanding accumulation and activation timing
/// PRINCIPLE 4: No sequential phase unlocking - all obstacles spawn at activation
/// Progress tracked by counting resolved obstacles, not completed goal IDs
/// </summary>
public class ActiveInvestigation
{
    public string InvestigationId { get; set; }

    /// <summary>
    /// Accumulated understanding points from completed phases (0-10 scale)
    /// Mental expertise resource that replaces Knowledge tokens
    /// Cumulative, grants access to more complex Mental challenges
    /// </summary>
    public int UnderstandingAccumulated { get; set; } = 0;

    /// <summary>
    /// Segment when this investigation was activated (for deadline tracking)
    /// Used to calculate time remaining for NPCCommissioned investigations
    /// Null for SelfDiscovered investigations
    /// </summary>
    public int? ActivationSegment { get; set; }
}
