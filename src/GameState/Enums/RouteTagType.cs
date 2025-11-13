/// <summary>
/// Route visibility and access tags.
/// Categorical route properties for UI display and gameplay.
/// Used for route description generation and CSS class mapping.
/// </summary>
public enum RouteTagType
{
    /// <summary>
    /// Public route - openly accessible, well-known
    /// </summary>
    PUBLIC,

    /// <summary>
    /// Discrete route - subtle passage, not obviously visible
    /// </summary>
    DISCRETE,

    /// <summary>
    /// Exposed route - open to weather and observation
    /// </summary>
    EXPOSED,

    /// <summary>
    /// Wilderness route - through untamed terrain
    /// </summary>
    WILDERNESS,

    /// <summary>
    /// Commercial route - trade and merchant traffic
    /// </summary>
    COMMERCIAL,

    /// <summary>
    /// Restricted route - access control, permission required
    /// </summary>
    RESTRICTED
}
