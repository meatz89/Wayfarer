
/// <summary>
/// UNIVERSAL categorical property: Relative authority/status between player and NPC
///
/// Derivation: Compare player.Authority vs npc.Authority (with threshold)
///
/// Scales:
/// - Confrontation difficulty: Dominant (0.6x threshold), Equal (1.0x), Submissive (1.4x)
/// - Negotiation thresholds: Who has leverage
/// - Social_maneuvering requirements: Easier to influence those below you
/// - Romance: Power imbalances affect approach options
///
/// Used by ALL NPC interaction situation archetypes.
/// </summary>
public enum PowerDynamic
{
Dominant,    // Player has authority/status over NPC (easier persuasion, intimidation)
Equal,       // Balanced relationship (standard difficulty)
Submissive   // NPC has power over player (harder to influence, must defer)
}
