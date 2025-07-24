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
    /// Get current weather condition
    /// </summary>
    WeatherCondition GetCurrentWeather();

    /// <summary>
    /// Get available routes considering current conditions
    /// </summary>
    IEnumerable<RouteOption> GetAvailableRoutes(string fromLocationId, Player player);

    /// <summary>
    /// Check if player has required equipment for a route
    /// </summary>
    bool PlayerHasRequiredEquipment(RouteOption route, Player player);
}