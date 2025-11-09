
/// <summary>
/// Defines a Scene to be spawned as a Choice reward
/// Part of Scene-Situation architecture where Choices can spawn new Scenes dynamically
/// Uses template identifier and placement relation (no hardcoded entity IDs)
/// </summary>
public class SceneSpawnReward
{
/// <summary>
/// Template identifier of Scene to spawn
/// References SceneTemplate in GameWorld.SceneTemplates
/// </summary>
public string SceneTemplateId { get; set; }

/// <summary>
/// Placement relation - where to spawn relative to current context
/// SameLocation: Spawn at same location where Choice was made
/// SameNPC: Spawn at same NPC where Choice was made
/// SameRoute: Spawn on same route where Choice was made
/// SpecificLocation: Spawn at location specified by SpecificPlacementId
/// SpecificNPC: Spawn at NPC specified by SpecificPlacementId
/// SpecificRoute: Spawn on route specified by SpecificPlacementId
/// </summary>
public PlacementRelation PlacementRelation { get; set; }

/// <summary>
/// Specific placement identifier (Location/NPC/Route ID) when using Specific* placement relations
/// null when using Same* placement relations (context-relative)
/// </summary>
public string SpecificPlacementId { get; set; }

/// <summary>
/// Delay in days before spawning (0 for immediate, positive for scheduled future spawn)
/// Enables time-delayed consequences - "3 days later, X happens"
/// </summary>
public int DelayDays { get; set; } = 0;
}
