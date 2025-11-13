
/// <summary>
/// DTO for ContextBinding - binds current context into spawned scene
/// Maps to ContextBinding domain entity
/// JSON property names MUST match C# properties (no JsonPropertyName attribute)
/// </summary>
public class ContextBindingDTO
{
    /// <summary>
    /// Marker key used in templates - e.g., "QUESTGIVER", "RETURN_LOCATION"
    /// Templates use placeholders like {QUESTGIVER_NAME} which resolve via this marker
    /// </summary>
    public string MarkerKey { get; set; }

    /// <summary>
    /// Source of context - which entity type to bind
    /// Values: "CurrentNpc", "CurrentLocation", "CurrentRoute", "PreviousScene"
    /// Maps to ContextSource enum in domain
    /// </summary>
    public string Source { get; set; }
}
