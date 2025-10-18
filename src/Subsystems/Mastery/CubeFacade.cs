using System;

/// <summary>
/// Facade for localized mastery cube management
/// Handles InvestigationCubes (Locations), StoryCubes (NPCs), ExplorationCubes (Routes)
/// </summary>
public class CubeFacade
{
    private readonly GameWorld _gameWorld;

    public CubeFacade(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    // ============================================
    // INVESTIGATION CUBES (Location Mastery)
    // ============================================

    /// <summary>
    /// Get InvestigationCubes for a location (0-10 scale)
    /// </summary>
    public int GetLocationCubes(string locationId)
    {
        return _gameWorld.GetLocationCubes(locationId);
    }

    /// <summary>
    /// Grant InvestigationCubes to a location (max 10)
    /// Reduces Mental Exposure at this location only
    /// </summary>
    public void GrantLocationCubes(string locationId, int amount)
    {
        int before = _gameWorld.GetLocationCubes(locationId);
        _gameWorld.GrantLocationCubes(locationId, amount);
        int after = _gameWorld.GetLocationCubes(locationId);

        int actualGranted = after - before;
        if (actualGranted > 0)
        { }
    }

    /// <summary>
    /// Calculate Mental Exposure reduction based on InvestigationCubes
    /// Each cube reduces exposure by 1 point
    /// </summary>
    public int GetLocationExposureReduction(string locationId)
    {
        return _gameWorld.GetLocationCubes(locationId);
    }

    /// <summary>
    /// Calculate effective exposure reduction at location
    /// Formula: EffectiveExposure = BaseExposure * (10 - InvestigationCubes) / 10
    /// Example: 6 base exposure, 3 cubes = 6 * 7 / 10 = 4.2 -> 4
    /// Example: 6 base exposure, 10 cubes = 6 * 0 / 10 = 0 (mastery)
    /// </summary>
    public int GetEffectiveExposure(string locationId, int baseExposure)
    {
        int cubes = _gameWorld.GetLocationCubes(locationId);
        int effectiveExposure = baseExposure * (10 - cubes) / 10;
        return effectiveExposure;
    }

    // ============================================
    // STORY CUBES (NPC Mastery)
    // ============================================

    /// <summary>
    /// Get StoryCubes for an NPC (0-10 scale)
    /// </summary>
    public int GetNPCCubes(string npcId)
    {
        return _gameWorld.GetNPCCubes(npcId);
    }

    /// <summary>
    /// Grant StoryCubes to an NPC (max 10)
    /// Reduces Social Doubt with this NPC only
    /// </summary>
    public void GrantNPCCubes(string npcId, int amount)
    {
        int before = _gameWorld.GetNPCCubes(npcId);
        _gameWorld.GrantNPCCubes(npcId, amount);
        int after = _gameWorld.GetNPCCubes(npcId);

        int actualGranted = after - before;
        if (actualGranted > 0)
        { }
    }

    /// <summary>
    /// Calculate Social Doubt reduction based on StoryCubes
    /// Each cube reduces doubt by 1 point
    /// </summary>
    public int GetNPCDoubtReduction(string npcId)
    {
        return _gameWorld.GetNPCCubes(npcId);
    }

    /// <summary>
    /// Calculate effective doubt reduction with NPC
    /// Formula: EffectiveDoubt = BaseDoubt * (10 - StoryCubes) / 10
    /// Example: 8 base doubt, 4 cubes = 8 * 6 / 10 = 4.8 -> 4
    /// Example: 8 base doubt, 10 cubes = 8 * 0 / 10 = 0 (complete understanding)
    /// </summary>
    public int GetEffectiveDoubt(string npcId, int baseDoubt)
    {
        int cubes = _gameWorld.GetNPCCubes(npcId);
        int effectiveDoubt = baseDoubt * (10 - cubes) / 10;
        return effectiveDoubt;
    }

    // ============================================
    // EXPLORATION CUBES (Route Mastery)
    // ============================================

    /// <summary>
    /// Get ExplorationCubes for a route (0-10 scale)
    /// </summary>
    public int GetRouteCubes(string routeId)
    {
        return _gameWorld.GetRouteCubes(routeId);
    }

    /// <summary>
    /// Grant ExplorationCubes to a route (max 10)
    /// Reveals hidden path options at thresholds
    /// </summary>
    public void GrantRouteCubes(string routeId, int amount)
    {
        int before = _gameWorld.GetRouteCubes(routeId);
        _gameWorld.GrantRouteCubes(routeId, amount);
        int after = _gameWorld.GetRouteCubes(routeId);

        int actualGranted = after - before;
        if (actualGranted > 0)
        { }
    }

    /// <summary>
    /// Check if a path is visible based on ExplorationCubes threshold
    /// </summary>
    public bool IsPathVisible(string routeId, int requiredCubes)
    {
        int currentCubes = _gameWorld.GetRouteCubes(routeId);
        return currentCubes >= requiredCubes;
    }

    /// <summary>
    /// Check if route is mastered (10 cubes - all paths revealed)
    /// </summary>
    public bool IsRouteMastered(string routeId)
    {
        return _gameWorld.GetRouteCubes(routeId) >= 10;
    }
}
