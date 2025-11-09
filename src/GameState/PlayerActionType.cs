/// <summary>
/// Catalogue of all player actions available globally regardless of location.
/// PlayerActions are always available to the player at any location.
/// Parser validates JSON actionType against this enum - throws on unknown types.
///
/// ADD NEW ACTIONS:
/// 1. Add enum value here
/// 2. Create intent class in PlayerIntent.cs
/// 3. Add handler in GameFacade.ProcessIntent()
/// 4. Add JSON entry in playerActions array
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
Wait,

/// <summary>
/// Sleep rough without shelter
/// Effect: -2 Health, no time advancement, no recovery
/// Tutorial: Save coins but take damage and risk
/// </summary>
SleepOutside,

/// <summary>
/// Look around at current location
/// Effect: Navigate to LookingAround view showing NPCs, challenges, and opportunities
/// UI: Always available at any location
/// </summary>
LookAround
}
