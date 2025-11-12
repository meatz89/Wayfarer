/// <summary>
/// Manages cleanup of dependent resources (locations, items) created by scenes.
/// Evaluates significance and removes temporary resources while preserving persistent ones.
/// HIGHLANDER: Single cleanup entry point for all dependent resources.
/// </summary>
public class DependentResourceCleanupService
{
    private readonly LocationSignificanceEvaluator _significanceEvaluator;
    private readonly RouteCleanupService _routeCleanupService;
    private readonly HexSynchronizationService _hexSyncService;

    public DependentResourceCleanupService(
        LocationSignificanceEvaluator significanceEvaluator,
        RouteCleanupService routeCleanupService,
        HexSynchronizationService hexSyncService)
    {
        _significanceEvaluator = significanceEvaluator;
        _routeCleanupService = routeCleanupService;
        _hexSyncService = hexSyncService;
    }

    /// <summary>
    /// Cleanup all dependent resources created by scene.
    /// Called when scene enters Completed or Expired state.
    /// </summary>
    public async Task CleanupSceneResources(string sceneId, GameWorld gameWorld)
    {
        // Find all locations created by this scene
        List<Location> sceneLocations = gameWorld.Locations
            .Where(l => l.Provenance != null && l.Provenance.SceneId == sceneId)
            .ToList();

        foreach (Location location in sceneLocations)
        {
            LocationSignificance significance = _significanceEvaluator.EvaluateSignificance(location, gameWorld);

            switch (significance)
            {
                case LocationSignificance.Temporary:
                    // Remove completely - never visited, no other references
                    await RemoveLocation(location.Id, gameWorld);
                    break;

                case LocationSignificance.Persistent:
                    // Persist but orphan - visited or multi-scene reference
                    // Promote to permanent by clearing provenance
                    location.Provenance = null;
                    break;

                case LocationSignificance.Critical:
                    // Should never happen (authored content doesn't have provenance)
                    // But if it does, don't touch it
                    break;
            }
        }

        // TODO: Cleanup dependent items when item system refactored
        // Similar pattern: evaluate significance, cleanup temporary, orphan persistent
    }

    /// <summary>
    /// Remove location and all dependent resources (routes, hex references).
    /// HIGHLANDER: Single removal entry point maintaining all invariants.
    /// </summary>
    private async Task RemoveLocation(string locationId, GameWorld gameWorld)
    {
        Location location = gameWorld.GetLocation(locationId);
        if (location == null) return;

        // 1. Remove all routes referencing this location
        await _routeCleanupService.RemoveRoutesForLocation(locationId, gameWorld);

        // 2. Clear Hex.LocationId reverse reference (HIGHLANDER sync)
        _hexSyncService.ClearHexLocationReference(locationId, gameWorld);

        // 3. Remove from Venue.LocationIds
        if (location.Venue != null)
        {
            location.Venue.LocationIds.Remove(locationId);
        }

        // 4. Remove from GameWorld.Locations
        gameWorld.RemoveLocation(locationId);
    }
}
