/// <summary>
/// Context for Physical challenges containing session and metadata.
/// Parallel to SocialChallengeContextBase for architectural consistency.
/// </summary>
public class PhysicalChallengeContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public string DeckId { get; set; }
    public string ObligationId { get; set; }
    public int PhaseIndex { get; set; }
    public PhysicalSession Session { get; set; }
    public Venue Venue { get; set; }
    public ResourceState PlayerResources { get; set; }
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }

    /// <summary>
    /// Reward to apply if physical challenge succeeds
    /// Set when challenge started from Choice with ActionType = StartChallenge
    /// Applied in PhysicalFacade.EndSession() when outcome.Success == true
    /// Tutorial Thomas path uses this to grant coins and bond after physical challenge
    /// </summary>
    public ChoiceReward CompletionReward { get; set; }

    public PhysicalChallengeContext()
    {
        IsValid = true;
        ErrorMessage = string.Empty;
    }
}
