/// <summary>
/// Lifecycle tracking for entities with spawn/completion timestamps.
/// Provides standardized lifecycle metadata that can be shared across
/// Scene, Situation, and other time-sensitive entities.
/// </summary>
/// <remarks>
/// This record implements the HIGHLANDER principle by providing
/// ONE definition of spawn/completion tracking that can be reused
/// across all entities that need lifecycle timestamps.
///
/// ARCHITECTURAL RATIONALE:
/// - Scene expiration will require SpawnedDay (planned feature)
/// - Situation already tracks spawn/completion (implemented)
/// - Extracting to shared record prevents future duplication
/// - Groups related lifecycle properties for semantic clarity
///
/// USAGE PATTERN:
/// Entity has Lifecycle property initialized inline:
///   public SpawnTracking Lifecycle { get; set; } = new SpawnTracking();
///
/// Parser/Facade populates spawn timestamps when entity created:
///   entity.Lifecycle = new SpawnTracking
///   {
///       SpawnedDay = currentDay,
///       SpawnedTimeBlock = currentTimeBlock,
///       SpawnedSegment = currentSegment
///   };
///
/// Completion updates mutable properties:
///   entity.Lifecycle.CompletedDay = currentDay;
///   entity.Lifecycle.CompletedTimeBlock = currentTimeBlock;
///   entity.Lifecycle.CompletedSegment = currentSegment;
/// </remarks>
public record SpawnTracking
{
    /// <summary>
    /// Day when this entity was spawned (instantiated from template)
    /// null if not yet spawned (template definition only)
    /// </summary>
    public int? SpawnedDay { get; init; }

    /// <summary>
    /// Time block when this entity was spawned
    /// null if not yet spawned (template definition only)
    /// </summary>
    public TimeBlocks? SpawnedTimeBlock { get; init; }

    /// <summary>
    /// Segment within time block when this entity was spawned (1-4)
    /// null if not yet spawned (template definition only)
    /// </summary>
    public int? SpawnedSegment { get; init; }

    /// <summary>
    /// Day when this entity was completed
    /// null if not yet completed
    /// </summary>
    public int? CompletedDay { get; set; }

    /// <summary>
    /// Time block when this entity was completed
    /// null if not yet completed
    /// </summary>
    public TimeBlocks? CompletedTimeBlock { get; set; }

    /// <summary>
    /// Segment within time block when this entity was completed (1-4)
    /// null if not yet completed
    /// </summary>
    public int? CompletedSegment { get; set; }
}
