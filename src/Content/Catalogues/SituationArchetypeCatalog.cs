using Wayfarer.GameState;
using Wayfarer.GameState.Enums;

namespace Wayfarer.Content.Catalogues;

/// <summary>
/// ⚠️ PARSE-TIME ONLY CATALOGUE ⚠️
///
/// Defines the 5 core situation archetypes used for procedural choice generation.
/// Creates learnable mechanical patterns players recognize and prepare for.
///
/// CATALOGUE PATTERN COMPLIANCE:
/// - Called ONLY from SceneTemplateParser at PARSE TIME
/// - NEVER called from facades, managers, or runtime code
/// - NEVER imported except in parser classes
/// - Generates archetype definitions that parser uses to create ChoiceTemplates
/// - Translation happens ONCE at game initialization
///
/// ARCHITECTURE:
/// Parser reads archetypeId from JSON → Calls GetArchetype() → Receives archetype structure
/// → Parser generates 4 ChoiceTemplates from archetype → Stores in SituationTemplate
/// → Runtime queries GameWorld.Situations (pre-populated), NO catalogue calls
///
/// THE FIVE ARCHETYPES:
/// 1. Confrontation - Authority challenges, physical barriers
/// 2. Negotiation - Price disputes, deal-making
/// 3. Investigation - Mysteries, puzzles, information gathering
/// 4. Social Maneuvering - Reputation management, relationship building
/// 5. Crisis - Emergencies, high-stakes moments
///
/// Each archetype defines:
/// - Which stats are tested (learnable patterns)
/// - Cost structure (0/low/high)
/// - Challenge types
/// - Fallback penalties
/// </summary>
public static class SituationArchetypeCatalog
{
    /// <summary>
    /// Get archetype definition by ID
    /// Called at parse time to generate choice structures
    /// Throws InvalidDataException on unknown archetype ID (fail fast)
    /// </summary>
    public static SituationArchetype GetArchetype(string archetypeId)
    {
        return archetypeId?.ToLowerInvariant() switch
        {
            "confrontation" => CreateConfrontation(),
            "negotiation" => CreateNegotiation(),
            "investigation" => CreateInvestigation(),
            "social_maneuvering" => CreateSocialManeuvering(),
            "crisis" => CreateCrisis(),
            _ => throw new InvalidDataException($"Unknown archetype ID: '{archetypeId}'. Valid values: confrontation, negotiation, investigation, social_maneuvering, crisis")
        };
    }

