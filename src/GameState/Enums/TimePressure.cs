namespace Wayfarer.GameState.Enums;

/// <summary>
/// UNIVERSAL categorical property: How urgent is the situation?
///
/// Derivation: Time of day (Night = Urgent), location properties (crisis, emergency), scene context
///
/// Scales:
/// - Available choices: Leisurely (4 choices), Urgent (3 choices), Desperate (2 choices)
/// - Time costs: Leisurely allows deliberation, Desperate requires immediate action
/// - Retry availability: Leisurely can retry, Desperate is one-shot
/// - Skip options: Leisurely has skip, Desperate forces engagement
///
/// Used by ALL situation archetypes for choice availability.
/// </summary>
public enum TimePressure
{
    Leisurely,  // Can take time, multiple attempts, all options available
    Urgent,     // Limited time, immediate action needed, some options unavailable
    Desperate   // Right now or never, minimal choices, no retry
}
