/// <summary>
/// DTO for SceneSpawnReward - defines procedural Scene spawning
/// Spawns a Scene instance from SceneTemplate with placement strategy
/// Maps to SceneSpawnReward domain entity
/// </summary>
public class SceneSpawnRewardDTO
{
/// <summary>
/// Which SceneTemplate to spawn
/// References SceneTemplate.Id in GameWorld.SceneTemplates
/// </summary>
public string SceneTemplateId { get; set; }

/// <summary>
/// Placement relation strategy
/// Values: "SameLocation", "SameNPC", "SameRoute", "SpecificLocation", "SpecificNPC", "SpecificRoute"
/// Maps to PlacementRelation enum
/// </summary>
public string PlacementRelation { get; set; }

/// <summary>
/// Specific entity ID (for SpecificLocation/SpecificNPC/SpecificRoute placement)
/// null for Same* placement relations (inherit from parent context)
/// </summary>
public string SpecificPlacementId { get; set; }

/// <summary>
/// Time-delayed consequences (number of days before Scene spawns)
/// 0 = immediate spawn
/// </summary>
public int DelayDays { get; set; } = 0;
}
