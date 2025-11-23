
/// <summary>
/// State of a Scene in its lifecycle
/// Scenes transition: Deferred → Active → Completed/Expired
/// </summary>
public enum SceneState
{
    /// <summary>
    /// Scene is deferred (created but not yet activated)
    /// Scene entity and Situations exist, but dependent resources NOT spawned yet
    /// Created when SceneTemplate spawns (startup for IsStarter, runtime for rewards)
    /// NO dependent locations/items created yet (deferred until activation)
    /// Stored in GameWorld.Scenes with State=Deferred
    /// Purpose: Separate scene creation from resource spawning (two-phase initialization)
    /// Transition: Activated when player enters location or queries scenes at context
    /// </summary>
    Deferred,

    /// <summary>
    /// Scene is active and available for player interaction
    /// Dependent resources spawned (locations placed, items created)
    /// Current Situation can be engaged
    /// Stored in GameWorld.Scenes with State=Active
    /// Transition: From Deferred when scene activated
    /// </summary>
    Active,

    /// <summary>
    /// Scene has been completed (all relevant Situations finished)
    /// No longer active, can be archived or removed
    /// </summary>
    Completed,

    /// <summary>
    /// Scene has expired (time limit reached without completion)
    /// No longer accessible, narrative opportunity missed
    /// </summary>
    Expired
}
