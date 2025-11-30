/// <summary>
/// Pure input DTO for scene archetype selection.
/// Contains categorical inputs that drive selection logic in ProceduralAStoryService.
///
/// HIGHLANDER PRINCIPLE (arc42 §8.28):
/// - ALL scene generation uses the SAME selection logic
/// - Authored content: provides hardcoded categorical properties
/// - Procedural content: derives properties from GameWorld history
/// - Same DTO structure, same logic—only source differs
///
/// HISTORY-DRIVEN GENERATION (gdd/01 §1.8):
/// - Selection is based on PAST scene history, not current player state
/// - Current Resolve, stats, resources NEVER influence selection
/// - The player's current state is their responsibility
///
/// TWO ORTHOGONAL DIMENSIONS (arc42 §8.26):
/// - Category = WHAT kind of narrative (Investigation, Social, Confrontation, Crisis)
/// - ArchetypeIntensity = HOW demanding mechanically (Recovery, Standard, Demanding)
/// - These combine freely: Crisis/Recovery, Investigation/Demanding, etc.
///
/// This DTO makes selection deterministic and testable.
/// </summary>
public class SceneSelectionInputs
{
    // ==================== LOCATION CONTEXT (Primary Driver) ====================
    // Influences which categories are appropriate for the setting

    /// <summary>
    /// Location safety level.
    /// Dangerous locations favor Confrontation/Crisis categories.
    /// Safe locations favor Social/Investigation categories.
    /// </summary>
    public LocationSafety LocationSafety { get; set; }

    /// <summary>
    /// Location purpose.
    /// Governance locations favor political archetypes.
    /// Commerce locations favor negotiation archetypes.
    /// </summary>
    public LocationPurpose LocationPurpose { get; set; }

    /// <summary>
    /// Location privacy level.
    /// Public locations have witnesses, affect scene stakes.
    /// Private locations enable different narrative options.
    /// </summary>
    public LocationPrivacy LocationPrivacy { get; set; }

    /// <summary>
    /// Location activity level.
    /// Busy locations favor social encounters.
    /// Quiet locations favor investigation.
    /// </summary>
    public LocationActivity LocationActivity { get; set; }

    // ==================== INTENSITY HISTORY (Primary Driver - Rhythm Phase) ====================
    // Determines where player is in Building/Test/Recovery rhythm

    /// <summary>
    /// Count of Demanding intensity scenes in recent history window.
    /// High count suggests recovery is needed.
    /// </summary>
    public int RecentDemandingCount { get; set; }

    /// <summary>
    /// Count of Recovery intensity scenes in recent history window.
    /// High count suggests testing is appropriate.
    /// </summary>
    public int RecentRecoveryCount { get; set; }

    /// <summary>
    /// Count of Standard intensity scenes in recent history window.
    /// Used for pacing calculations.
    /// </summary>
    public int RecentStandardCount { get; set; }

    /// <summary>
    /// Number of scenes since last Recovery intensity.
    /// High value indicates player needs breathing room.
    /// </summary>
    public int ScenesSinceRecovery { get; set; }

    /// <summary>
    /// Number of scenes since last Demanding intensity.
    /// High value indicates player is overdue for challenge.
    /// </summary>
    public int ScenesSinceDemanding { get; set; }

    /// <summary>
    /// Pre-computed flag: recent history is intensity-heavy.
    /// When true, selection should favor Recovery intensity.
    /// </summary>
    public bool IsIntensityHeavy { get; set; }

    /// <summary>
    /// Total scenes in intensity history window.
    /// Zero indicates game start (tutorial or new procedural).
    /// </summary>
    public int TotalIntensityHistoryCount { get; set; }

    // ==================== RHYTHM PHASE (Primary Driver - Derived from History) ====================
    // Pre-computed rhythm state for selection logic

    /// <summary>
    /// Whether last scene used Crisis rhythm pattern.
    /// After crisis, selection should favor Building/Recovery.
    /// </summary>
    public bool LastSceneWasCrisisRhythm { get; set; }

    /// <summary>
    /// Intensity of the last completed scene.
    /// Null indicates no history (game start).
    /// </summary>
    public ArchetypeIntensity? LastSceneIntensity { get; set; }

    /// <summary>
    /// Count of consecutive Standard intensity scenes.
    /// High count suggests variety is needed.
    /// </summary>
    public int ConsecutiveStandardCount { get; set; }

    /// <summary>
    /// Computed rhythm phase based on intensity history.
    /// Accumulation = grant opportunities to grow
    /// Test = challenge based on expected stats
    /// Recovery = restore after crisis
    /// </summary>
    public RhythmPhase RhythmPhase { get; set; }

    // ==================== ANTI-REPETITION (Secondary Driver) ====================
    // Prevents same category appearing multiple times in a row

    /// <summary>
    /// Categories used in last 2 scenes.
    /// Selection avoids these for variety.
    /// </summary>
    public List<string> RecentCategories { get; set; } = new List<string>();

    /// <summary>
    /// Archetypes used in recent scenes.
    /// Selection avoids these for variety.
    /// </summary>
    public List<string> RecentArchetypes { get; set; } = new List<string>();

    // ==================== NPC CONTEXT (Secondary Driver) ====================
    // When scene involves specific NPC, their properties influence selection

    /// <summary>
    /// NPC demeanor if scene involves specific NPC.
    /// Hostile NPCs favor Confrontation categories.
    /// Friendly NPCs favor Social categories.
    /// </summary>
    public NPCDemeanor? NpcDemeanor { get; set; }

    /// <summary>
    /// Relationship level with NPC if applicable.
    /// Strong relationships enable different narrative options.
    /// </summary>
    public int? NpcRelationshipLevel { get; set; }

    // ==================== TIER (Secondary Driver) ====================
    // Higher tiers enable more variety and intensity

    /// <summary>
    /// Current story tier.
    /// Higher tiers enable more demanding intensity options.
    /// </summary>
    public int Tier { get; set; }

    /// <summary>
    /// Create inputs with all defaults (for testing).
    /// Represents game start with no history.
    /// </summary>
    public static SceneSelectionInputs CreateDefault()
    {
        return new SceneSelectionInputs
        {
            LocationSafety = LocationSafety.Safe,
            LocationPurpose = LocationPurpose.Civic,
            LocationPrivacy = LocationPrivacy.Public,
            LocationActivity = LocationActivity.Moderate,
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
            RhythmPhase = RhythmPhase.Accumulation,
            RecentCategories = new List<string>(),
            RecentArchetypes = new List<string>(),
            NpcDemeanor = null,
            NpcRelationshipLevel = null,
            Tier = 0
        };
    }
}

/// <summary>
/// Rhythm phase computed from intensity history.
/// Determines what kind of scene is appropriate next.
/// </summary>
public enum RhythmPhase
{
    /// <summary>
    /// Player is building stats/resources.
    /// Grant opportunities to grow.
    /// Favor Building situations with Recovery/Standard intensity.
    /// </summary>
    Accumulation,

    /// <summary>
    /// Player has accumulated enough - time to test.
    /// Challenge based on expected stats.
    /// Favor Crisis situations with Standard/Demanding intensity.
    /// </summary>
    Test,

    /// <summary>
    /// Player just survived a crisis.
    /// Restore after the challenge.
    /// Favor Building situations with Recovery intensity.
    /// </summary>
    Recovery
}
