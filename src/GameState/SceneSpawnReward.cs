
/// <summary>
/// Defines a Scene to be spawned as a Choice reward.
/// Part of Scene-Situation architecture where Choices can spawn new Scenes dynamically.
///
/// CONTEXT INJECTION (HIGHLANDER, arc42 §8.28):
/// - Authored content: Sets categorical properties that flow through selection logic
/// - Procedural content: Properties derived from GameWorld history and location
/// - SAME selection logic processes both - no overrides, no bypasses
///
/// HISTORY-DRIVEN GENERATION (gdd/01 §1.8):
/// - No TargetCategory override - authored content uses categorical inputs
/// - Selection based on rhythm phase + location context + history
/// - Current player state NEVER influences selection
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

    // ==================== CATEGORICAL INPUTS (AUTHORED) ====================
    // When set by authored content, these flow through SAME selection logic as procedural.
    // Selection logic produces appropriate category+intensity from these inputs.
    // NO OVERRIDES - same deterministic logic for authored and procedural.

    /// <summary>
    /// Location safety context for selection.
    /// Dangerous favors Confrontation/Crisis; Safe favors Social/Investigation.
    /// When null, derived from target location at spawn time.
    /// </summary>
    public LocationSafety? LocationSafetyContext { get; set; }

    /// <summary>
    /// Location purpose context for selection.
    /// Governance favors political archetypes; Commerce favors negotiation.
    /// When null, derived from target location at spawn time.
    /// </summary>
    public LocationPurpose? LocationPurposeContext { get; set; }

    /// <summary>
    /// Explicit rhythm phase for selection.
    /// Accumulation grants growth; Test challenges; Recovery restores.
    /// When null, computed from intensity history at spawn time.
    /// </summary>
    public RhythmPhase? RhythmPhaseContext { get; set; }

    /// <summary>
    /// Story tier for selection.
    /// Higher tiers enable more demanding intensity options.
    /// When null, computed from story sequence at spawn time.
    /// </summary>
    public int? TierContext { get; set; }

    /// <summary>
    /// Build SceneSelectionInputs from this reward's categorical inputs.
    /// Returns inputs with authored values set; caller fills in remaining fields from history.
    /// HIGHLANDER: These inputs flow through SAME selection logic as procedural.
    /// </summary>
    public SceneSelectionInputs BuildAuthoredInputs()
    {
        // NULL COALESCING RATIONALE (FAIL-FAST compatible):
        // Authored content may intentionally omit categorical properties to use game-start defaults.
        // This enables partial authored context (e.g., specify only RhythmPhase, let location derive).
        // These are NOT hiding errors - null means "use default for new game scenario".
        return new SceneSelectionInputs
        {
            // Location context - use authored or defaults (Safe/Civic = new game)
            LocationSafety = LocationSafetyContext ?? LocationSafety.Safe,
            LocationPurpose = LocationPurposeContext ?? LocationPurpose.Civic,

            // Rhythm phase - use authored or default to Accumulation (game start = building phase)
            RhythmPhase = RhythmPhaseContext ?? RhythmPhase.Accumulation,

            // Tier - use authored or default to 0 (tutorial tier)
            Tier = TierContext ?? 0,

            // History fields default to empty (game start scenario)
            // Caller can override with actual history if available
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

    /// <summary>
    /// Check if this reward has authored context (explicit categorical inputs).
    /// Even one authored property means we use authored inputs as base.
    /// </summary>
    public bool HasAuthoredContext =>
        LocationSafetyContext.HasValue
        || LocationPurposeContext.HasValue
        || RhythmPhaseContext.HasValue
        || TierContext.HasValue;
}
