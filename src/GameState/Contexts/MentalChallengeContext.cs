/// <summary>
/// Context for Mental challenges containing session and metadata.
/// Parallel to SocialChallengeContextBase for architectural consistency.
/// </summary>
public class MentalChallengeContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public string ChallengeTypeId { get; set; }
    public string InvestigationId { get; set; }
    public int PhaseIndex { get; set; }
    public MentalSession Session { get; set; }
    public Location Location { get; set; }
    public ResourceState PlayerResources { get; set; }
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }

    public MentalChallengeContext()
    {
        IsValid = true;
        ErrorMessage = string.Empty;
    }
}
