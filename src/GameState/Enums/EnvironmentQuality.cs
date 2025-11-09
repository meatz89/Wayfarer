namespace Wayfarer.GameState.Enums;

/// <summary>
/// UNIVERSAL categorical property: Environmental comfort/amenity level
///
/// Derivation: Location.LocationProperties (luxurious/opulent = Premium, comfortable/restful = Standard, else = Basic)
///
/// Scales restoration and effectiveness across ALL activity types:
/// - Rest: Health/Stamina/Focus restoration multiplier (Basic 1x, Standard 2x, Premium 3x)
/// - Study: Learning speed bonus
/// - Crafting: Quality bonus to created items
/// - Recovery: Healing rate from injuries/illness
///
/// Replaces narrow SpotComfort enum with universal environment system.
/// </summary>
public enum EnvironmentQuality
{
    Basic,     // Rough, uncomfortable, minimal amenities (1x restoration)
    Standard,  // Comfortable, adequate amenities (2x restoration)
    Premium    // Luxurious, exceptional amenities, ideal conditions (3x restoration)
}
