/// <summary>
/// Manages cleanup of routes when locations are removed.
/// Prevents orphaned routes from referencing deleted locations.
/// </summary>
public class RouteCleanupService
{
    /// <summary>
    /// Remove all routes with this location as origin or destination.
    /// Called when location is being deleted to maintain referential integrity.
    /// </summary>
    public async Task RemoveRoutesForLocation(string locationId, GameWorld gameWorld)
    {
        // Find all routes referencing this location (either direction)
        List<RouteOption> orphanedRoutes = gameWorld.Routes
            .Where(r => r.OriginLocationId == locationId || r.DestinationLocationId == locationId)
            .ToList();

        foreach (RouteOption route in orphanedRoutes)
        {
            gameWorld.RemoveRoute(route.Id);
        }

        // Note: This is synchronous in current implementation
        // Async signature for future extensibility (e.g., persistence layer)
        await Task.CompletedTask;
    }
}
