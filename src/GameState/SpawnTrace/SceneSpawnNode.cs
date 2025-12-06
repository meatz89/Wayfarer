/// <summary>
/// Trace node capturing complete scene spawn event
/// Records what spawned what, when, why, and with what properties
/// </summary>
public class SceneSpawnNode
{
    // ==================== CORE IDENTITY ====================

    /// <summary>
    /// Which SceneTemplate spawned this scene
    /// null for authored content (non-template scenes)
    /// </summary>
    public string SceneTemplateId { get; set; }

    /// <summary>
    /// Display name of the scene (for UI)
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// When scene was spawned (absolute timestamp)
    /// </summary>
    public DateTime SpawnTimestamp { get; set; }

    /// <summary>
    /// Which game day scene was spawned
    /// </summary>
    public int GameDay { get; set; }

    /// <summary>
    /// Which time block scene was spawned
    /// </summary>
    public TimeBlocks GameTimeBlock { get; set; }

    // ==================== SPAWN CONTEXT ====================

    /// <summary>
    /// What triggered this scene spawn
    /// </summary>
    public SpawnTriggerType SpawnTrigger { get; set; }

    /// <summary>
    /// Parent scene (if spawned by another scene)
    /// null for root scenes
    /// HIGHLANDER: Direct object reference, no ID strings
    /// </summary>
    public SceneSpawnNode ParentScene { get; set; }

    /// <summary>
    /// Parent situation that spawned this scene
    /// null if not spawned by situation
    /// HIGHLANDER: Direct object reference, no ID strings
    /// </summary>
    public SituationSpawnNode ParentSituation { get; set; }

    /// <summary>
    /// Choice that spawned this scene
    /// null if not spawned by choice
    /// HIGHLANDER: Direct object reference, no ID strings
    /// </summary>
    public ChoiceExecutionNode ParentChoice { get; set; }

    // ==================== SCENE PROPERTIES ====================

    /// <summary>
    /// Reference to spawned Scene entity for transition visualization
    /// Enables SpawnGraphBuilder to access Scene.SpawnRules.Transitions
    /// HIGHLANDER: Direct object reference for transition flow visualization
    /// </summary>
    public Scene SceneEntity { get; set; }

    /// <summary>
    /// Story category (MainStory, SideStory, Encounter)
    /// </summary>
    public StoryCategory Category { get; set; }

    /// <summary>
    /// A-story sequence number (null if not main story)
    /// </summary>
    public int? MainStorySequence { get; set; }

    /// <summary>
    /// Estimated difficulty level
    /// </summary>
    public string EstimatedDifficulty { get; set; }

    /// <summary>
    /// Initial state when spawned (usually Provisional or Active)
    /// </summary>
    public SceneState State { get; set; }

    /// <summary>
    /// Progression mode (Breathe or Cascade)
    /// </summary>
    public ProgressionMode ProgressionMode { get; set; }

    /// <summary>
    /// Number of situations in this scene
    /// </summary>
    public int SituationCount { get; set; }

    /// <summary>
    /// Whether this scene was procedurally generated (true) or authored (false)
    /// </summary>
    public bool IsProcedurallyGenerated { get; set; }

    // ==================== PLACEMENT PROPERTIES ====================

    /// <summary>
    /// Location where scene was placed (snapshot at spawn time)
    /// null if scene has no specific location
    /// </summary>
    public LocationSnapshot PlacedLocation { get; set; }

    /// <summary>
    /// Placement filter used to resolve location (if procedural)
    /// null for authored content
    /// </summary>
    public PlacementFilterSnapshot PlacementFilter { get; set; }

    // ==================== CHILDREN ====================

    /// <summary>
    /// Situations embedded in this scene
    /// Recorded when scene is instantiated
    /// </summary>
    public List<SituationSpawnNode> Situations { get; set; } = new List<SituationSpawnNode>();

    /// <summary>
    /// Scenes spawned by choices within this scene
    /// Populated as choices execute
    /// HIGHLANDER: Direct object references, no ID strings
    /// </summary>
    public List<SceneSpawnNode> SpawnedScenes { get; set; } = new List<SceneSpawnNode>();

    // ==================== LIFECYCLE ====================

    /// <summary>
    /// When player activated this scene (transitioned from Provisional to Active)
    /// null if not yet activated
    /// </summary>
    public DateTime? ActivatedTimestamp { get; set; }

    /// <summary>
    /// Game day when activated
    /// </summary>
    public int? ActivatedGameDay { get; set; }

    /// <summary>
    /// When scene completed
    /// null if not yet completed
    /// </summary>
    public DateTime? CompletedTimestamp { get; set; }

    /// <summary>
    /// Game day when completed
    /// </summary>
    public int? CompletedGameDay { get; set; }

    /// <summary>
    /// When scene expired (if applicable)
    /// null if not expired
    /// </summary>
    public DateTime? ExpiredTimestamp { get; set; }

    /// <summary>
    /// Current state (updated as scene progresses)
    /// </summary>
    public SceneState CurrentState { get; set; }
}
