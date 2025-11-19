/// <summary>
/// Repository interface for Route-related operations
/// HIGHLANDER: All methods accept typed objects, not string IDs
/// </summary>
public interface IRouteRepository
{
    /// <summary>
    /// Get routes from a specific location
    /// HIGHLANDER: Accept Location object, compare objects
    /// </summary>
    IEnumerable<RouteOption> GetRoutesFromLocation(Location location);

    /// <summary>
    /// Check if a route is blocked
    /// HIGHLANDER: Accepts RouteOption object, not string ID
    /// </summary>
    bool IsRouteBlocked(RouteOption route);

    /// <summary>
    /// Get available routes considering current conditions
    /// HIGHLANDER: Accept Location object
    /// </summary>
    IEnumerable<RouteOption> GetAvailableRoutes(Location fromLocation, Player player);

}
