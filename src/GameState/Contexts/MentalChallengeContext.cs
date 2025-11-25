/// <summary>
/// Context for Mental challenges containing session and metadata.
/// Parallel to SocialChallengeContextBase for architectural consistency.
/// </summary>
public class MentalChallengeContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    // ADR-007: DeckId DELETED - unused dead code (never read)
    // ADR-007: ObligationId DELETED - unused dead code (never read)
    public int PhaseIndex { get; set; }
    public MentalSession Session { get; set; }
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
    /// Reward to apply if mental challenge succeeds
    /// Set when challenge started from Choice with ActionType = StartChallenge
    /// Applied in MentalFacade.EndSession() when outcome.Success == true
    /// </summary>
    public ChoiceReward CompletionReward { get; set; }

    /// <summary>
    /// Reward to apply if mental challenge fails
    /// Set when challenge started from Choice with ActionType = StartChallenge
    /// Applied when player escapes or fails challenge
    /// Enables OnFailure transitions in scene state machine
    /// </summary>
    public ChoiceReward FailureReward { get; set; }

    /// <summary>
    /// PROCEDURAL CONTENT TRACING: Choice execution trace node
    /// Set when challenge started from scene choice
    /// Used to link spawned scenes to the choice that triggered the challenge
    /// Enables choice→challenge→scene tracing relationship
    /// HIGHLANDER: Direct object reference, no NodeId string
    /// </summary>
    public ChoiceExecutionNode ChoiceExecution { get; set; }

    public MentalChallengeContext()
    {
        IsValid = true;
        ErrorMessage = string.Empty;
    }
}
