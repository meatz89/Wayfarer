/// <summary>
/// DTO for SceneSpawnReward - defines procedural Scene spawning
/// Spawns a Scene instance from SceneTemplate using its PlacementFilter
/// Maps to SceneSpawnReward domain entity
/// Supports parametric spawning via Parameters dictionary
/// </summary>
public class SceneSpawnRewardDTO
{
    /// <summary>
    /// Which SceneTemplate to spawn
    /// References SceneTemplate.Id in GameWorld.SceneTemplates
    /// Spawned scene inherits SceneTemplate.PlacementFilter for entity resolution
    /// </summary>
    public string SceneTemplateId { get; set; }

    /// <summary>
    /// Parameters for dynamic scene configuration (parametric spawning)
    /// Example: {"ContractPayment": 20, "Difficulty": 3}
    /// Maps to SceneSpawnReward.Parameters domain property
    /// </summary>
    public Dictionary<string, int> Parameters { get; set; }
}
