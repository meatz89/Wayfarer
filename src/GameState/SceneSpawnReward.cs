
/// <summary>
/// Trigger for spawning a Scene as a Choice reward.
/// THE source of RhythmPattern context for scene selection.
///
/// CONTEXT INJECTION (arc42 §8.28):
/// - RhythmPatternContext MUST be set before use
/// - For authored content: set explicitly in JSON
/// - For procedural content: computed and SET at activation time
/// - NO derivation at consumption time - spawn reward is THE source
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
    /// MUST be set before use - either authored or computed at activation.
    /// </summary>
    public RhythmPattern? RhythmPatternContext { get; set; }

    /// <summary>
    /// Get RhythmPattern, throwing if not set.
    /// Call EnsureRhythmPatternSet() before using this.
    /// </summary>
    public RhythmPattern GetRhythmPattern()
    {
        if (!RhythmPatternContext.HasValue)
        {
            throw new InvalidOperationException(
                "SceneSpawnReward.RhythmPatternContext is not set. " +
                "Call EnsureRhythmPatternSet() before using spawn reward. " +
                "arc42 §8.28: Context MUST be on spawn reward, no derivation at consumption.");
        }
        return RhythmPatternContext.Value;
    }

    /// <summary>
    /// Build SceneSelectionInputs from spawn reward context.
    /// REQUIRES: RhythmPatternContext is set.
    /// </summary>
    public SceneSelectionInputs BuildSelectionInputs(List<string> recentCategories, List<string> recentArchetypes)
    {
        return new SceneSelectionInputs
        {
            RhythmPattern = GetRhythmPattern(),
            RecentCategories = recentCategories ?? new List<string>(),
            RecentArchetypes = recentArchetypes ?? new List<string>()
        };
    }
}
