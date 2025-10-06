/// <summary>
/// Result returned when an investigation intro action is completed
/// Contains data for displaying the investigation activation modal
/// </summary>
public class InvestigationActivationResult
{
    public string InvestigationId { get; set; }
    public string InvestigationName { get; set; }
    public string IntroNarrative { get; set; }
}
