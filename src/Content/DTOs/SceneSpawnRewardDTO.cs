/// <summary>
/// DTO for SceneSpawnReward - defines procedural Scene spawning
/// Spawns a Scene instance from SceneTemplate with categorical placement
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
    /// Optional PlacementFilter override
    /// If specified, overrides SceneTemplate.PlacementFilter for this specific spawn
    /// Enables concrete placement in tutorial/story while keeping templates reusable
    /// If null, uses SceneTemplate.PlacementFilter
    /// Categories → FindOrGenerate → Concrete ID stored on Scene
    /// </summary>
    public PlacementFilterDTO PlacementFilterOverride { get; set; }
}
