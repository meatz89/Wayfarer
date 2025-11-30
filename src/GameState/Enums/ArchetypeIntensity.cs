
/// <summary>
/// Categorizes situation archetypes by their inherent challenge level.
/// Content categorization ONLY - does NOT affect which situations display to players.
///
/// THREE-LEVEL SYSTEM (named to avoid collision with RhythmPattern values):
/// - RECOVERY: Restoration-focused archetypes (rest, study, casual encounters)
/// - STANDARD: Moderate trade-off archetypes (investigation, social maneuvering)
/// - DEMANDING: High-stakes archetypes (crisis, confrontation)
///
/// CHALLENGE PHILOSOPHY: Player state does NOT filter situation visibility.
/// All situations display regardless of player Resolve. Learning comes from seeing
/// choices players cannot afford (greyed-out requirements), not hidden situations.
///
/// ORTHOGONAL TO RHYTHM: ArchetypeIntensity describes content challenge level.
/// RhythmPattern (Building/Crisis/Mixed) controls HOW choices are structured.
/// See arc42/08_crosscutting_concepts.md §8.26 and gdd/06_balance.md §6.8
/// </summary>
public enum ArchetypeIntensity
{
    /// <summary>
    /// Lowest intensity - recovery-focused archetypes.
    /// No Resolve cost, no stat requirements, purely positive outcomes.
    /// Generated as earned structural respite every 8th sequence (Peaceful category).
    /// Examples: QuietReflection, CasualEncounter, ScholarlyPursuit
    /// </summary>
    Recovery,

    /// <summary>
    /// Standard intensity - normal trade-off archetypes.
    /// Standard Resolve cost, stat requirements present.
    /// Generated for Investigation and Social categories.
    /// Examples: Negotiation, Investigation, SocialManeuvering, Service transactions
    /// </summary>
    Standard,

    /// <summary>
    /// High intensity - demanding archetypes.
    /// High Resolve cost, significant stat requirements.
    /// Generated for Crisis and Confrontation categories.
    /// Examples: Crisis, Confrontation, EmergencyAid
    /// </summary>
    Demanding
}
