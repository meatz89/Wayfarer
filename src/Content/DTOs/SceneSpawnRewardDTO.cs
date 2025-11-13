/// <summary>
/// DTO for SceneSpawnReward - defines procedural Scene spawning
/// Spawns a Scene instance from SceneTemplate using its PlacementFilter
/// Maps to SceneSpawnReward domain entity
/// </summary>
public class SceneSpawnRewardDTO
{
    /// <summary>
    /// Which SceneTemplate to spawn
    /// References SceneTemplate.Id in GameWorld.SceneTemplates
    /// Spawned scene inherits SceneTemplate.PlacementFilter for entity resolution
    /// </summary>
    public string SceneTemplateId { get; set; }
}
