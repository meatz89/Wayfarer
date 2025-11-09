/// <summary>
/// Provenance tracking for dependent resources (Locations, Items) created by scenes
/// Enables forensic tracking of resource lifecycle and cleanup coordination
/// HIGHLANDER: Single source of truth for "which scene created this resource"
/// </summary>
/// <remarks>
/// ARCHITECTURAL RATIONALE:
/// - DependentResourceSpecs define resources scenes CAN create
/// - DependentResourceOrchestrationService instantiates resources at spawn time
/// - SceneProvenance tracks which scene instance ACTUALLY created each resource
/// - Enables future features: cleanup, ownership queries, resource graphs
///
/// USAGE PATTERN:
/// Resource entity has Provenance property initialized inline:
///   public SceneProvenance Provenance { get; set; } = null;
///
/// DependentResourceOrchestrationService populates when creating resource:
///   location.Provenance = new SceneProvenance
///   {
///       SceneId = scene.Id,
///       CreatedDay = currentDay,
///       CreatedTimeBlock = currentTimeBlock,
///       CreatedSegment = currentSegment
///   };
///
/// FORENSIC QUERIES:
/// - "Which scene created this location?" → location.Provenance.SceneId
/// - "Find all resources from scene X" → locations.Where(l => l.Provenance?.SceneId == sceneId)
/// - "Resources created today" → locations.Where(l => l.Provenance?.CreatedDay == currentDay)
/// </remarks>
public record SceneProvenance
{
/// <summary>
/// Scene ID that created this resource
/// References Scene.Id in GameWorld.Scenes
/// </summary>
public string SceneId { get; init; }

/// <summary>
/// Day when this resource was created (spawned/instantiated)
/// </summary>
public int CreatedDay { get; init; }

/// <summary>
/// Time block when this resource was created
/// </summary>
public TimeBlocks CreatedTimeBlock { get; init; }

/// <summary>
/// Segment within time block when this resource was created (1-4)
/// </summary>
public int CreatedSegment { get; init; }
}
