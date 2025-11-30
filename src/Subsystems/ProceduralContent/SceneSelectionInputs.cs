/// <summary>
/// Pure input DTO for scene archetype category selection.
/// Contains ALL required inputs for ArchetypeCategorySelector.
///
/// HIGHLANDER PRINCIPLE (arc42 §8.28):
/// - ALL scene generation uses the SAME code path
/// - ALL fields are REQUIRED (no optional parameters)
/// - Authored content: provides explicit values from SceneSpawnReward
/// - Procedural content: computes values from GameWorld state
/// - Same DTO structure regardless of source
///
/// This DTO makes selection deterministic and testable.
/// </summary>
public class SceneSelectionInputs
{
    // ==================== REQUIRED INPUTS ====================
    // All fields must have values - no nulls, no optionals

    /// <summary>
    /// A-story sequence number for base rotation calculation.
    /// </summary>
    public int Sequence { get; set; }

    // ==================== LOCATION CONTEXT (Factor 2) ====================

    /// <summary>
    /// Location safety level for context scoring.
    /// Dangerous locations favor Crisis/Confrontation.
    /// Safe locations favor Peaceful/Social.
    /// </summary>
    public LocationSafety LocationSafety { get; set; }

    /// <summary>
    /// Location purpose for context scoring.
    /// Governance favors political archetypes.
    /// Worship favors peaceful reflection.
    /// </summary>
    public LocationPurpose LocationPurpose { get; set; }

    // ==================== INTENSITY TRACKING (Factor 3) ====================

    /// <summary>
    /// Count of Demanding intensity scenes in recent history.
    /// </summary>
    public int RecentDemandingCount { get; set; }

    /// <summary>
    /// Count of Recovery intensity scenes in recent history.
    /// </summary>
    public int RecentRecoveryCount { get; set; }

    /// <summary>
    /// Count of Standard intensity scenes in recent history.
    /// </summary>
    public int RecentStandardCount { get; set; }

    /// <summary>
    /// Number of scenes since last Recovery intensity.
    /// </summary>
    public int ScenesSinceRecovery { get; set; }

    /// <summary>
    /// Number of scenes since last Demanding intensity.
    /// </summary>
    public int ScenesSinceDemanding { get; set; }

    /// <summary>
    /// Pre-computed flag: recent history is intensity-heavy.
    /// </summary>
    public bool IsIntensityHeavy { get; set; }

    /// <summary>
    /// Total scenes in intensity history.
    /// </summary>
    public int TotalIntensityHistoryCount { get; set; }

    // ==================== RHYTHM PHASE (Factor 4) ====================

    /// <summary>
    /// Whether last scene was Crisis rhythm.
    /// </summary>
    public bool LastSceneWasCrisisRhythm { get; set; }

    /// <summary>
    /// Intensity of the last completed scene (null if no history).
    /// </summary>
    public ArchetypeIntensity? LastSceneIntensity { get; set; }

    /// <summary>
    /// Count of consecutive Standard intensity scenes.
    /// </summary>
    public int ConsecutiveStandardCount { get; set; }

    // ==================== ANTI-REPETITION (Factor 5) ====================

    /// <summary>
    /// Categories used in last 2 scenes.
    /// </summary>
    public List<string> RecentCategories { get; set; } = new List<string>();

    // ==================== PLAYER READINESS (CURRENT STATE) ====================

    /// <summary>
    /// Maximum archetype intensity safe for current player state.
    /// Derived from PlayerReadinessService.GetMaxSafeIntensity().
    /// Categories mapped: Investigation/Social → Standard, Confrontation/Crisis → Demanding, Peaceful → Recovery
    /// </summary>
    public ArchetypeIntensity MaxSafeIntensity { get; set; } = ArchetypeIntensity.Standard;

    // ==================== EXPLICIT SELECTION (AUTHORED OVERRIDE) ====================

    /// <summary>
    /// AUTHORED CONTENT: Explicit target category.
    /// When set by authored content, selector uses this category directly.
    /// When null/empty, selector uses weighted scoring.
    /// </summary>
    public string TargetCategory { get; set; }

    /// <summary>
    /// Categories to exclude from selection.
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
