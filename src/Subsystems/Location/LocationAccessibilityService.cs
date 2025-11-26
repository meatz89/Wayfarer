/// <summary>
/// Domain service for query-based location accessibility
///
/// DUAL-MODEL ACCESSIBILITY (ADR-012):
/// - AUTHORED locations (Origin == Authored): ALWAYS accessible per TIER 1 design pillar (No Soft-Locks)
/// - SCENE-CREATED locations (Origin == SceneCreated): Accessible only when active scene's current situation is at this location
///
/// DESIGN RATIONALE (gdd/01_vision.md Section 1.5 - Requirement Inversion):
/// "Stat requirements affect COST, not ACCESS. Content exists from game start; requirements filter optimal paths, not availability."
///
/// Authored locations exist from game start and must always be reachable for forward progress.
/// Scene-created locations (created dynamically by scenes) require scene progression to unlock (e.g., private room unlocked after negotiation).
///
/// CLEAN ARCHITECTURE: Uses explicit LocationOrigin enum instead of null-as-domain-meaning pattern.
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
    /// DUAL-MODEL (ADR-012):
    /// - Authored locations (Origin == Authored): Always accessible (No Soft-Locks principle)
    /// - Scene-created locations (Origin == SceneCreated): Accessible when active scene's current situation is at this location
    ///
    /// CLEAN ARCHITECTURE: Uses explicit LocationOrigin enum instead of null checks.
    /// Pure query, no state modification.
    /// </summary>
    /// <param name="location">Location to check accessibility</param>
    /// <returns>True if accessible, false otherwise</returns>
    public bool IsLocationAccessible(Location location)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        // AUTHORED LOCATIONS: Always accessible per TIER 1 design pillar (No Soft-Locks)
        // These are defined in base game content JSON (inns, markets, checkpoints, etc.)
        if (location.Origin == LocationOrigin.Authored)
            return true;

        // SCENE-CREATED LOCATIONS: Require scene-based accessibility grant
        // These are created dynamically by scenes during gameplay (private rooms, meeting chambers)
        // Player must progress through scene (e.g., negotiate lodging) to unlock access
        return CheckSceneGrantsAccess(location);
    }

    /// <summary>
    /// Check if any active scene's current situation is at this scene-created location.
    ///
    /// SIMPLIFIED LOGIC: If a situation exists at a scene-created location, the player
    /// MUST be able to access it to engage with the situation. No property needed -
    /// situation presence implies access (otherwise it would be a soft-lock).
    /// </summary>
    private bool CheckSceneGrantsAccess(Location location)
    {
        return _gameWorld.Scenes
            .Where(scene => scene.State == SceneState.Active)
            .Where(scene => scene.CurrentSituationIndex >= 0 && scene.CurrentSituationIndex < scene.Situations.Count)
            .Any(scene => scene.CurrentSituation?.Location == location);
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
