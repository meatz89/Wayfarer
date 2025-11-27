
/// <summary>
/// Defines a Scene to be spawned as a Choice reward
/// Part of Scene-Situation architecture where Choices can spawn new Scenes dynamically
/// NO ID STRINGS - uses boolean flags and direct object references
/// </summary>
public class SceneSpawnReward
{
    /// <summary>
    /// TRUE = spawn next MainStory scene in sequence
    /// System determines which template based on Player.CurrentMainStorySequence:
    /// - If authored template exists for next sequence → use it
    /// - If not → generate procedurally
    /// NO ID STRINGS - sequence-based lookup only
    /// </summary>
    public bool SpawnNextMainStoryScene { get; set; }

    /// <summary>
    /// For non-MainStory scenes: direct template reference
    /// Resolved at parse time from GameWorld.SceneTemplates
    /// NO ID STRINGS - object reference only
    /// </summary>
    public SceneTemplate Template { get; set; }
}
