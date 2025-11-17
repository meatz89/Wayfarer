/// <summary>
/// Obligation Journal - tracks player's obligation progress
/// Obligations have four states: Potential → Discovered → Active → Completed
/// HIGHLANDER: Object references ONLY, no ID collections
/// </summary>
public class ObligationJournal
{
    /// <summary>
    /// Potential obligations - loaded from JSON but not yet discovered
    /// Awaiting discovery trigger evaluation
    /// NOT displayed in UI
    /// </summary>
    public List<Obligation> PotentialObligations { get; set; } = new List<Obligation>();

    /// <summary>
    /// Discovered obligations - trigger fired, intro action available
    /// Player can see these in journal but hasn't completed intro yet
    /// Displayed in journal's "Discovered" tab
    /// </summary>
    public List<Obligation> DiscoveredObligations { get; set; } = new List<Obligation>();

    /// <summary>
    /// Active obligations - intro completed, situations are being created and evaluated
    /// Displayed in journal's "Active" tab
    /// </summary>
    public List<ActiveObligation> ActiveObligations { get; set; } = new List<ActiveObligation>();

    /// <summary>
    /// Completed obligations - all required situations finished
    /// Displayed in journal's "Completed" tab
    /// </summary>
    public List<Obligation> CompletedObligations { get; set; } = new List<Obligation>();
}

/// <summary>
/// Active obligation state - tracks understanding accumulation and activation timing
/// PRINCIPLE 4: No sequential phase unlocking - all scenes spawn at activation
/// Progress tracked by counting resolved scenes, not completed situation IDs
/// </summary>
public class ActiveObligation
{
    public Obligation Obligation { get; set; }

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
}
