using System.Collections.Generic;

/// <summary>
/// Result returned when an investigation is discovered
/// Contains data for displaying the investigation discovery modal
/// </summary>
public class InvestigationDiscoveryResult
{
    public string InvestigationId { get; set; }
    public string InvestigationName { get; set; }
    public string IntroNarrative { get; set; }
    public string IntroActionText { get; set; }
    public string ColorCode { get; set; }
}
