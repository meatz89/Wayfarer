
/// <summary>
/// Defines a Scene to be spawned as a Choice reward
/// Part of Scene-Situation architecture where Choices can spawn new Scenes dynamically
/// Spawned scene uses SceneTemplate.PlacementFilter (categorical specification)
/// </summary>
public class SceneSpawnReward
{
    /// <summary>
    /// Template identifier of Scene to spawn
    /// References SceneTemplate in GameWorld.SceneTemplates
    /// Spawned scene inherits SceneTemplate.PlacementFilter for entity resolution
    /// </summary>
    public string SceneTemplateId { get; set; }
}
