/// <summary>
/// Data Transfer Object for Scene instances - serves BOTH creation and runtime.
/// Used by authored (JSON) and procedural (code) paths identically.
/// Maps to Scene domain entity.
///
/// CONTEXT INJECTION (arc42 §8.28):
/// - RhythmPattern is THE context for scene selection
/// - Choice scaling uses Location.Difficulty (orthogonal system)
/// - Same DTO for both paths - parser has no knowledge of source
/// </summary>
public class SceneDTO
{
    /// <summary>
    /// Unique identifier for this scene instance
    /// Authored: "a1_secure_lodging", Procedural: generated at spawn time
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Reference to SceneTemplate ID that generated this instance.
    /// Links back to source template for traceability.
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Reference to SceneTemplate archetype from catalog.
    /// Values: InnLodging, DeliveryContract, RouteSegmentTravel, SeekAudience, etc.
    /// </summary>
    public string SceneArchetype { get; set; }

    // ==================== CONTEXT (CREATION-TIME) ====================

    /// <summary>
    /// Sir Brante rhythm pattern - context for scene selection.
    /// Values: "Building", "Crisis", "Mixed"
    /// </summary>
    public string RhythmPattern { get; set; }

    /// <summary>
    /// Whether this scene is created at game start.
    /// true = Created during SpawnStarterScenes (e.g., A1)
    /// false = Created when spawn reward fires
    /// </summary>
    public bool IsStarter { get; set; }

    // ==================== ACTIVATION FILTERS ====================

    /// <summary>
    /// Location activation filter - determines WHEN scene activates
    /// </summary>
    public PlacementFilterDTO LocationActivationFilter { get; set; }

    /// <summary>
    /// NPC activation filter - optional NPC presence requirement
    /// </summary>
    public PlacementFilterDTO NpcActivationFilter { get; set; }

    /// <summary>
    /// Temporal eligibility conditions
    /// </summary>
    public SpawnConditionsDTO SpawnConditions { get; set; }

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
    /// Intro narrative template with {placeholders}
    /// </summary>
    public string IntroNarrativeTemplate { get; set; }

    /// <summary>
    /// How this scene presents to the player
    /// Values: "Atmospheric", "Modal"
    /// </summary>
    public string PresentationMode { get; set; }

    /// <summary>
    /// How situations progress through the scene
    /// Values: "Breathe", "Cascade"
    /// </summary>
    public string ProgressionMode { get; set; }

    /// <summary>
    /// Story category classification
    /// Values: "MainStory", "SideStory", "Service"
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Main story sequence number
    /// 1-10 = Authored tutorial scenes, 11+ = Procedural
    /// null = Not part of A-story
    /// </summary>
    public int? MainStorySequence { get; set; }

    // ==================== RUNTIME STATE ====================

    /// <summary>
    /// Current lifecycle state
    /// Values: "Deferred", "Active", "Completed", "Expired"
    /// </summary>
    public string State { get; set; }

    /// <summary>
    /// Generated display name with placeholders replaced
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Generated intro narrative with placeholders replaced
    /// </summary>
    public string IntroNarrative { get; set; }

    /// <summary>
    /// Day when this scene expires (time-limited scenes)
    /// null = no expiration
    /// </summary>
    public int? ExpiresOnDay { get; set; }

    /// <summary>
    /// Situation ID player is currently engaged with
    /// null = scene not started or completed
    /// </summary>
    public string CurrentSituationId { get; set; }

    /// <summary>
    /// Source situation that spawned this provisional scene
    /// null for finalized scenes
    /// </summary>
    public string SourceSituationId { get; set; }

    /// <summary>
    /// Situation spawn rules defining cascade structure
    /// </summary>
    public SituationSpawnRulesDTO SpawnRules { get; set; }

    /// <summary>
    /// Embedded situations owned by this scene
    /// </summary>
    public List<SituationDTO> Situations { get; set; } = new List<SituationDTO>();
}
