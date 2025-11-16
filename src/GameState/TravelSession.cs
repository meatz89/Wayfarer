/// <summary>
/// Represents an active travel session with stamina management and progression tracking
/// </summary>
public class TravelSession
{
    /// <summary>
    /// Route being traveled
    /// HIGHLANDER: Object reference ONLY, no RouteId
    /// </summary>
    public RouteOption Route { get; set; }

    public int CurrentSegment { get; set; } = 1;
    public int StaminaRemaining { get; set; }
    public int StaminaCapacity { get; set; } = 3; // Based on state
    public TravelState CurrentState { get; set; } = TravelState.Fresh;
    public int SegmentsElapsed { get; set; } = 0;
    public List<string> CompletedSegments { get; set; } = new();
    public string SelectedPathId { get; set; }

    // Event tracking for current segment (used when segment type is Event)
    public string CurrentEventId { get; set; }

    // Track which event was drawn for each segment (for deterministic behavior)
    public Dictionary<string, string> SegmentEventDraws { get; set; } = new Dictionary<string, string>();

    // Current event narrative text for UI display (from the selected event)
    public string CurrentEventNarrative { get; set; }

    // Card reveal state tracking for face-down card reveal mechanic
    public bool IsRevealingCard { get; set; } = false;
    public string RevealedCardId { get; set; }

    // Journey completion state - true when last segment is completed and ready to finish
    public bool IsReadyToComplete { get; set; } = false;

    // Pending scene ID - set when path has scene that must be resolved before progressing
    public string PendingSceneId { get; set; }
}

public enum TravelState
{
    Fresh,    // 3 stamina
    Steady,   // 4 stamina  
    Tired,    // 2 stamina
    Weary,    // 1 stamina
    Exhausted // 0 stamina
}