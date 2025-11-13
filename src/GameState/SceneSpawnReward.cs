
/// <summary>
/// Defines a Scene to be spawned as a Choice reward
/// Part of Scene-Situation architecture where Choices can spawn new Scenes dynamically
/// Uses categorical properties to find or generate entities
/// </summary>
public class SceneSpawnReward
{
    /// <summary>
    /// Template identifier of Scene to spawn
    /// References SceneTemplate in GameWorld.SceneTemplates
    /// </summary>
    public string SceneTemplateId { get; set; }

    /// <summary>
    /// Optional PlacementFilter override
    /// If specified, overrides SceneTemplate.PlacementFilter for this specific spawn
    /// Enables concrete placement in tutorial/story while keeping templates reusable
    /// If null, uses SceneTemplate.PlacementFilter
    /// Categories → FindOrGenerate → Concrete ID stored on Scene
    /// </summary>
    public PlacementFilter PlacementFilterOverride { get; set; }
}
