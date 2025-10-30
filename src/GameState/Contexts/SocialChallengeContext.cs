/// <summary>
/// Context for Social challenges containing session and metadata.
/// Parallel to MentalChallengeContext and PhysicalChallengeContext for architectural consistency.
/// Domain logic now lives in situation cards, not context subclasses.
/// </summary>
public class SocialChallengeContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public string NpcId { get; set; }
    public NPC Npc { get; set; }
    public string RequestId { get; set; }
    public string ConversationTypeId { get; set; }
    public ConnectionState InitialState { get; set; }
    public SocialSession Session { get; set; }
    public List<CardInstance> ObservationCards { get; set; }
    public ResourceState PlayerResources { get; set; }
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }
    public string RequestText { get; set; }  // Text displayed when NPC presents a request

    /// <summary>
    /// Reward to apply if conversation succeeds
    /// Set when challenge started from Choice with ActionType = StartChallenge
    /// Applied in SocialFacade.EndConversation() when outcome.Success == true
    /// Tutorial Elena path uses this to grant coins and bond after social challenge
    /// </summary>
    public ChoiceReward CompletionReward { get; set; }

    public SocialChallengeContext()
    {
        ObservationCards = new List<CardInstance>();
        IsValid = true;
        ErrorMessage = string.Empty;
    }
}