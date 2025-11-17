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

    // ADR-007: SelectedPathId DELETED - use SelectedPath object reference
    public PathCardDTO SelectedPath { get; set; }

    // ADR-007: CurrentEventId DELETED - use CurrentEvent object reference
    public TravelEventDTO CurrentEvent { get; set; }

    // Track which event was drawn for each segment (for deterministic behavior)
    public Dictionary<string, string> SegmentEventDraws { get; set; } = new Dictionary<string, string>();

    // Current event narrative text for UI display (from the selected event)
    public string CurrentEventNarrative { get; set; }

    // Card reveal state tracking for face-down card reveal mechanic
    public bool IsRevealingCard { get; set; } = false;

    // ADR-007: RevealedCardId DELETED - use RevealedCard object reference
    public PathCardDTO RevealedCard { get; set; }

    // Journey completion state - true when last segment is completed and ready to finish
    public bool IsReadyToComplete { get; set; } = false;

    // ADR-007: PendingSceneId DELETED - use PendingScene object reference
    public Scene PendingScene { get; set; }
}

public enum TravelState
{
    Fresh,    // 3 stamina
    Steady,   // 4 stamina  
    Tired,    // 2 stamina
    Weary,    // 1 stamina
    Exhausted // 0 stamina
}