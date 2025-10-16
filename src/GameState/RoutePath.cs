/// <summary>
/// Represents a specific path option within a route segment (Core Loop design).
/// Each segment offers 1-3 paths with different time/stamina/obstacle trade-offs.
/// Players choose based on equipment, risk tolerance, and resource availability.
/// Hidden paths revealed when Route.ExplorationCubes reach HiddenUntilExploration threshold.
/// </summary>
public class RoutePath
{
    /// <summary>
    /// Unique identifier for this path option
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Time cost in segments (1-2)
    /// </summary>
    public int TimeSegments { get; set; }

    /// <summary>
    /// Stamina cost (1-2 on 6-point scale)
    /// </summary>
    public int StaminaCost { get; set; }

    /// <summary>
    /// Optional obstacle ID (nullable - not all paths have obstacles)
    /// References GameWorld.Obstacles (single source of truth)
    /// </summary>
    public string OptionalObstacleId { get; set; }

    /// <summary>
    /// Narrative description of this path choice
    /// Example: "Shallow ford through creek" or "Narrow rope bridge"
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Exploration cube threshold required to reveal this path (0-10 scale)
    /// 0 = Always visible (obvious path)
    /// 2+ = Hidden until route familiarity built
    /// 10 = Extremely well-hidden (master-level knowledge)
    /// </summary>
    public int HiddenUntilExploration { get; set; } = 0;
}
