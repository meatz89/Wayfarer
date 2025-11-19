/// <summary>
/// Context for Social challenges containing session and metadata.
/// Parallel to MentalChallengeContext and PhysicalChallengeContext for architectural consistency.
/// Domain logic now lives in situation cards, not context subclasses.
/// </summary>
public class SocialChallengeContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public NPC Npc { get; set; }
    public ConnectionState InitialState { get; set; }
    public SocialSession Session { get; set; }
    public List<CardInstance> ObservationCards { get; set; }
    public ResourceState PlayerResources { get; set; }
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }
    public string RequestText { get; set; }  // Text displayed when NPC presents a request

    /// <summary>
    /// Situation for scene progression tracking (object reference, NO ID)
    /// Set when challenge started from scene choice
    /// Used for LastChallengeSucceeded tracking and scene advancement
    /// </summary>
    public Situation Situation { get; set; }

    /// <summary>
    /// Reward to apply if conversation succeeds
    /// Set when challenge started from Choice with ActionType = StartChallenge
    /// Applied in SocialFacade.EndConversation() when outcome.Success == true
    /// Tutorial Elena path uses this to grant coins and bond after social challenge
    /// </summary>
    public ChoiceReward CompletionReward { get; set; }

    /// <summary>
    /// Reward to apply if conversation fails
    /// Set when challenge started from Choice with ActionType = StartChallenge
    /// Applied when player abandons or fails conversation
    /// Enables OnFailure transitions in scene state machine
    /// </summary>
    public ChoiceReward FailureReward { get; set; }

    public SocialChallengeContext()
    {
        ObservationCards = new List<CardInstance>();
        IsValid = true;
        ErrorMessage = string.Empty;
    }
}