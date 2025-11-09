/// <summary>
/// Type of TravelScene encounter on a route.
/// Categorizes the nature of the obstacle for display and mechanical purposes.
/// </summary>
public enum SceneType
{
    /// <summary>
    /// Natural hazard (weather, terrain, environmental obstacle)
    /// </summary>
    Environmental,

    /// <summary>
    /// Hostile encounter (bandits, wild animals, combat)
    /// </summary>
    Hostile,

    /// <summary>
    /// Social interaction (traveler, guard post, merchant caravan)
    /// </summary>
    Social,

    /// <summary>
    /// Mystery or puzzle (strange occurrence, investigation needed)
    /// </summary>
    Mystery
}
