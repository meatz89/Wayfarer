/// <summary>
/// Pure input DTO for scene archetype category selection.
/// Contains inputs for SelectArchetypeCategory in ProceduralAStoryService.
///
/// HIGHLANDER PRINCIPLE (arc42 §8.28):
/// - ALL scene generation uses the SAME code path
/// - Authored content: provides explicit TargetCategory from SceneSpawnReward
/// - Procedural content: computes values from GameWorld state
/// - Same DTO structure regardless of source
///
/// CURRENT IMPLEMENTATION:
/// - Uses: Sequence (rotation), MaxSafeIntensity (player readiness), TargetCategory, ExcludedCategories
/// - Populated but reserved for future: Location context, intensity history, rhythm phase, anti-repetition
///
/// This DTO makes selection deterministic and testable.
/// </summary>
public class SceneSelectionInputs
{
    // ==================== CURRENTLY USED ====================

    /// <summary>
    /// A-story sequence number for base rotation calculation.
    /// USED: cyclePosition = (Sequence - 1) % 4
    /// </summary>
    public int Sequence { get; set; }

    // ==================== LOCATION CONTEXT (Future: Factor 2) ====================
    // Populated by BuildSelectionInputs, reserved for future weighted scoring

    /// <summary>
    /// Location safety level. FUTURE: Dangerous locations could favor Crisis/Confrontation.
    /// </summary>
    public LocationSafety LocationSafety { get; set; }

    /// <summary>
    /// Location purpose. FUTURE: Governance could favor political archetypes.
    /// </summary>
    public LocationPurpose LocationPurpose { get; set; }

    // ==================== INTENSITY TRACKING (Future: Factor 3) ====================
    // Populated by PopulateIntensityHistory, reserved for future weighted scoring

    /// <summary>
    /// Count of Demanding intensity scenes in recent history. FUTURE USE.
    /// </summary>
    public int RecentDemandingCount { get; set; }

    /// <summary>
    /// Count of Recovery intensity scenes in recent history. FUTURE USE.
    /// </summary>
    public int RecentRecoveryCount { get; set; }

    /// <summary>
    /// Count of Standard intensity scenes in recent history. FUTURE USE.
    /// </summary>
    public int RecentStandardCount { get; set; }

    /// <summary>
    /// Number of scenes since last Recovery intensity. FUTURE USE.
    /// </summary>
    public int ScenesSinceRecovery { get; set; }

    /// <summary>
    /// Number of scenes since last Demanding intensity. FUTURE USE.
    /// </summary>
    public int ScenesSinceDemanding { get; set; }

    /// <summary>
    /// Pre-computed flag: recent history is intensity-heavy. FUTURE USE.
    /// </summary>
    public bool IsIntensityHeavy { get; set; }

    /// <summary>
    /// Total scenes in intensity history. FUTURE USE.
    /// </summary>
    public int TotalIntensityHistoryCount { get; set; }

    // ==================== RHYTHM PHASE (Future: Factor 4) ====================
    // Populated by PopulateIntensityHistory, reserved for future weighted scoring

    /// <summary>
    /// Whether last scene was Crisis rhythm. FUTURE USE.
    /// </summary>
    public bool LastSceneWasCrisisRhythm { get; set; }

    /// <summary>
    /// Intensity of the last completed scene (null if no history). FUTURE USE.
    /// </summary>
    public ArchetypeIntensity? LastSceneIntensity { get; set; }

    /// <summary>
    /// Count of consecutive Standard intensity scenes. FUTURE USE.
    /// </summary>
    public int ConsecutiveStandardCount { get; set; }

    // ==================== ANTI-REPETITION (Future: Factor 5) ====================
    // Populated by PopulateIntensityHistory, reserved for future weighted scoring

    /// <summary>
    /// Categories used in last 2 scenes. FUTURE USE.
    /// </summary>
    public List<string> RecentCategories { get; set; } = new List<string>();

    // ==================== CURRENTLY USED ====================

    /// <summary>
    /// Maximum archetype intensity safe for current player state.
    /// USED: Filters categories by player Resolve level.
    /// Derived from PlayerReadinessService.GetMaxSafeIntensity().
    /// </summary>
    public ArchetypeIntensity MaxSafeIntensity { get; set; } = ArchetypeIntensity.Standard;

    // ==================== EXPLICIT SELECTION (AUTHORED OVERRIDE) ====================

    /// <summary>
    /// AUTHORED CONTENT: Explicit target category.
    /// USED: When set, selector returns this category directly.
    /// When null/empty, selector uses rotation logic with player readiness filtering.
    /// </summary>
    public string TargetCategory { get; set; }

    /// <summary>
    /// Categories to exclude from selection.
    /// USED: Skipped during category selection.
    /// </summary>
    public List<string> ExcludedCategories { get; set; } = new List<string>();

    /// <summary>
    /// Create inputs with all defaults (for testing).
    /// </summary>
    public static SceneSelectionInputs CreateDefault(int sequence)
    {
        return new SceneSelectionInputs
        {
            Sequence = sequence,
            LocationSafety = LocationSafety.Safe,
            LocationPurpose = LocationPurpose.Civic,
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
            MaxSafeIntensity = ArchetypeIntensity.Standard,
            TargetCategory = null,
            ExcludedCategories = new List<string>()
        };
    }
}
