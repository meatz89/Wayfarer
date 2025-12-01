
/// <summary>
/// Trigger for spawning a Scene as a Choice reward.
/// THE source of RhythmPattern context for scene selection.
///
/// CONTEXT INJECTION (arc42 §8.28):
/// - RhythmPatternContext: If set, use it for selection
/// - If null: Derive RhythmPattern from intensity history
/// - ONE INPUT: RhythmPattern only (LocationSafety/Purpose/Tier REMOVED)
///
/// WHY CONTEXT ON SPAWN REWARD:
/// Different choices in the same situation can spawn scenes with different contexts.
/// "Accept challenge" might spawn a Crisis scene while "Decline" spawns a Building scene.
/// </summary>
public class SceneSpawnReward
{
    /// <summary>
    /// TRUE = spawn next MainStory scene in sequence
    /// System determines which template based on Player.CurrentMainStorySequence:
    /// - If authored template exists for next sequence → use it
    /// - If not → generate procedurally with RhythmPattern context
    /// </summary>
    public bool SpawnNextMainStoryScene { get; set; }

    /// <summary>
    /// For non-MainStory scenes: direct template reference
    /// Resolved at parse time from GameWorld.SceneTemplates
    /// </summary>
    public SceneTemplate Template { get; set; }

    /// <summary>
    /// Rhythm pattern for scene selection.
    /// If set: Use this RhythmPattern for archetype category selection.
    /// If null: Derive from intensity history at spawn time.
    /// </summary>
    public RhythmPattern? RhythmPatternContext { get; set; }

    /// <summary>
    /// TRUE if RhythmPattern context is authored.
    /// When true, selection uses this value directly.
    /// When false, selection derives from intensity history.
    /// </summary>
    public bool HasAuthoredContext => RhythmPatternContext.HasValue;

    /// <summary>
    /// Build SceneSelectionInputs from authored RhythmPattern context.
    /// REQUIRES: RhythmPatternContext is set.
    /// </summary>
    public SceneSelectionInputs BuildAuthoredInputs()
    {
        if (!RhythmPatternContext.HasValue)
        {
            throw new InvalidOperationException(
                "SceneSpawnReward.BuildAuthoredInputs() called but RhythmPatternContext is null. " +
                "Check HasAuthoredContext before calling.");
        }

        return new SceneSelectionInputs
        {
            RhythmPattern = RhythmPatternContext.Value,
            RecentCategories = new List<string>(),
            RecentArchetypes = new List<string>()
        };
    }
}
