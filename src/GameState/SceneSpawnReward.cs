
/// <summary>
/// Defines a Scene to be spawned as a Choice reward.
/// Part of Scene-Situation architecture where Choices can spawn new Scenes dynamically.
///
/// CONTEXT INJECTION (HIGHLANDER, arc42 §8.28):
/// - Authored content: Sets explicit context fields to specify next scene
/// - Procedural content: Context computed from GameWorld at spawn time
/// - Same code path: fields → SceneSelectionInputs → selector → generation
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

    // ==================== CONTEXT SPECIFICATION (AUTHORED) ====================
    // When set by authored content, these fields specify EXACTLY what the next scene should be.
    // When null, procedural generation computes values from GameWorld state.

    /// <summary>
    /// Explicit target category: Investigation, Social, Confrontation, Crisis, Peaceful.
    /// When set, selector uses this category directly (deterministic authored sequence).
    /// When null, selector uses weighted scoring from computed inputs.
    /// </summary>
    public string TargetCategory { get; set; }

    /// <summary>
    /// Explicit location safety context for selection.
    /// When set, overrides computed value from target location.
    /// </summary>
    public LocationSafety? LocationSafetyContext { get; set; }

    /// <summary>
    /// Explicit location purpose context for selection.
    /// When set, overrides computed value from target location.
    /// </summary>
    public LocationPurpose? LocationPurposeContext { get; set; }

    /// <summary>
    /// Categories to exclude from selection.
    /// </summary>
    public List<string> ExcludedCategories { get; set; } = new List<string>();

    /// <summary>
    /// Build SceneSelectionInputs from this reward's explicit context.
    /// Returns inputs with authored values set; caller fills in remaining fields from GameWorld.
    /// </summary>
    public SceneSelectionInputs BuildAuthoredInputs(int sequence)
    {
        return new SceneSelectionInputs
        {
            Sequence = sequence,
            TargetCategory = TargetCategory,
            LocationSafety = LocationSafetyContext ?? LocationSafety.Safe,
            LocationPurpose = LocationPurposeContext ?? LocationPurpose.Civic,
            ExcludedCategories = ExcludedCategories ?? new List<string>()
        };
    }

    /// <summary>
    /// Check if this reward has authored context (explicit specification).
    /// </summary>
    public bool HasAuthoredContext => !string.IsNullOrEmpty(TargetCategory)
        || LocationSafetyContext.HasValue
        || LocationPurposeContext.HasValue
        || (ExcludedCategories != null && ExcludedCategories.Count > 0);
}
