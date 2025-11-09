namespace Wayfarer.GameState.Enums;

/// <summary>
/// UNIVERSAL categorical property: How risky is failure in this context?
///
/// Derivation: Location properties (dangerous, combat_zone, wilderness), NPC hostility, Player health
///
/// Scales:
/// - Crisis archetype: Safe (minor penalties), Risky (significant loss), Deadly (permanent consequences)
/// - Physical challenges: Damage amounts, injury risk
/// - Confrontation escalation: Violence likelihood
/// - Investigation: Trap severity on failure
///
/// Used by ALL situation archetypes, not domain-specific.
/// </summary>
public enum DangerLevel
{
    Safe,    // Minor penalties, can retry, no permanent loss
    Risky,   // Significant consequences, hard to recover
    Deadly   // Permanent loss possible, character death risk
}
