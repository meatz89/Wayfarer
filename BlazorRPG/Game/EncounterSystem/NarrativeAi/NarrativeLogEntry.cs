using System.Text.Json.Serialization;
/// <summary>
/// Represents a complete log entry for a narrative API interaction
/// </summary>
public class NarrativeLogEntry
{
    [JsonPropertyName("conversation_id")]
    public string ConversationId { get; set; }

    [JsonPropertyName("conversation_history")]
    public List<ConversationEntry> ConversationHistory { get; set; }

    [JsonPropertyName("request")]
    public object Request { get; set; }

    [JsonPropertyName("raw_response")]
    public string RawResponse { get; set; }

    [JsonPropertyName("generated_content")]
    public string GeneratedContent { get; set; }

    [JsonPropertyName("error")]
    public string ErrorMessage { get; set; }
}