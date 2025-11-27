/// <summary>
/// DTO for SceneSpawnReward - defines procedural Scene spawning
/// NO ID STRINGS - uses boolean flag for MainStory sequencing
/// </summary>
public class SceneSpawnRewardDTO
{
    /// <summary>
    /// TRUE = spawn next MainStory scene in sequence
    /// System determines which template based on Player.CurrentMainStorySequence
    /// NO ID STRINGS - sequence-based lookup only
    /// </summary>
    public bool SpawnNextMainStoryScene { get; set; }
}
