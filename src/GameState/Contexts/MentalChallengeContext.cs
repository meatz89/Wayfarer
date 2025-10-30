/// <summary>
/// Context for Mental challenges containing session and metadata.
/// Parallel to SocialChallengeContextBase for architectural consistency.
/// </summary>
public class MentalChallengeContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public string DeckId { get; set; }
    public string ObligationId { get; set; }
    public int PhaseIndex { get; set; }
    public MentalSession Session { get; set; }
    public Venue Venue { get; set; }
    public ResourceState PlayerResources { get; set; }
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }

    /// <summary>
    /// Reward to apply if mental challenge succeeds
    /// Set when challenge started from Choice with ActionType = StartChallenge
    /// Applied in MentalFacade.EndSession() when outcome.Success == true
    /// </summary>
    public ChoiceReward CompletionReward { get; set; }

    public MentalChallengeContext()
    {
        IsValid = true;
        ErrorMessage = string.Empty;
    }
}
