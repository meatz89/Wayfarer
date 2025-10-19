/// <summary>
/// Catalogue of all player actions available globally regardless of location.
/// PlayerActions are always available to the player at any location.
/// Parser validates JSON actionType against this enum - throws on unknown types.
///
/// ADD NEW ACTIONS:
/// 1. Add enum value here
/// 2. Add handler in GameFacade.ExecutePlayerAction()
/// 3. Add JSON entry in playerActions array
/// </summary>
public enum PlayerActionType
{
    /// <summary>
    /// View inventory and equipment
    /// </summary>
    CheckBelongings,

    /// <summary>
    /// Skip 1 time segment with no resource recovery
    /// Effect: +1 segment, +5 hunger (time passes)
    /// </summary>
    Wait
}
