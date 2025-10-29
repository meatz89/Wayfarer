/// <summary>
/// SceneOutcome - Result of attempting a TravelScene approach.
/// Represents consequences (costs, health changes, route improvements) of success or failure.
/// Part of Physical challenge system (route obstacles).
/// </summary>
public class SceneOutcome
{
    public string Description { get; set; }
    public int TimeSegmentCost { get; set; }
    public int StaminaCost { get; set; }
    public int HealthChange { get; set; }

    // Route improvement reward
    public RouteImprovement RouteImprovement { get; set; }
}
