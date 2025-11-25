/// <summary>
/// Domain service for query-based location accessibility
///
/// DUAL-MODEL ACCESSIBILITY:
/// - AUTHORED locations (Provenance == null): ALWAYS accessible per TIER 1 design pillar (No Soft-Locks)
/// - DEPENDENT locations (Provenance != null): Accessible only when active scene's current situation grants access
///
/// DESIGN RATIONALE (gdd/01_vision.md Section 1.5 - Requirement Inversion):
/// "Stat requirements affect COST, not ACCESS. Content exists from game start; requirements filter optimal paths, not availability."
///
/// Authored locations exist from game start and must always be reachable for forward progress.
/// Dependent locations (created by scenes) require scene progression to unlock (e.g., private room unlocked after negotiation).
///
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
    /// Check if location is accessible to player
    ///
    /// DUAL-MODEL:
    /// - Authored locations (Provenance == null): Always accessible (No Soft-Locks principle)
    /// - Dependent locations (Provenance != null): Accessible when active scene grants access
    ///
    /// Pure query, no state modification.
    /// </summary>
    /// <param name="location">Location to check accessibility</param>
    /// <returns>True if accessible, false otherwise</returns>
    public bool IsLocationAccessible(Location location)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        // AUTHORED LOCATIONS: Always accessible per TIER 1 design pillar (No Soft-Locks)
        // Provenance == null means location was defined in base game content (not created by scene)
        if (location.Provenance == null)
            return true;

        // DEPENDENT LOCATIONS: Require scene-based accessibility grant
        // Provenance != null means location was created by a scene during gameplay
        // Player must progress through scene (e.g., negotiate lodging) to unlock access
        return CheckSceneGrantsAccess(location);
    }

    /// <summary>
    /// Check if any active scene's current situation grants access to this dependent location
    /// Used only for dependent locations (scene-created resources)
    /// </summary>
    private bool CheckSceneGrantsAccess(Location location)
    {
        return _gameWorld.Scenes
            .Where(scene => scene.State == SceneState.Active)
            .Where(scene => scene.CurrentSituationIndex >= 0 && scene.CurrentSituationIndex < scene.Situations.Count)
            .Any(scene =>
            {
                Situation currentSituation = scene.CurrentSituation;

                if (currentSituation == null)
                    return false;

                // Check if situation grants access (default is true)
                if (currentSituation.Template?.GrantsLocationAccess != true)
                    return false;

                // Check if situation is at target location
                if (currentSituation.Location == location)
                    return true;

                return false;
            });
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
