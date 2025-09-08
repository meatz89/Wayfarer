using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// DTO for the properties object structure in location spots JSON.
/// Maps time periods to property lists.
/// </summary>
public class SpotPropertiesDTO
{
    [JsonPropertyName("base")]
    public List<string> Base { get; set; } = new List<string>();
    
    [JsonPropertyName("morning")]
    public List<string> Morning { get; set; } = new List<string>();
    
    [JsonPropertyName("afternoon")]
    public List<string> Afternoon { get; set; } = new List<string>();
    
    [JsonPropertyName("evening")]
    public List<string> Evening { get; set; } = new List<string>();
    
    [JsonPropertyName("night")]
    public List<string> Night { get; set; } = new List<string>();
    
    [JsonPropertyName("latenight")]
    public List<string> LateNight { get; set; } = new List<string>();
    
    [JsonPropertyName("dawn")]
    public List<string> Dawn { get; set; } = new List<string>();
}