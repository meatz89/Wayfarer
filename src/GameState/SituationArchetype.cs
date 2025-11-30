
/// <summary>
/// SituationArchetype - mechanical template for procedural situation generation
/// Defines the 4-choice structure pattern that players learn to recognize and prepare for
///
/// ARCHITECTURE:
/// - Used ONLY at parse time by SituationArchetypeCatalog
/// - Defines stat requirements, costs, and challenge types for each archetype
/// - Catalogue generates 4 ChoiceTemplates from archetype structure
/// - Domain associations create learnable patterns (Economic → Negotiation/Diplomacy)
/// - RhythmPattern (Building/Crisis/Mixed) determines choice structure at generation time
///
/// THE FOUR CHOICE PATTERN (Always):
/// 1. Stat-Gated: Best outcome, free if stat requirement met
/// 2. Money: Guaranteed success, expensive
/// 3. Challenge: Variable outcome, risky
/// 4. Fallback: Poor outcome, always available
///
/// See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
/// </summary>
public class SituationArchetype
{
    /// <summary>
    /// Archetype type - strongly-typed enum for compile-time validation.
    /// Replaces string ID for type safety.
    /// </summary>
    public SituationArchetypeType Type { get; init; }

    /// <summary>
    /// Display name for this archetype
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Primary domain where this archetype commonly appears
    /// Creates learnable patterns: Economic → Negotiation, Authority → Confrontation, etc.
    /// </summary>
    public Domain Domain { get; init; }

    /// <summary>
    /// Primary stat tested by Choice 1 (stat-gated option)
    /// Confrontation → Authority, Negotiation → Diplomacy, Investigation → Insight, etc.
    /// </summary>
    public PlayerStatType PrimaryStat { get; init; }

    /// <summary>
    /// Secondary stat tested by Choice 1 (alternative for stat-gated option)
    /// Allows multiple build paths: Negotiation can use Diplomacy OR Rapport
    /// </summary>
    public PlayerStatType SecondaryStat { get; init; }

    /// <summary>
    /// Stat threshold required for Choice 1 (stat-gated option)
    /// Standard: 3+, Crisis: 4+
    /// </summary>
    public int StatThreshold { get; init; }

    /// <summary>
    /// Coin cost for Choice 2 (money option)
    /// Investigation: 10, Negotiation/Confrontation: 15, Crisis: 25+
    /// </summary>
    public int CoinCost { get; init; }

    /// <summary>
    /// Challenge type for Choice 3 (challenge option)
    /// Physical, Mental, or Social tactical system
    /// </summary>
    public TacticalSystemType ChallengeType { get; init; }

    /// <summary>
    /// Deck ID for Choice 3 (challenge option)
    /// Specifies which challenge deck to use within the tactical system
    /// Social: "friendly_chat" or "desperate_request"
    /// Mental: "mental_challenge"
    /// Physical: "physical_challenge"
    /// Determined at parse-time based on archetype identity
    /// </summary>
    public string DeckId { get; init; }

    /// <summary>
    /// Resolve cost for Choice 3 (challenge option)
    /// Resource consumed when entering challenge
    /// </summary>
    public int ResolveCost { get; init; }

    /// <summary>
    /// Time segments cost for Choice 4 (fallback option)
    /// Fallback always available but wastes time or incurs penalty
    /// </summary>
    public int FallbackTimeCost { get; init; }

    /// <summary>
    /// Intensity level of this archetype - determines player readiness filtering.
    /// Recovery = exhausted player safe, Demanding = only for well-resourced players.
    /// Used by ProceduralAStoryService to filter archetype selection.
    /// </summary>
    public ArchetypeIntensity Intensity { get; init; }
}
