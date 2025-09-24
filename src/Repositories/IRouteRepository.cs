using System.Collections.Generic;

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
    /// </summary>
    bool IsRouteBlocked(string routeId);


    /// <summary>
    /// Get available routes considering current conditions
    /// </summary>
    IEnumerable<RouteOption> GetAvailableRoutes(string fromLocationId, Player player);

}