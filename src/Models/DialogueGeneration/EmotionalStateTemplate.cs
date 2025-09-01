using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Template for dialogue based on emotional state
/// </summary>
public class EmotionalStateTemplate
{
    [JsonPropertyName("contextual")]
    public Dictionary<string, object> Contextual { get; set; }

    [JsonPropertyName("personality")]
    public Dictionary<string, List<string>> Personality { get; set; }

    [JsonPropertyName("default")]
    public List<string> Default { get; set; }
}