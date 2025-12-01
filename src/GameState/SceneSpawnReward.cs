
/// <summary>
/// Trigger for spawning a Scene as a Choice reward.
/// Contains optional context that, if specified, MUST be complete (no merging).
///
/// CONTEXT INJECTION (arc42 §8.28):
/// - If context properties are set: Use authored context (all 4 required)
/// - If context properties are null: Derive ALL from game state
/// - NO MERGING - it's one or the other, never partial
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
    /// - If not → generate procedurally with context (authored or derived)
    /// </summary>
    public bool SpawnNextMainStoryScene { get; set; }

    /// <summary>
    /// For non-MainStory scenes: direct template reference
    /// Resolved at parse time from GameWorld.SceneTemplates
    /// </summary>
    public SceneTemplate Template { get; set; }

    // ==================== OPTIONAL CONTEXT (ALL OR NOTHING) ====================
    // If ANY context property is set, ALL must be set (no merging with game state).
    // If NONE are set, all context is derived from game state at spawn time.

    /// <summary>
    /// Location safety context for selection.
    /// Part of authored context - if set, all 4 context properties must be set.
    /// </summary>
    public LocationSafety? LocationSafetyContext { get; set; }

    /// <summary>
    /// Location purpose context for selection.
    /// Part of authored context - if set, all 4 context properties must be set.
    /// </summary>
    public LocationPurpose? LocationPurposeContext { get; set; }

    /// <summary>
    /// Rhythm pattern for selection.
    /// Part of authored context - if set, all 4 context properties must be set.
    /// </summary>
    public RhythmPattern? RhythmPatternContext { get; set; }

    /// <summary>
    /// Story tier for selection.
    /// Part of authored context - if set, all 4 context properties must be set.
    /// </summary>
    public int? TierContext { get; set; }

    /// <summary>
    /// TRUE if any context property is set (indicating authored context mode).
    /// When true, all 4 properties must be set - partial context is invalid.
    /// </summary>
    public bool HasAuthoredContext =>
        LocationSafetyContext.HasValue
        || LocationPurposeContext.HasValue
        || RhythmPatternContext.HasValue
        || TierContext.HasValue;

    /// <summary>
    /// Validate that context is complete (all 4 properties set) if any are set.
    /// FAIL-FAST: Throws if partial context is specified.
    /// </summary>
    public void ValidateContext()
    {
        if (!HasAuthoredContext) return; // No context = derive from game state

        // If any context is specified, all must be specified (no merging)
        if (!LocationSafetyContext.HasValue)
            throw new InvalidOperationException("SceneSpawnReward has partial context: LocationSafetyContext is missing");
        if (!LocationPurposeContext.HasValue)
            throw new InvalidOperationException("SceneSpawnReward has partial context: LocationPurposeContext is missing");
        if (!RhythmPatternContext.HasValue)
            throw new InvalidOperationException("SceneSpawnReward has partial context: RhythmPatternContext is missing");
        if (!TierContext.HasValue)
            throw new InvalidOperationException("SceneSpawnReward has partial context: TierContext is missing");
    }

    /// <summary>
    /// Build SceneSelectionInputs from authored context.
    /// REQUIRES: All 4 context properties are set (call ValidateContext first).
    /// </summary>
    public SceneSelectionInputs BuildAuthoredInputs()
    {
        ValidateContext();

        return new SceneSelectionInputs
        {
            LocationSafety = LocationSafetyContext.Value,
            LocationPurpose = LocationPurposeContext.Value,
            RhythmPattern = RhythmPatternContext.Value,
            Tier = TierContext.Value,
            // History fields empty - authored context is self-contained
            RecentDemandingCount = 0,
            RecentRecoveryCount = 0,
            RecentStandardCount = 0,
            ScenesSinceRecovery = 0,
            ScenesSinceDemanding = 0,
            IsIntensityHeavy = false,
            TotalIntensityHistoryCount = 0,
            LastSceneWasCrisisRhythm = false,
            LastSceneIntensity = null,
            ConsecutiveStandardCount = 0,
            RecentCategories = new List<string>(),
            RecentArchetypes = new List<string>()
        };
    }
}