    /// <summary>
    /// CONFRONTATION archetype
    ///
    /// When Used: Authority challenges, physical barriers, intimidation moments
    /// Common In: Authority domain (guard posts, government, courts)
    /// Player Learns: "Authority matters in Authority zones"
    ///
    /// Choice Pattern:
    /// 1. Authority 3+ → Command, assert dominance (best, free)
    /// 2. 15 coins → Bribe, pay off (decent, expensive)
    /// 3. Physical challenge → Fight, endure (risky, variable)
    /// 4. Fallback → Submit, back down (poor, always available)
    /// </summary>
    private static SituationArchetype CreateConfrontation()
    {
        return new SituationArchetype
        {
            Id = "confrontation",
            Name = "Confrontation",
            Domain = Domain.Authority,
            PrimaryStat = PlayerStatType.Authority,
            SecondaryStat = PlayerStatType.Authority, // Using Authority for both (Intimidation not in PlayerStatType)
            StatThreshold = 3,
            CoinCost = 15,
            ChallengeType = TacticalSystemType.Physical,
            ResolveCost = 2,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// NEGOTIATION archetype
    ///
    /// When Used: Price disputes, deal-making, compromise seeking
    /// Common In: Economic domain (merchant quarters, markets, guilds)
    /// Player Learns: "Diplomacy matters in Economic zones"
    ///
    /// Choice Pattern:
    /// 1. Diplomacy/Rapport 3+ → Persuade, charm (best, free)
    /// 2. 15 coins → Pay premium, sweeten deal (decent, expensive)
    /// 3. Mental challenge → Debate, cite regulations (risky, cerebral)
    /// 4. Fallback → Accept unfavorable terms (poor, always available)
    /// </summary>
    private static SituationArchetype CreateNegotiation()
    {
        return new SituationArchetype
        {
            Id = "negotiation",
            Name = "Negotiation",
            Domain = Domain.Economic,
            PrimaryStat = PlayerStatType.Diplomacy,
            SecondaryStat = PlayerStatType.Rapport,
            StatThreshold = 3,
            CoinCost = 15,
            ChallengeType = TacticalSystemType.Mental,
            ResolveCost = 2,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// INVESTIGATION archetype
    ///
    /// When Used: Mysteries, puzzles, information gathering, deduction
    /// Common In: Mental domain (libraries, laboratories, scholarly societies)
    /// Player Learns: "Insight matters in Mental zones"
    ///
    /// Choice Pattern:
    /// 1. Insight/Cunning 3+ → Deduce, analyze (best, free)
    /// 2. 10 coins → Pay informant, hire expert (decent, moderate cost)
    /// 3. Mental challenge → Work through puzzle (risky, time-consuming)
    /// 4. Fallback → Guess, give up (poor, miss information)
    /// </summary>
    private static SituationArchetype CreateInvestigation()
    {
        return new SituationArchetype
        {
            Id = "investigation",
            Name = "Investigation",
            Domain = Domain.Mental,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Cunning,
            StatThreshold = 3,
            CoinCost = 10,
            ChallengeType = TacticalSystemType.Mental,
            ResolveCost = 2,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// SOCIAL MANEUVERING archetype
    ///
    /// When Used: Reputation management, relationship building, social hierarchy
    /// Common In: Social domain (taverns, noble estates, social gatherings)
    /// Player Learns: "Rapport matters in Social zones"
    ///
    /// Choice Pattern:
    /// 1. Rapport/Cunning 3+ → Read people, empathize (best, free)
    /// 2. 10 coins → Gift, favor, social currency (decent, transactional)
    /// 3. Social challenge → Risk reputation, bold statement (risky, high stakes)
    /// 4. Fallback → Alienate, offend, awkward exit (poor, damages relationship)
    /// </summary>
    private static SituationArchetype CreateSocialManeuvering()
    {
        return new SituationArchetype
        {
            Id = "social_maneuvering",
            Name = "Social Maneuvering",
            Domain = Domain.Social,
            PrimaryStat = PlayerStatType.Rapport,
            SecondaryStat = PlayerStatType.Cunning,
            StatThreshold = 3,
            CoinCost = 10,
            ChallengeType = TacticalSystemType.Social,
            ResolveCost = 2,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// CRISIS archetype
    ///
    /// When Used: Emergencies, high-stakes moments, moral dilemmas, scene climaxes
    /// Common In: Any domain (rare, high-impact moments)
    /// Player Learns: "Crisis moments demand expertise or sacrifice"
    ///
    /// Choice Pattern:
    /// 1. Authority 4+ → Heroic action, expert solution (best, high requirement)
    /// 2. 25 coins → Expensive emergency solution (guaranteed, financially ruinous)
    /// 3. Physical challenge → Personal risk, gamble everything (risky, heroic)
    /// 4. Fallback → Flee, accept severe consequence (worst, permanent guilt)
    /// </summary>
    private static SituationArchetype CreateCrisis()
    {
        return new SituationArchetype
        {
            Id = "crisis",
            Name = "Crisis",
            Domain = Domain.Physical, // Can appear in any domain, but Physical for default
            PrimaryStat = PlayerStatType.Authority, // Leadership in crisis
            SecondaryStat = PlayerStatType.Insight, // Alternative: Understanding the situation
            StatThreshold = 4, // Higher requirement for crisis
            CoinCost = 25, // Very expensive
            ChallengeType = TacticalSystemType.Physical, // Often physical danger
            ResolveCost = 3, // Higher resolve cost
            FallbackTimeCost = 2 // Worse penalty
        };
    }
}
