namespace Wayfarer.GameState.Enums;

/// <summary>
/// State of a Scene in its lifecycle
/// Scenes transition: Provisional → Active → Completed/Expired
/// </summary>
public enum SceneState
{
    /// <summary>
    /// Scene is provisional (mechanical skeleton awaiting player Choice selection)
    /// Created when Situation instantiated (for each Choice with SceneSpawnReward)
    /// Has concrete placement but NO narrative content yet (placeholders unresolved)
    /// Stored in GameWorld.ProvisionalScenes (temporary storage)
    /// Purpose: Perfect information - player sees WHERE Scene spawns before selecting Choice
    /// Transition: Finalized to Active when Choice selected, OR deleted if different Choice selected
    /// </summary>
    Provisional,

    /// <summary>
    /// Scene is active and available for player interaction
    /// Finalized from Provisional state with narrative content (placeholders resolved)
    /// Current Situation can be engaged
    /// Stored in GameWorld.Scenes (permanent storage)
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
