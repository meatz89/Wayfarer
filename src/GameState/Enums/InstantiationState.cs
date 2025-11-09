
/// <summary>
/// Controls whether ChoiceTemplate actions have been materialized into GameWorld collections
/// Part of three-tier timing model: Parse → Instantiation → Query
/// Orthogonal to LifecycleStatus (progression tracking)
/// </summary>
public enum InstantiationState
{
/// <summary>
/// Situation exists as data but ChoiceTemplates NOT instantiated into actions
/// NO actions exist in GameWorld.LocationActions/NPCActions/PathCards
/// Waiting for player to enter context (location/NPC/route)
/// Lazy evaluation - actions created on demand at query time
/// </summary>
Deferred,

/// <summary>
/// Player entered context, ChoiceTemplates instantiated into action entities
/// Actions exist in GameWorld.LocationActions/NPCActions/PathCards collections
/// Provisional Scenes created for actions with SceneSpawnRewards
/// Actions available for player selection in UI
/// </summary>
Instantiated
}
