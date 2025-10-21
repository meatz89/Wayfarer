using System.Collections.Generic;

/// <summary>
/// Obligation Journal - tracks player's obligation progress
/// Obligations have four states: Potential → Discovered → Active → Completed
/// </summary>
public class ObligationJournal
{
    /// <summary>
    /// Potential obligations - loaded from JSON but not yet discovered
    /// Awaiting discovery trigger evaluation
    /// NOT displayed in UI
    /// </summary>
    public List<string> PotentialObligationIds { get; set; } = new List<string>();

    /// <summary>
    /// Discovered obligations - trigger fired, intro action available
    /// Player can see these in journal but hasn't completed intro yet
    /// Displayed in journal's "Discovered" tab
    /// </summary>
    public List<string> DiscoveredObligationIds { get; set; } = new List<string>();

    /// <summary>
    /// Active obligations - intro completed, goals are being created and evaluated
    /// Displayed in journal's "Active" tab
    /// </summary>
    public List<ActiveObligation> ActiveObligations { get; set; } = new List<ActiveObligation>();

    /// <summary>
    /// Completed obligations - all required goals finished
    /// Displayed in journal's "Completed" tab
    /// </summary>
    public List<string> CompletedObligationIds { get; set; } = new List<string>();
}

/// <summary>
/// Active obligation state - tracks understanding accumulation and activation timing
/// PRINCIPLE 4: No sequential phase unlocking - all obstacles spawn at activation
/// Progress tracked by counting resolved obstacles, not completed goal IDs
/// </summary>
public class ActiveObligation
{
    public string ObligationId { get; set; }

    /// <summary>
    /// Accumulated understanding points from completed phases (0-10 scale)
    /// Mental expertise resource that replaces Knowledge tokens
    /// Cumulative, grants access to more complex Mental challenges
    /// </summary>
    public int UnderstandingAccumulated { get; set; } = 0;

    /// <summary>
    /// Segment when this obligation was activated (for deadline tracking)
    /// Used to calculate time remaining for NPCCommissioned obligations
    /// Null for SelfDiscovered obligations
    /// </summary>
    public int? ActivationSegment { get; set; }

    /// <summary>
    /// Object reference to obligation (for runtime navigation)
    /// Populated at initialization time from ObligationId
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Obligation Obligation { get; set; }
}
