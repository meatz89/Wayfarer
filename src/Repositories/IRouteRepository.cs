/// <summary>
/// Repository interface for Route-related operations
/// </summary>
public interface IRouteRepository
{
    /// <summary>
    /// Get routes from a specific location
    /// </summary>
    IEnumerable<RouteOption> GetRoutesFromLocation(string locationId);

    /// <summary>
    /// Check if a route is blocked
    /// HIGHLANDER: Accepts RouteOption object, not string ID
    /// </summary>
    bool IsRouteBlocked(RouteOption route);

    /// <summary>
    /// Get available routes considering current conditions
    /// </summary>
    IEnumerable<RouteOption> GetAvailableRoutes(string fromLocationId, Player player);

}