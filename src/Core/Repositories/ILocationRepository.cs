using System.Collections.Generic;

namespace Wayfarer.Core.Repositories;

/// <summary>
/// Repository interface for Location entities
/// </summary>
public interface ILocationRepository : IRepository<Location>
{
    /// <summary>
    /// Get locations by region
    /// </summary>
    IEnumerable<Location> GetLocationsByRegion(string region);

    /// <summary>
    /// Get locations that have specific route types
    /// </summary>
    IEnumerable<Location> GetLocationsWithRouteType(RouteType routeType);

    /// <summary>
    /// Check if a location exists
    /// </summary>
    bool LocationExists(string locationId);
}