using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Template for card-based dialogue
/// </summary>
public class CardDialogueTemplate
{
    [JsonPropertyName("categories")]
    public Dictionary<string, CardCategoryDialogue> Categories { get; set; }
}