/// <summary>
/// Context for Physical challenges containing session and metadata.
/// Parallel to SocialChallengeContextBase for architectural consistency.
/// </summary>
public class PhysicalChallengeContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    // ADR-007: DeckId DELETED - unused dead code (never read)
    public Obligation Obligation { get; set; }
    public int PhaseIndex { get; set; }
    public PhysicalSession Session { get; set; }
    public Venue Venue { get; set; }
    public ResourceState PlayerResources { get; set; }
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }

    /// <summary>
    /// Situation for scene progression tracking (object reference, NO ID)
    /// Set when challenge started from scene choice
    /// Used for LastChallengeSucceeded tracking and scene advancement
    /// </summary>
    public Situation Situation { get; set; }

    /// <summary>
    /// Reward to apply if physical challenge succeeds
    /// Set when challenge started from Choice with ActionType = StartChallenge
    /// Applied in PhysicalFacade.EndSession() when outcome.Success == true
    /// Tutorial Thomas path uses this to grant coins and bond after physical challenge
    /// </summary>
    public ChoiceReward CompletionReward { get; set; }

    /// <summary>
    /// Reward to apply if physical challenge fails
    /// Set when challenge started from Choice with ActionType = StartChallenge
    /// Applied when player escapes or fails challenge
    /// Enables OnFailure transitions in scene state machine
    /// </summary>
    public ChoiceReward FailureReward { get; set; }

    /// <summary>
    /// PROCEDURAL CONTENT TRACING: NodeId of choice execution trace node
    /// Set when challenge started from scene choice
    /// Used to link spawned scenes to the choice that triggered the challenge
    /// Enables choice→challenge→scene tracing relationship
    /// </summary>
    public string ChoiceExecutionNodeId { get; set; }

    public PhysicalChallengeContext()
    {
        IsValid = true;
        ErrorMessage = string.Empty;
    }
}
