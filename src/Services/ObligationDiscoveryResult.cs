/// <summary>
/// Result returned when an obligation is discovered
/// Contains data for displaying the obligation discovery modal
/// </summary>
public class ObligationDiscoveryResult
{
    public string ObligationId { get; set; }
    public string ObligationName { get; set; }
    public string IntroNarrative { get; set; }
    public string IntroActionText { get; set; }
    public string ColorCode { get; set; }
    public string LocationName { get; set; }
    public string SpotName { get; set; }
}
