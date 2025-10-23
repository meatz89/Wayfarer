/// <summary>
/// Catalogue of all location-specific actions matched by location properties.
/// LocationActions are only available at locations with matching properties.
/// Parser validates JSON actionType against this enum - throws on unknown types.
///
/// ADD NEW ACTIONS:
/// 1. Add enum value here
/// 2. Add handler in GameFacade.ExecuteLocationAction()
/// 3. Add JSON entry in locationActions array with property requirements
/// </summary>
public enum LocationActionType
{
    /// <summary>
    /// Navigate to travel routes screen
    /// Available at: Locations with Crossroads property
    /// </summary>
    Travel,

    /// <summary>
    /// Recover health and stamina at safe locations
    /// Effect: +1 segment, +5 hunger, +1 health, +1 stamina
    /// Available at: Locations with Rest or Restful properties
    /// </summary>
    Rest,

    /// <summary>
    /// Pay for secure room with full recovery
    /// Effect: Advance to next morning, full health/stamina/focus recovery, hunger reset to 0
    /// Available at: Locations with Lodging property
    /// Costs: 20 coins (or configured cost)
    /// </summary>
    SecureRoom,

    /// <summary>
    /// Earn coins through work
    /// Effect: +1 segment, earn 8 coins (modified by hunger)
    /// Available at: Locations with Commercial property
    /// </summary>
    Work,

    /// <summary>
    /// Gain location familiarity
    /// Effect: +1 segment, +familiarity based on location type
    /// Available at: Locations with canInvestigate=true
    /// </summary>
    Investigate
}
