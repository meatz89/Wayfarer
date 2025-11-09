namespace Wayfarer.GameState.Enums;

/// <summary>
/// UNIVERSAL categorical property: Ethical implications of this situation
///
/// Derivation: NPC morality (CRUEL = Clear evil), location properties (holy, temple = Clear good), default Ambiguous
///
/// Scales:
/// - Narrative framing: Clear = obvious hero/villain, Ambiguous = gray area, Dilemma = no good option
/// - Long-term consequences: Conscience tracking for ambiguous choices
/// - Player reputation: Clear good/evil affects faction standing
/// - Quest outcomes: Dilemma choices create branching paths
///
/// Used for narrative generation and long-term consequence tracking.
/// </summary>
public enum MoralClarity
{
    Clear,      // Obvious right/wrong (holy site, cruel villain, clear heroism)
    Ambiguous,  // Gray area, complex situation, trade-offs (most scenarios)
    Dilemma     // No good option, forced to choose lesser evil
}
