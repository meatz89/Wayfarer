using System.Collections.Generic;

/// <summary>
/// Result returned when player clicks investigation intro button
/// Contains data for displaying the RPG quest acceptance modal
/// Modal shows narrative and "Begin Investigation" button
/// </summary>
public class InvestigationIntroResult
{
    public string InvestigationId { get; set; }
    public string InvestigationName { get; set; }
    public string IntroNarrative { get; set; }
    public string IntroActionText { get; set; }
    public string ColorCode { get; set; }
    public string LocationName { get; set; }
    public string SpotName { get; set; }
}
