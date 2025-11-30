/// <summary>
/// DTO for SceneSpawnReward - defines procedural Scene spawning.
/// NO ID STRINGS - uses boolean flag for MainStory sequencing.
///
/// CONTEXT INJECTION (HIGHLANDER, arc42 §8.28):
/// Authored content specifies explicit context; procedural computes from GameWorld.
/// Same DTO structure regardless of source.
/// </summary>
public class SceneSpawnRewardDTO
{
    /// <summary>
    /// TRUE = spawn next MainStory scene in sequence
    /// System determines which template based on Player.CurrentMainStorySequence
    /// NO ID STRINGS - sequence-based lookup only
    /// </summary>
    public bool SpawnNextMainStoryScene { get; set; }

    // ==================== CONTEXT SPECIFICATION (AUTHORED) ====================

    /// <summary>
    /// Explicit target category: Investigation, Social, Confrontation, Crisis, Peaceful.
    /// When set, selector uses this category directly.
    /// </summary>
    public string TargetCategory { get; set; }

    /// <summary>
    /// Explicit location safety context: Safe, Risky, Dangerous.
    /// </summary>
    public string LocationSafetyContext { get; set; }

    /// <summary>
    /// Explicit location purpose context: Commerce, Governance, Worship, etc.
    /// </summary>
    public string LocationPurposeContext { get; set; }

    /// <summary>
    /// Categories to exclude from selection (comma-separated).
    /// </summary>
    public string ExcludedCategories { get; set; }
}
