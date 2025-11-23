/// <summary>
/// Trace node capturing complete scene spawn event
/// Records what spawned what, when, why, and with what properties
/// </summary>
public class SceneSpawnNode
{
    // ==================== CORE IDENTITY ====================

    /// <summary>
    /// Unique identifier for this trace node (GUID)
    /// NOT a domain entity ID - this is meta-data for tracing only
    /// Stable reference across method calls and parent-child linking
    /// </summary>
    public string NodeId { get; set; }

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
    /// NodeId of parent scene (if spawned by another scene)
    /// null for root scenes
    /// </summary>
    public string ParentSceneNodeId { get; set; }

    /// <summary>
    /// NodeId of parent situation that spawned this scene
    /// null if not spawned by situation
    /// </summary>
    public string ParentSituationNodeId { get; set; }

    /// <summary>
    /// NodeId of choice that spawned this scene
    /// null if not spawned by choice
    /// </summary>
    public string ParentChoiceNodeId { get; set; }

    // ==================== SCENE PROPERTIES ====================

    /// <summary>
    /// Story category (MainStory, SideStory, Service)
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
    /// NodeIds of scenes spawned by choices within this scene
    /// Populated as choices execute
    /// </summary>
    public List<string> SpawnedSceneNodeIds { get; set; } = new List<string>();

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
