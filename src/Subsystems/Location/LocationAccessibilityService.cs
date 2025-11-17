/// <summary>
/// Domain service for query-based location accessibility
/// NEW ARCHITECTURE: Replaces flag-based Location.IsLocked pattern
/// Query pattern: Location accessible if active situation grants access
/// STATELESS: Pure query service operating on GameWorld
/// </summary>
public class LocationAccessibilityService
{
    private readonly GameWorld _gameWorld;

    public LocationAccessibilityService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Check if location is accessible to player via query-based accessibility
    /// NEW ARCHITECTURE: Pure query, no state modification
    /// Returns true if any active scene has current situation granting access to this location
    /// </summary>
    /// <param name="location">Location to check accessibility</param>
    /// <returns>True if accessible (active situation grants access), false otherwise</returns>
    public bool IsLocationAccessible(Location location)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        // Query all active scenes for situations granting access to target location
        bool accessGranted = _gameWorld.Scenes
            .Where(scene => scene.State == SceneState.Active)
            .Where(scene => scene.CurrentSituationIndex >= 0 && scene.CurrentSituationIndex < scene.Situations.Count)
            .Any(scene =>
            {
                // Get current situation via computed property
                Situation currentSituation = scene.CurrentSituation;

                if (currentSituation == null)
                    return false;

                // Check if situation grants access
                if (currentSituation.Template?.GrantsLocationAccess != true)
                    return false;

                // Check if situation is at target location
                if (currentSituation.Location == location)
                    return true;

                return false;
            });

        return accessGranted;
    }

    /// <summary>
    /// Get all locations accessible to player (query-based)
    /// Filters location list by accessibility
    /// Used by: LocationFacade for accessible location queries
    /// </summary>
    /// <param name="locations">Candidate locations to filter</param>
    /// <returns>List of accessible locations</returns>
    public List<Location> FilterAccessibleLocations(List<Location> locations)
    {
        if (locations == null)
            return new List<Location>();

        return locations
            .Where(loc => IsLocationAccessible(loc))
            .ToList();
    }
}
