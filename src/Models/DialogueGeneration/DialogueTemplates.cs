using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Container for all dialogue templates loaded from JSON
/// </summary>
public class DialogueTemplates
{
    [JsonPropertyName("emotionalStateDialogue")]
    public Dictionary<string, EmotionalStateTemplate> EmotionalStateDialogue { get; set; }

    [JsonPropertyName("cardDialogue")]
    public CardDialogueTemplate CardDialogue { get; set; }

    [JsonPropertyName("npcDescriptions")]
    public NpcDescriptionTemplate NpcDescriptions { get; set; }

    [JsonPropertyName("narrativeElements")]
    public Dictionary<string, Dictionary<string, string>> NarrativeElements { get; set; }
}