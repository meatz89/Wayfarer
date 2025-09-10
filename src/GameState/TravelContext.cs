using System.Collections.Generic;

/// <summary>
/// Context for active travel UI containing all necessary state
/// </summary>
public class TravelContext
{
    public RouteOption CurrentRoute { get; set; }
    public TravelSession Session { get; set; }
    public List<PathCardDTO> CurrentSegmentCards { get; set; }
    public Dictionary<string, bool> CardDiscoveries { get; set; }
    public Player Player { get; set; }
    public bool CanRest => Session.CurrentState != TravelState.Exhausted;
    public bool MustTurnBack { get; set; } = false;
}