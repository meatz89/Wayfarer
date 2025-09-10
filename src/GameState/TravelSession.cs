using System.Collections.Generic;

/// <summary>
/// Represents an active travel session with stamina management and progression tracking
/// </summary>
public class TravelSession
{
    public string RouteId { get; set; }
    public int CurrentSegment { get; set; } = 1;
    public int StaminaRemaining { get; set; }
    public int StaminaCapacity { get; set; } = 3; // Based on state
    public TravelState CurrentState { get; set; } = TravelState.Fresh;
    public int TimeElapsed { get; set; } = 0;
    public List<string> CompletedSegments { get; set; } = new();
    public string SelectedPathId { get; set; }
}

public enum TravelState
{
    Fresh,    // 3 stamina
    Steady,   // 4 stamina  
    Tired,    // 2 stamina
    Weary,    // 1 stamina
    Exhausted // 0 stamina
}