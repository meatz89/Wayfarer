
/// <summary>
/// Categorizes situation archetypes by their demand on player resources.
/// Used by ProceduralAStoryService to filter archetype selection based on player readiness.
///
/// THREE-LEVEL SYSTEM (named to avoid collision with RhythmPattern values):
/// - Exhausted player (Resolve less than 3) → RECOVERY only
/// - Normal player (Resolve 3-15) → RECOVERY, STANDARD
/// - Capable player (Resolve greater than 15) → Any intensity including DEMANDING
///
/// ORTHOGONAL TO RHYTHM: ArchetypeIntensity controls WHICH archetypes are selected.
/// RhythmPattern (Building/Crisis/Mixed) controls HOW choices within archetypes are structured.
/// See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
/// </summary>
public enum ArchetypeIntensity
{
    /// <summary>
    /// Lowest intensity - recovery-focused archetypes.
    /// No Resolve cost, no stat requirements, purely positive outcomes.
    /// Used when player is exhausted and needs breathing room.
    /// Examples: MeditationAndReflection, LocalConversation, StudyInLibrary
    /// </summary>
    Recovery,

    /// <summary>
    /// Standard intensity - normal trade-off archetypes.
    /// Standard Resolve cost, stat requirements present.
    /// Used for normal gameplay progression.
    /// Examples: Negotiation, Investigation, SocialManeuvering, Service transactions
    /// </summary>
    Standard,

    /// <summary>
    /// High intensity - demanding archetypes.
    /// High Resolve cost, significant stat requirements.
    /// Only appropriate when player is well-resourced.
    /// Examples: Crisis, Confrontation, EmergencyAid
    /// </summary>
    Demanding
}
