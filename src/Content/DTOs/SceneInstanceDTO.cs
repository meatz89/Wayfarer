/// <summary>
/// DTO for scene instance creation - template reference + complete context.
/// Used by BOTH authored (JSON → DTO) and procedural (code → DTO) paths.
///
/// CONTEXT INJECTION (arc42 §8.28):
/// - SceneArchetype references the pure template (InnLodging, SeekAudience, etc.)
/// - Context properties (tier, rhythmPattern, locationSafety, locationPurpose) are REQUIRED
/// - No nullable context - complete at parse time, no defaults
/// - Parser validates all required properties at load time (FAIL-FAST)
///
/// HIGHLANDER: Same DTO for both paths. Difference is only the SOURCE:
/// - Authored: JSON file → SceneInstanceDTO → Parser
/// - Procedural: Code creates SceneInstanceDTO → Parser
/// </summary>
public class SceneInstanceDTO
{
    /// <summary>
    /// Unique identifier for this scene instance.
    /// Authored: "a1_secure_lodging", "a2_morning"
    /// Procedural: Generated at creation time
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Reference to SceneTemplate archetype from SceneArchetypeCatalog.
    /// REQUIRED - the pure template this instance is based on.
    /// Values: InnLodging, DeliveryContract, RouteSegmentTravel, SeekAudience, etc.
    /// </summary>
    public string SceneArchetype { get; set; }

    /// <summary>
    /// Story category - REQUIRED.
    /// Values: "MainStory", "SideStory", "Service"
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Main story sequence number - REQUIRED for MainStory category.
    /// 1-10 = Authored tutorial scenes, 11+ = Procedural continuation
    /// </summary>
    public int? MainStorySequence { get; set; }

    /// <summary>
    /// Whether this scene is created as Deferred at game start.
    /// true = Created during SpawnStarterScenes (e.g., A1)
    /// false = Created when spawn reward fires (e.g., A2, A3, procedural)
    /// </summary>
    public bool IsStarter { get; set; }

    // ==================== CONTEXT (ALL REQUIRED, NO NULLS) ====================

    /// <summary>
    /// Complexity tier (0-3) - REQUIRED, no defaults.
    /// Part of complete context for scene instantiation.
    /// </summary>
    public int Tier { get; set; }

    /// <summary>
    /// Sir Brante rhythm pattern - REQUIRED, no defaults.
    /// Values: "Building", "Crisis", "Mixed"
    /// Determines choice structure and consequence polarity.
    /// </summary>
    public string RhythmPattern { get; set; }

    /// <summary>
    /// Location safety context - REQUIRED, no defaults.
    /// Values: "Safe", "Neutral", "Dangerous"
    /// Affects archetype selection and difficulty scaling.
    /// </summary>
    public string LocationSafety { get; set; }

    /// <summary>
    /// Location purpose context - REQUIRED, no defaults.
    /// Values: "Transit", "Dwelling", "Commerce", "Civic", "Defense",
    ///         "Governance", "Worship", "Learning", "Entertainment", "Generic"
    /// Affects available actions and narrative flavor.
    /// </summary>
    public string LocationPurpose { get; set; }

    // ==================== SCENE STRUCTURE ====================

    /// <summary>
    /// Scene pattern classification.
    /// Values: "Linear", "HubAndSpoke", "Branching", "Converging", "Discovery"
    /// </summary>
    public string Archetype { get; set; }

    /// <summary>
    /// Display name template with {placeholders}
    /// </summary>
    public string DisplayNameTemplate { get; set; }

    /// <summary>
    /// Presentation mode - how scene appears.
    /// Values: "Atmospheric", "Modal"
    /// </summary>
    public string PresentationMode { get; set; }

    /// <summary>
    /// Progression mode - how situations flow.
    /// Values: "Breathe", "Cascade"
    /// </summary>
    public string ProgressionMode { get; set; }

    // ==================== ACTIVATION TRIGGERS ====================

    /// <summary>
    /// Location activation filter - when scene activates.
    /// </summary>
    public PlacementFilterDTO LocationActivationFilter { get; set; }

    /// <summary>
    /// NPC activation filter - optional NPC presence requirement.
    /// </summary>
    public PlacementFilterDTO NpcActivationFilter { get; set; }

    /// <summary>
    /// Temporal eligibility conditions.
    /// </summary>
    public SpawnConditionsDTO SpawnConditions { get; set; }

    // ==================== NARRATIVE ====================

    /// <summary>
    /// Intro narrative template with {placeholders}
    /// </summary>
    public string IntroNarrativeTemplate { get; set; }
}
