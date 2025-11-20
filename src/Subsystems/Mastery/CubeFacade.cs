/// <summary>
/// Facade for localized mastery cube management
/// Handles all four cube systems (0-10 scale each):
/// - InvestigationCubes (Locations): Reduce Mental Exposure
/// - StoryCubes (NPCs): Reduce Social Doubt
/// - ExplorationCubes (Routes): Reveal hidden paths
/// - MasteryCubes (Physical Decks): Reduce Physical Danger
/// </summary>
public class CubeFacade
{
    private readonly GameWorld _gameWorld;

    public CubeFacade(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    // ============================================
    // OBLIGATION CUBES (Location Mastery)
    // ============================================

    /// <summary>
    /// Get InvestigationCubes for a location (0-10 scale)
    /// HIGHLANDER: Accept Location object, not string ID
    /// </summary>
    public int GetLocationCubes(Location location)
    {
        return _gameWorld.GetLocationCubes(location);
    }

    /// <summary>
    /// Grant InvestigationCubes to a location (max 10)
    /// Reduces Mental Exposure at this location only
    /// HIGHLANDER: Accept Location object, not string ID
    /// </summary>
    public void GrantLocationCubes(Location location, int amount)
    {
        int before = _gameWorld.GetLocationCubes(location);
        _gameWorld.GrantLocationCubes(location, amount);
        int after = _gameWorld.GetLocationCubes(location);

        int actualGranted = after - before;
        if (actualGranted > 0)
        { }
    }

    /// <summary>
    /// Calculate Mental Exposure reduction based on InvestigationCubes
    /// Each cube reduces exposure by 1 point
    /// HIGHLANDER: Accept Location object, not string ID
    /// </summary>
    public int GetLocationExposureReduction(Location location)
    {
        return _gameWorld.GetLocationCubes(location);
    }

    // ============================================
    // STORY CUBES (NPC Mastery)
    // ============================================

    /// <summary>
    /// Get StoryCubes for an NPC (0-10 scale)
    /// HIGHLANDER: Accept NPC object, not string ID
    /// </summary>
    public int GetNPCCubes(NPC npc)
    {
        return _gameWorld.GetNPCCubes(npc);
    }

    /// <summary>
    /// Grant StoryCubes to an NPC (max 10)
    /// Reduces Social Doubt with this NPC only
    /// HIGHLANDER: Accept NPC object, not string ID
    /// </summary>
    public void GrantNPCCubes(NPC npc, int amount)
    {
        int before = _gameWorld.GetNPCCubes(npc);
        _gameWorld.GrantNPCCubes(npc, amount);
        int after = _gameWorld.GetNPCCubes(npc);

        int actualGranted = after - before;
        if (actualGranted > 0)
        { }
    }

    /// <summary>
    /// Calculate Social Doubt reduction based on StoryCubes
    /// Each cube reduces doubt by 1 point
    /// HIGHLANDER: Accept NPC object, not string ID
    /// </summary>
    public int GetNPCDoubtReduction(NPC npc)
    {
        return _gameWorld.GetNPCCubes(npc);
    }

    // ============================================
    // EXPLORATION CUBES (Route Mastery)
    // ============================================

    /// <summary>
    /// Get ExplorationCubes for a route (0-10 scale)
    /// HIGHLANDER: Accept RouteOption object, not string ID
    /// </summary>
    public int GetRouteCubes(RouteOption route)
    {
        return _gameWorld.GetRouteCubes(route);
    }

    /// <summary>
    /// Grant ExplorationCubes to a route (max 10)
    /// Reveals hidden path options at thresholds
    /// HIGHLANDER: Accept RouteOption object, not string ID
    /// </summary>
    public void GrantRouteCubes(RouteOption route, int amount)
    {
        int before = _gameWorld.GetRouteCubes(route);
        _gameWorld.GrantRouteCubes(route, amount);
        int after = _gameWorld.GetRouteCubes(route);

        int actualGranted = after - before;
        if (actualGranted > 0)
        { }
    }

    /// <summary>
    /// Check if a path is visible based on ExplorationCubes threshold
    /// HIGHLANDER: Accept RouteOption object, not string ID
    /// </summary>
    public bool IsPathVisible(RouteOption route, int requiredCubes)
    {
        int currentCubes = _gameWorld.GetRouteCubes(route);
        return currentCubes >= requiredCubes;
    }

    /// <summary>
    /// Check if route is mastered (10 cubes - all paths revealed)
    /// HIGHLANDER: Accept RouteOption object, not string ID
    /// </summary>
    public bool IsRouteMastered(RouteOption route)
    {
        return _gameWorld.GetRouteCubes(route) >= 10;
    }

}
