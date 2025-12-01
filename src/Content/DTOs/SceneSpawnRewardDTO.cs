/// <summary>
/// DTO for SceneSpawnReward - defines procedural Scene spawning.
/// NO ID STRINGS - uses boolean flag for MainStory sequencing.
///
/// CONTEXT INJECTION (HIGHLANDER, arc42 §8.28):
/// Authored content specifies categorical inputs; procedural derives from GameWorld.
/// Same selection logic processes both - no overrides, no bypasses.
///
/// HISTORY-DRIVEN GENERATION (gdd/01 §1.8):
/// No TargetCategory override - authored uses categorical properties.
/// Selection based on rhythm pattern + location context + history.
/// </summary>
public class SceneSpawnRewardDTO
{
    /// <summary>
    /// TRUE = spawn next MainStory scene in sequence
    /// System determines which template based on Player.CurrentMainStorySequence
    /// NO ID STRINGS - sequence-based lookup only
    /// </summary>
    public bool SpawnNextMainStoryScene { get; set; }

    // ==================== CATEGORICAL INPUTS (AUTHORED) ====================
    // These flow through SAME selection logic as procedural content.
    // Parser converts strings to enums; selection logic processes identically.

    /// <summary>
    /// Location safety context: Safe, Risky, Dangerous.
    /// Dangerous favors Confrontation/Crisis; Safe favors Social/Investigation.
    /// </summary>
    public string LocationSafetyContext { get; set; }

    /// <summary>
    /// Location purpose context: Commerce, Governance, Worship, etc.
    /// Governance favors political archetypes; Commerce favors negotiation.
    /// </summary>
    public string LocationPurposeContext { get; set; }

    /// <summary>
    /// Rhythm pattern context: Building, Crisis, Mixed.
    /// Determines which categories are appropriate for selection.
    /// </summary>
    public string RhythmPatternContext { get; set; }

    /// <summary>
    /// Story tier for selection (0-3).
    /// Higher tiers enable more demanding intensity options.
    /// </summary>
    public int? TierContext { get; set; }
}
