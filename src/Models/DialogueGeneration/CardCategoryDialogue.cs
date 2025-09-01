using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Dialogue options for a card category
/// </summary>
public class CardCategoryDialogue
{
    [JsonPropertyName("player")]
    public List<string> Player { get; set; }

    [JsonPropertyName("npc")]
    public List<string> Npc { get; set; }
}