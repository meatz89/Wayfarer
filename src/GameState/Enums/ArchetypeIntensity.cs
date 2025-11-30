
/// <summary>
/// Categorizes situation archetypes by their demand on player resources.
/// Used by ProceduralAStoryService to filter archetype selection based on player readiness.
///
/// PLAYER READINESS MATCHING:
/// - Exhausted player (Resolve less than 3) → PEACEFUL only
/// - Normal player (Resolve 3-15) → PEACEFUL, BUILDING, TESTING
/// - Capable player (Resolve greater than 15) → Any intensity including CRISIS
///
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
    Peaceful,

    /// <summary>
    /// Low intensity - stat-granting archetypes.
    /// Minimal Resolve cost, choices grant stats rather than require them.
    /// Used during recovery periods or positive momentum.
    /// Examples: ServiceTransaction, RestPreparation
    /// </summary>
    Building,

    /// <summary>
    /// Standard intensity - normal trade-off archetypes.
    /// Standard Resolve cost (5), stat requirements present.
    /// Used for normal gameplay progression.
    /// Examples: Negotiation, Investigation, SocialManeuvering
    /// </summary>
    Testing,

    /// <summary>
    /// High intensity - demanding archetypes.
    /// High Resolve cost (10+), significant stat requirements.
    /// Only appropriate when player is well-resourced.
    /// Examples: Crisis, Confrontation, EmergencyAid
    /// </summary>
    Crisis
}
