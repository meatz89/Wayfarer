/// <summary>
/// Trace node capturing complete choice execution event
/// Records what player chose, what they paid, what they received, and what spawned as consequence
/// </summary>
public class ChoiceExecutionNode
{
    // ==================== CORE IDENTITY ====================

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
    /// Situation this choice belongs to
    /// HIGHLANDER: Direct object reference, no ID strings
    /// </summary>
    public SituationSpawnNode ParentSituation { get; set; }

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
    /// Scenes spawned by this choice
    /// Populated as rewards are applied
    /// HIGHLANDER: Direct object references, no ID strings
    /// </summary>
    public List<SceneSpawnNode> SpawnedScenes { get; set; } = new List<SceneSpawnNode>();

    /// <summary>
    /// Situations spawned by this choice
    /// Populated as rewards are applied
    /// HIGHLANDER: Direct object references, no ID strings
    /// </summary>
    public List<SituationSpawnNode> SpawnedSituations { get; set; } = new List<SituationSpawnNode>();

    // ==================== NAVIGATION (if applicable) ====================

    /// <summary>
    /// Destination location for navigation choices
    /// null for non-navigation choices
    /// </summary>
    public LocationSnapshot DestinationLocation { get; set; }
}
