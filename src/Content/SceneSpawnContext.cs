namespace Wayfarer.Content;

/// <summary>
/// Context object containing all data needed to create a provisional Scene from a SceneTemplate
/// Provides concrete entity data for placement selection
/// Used by SceneInstantiator.CreateProvisionalScene() and FinalizeScene()
/// SIMPLIFIED: Only contains entity references, NOT placement logic (that's in SceneSpawnReward)
/// </summary>
public class SceneSpawnContext
{
    /// <summary>
    /// Current Situation that triggered the Scene spawn
    /// Used to extract placement context (location/NPC/route)
    /// </summary>
    public Situation CurrentSituation { get; set; }

    /// <summary>
    /// Venue (location) where current Situation exists (nullable)
    /// Used for SameLocation placement and location-based filters
    /// </summary>
    public Venue CurrentLocation { get; set; }

    /// <summary>
    /// NPC involved in current Situation (nullable)
    /// Used for SameNPC placement and NPC-based filters
    /// </summary>
    public NPC CurrentNPC { get; set; }

    /// <summary>
    /// RouteOption where current Situation exists (nullable)
    /// Used for SameRoute placement and route-based filters
    /// </summary>
    public RouteOption CurrentRoute { get; set; }

    /// <summary>
    /// Player state for requirement validation and placeholder data
    /// Provides bond levels, scale positions, achievements, states
    /// </summary>
    public Player Player { get; set; }
}
