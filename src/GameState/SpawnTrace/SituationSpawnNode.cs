/// <summary>
/// Trace node capturing complete situation spawn event
/// Records when situation was created, what context it requires, and what choices were made
/// </summary>
public class SituationSpawnNode
{
    // ==================== CORE IDENTITY ====================

    /// <summary>
    /// Unique identifier for this trace node (GUID)
    /// </summary>
    public string NodeId { get; set; }

    /// <summary>
    /// Which SituationTemplate spawned this situation
    /// null for authored situations (non-template)
    /// </summary>
    public string SituationTemplateId { get; set; }

    /// <summary>
    /// Display name of the situation
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Narrative description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// When situation was spawned
    /// </summary>
    public DateTime SpawnTimestamp { get; set; }

    // ==================== SPAWN CONTEXT ====================

    /// <summary>
    /// NodeId of parent scene containing this situation
    /// </summary>
    public string ParentSceneNodeId { get; set; }

    /// <summary>
    /// What triggered this situation spawn
    /// </summary>
    public SituationSpawnTriggerType SpawnTrigger { get; set; }

    /// <summary>
    /// NodeId of parent situation that spawned this situation (for cascades)
    /// null if initial situation in scene
    /// </summary>
    public string ParentSituationNodeId { get; set; }

    // ==================== SITUATION PROPERTIES ====================

    /// <summary>
    /// Situation type (Normal or Crisis)
    /// </summary>
    public SituationType Type { get; set; }

    /// <summary>
    /// Tactical system type (Social, Mental, Physical)
    /// </summary>
    public TacticalSystemType SystemType { get; set; }

    /// <summary>
    /// Interaction type (Instant, Challenge, Navigation)
    /// </summary>
    public SituationInteractionType InteractionType { get; set; }

    // ==================== PLACEMENT PROPERTIES ====================

    /// <summary>
    /// Location where situation takes place (snapshot at spawn time)
    /// null if no location requirement
    /// </summary>
    public LocationSnapshot Location { get; set; }

    /// <summary>
    /// NPC involved in situation (snapshot at spawn time)
    /// null if no NPC requirement
    /// </summary>
    public NPCSnapshot NPC { get; set; }

    /// <summary>
    /// Route associated with situation (snapshot at spawn time)
    /// null if no route requirement
    /// </summary>
    public RouteSnapshot Route { get; set; }

    /// <summary>
    /// Specific segment index for route situations
    /// null if not route-based
    /// </summary>
    public int? SegmentIndex { get; set; }

    // ==================== CHILDREN ====================

    /// <summary>
    /// Choices executed within this situation
    /// Populated as player makes choices
    /// </summary>
    public List<ChoiceExecutionNode> Choices { get; set; } = new List<ChoiceExecutionNode>();

    /// <summary>
    /// NodeIds of situations spawned by choices in this situation (cascades)
    /// Populated as choices execute and spawn new situations
    /// </summary>
    public List<string> SpawnedSituationNodeIds { get; set; } = new List<string>();

    // ==================== LIFECYCLE ====================

    /// <summary>
    /// Lifecycle status (Selectable, Completed, Failed, etc.)
    /// </summary>
    public LifecycleStatus LifecycleStatus { get; set; }

    /// <summary>
    /// When situation was completed
    /// null if not yet completed
    /// </summary>
    public DateTime? CompletedTimestamp { get; set; }

    /// <summary>
    /// Whether the last challenge succeeded
    /// null if no challenge attempted, or instant action
    /// </summary>
    public bool? LastChallengeSucceeded { get; set; }
}
