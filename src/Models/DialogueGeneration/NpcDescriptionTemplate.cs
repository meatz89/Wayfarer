using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Template for NPC description generation
/// </summary>
public class NpcDescriptionTemplate
{
    [JsonPropertyName("professionBase")]
    public Dictionary<string, List<string>> ProfessionBase { get; set; }

    [JsonPropertyName("emotionalModifiers")]
    public Dictionary<string, Dictionary<string, List<string>>> EmotionalModifiers { get; set; }
}