/// <summary>
/// Trace node capturing complete choice execution event
/// Records what player chose, what they paid, what they received, and what spawned as consequence
/// </summary>
public class ChoiceExecutionNode
{
    // ==================== CORE IDENTITY ====================

    /// <summary>
    /// Unique identifier for this trace node (GUID)
    /// </summary>
    public string NodeId { get; set; }

    /// <summary>
    /// ChoiceTemplate ID that was executed
    /// </summary>
    public string ChoiceId { get; set; }

    /// <summary>
    /// Action text displayed to player (after placeholder replacement)
    /// </summary>
    public string ActionText { get; set; }

    /// <summary>
    /// When choice was executed
    /// </summary>
    public DateTime ExecutionTimestamp { get; set; }

    // ==================== EXECUTION CONTEXT ====================

    /// <summary>
    /// NodeId of situation this choice belongs to
    /// </summary>
    public string ParentSituationNodeId { get; set; }

    /// <summary>
    /// Choice path type (InstantSuccess, Challenge, Fallback, etc.)
    /// </summary>
    public ChoicePathType PathType { get; set; }

    /// <summary>
    /// Action type (Instant, StartChallenge, Navigate)
    /// </summary>
    public ChoiceActionType ActionType { get; set; }

    // ==================== REQUIREMENTS & COSTS ====================

    /// <summary>
    /// Requirements snapshot at execution time
    /// null if no requirements
    /// </summary>
    public RequirementSnapshot RequirementSnapshot { get; set; }

    /// <summary>
    /// Cost snapshot - what player actually paid
    /// </summary>
    public CostSnapshot CostSnapshot { get; set; }

    /// <summary>
    /// Whether player met requirements (true) or took fallback/free choice (false)
    /// </summary>
    public bool PlayerMetRequirements { get; set; }

    // ==================== CHALLENGE DETAILS (if applicable) ====================

    /// <summary>
    /// Challenge ID if choice started challenge
    /// null for instant actions
    /// </summary>
    public string ChallengeId { get; set; }

    /// <summary>
    /// Deck ID used for challenge
    /// null for instant actions
    /// </summary>
    public string DeckId { get; set; }

    /// <summary>
    /// Whether challenge succeeded
    /// null if no challenge (instant action)
    /// </summary>
    public bool? ChallengeSucceeded { get; set; }

    /// <summary>
    /// Human-readable challenge outcome summary
    /// Example: "Drew 3 hearts, needed 2 - SUCCESS"
    /// null if no challenge
    /// </summary>
    public string ChallengeOutcome { get; set; }

    // ==================== REWARDS ====================

    /// <summary>
    /// Base reward snapshot (applied immediately)
    /// </summary>
    public RewardSnapshot RewardSnapshot { get; set; }

    /// <summary>
    /// Success reward snapshot (applied if challenge succeeded)
    /// null if no challenge or challenge failed
    /// </summary>
    public RewardSnapshot OnSuccessRewardSnapshot { get; set; }

    /// <summary>
    /// Failure reward snapshot (applied if challenge failed)
    /// null if no challenge or challenge succeeded
    /// </summary>
    public RewardSnapshot OnFailureRewardSnapshot { get; set; }

    // ==================== SPAWN CONSEQUENCES ====================

    /// <summary>
    /// NodeIds of scenes spawned by this choice
    /// Populated as rewards are applied
    /// </summary>
    public List<string> SpawnedSceneNodeIds { get; set; } = new List<string>();

    /// <summary>
    /// NodeIds of situations spawned by this choice
    /// Populated as rewards are applied
    /// </summary>
    public List<string> SpawnedSituationNodeIds { get; set; } = new List<string>();

    // ==================== NAVIGATION (if applicable) ====================

    /// <summary>
    /// Destination location for navigation choices
    /// null for non-navigation choices
    /// </summary>
    public LocationSnapshot DestinationLocation { get; set; }
}
