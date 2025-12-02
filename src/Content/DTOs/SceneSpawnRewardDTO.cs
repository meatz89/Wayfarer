/// <summary>
/// DTO for SceneSpawnReward - defines procedural Scene spawning.
/// NO ID STRINGS - uses boolean flag for MainStory sequencing.
///
/// CONTEXT INJECTION (HIGHLANDER, arc42 §8.28):
/// RhythmPattern is THE ONLY context input for scene selection.
/// LocationSafety, LocationPurpose, Tier REMOVED (legacy).
/// </summary>
public class SceneSpawnRewardDTO
{
    /// <summary>
    /// TRUE = spawn next MainStory scene in sequence
    /// System determines which template based on Player.CurrentMainStorySequence
    /// NO ID STRINGS - sequence-based lookup only
    /// </summary>
    public bool SpawnNextMainStoryScene { get; set; }

    /// <summary>
    /// Rhythm pattern context: Building, Crisis, Mixed.
    /// THE ONLY input for scene archetype category selection.
    /// If null: Derive from intensity history at spawn time.
    /// </summary>
    public string RhythmPatternContext { get; set; }
}
