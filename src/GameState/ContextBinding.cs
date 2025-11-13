
/// <summary>
/// Context binding for scene spawning - binds current context entities into spawned scene
/// Populated at choice display time, merged into MarkerResolutionMap at spawn time
/// Enables narrative continuity without hardcoded entity IDs
/// </summary>
public class ContextBinding
{
    /// <summary>
    /// Marker key used in templates - e.g., "QUESTGIVER", "RETURN_LOCATION"
    /// Templates use placeholders like {QUESTGIVER_NAME} which resolve via this marker
    /// </summary>
    public string MarkerKey { get; set; }

    /// <summary>
    /// Source of context - which current entity type to bind
    /// Determines which Resolved*Id property will be populated
    /// </summary>
    public ContextSource Source { get; set; }

    /// <summary>
    /// Resolved NPC ID - populated when Source == CurrentNpc
    /// </summary>
    public string ResolvedNpcId { get; set; }

    /// <summary>
    /// Resolved Location ID - populated when Source == CurrentLocation
    /// </summary>
    public string ResolvedLocationId { get; set; }

    /// <summary>
    /// Resolved Route ID - populated when Source == CurrentRoute
    /// </summary>
    public string ResolvedRouteId { get; set; }

    /// <summary>
    /// Resolved Scene ID - populated when Source == PreviousScene
    /// </summary>
    public string ResolvedSceneId { get; set; }
}

/// <summary>
/// Source of context binding - which current entity type
/// </summary>
public enum ContextSource
{
    /// <summary>
    /// NPC player is currently talking to (Scene.PlacementType == NPC)
    /// </summary>
    CurrentNpc,

    /// <summary>
    /// Location player is currently at (Player.CurrentPosition → Location)
    /// </summary>
    CurrentLocation,

    /// <summary>
    /// Route player is currently traveling (GameWorld.CurrentRouteOption)
    /// </summary>
    CurrentRoute,

    /// <summary>
    /// Scene that spawned this scene (parent scene ID)
    /// </summary>
    PreviousScene
}
