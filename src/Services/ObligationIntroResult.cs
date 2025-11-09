/// <summary>
/// Result returned when player clicks obligation intro button
/// Contains data for displaying the RPG quest acceptance modal
/// Modal shows narrative and "Begin Obligation" button
/// </summary>
public class ObligationIntroResult
{
public string ObligationId { get; set; }
public string ObligationName { get; set; }
public string IntroNarrative { get; set; }
public string IntroActionText { get; set; }
public string ColorCode { get; set; }
public string LocationName { get; set; }
public string SpotName { get; set; }
}
