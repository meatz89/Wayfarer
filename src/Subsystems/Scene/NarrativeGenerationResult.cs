/// <summary>
/// Result of a single narrative generation attempt.
/// Includes metadata about how the narrative was generated.
///
/// Used by SceneNarrativeService.GenerateSituationNarrativeWithMetadataAsync()
/// to return both the narrative AND generation metadata (fallback used, reason, etc.)
/// </summary>
public class NarrativeGenerationResult
{
    public string Prompt { get; set; } = "";
    public string Narrative { get; set; } = "";
    public bool UsedFallback { get; set; }
    public string FallbackReason { get; set; } = "";
}
