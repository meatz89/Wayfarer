using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Template for dialogue based on connection state
/// </summary>
public class ConnectionStateTemplate
{
    [JsonPropertyName("personality")]
    public Dictionary<string, List<string>> Personality { get; set; }

    [JsonPropertyName("default")]
    public List<string> Default { get; set; }
}