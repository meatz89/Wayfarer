/// <summary>
/// DTO for AI batch choice label generation response.
/// Expected JSON format: { "choices": ["label1", "label2", ...] }
///
/// ARCHITECTURE: Part of JSON → DTO → Parser pipeline for AI responses.
/// Used by SceneNarrativeService to parse batch choice label JSON.
/// </summary>
public class BatchChoiceLabelsDTO
{
    public List<string> Choices { get; set; } = new List<string>();
}
