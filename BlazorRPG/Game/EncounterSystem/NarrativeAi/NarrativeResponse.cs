
/// <summary>
/// Model class for narrative responses
/// </summary>
public class NarrativeResponse
{
    public string ActionOutcome { get; set; } = string.Empty;
    public string NewSituation { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = new List<string>();
    public string Atmosphere { get; set; } = string.Empty;
}