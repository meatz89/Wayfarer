using Wayfarer.GameState;
using Wayfarer.GameState.Enums;

namespace Wayfarer.Content.Catalogues;

/// <summary>
/// ⚠️ PARSE-TIME ONLY CATALOGUE ⚠️
///
/// Defines 15 situation archetypes for procedural choice generation.
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
/// ARCHETYPE LIBRARY (15 patterns):
/// Core:
/// 1. Confrontation - Authority challenges, physical barriers
/// 2. Negotiation - Price disputes, deal-making
/// 3. Investigation - Mysteries, puzzles, information gathering
/// 4. Social Maneuvering - Reputation management, relationship building
/// 5. Crisis - Emergencies, high-stakes moments
///
/// Expanded:
/// 6. Service Transaction - Paying for services, economic exchanges
/// 7. Access Control - Gatekeepers, locked doors, restricted areas
/// 8. Information Gathering - Rumors, gossip, local knowledge
/// 9. Skill Demonstration - Proving competence, showing credentials
/// 10. Reputation Challenge - Defending honor, responding to accusations
/// 11. Emergency Aid - Medical crisis, rescue situations
/// 12. Administrative Procedure - Bureaucracy, permits, official processes
/// 13. Trade Dispute - Disagreements over goods, quality, terms
/// 14. Cultural Faux Pas - Social blunders, tradition violations
/// 15. Recruitment - Join requests, commitment decisions
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
            "service_transaction" => CreateServiceTransaction(),
            "access_control" => CreateAccessControl(),
            "information_gathering" => CreateInformationGathering(),
            "skill_demonstration" => CreateSkillDemonstration(),
            "reputation_challenge" => CreateReputationChallenge(),
            "emergency_aid" => CreateEmergencyAid(),
            "administrative_procedure" => CreateAdministrativeProcedure(),
            "trade_dispute" => CreateTradeDispute(),
            "cultural_faux_pas" => CreateCulturalFauxPas(),
            "recruitment" => CreateRecruitment(),
            "rest_preparation" => CreateRestPreparation(),
            "entering_private_space" => CreateEnteringPrivateSpace(),
            "departing_private_space" => CreateDepartingPrivateSpace(),
            "service_negotiation" => CreateServiceNegotiation(),
            "service_execution_rest" => CreateServiceExecutionRest(),
            "service_departure" => CreateServiceDeparture(),
            _ => throw new InvalidDataException($"Unknown archetype ID: '{archetypeId}'. Valid values: confrontation, negotiation, investigation, social_maneuvering, crisis, service_transaction, access_control, information_gathering, skill_demonstration, reputation_challenge, emergency_aid, administrative_procedure, trade_dispute, cultural_faux_pas, recruitment, rest_preparation, entering_private_space, departing_private_space, service_negotiation, service_execution_rest, service_departure")
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

    /// <summary>
    /// SERVICE TRANSACTION archetype
    ///
    /// When Used: Paying for services (lodging, food, healing, ferry passage)
    /// Common In: Economic domain (inns, taverns, shops, service providers)
    /// Player Learns: "Services cost money or goodwill"
    ///
    /// Choice Pattern:
    /// 1. Rapport 3+ → Request favor, use goodwill (best, free if liked)
    /// 2. 10 coins → Pay standard rate (decent, straightforward)
    /// 3. Mental challenge → Debate price, cite reasons (risky, haggling)
    /// 4. Fallback → Go without, find alternative (poor, inconvenient)
    /// </summary>
    private static SituationArchetype CreateServiceTransaction()
    {
        return new SituationArchetype
        {
            Id = "service_transaction",
            Name = "Service Transaction",
            Domain = Domain.Economic,
            PrimaryStat = PlayerStatType.None,
            SecondaryStat = PlayerStatType.None,
            StatThreshold = 0,
            CoinCost = 5,
            ChallengeType = TacticalSystemType.Mental,
            ResolveCost = 1,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// ACCESS CONTROL archetype
    ///
    /// When Used: Getting past gatekeepers, locked doors, restricted areas
    /// Common In: Authority domain (checkpoints, private areas, guarded locations)
    /// Player Learns: "Access requires authority or clever bypass"
    ///
    /// Choice Pattern:
    /// 1. Authority 3+ → Flash credentials, demand entry (best, commanding)
    /// 2. 15 coins → Pay bribe, grease palms (decent, costly)
    /// 3. Physical challenge → Sneak, climb, force entry (risky, illegal)
    /// 4. Fallback → Turn back, find other route (poor, time loss)
    /// </summary>
    private static SituationArchetype CreateAccessControl()
    {
        return new SituationArchetype
        {
            Id = "access_control",
            Name = "Access Control",
            Domain = Domain.Authority,
            PrimaryStat = PlayerStatType.Authority,
            SecondaryStat = PlayerStatType.Cunning,
            StatThreshold = 3,
            CoinCost = 15,
            ChallengeType = TacticalSystemType.Physical,
            ResolveCost = 2,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// INFORMATION GATHERING archetype
    ///
    /// When Used: Rumors, news, local knowledge, gossip networks
    /// Common In: Social domain (taverns, markets, social gatherings)
    /// Player Learns: "Information flows through social channels"
    ///
    /// Choice Pattern:
    /// 1. Rapport 3+ → Ask friends, leverage connections (best, trusted source)
    /// 2. 8 coins → Pay informant, buy drinks (decent, direct)
    /// 3. Social challenge → Eavesdrop, infiltrate group (risky, may offend)
    /// 4. Fallback → Guess, make assumptions (poor, unreliable)
    /// </summary>
    private static SituationArchetype CreateInformationGathering()
    {
        return new SituationArchetype
        {
            Id = "information_gathering",
            Name = "Information Gathering",
            Domain = Domain.Social,
            PrimaryStat = PlayerStatType.Rapport,
            SecondaryStat = PlayerStatType.Insight,
            StatThreshold = 3,
            CoinCost = 8,
            ChallengeType = TacticalSystemType.Social,
            ResolveCost = 1,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// SKILL DEMONSTRATION archetype
    ///
    /// When Used: Proving competence, showing credentials, demonstrating expertise
    /// Common In: Economic domain (guilds, hiring, professional contexts)
    /// Player Learns: "Skills open doors in professional settings"
    ///
    /// Choice Pattern:
    /// 1. Diplomacy 3+ → Talk up experience, impress with history (best, convincing)
    /// 2. 12 coins → Provide sample, pay examination fee (decent, proof)
    /// 3. Mental challenge → Puzzle, test, demonstration (risky, public failure)
    /// 4. Fallback → Admit inexperience, withdraw (poor, reputation loss)
    /// </summary>
    private static SituationArchetype CreateSkillDemonstration()
    {
        return new SituationArchetype
        {
            Id = "skill_demonstration",
            Name = "Skill Demonstration",
            Domain = Domain.Economic,
            PrimaryStat = PlayerStatType.Diplomacy,
            SecondaryStat = PlayerStatType.Insight,
            StatThreshold = 3,
            CoinCost = 12,
            ChallengeType = TacticalSystemType.Mental,
            ResolveCost = 2,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// REPUTATION CHALLENGE archetype
    ///
    /// When Used: Someone questioning your standing, honor, or claims
    /// Common In: Social domain (public spaces, social circles, rumor mills)
    /// Player Learns: "Reputation must be defended or grows worse"
    ///
    /// Choice Pattern:
    /// 1. Authority 3+ → Demand apology, assert dominance (best, shows strength)
    /// 2. 10 coins → Buy silence, pay off accuser (decent, temporary fix)
    /// 3. Social challenge → Public debate, prove worth (risky, high stakes)
    /// 4. Fallback → Ignore, let rumors spread (poor, reputation damage)
    /// </summary>
    private static SituationArchetype CreateReputationChallenge()
    {
        return new SituationArchetype
        {
            Id = "reputation_challenge",
            Name = "Reputation Challenge",
            Domain = Domain.Social,
            PrimaryStat = PlayerStatType.Authority,
            SecondaryStat = PlayerStatType.Diplomacy,
            StatThreshold = 3,
            CoinCost = 10,
            ChallengeType = TacticalSystemType.Social,
            ResolveCost = 2,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// EMERGENCY AID archetype
    ///
    /// When Used: Medical crisis, immediate danger to others, rescue situations
    /// Common In: Physical domain (accidents, disasters, sudden illness)
    /// Player Learns: "Emergencies demand quick thinking or resources"
    ///
    /// Choice Pattern:
    /// 1. Insight 3+ → Diagnose, apply first aid (best, expert solution)
    /// 2. 20 coins → Hire professional, emergency services (decent, expensive)
    /// 3. Physical challenge → Carry, rescue, intervene (risky, heroic)
    /// 4. Fallback → Stand by helplessly, flee (worst, guilt)
    /// </summary>
    private static SituationArchetype CreateEmergencyAid()
    {
        return new SituationArchetype
        {
            Id = "emergency_aid",
            Name = "Emergency Aid",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Authority,
            StatThreshold = 3,
            CoinCost = 20,
            ChallengeType = TacticalSystemType.Physical,
            ResolveCost = 2,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// ADMINISTRATIVE PROCEDURE archetype
    ///
    /// When Used: Bureaucracy, paperwork, permits, official processes
    /// Common In: Authority domain (government offices, customs, registration)
    /// Player Learns: "Bureaucracy has rules and shortcuts"
    ///
    /// Choice Pattern:
    /// 1. Diplomacy 3+ → Navigate system, know procedures (best, smooth)
    /// 2. 12 coins → Pay expedite fee, grease wheels (decent, faster)
    /// 3. Mental challenge → Find loophole, cite regulations (risky, argumentative)
    /// 4. Fallback → Endure delays, red tape (poor, time sink)
    /// </summary>
    private static SituationArchetype CreateAdministrativeProcedure()
    {
        return new SituationArchetype
        {
            Id = "administrative_procedure",
            Name = "Administrative Procedure",
            Domain = Domain.Authority,
            PrimaryStat = PlayerStatType.Diplomacy,
            SecondaryStat = PlayerStatType.Insight,
            StatThreshold = 3,
            CoinCost = 12,
            ChallengeType = TacticalSystemType.Mental,
            ResolveCost = 1,
            FallbackTimeCost = 2
        };
    }

    /// <summary>
    /// TRADE DISPUTE archetype
    ///
    /// When Used: Disagreement over goods, prices, quality, contract terms
    /// Common In: Economic domain (markets, shops, merchant quarters)
    /// Player Learns: "Trade conflicts need resolution or escalate"
    ///
    /// Choice Pattern:
    /// 1. Insight 3+ → Spot defect, cite precedent (best, proves case)
    /// 2. 15 coins → Settle, split difference (decent, pragmatic)
    /// 3. Mental challenge → Arbitration, formal complaint (risky, legal)
    /// 4. Fallback → Accept loss, walk away (poor, cheated)
    /// </summary>
    private static SituationArchetype CreateTradeDispute()
    {
        return new SituationArchetype
        {
            Id = "trade_dispute",
            Name = "Trade Dispute",
            Domain = Domain.Economic,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Diplomacy,
            StatThreshold = 3,
            CoinCost = 15,
            ChallengeType = TacticalSystemType.Mental,
            ResolveCost = 2,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// CULTURAL FAUX PAS archetype
    ///
    /// When Used: Social blunder, tradition violation, etiquette breach
    /// Common In: Social domain (formal events, cultural contexts, traditions)
    /// Player Learns: "Culture matters, mistakes have social cost"
    ///
    /// Choice Pattern:
    /// 1. Rapport 3+ → Apologize gracefully, show cultural awareness (best, saves face)
    /// 2. 10 coins → Offer gift, make amends (decent, transactional)
    /// 3. Social challenge → Defend action, explain misunderstanding (risky, doubles down)
    /// 4. Fallback → Ignore, act oblivious (poor, alienates group)
    /// </summary>
    private static SituationArchetype CreateCulturalFauxPas()
    {
        return new SituationArchetype
        {
            Id = "cultural_faux_pas",
            Name = "Cultural Faux Pas",
            Domain = Domain.Social,
            PrimaryStat = PlayerStatType.Rapport,
            SecondaryStat = PlayerStatType.Insight,
            StatThreshold = 3,
            CoinCost = 10,
            ChallengeType = TacticalSystemType.Social,
            ResolveCost = 1,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// RECRUITMENT archetype
    ///
    /// When Used: Someone wants you to join, work for them, commit to cause
    /// Common In: Social domain (guilds, factions, organizations)
    /// Player Learns: "Commitments have benefits and obligations"
    ///
    /// Choice Pattern:
    /// 1. Cunning 3+ → Negotiate terms, secure advantage (best, favorable deal)
    /// 2. 8 coins → Buy time, delay decision (decent, postpones)
    /// 3. Social challenge → Counter-offer, demand concessions (risky, bold)
    /// 4. Fallback → Refuse bluntly, burn bridge (poor, makes enemy)
    /// </summary>
    private static SituationArchetype CreateRecruitment()
    {
        return new SituationArchetype
        {
            Id = "recruitment",
            Name = "Recruitment",
            Domain = Domain.Social,
            PrimaryStat = PlayerStatType.Cunning,
            SecondaryStat = PlayerStatType.Diplomacy,
            StatThreshold = 3,
            CoinCost = 8,
            ChallengeType = TacticalSystemType.Social,
            ResolveCost = 2,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// REST PREPARATION archetype
    ///
    /// When Used: Preparing to rest in private space, optimizing recovery
    /// Common In: Physical domain (lodging, safe havens, private rooms)
    /// Player Learns: "Preparation affects recovery quality"
    ///
    /// Choice Pattern:
    /// 1. Insight 3+ → Optimize rest conditions (best, maximum recovery)
    /// 2. 8 coins → Use comfort items (decent, good recovery + bonus)
    /// 3. Mental challenge → Force relaxation despite anxiety (risky, variable)
    /// 4. Fallback → Collapse from exhaustion (poor, minimal recovery)
    /// </summary>
    private static SituationArchetype CreateRestPreparation()
    {
        return new SituationArchetype
        {
            Id = "rest_preparation",
            Name = "Rest Preparation",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Authority,
            StatThreshold = 3,
            CoinCost = 8,
            ChallengeType = TacticalSystemType.Mental,
            ResolveCost = 1,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// ENTERING PRIVATE SPACE archetype
    ///
    /// When Used: First entry into private room/space player has rented/unlocked
    /// Common In: Physical domain (lodging, private chambers, secured areas)
    /// Player Learns: "Room quality assessment affects comfort"
    ///
    /// Choice Pattern:
    /// 1. Insight 3+ → Thoroughly inspect and optimize space (best, detailed assessment)
    /// 2. 8 coins → Request comfort amenities (decent, service upgrade)
    /// 3. Mental challenge → Push through discomfort mentally (risky, force adaptation)
    /// 4. Fallback → Collapse immediately without preparation (poor, exhausted entry)
    ///
    /// </summary>
    private static SituationArchetype CreateEnteringPrivateSpace()
    {
        return new SituationArchetype
        {
            Id = "entering_private_space",
            Name = "Entering Private Space",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Authority,
            StatThreshold = 3,
            CoinCost = 8,
            ChallengeType = TacticalSystemType.Mental,
            ResolveCost = 1,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// Departing Private Space: Leaving lodging after rest
    /// Domain: Physical (organizing departure)
    /// Primary: Insight (noticing forgotten items)
    /// Challenge: Mental (emotional transition)
    /// </summary>
    private static SituationArchetype CreateDepartingPrivateSpace()
    {
        return new SituationArchetype
        {
            Id = "departing_private_space",
            Name = "Departing Private Space",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Cunning,
            StatThreshold = 2,
            CoinCost = 5,
            ChallengeType = TacticalSystemType.Mental,
            ResolveCost = 1,
            FallbackTimeCost = 2
        };
    }

    /// <summary>
    /// SERVICE_NEGOTIATION archetype (REUSABLE)
    ///
    /// When Used: Negotiating access to any service (lodging, bathing, healing)
    /// Domain: Economic (service transaction)
    /// Player Learns: "Services require payment or goodwill"
    ///
    /// Choice Pattern (4 choices):
    /// 1. Rapport 3+ (scaled by NPC.Demeanor) → Leverage relationship (best, free)
    /// 2. 5 coins (scaled by Service.Quality) → Pay for service (decent, straightforward)
    /// 3. Social challenge → Negotiate better terms (risky, variable)
    /// 4. Fallback → Politely decline (poor, no service)
    ///
    /// Context-Aware Scaling:
    /// - NPC.Demeanor: Friendly (0.6x threshold), Neutral (1.0x), Hostile (1.4x)
    /// - Service.Quality: Basic (0.6x cost), Standard (1.0x), Premium (1.6x), Luxury (2.4x)
    /// - Service.Type: Determines which item granted (room_key, bathhouse_token, treatment_receipt)
    /// </summary>
    private static SituationArchetype CreateServiceNegotiation()
    {
        return new SituationArchetype
        {
            Id = "service_negotiation",
            Name = "Service Negotiation",
            Domain = Domain.Economic,
            PrimaryStat = PlayerStatType.Rapport,
            SecondaryStat = PlayerStatType.Diplomacy,
            StatThreshold = 3,
            CoinCost = 5,
            ChallengeType = TacticalSystemType.Social,
            ResolveCost = 1,
            FallbackTimeCost = 0
        };
    }

    /// <summary>
    /// SERVICE_EXECUTION_REST archetype (REUSABLE)
    ///
    /// When Used: Using service in private space (lodging, bathing, healing)
    /// Domain: Physical (rest and recovery)
    /// Player Learns: "Different rest approaches restore different resources"
    ///
    /// Choice Pattern (4 choices, all succeed):
    /// 1. Balanced → Restore health/stamina/focus evenly
    /// 2. Physical focus → Restore health/stamina primarily
    /// 3. Mental focus → Restore stamina/focus primarily
    /// 4. Special → Balanced restoration + unique buff
    ///
    /// All choices advance time to next day Morning.
    ///
    /// Context-Aware Scaling:
    /// - Service.Type: Determines which stats restore (Lodging: all 3, Bathing: cleanliness, Healing: health)
    /// - Spot.Comfort: Scales restoration amounts (Basic: 1x, Standard: 2x, Premium: 3x)
    /// </summary>
    private static SituationArchetype CreateServiceExecutionRest()
    {
        return new SituationArchetype
        {
            Id = "service_execution_rest",
            Name = "Service Execution (Rest)",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.None,
            SecondaryStat = PlayerStatType.None,
            StatThreshold = 0,
            CoinCost = 0,
            ChallengeType = TacticalSystemType.None,
            ResolveCost = 0,
            FallbackTimeCost = 0
        };
    }

    /// <summary>
    /// SERVICE_DEPARTURE archetype (REUSABLE)
    ///
    /// When Used: Leaving service space (lodging, bathing, healing)
    /// Domain: Physical (organization and preparation)
    /// Player Learns: "Taking time to prepare grants bonuses"
    ///
    /// Choice Pattern (2 choices, both succeed):
    /// 1. Leave immediately → Quick exit, no bonus (free, 0 time)
    /// 2. Gather carefully → Organized exit, preparation buff (costs 1 time segment)
    ///
    /// Both choices clean up service (remove key, lock location).
    ///
    /// Context-Aware Scaling:
    /// - Service.Type: Determines which buff granted
    ///   - Lodging: Focused (organization/preparation)
    ///   - Bathing: WellGroomed (appearance/social)
    ///   - Healing: Rested (health management)
    /// </summary>
    private static SituationArchetype CreateServiceDeparture()
    {
        return new SituationArchetype
        {
            Id = "service_departure",
            Name = "Service Departure",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Cunning,
            StatThreshold = 2,
            CoinCost = 0,
            ChallengeType = TacticalSystemType.Mental,
            ResolveCost = 0,
            FallbackTimeCost = 1
        };
    }

    public static List<ChoiceTemplate> GenerateChoiceTemplates(
        SituationArchetype archetype,
        string situationTemplateId)
    {
        List<ChoiceTemplate> choices = new List<ChoiceTemplate>();

        ChoiceTemplate statGatedChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_stat",
            ActionTextTemplate = GenerateStatGatedActionText(archetype),
            RequirementFormula = CreateStatRequirement(archetype),
            CostTemplate = new ChoiceCost(),
            RewardTemplate = new ChoiceReward(),
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(statGatedChoice);

        ChoiceTemplate moneyChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_money",
            ActionTextTemplate = GenerateMoneyActionText(archetype),
            RequirementFormula = new CompoundRequirement(),
            CostTemplate = new ChoiceCost { Coins = archetype.CoinCost },
            RewardTemplate = new ChoiceReward(),
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(moneyChoice);

        ChoiceTemplate challengeChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_challenge",
            ActionTextTemplate = GenerateChallengeActionText(archetype),
            RequirementFormula = new CompoundRequirement(),
            CostTemplate = new ChoiceCost { Resolve = archetype.ResolveCost },
            RewardTemplate = new ChoiceReward(),
            ActionType = ChoiceActionType.StartChallenge,
            ChallengeId = null,
            ChallengeType = archetype.ChallengeType
        };
        choices.Add(challengeChoice);

        ChoiceTemplate fallbackChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_fallback",
            ActionTextTemplate = GenerateFallbackActionText(archetype),
            RequirementFormula = new CompoundRequirement(),
            CostTemplate = new ChoiceCost { TimeSegments = archetype.FallbackTimeCost },
            RewardTemplate = new ChoiceReward(),
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(fallbackChoice);

        return choices;
    }

    private static CompoundRequirement CreateStatRequirement(SituationArchetype archetype)
    {
        CompoundRequirement requirement = new CompoundRequirement();

        OrPath primaryPath = new OrPath
        {
            Label = $"{archetype.PrimaryStat} {archetype.StatThreshold}+",
            NumericRequirements = new List<NumericRequirement>
            {
                new NumericRequirement
                {
                    Type = "PlayerStat",
                    Context = archetype.PrimaryStat.ToString(),
                    Threshold = archetype.StatThreshold,
                    Label = $"{archetype.PrimaryStat} {archetype.StatThreshold}+"
                }
            }
        };
        requirement.OrPaths.Add(primaryPath);

        if (archetype.SecondaryStat != archetype.PrimaryStat)
        {
            OrPath secondaryPath = new OrPath
            {
                Label = $"{archetype.SecondaryStat} {archetype.StatThreshold}+",
                NumericRequirements = new List<NumericRequirement>
                {
                    new NumericRequirement
                    {
                        Type = "PlayerStat",
                        Context = archetype.SecondaryStat.ToString(),
                        Threshold = archetype.StatThreshold,
                        Label = $"{archetype.SecondaryStat} {archetype.StatThreshold}+"
                    }
                }
            };
            requirement.OrPaths.Add(secondaryPath);
        }

        return requirement;
    }

    private static string GenerateStatGatedActionText(SituationArchetype archetype)
    {
        return archetype.Id switch
        {
            "confrontation" => "Assert authority and take command",
            "negotiation" => "Negotiate favorable terms",
            "investigation" => "Deduce the solution through analysis",
            "social_maneuvering" => "Read the social dynamics and navigate skillfully",
            "crisis" => "Take decisive action with expertise",
            "service_transaction" => "Use your expertise",
            "access_control" => "Present credentials",
            "information_gathering" => "Ask the right questions",
            "skill_demonstration" => "Demonstrate your competence",
            "reputation_challenge" => "Defend your honor",
            "emergency_aid" => "Apply expert treatment",
            "administrative_procedure" => "Navigate bureaucracy skillfully",
            "trade_dispute" => "Leverage your position",
            "cultural_faux_pas" => "Apologize gracefully",
            "recruitment" => "Negotiate terms",
            "rest_preparation" => "Optimize rest conditions",
            "entering_private_space" => "Thoroughly inspect and optimize the space",
            "departing_private_space" => "Systematically prepare and check everything",
            _ => "Use your expertise"
        };
    }

    private static string GenerateMoneyActionText(SituationArchetype archetype)
    {
        return archetype.Id switch
        {
            "confrontation" => "Pay off the opposition",
            "negotiation" => "Pay the premium price",
            "investigation" => "Hire an expert or pay for information",
            "social_maneuvering" => "Offer a generous gift",
            "crisis" => "Pay for emergency solution",
            "service_transaction" => "Pay the asking price",
            "access_control" => "Bribe the gatekeeper",
            "information_gathering" => "Buy the information",
            "skill_demonstration" => "Hire someone to vouch",
            "reputation_challenge" => "Offer compensation",
            "emergency_aid" => "Pay for premium care",
            "administrative_procedure" => "Expedite with payment",
            "trade_dispute" => "Offer settlement",
            "cultural_faux_pas" => "Offer gift as amends",
            "recruitment" => "Buy time",
            "rest_preparation" => "Use comfort items for better rest",
            "entering_private_space" => "Request comfort amenities",
            "departing_private_space" => "Leave generous gratuity for staff",
            _ => "Pay to resolve"
        };
    }

    private static string GenerateChallengeActionText(SituationArchetype archetype)
    {
        return archetype.Id switch
        {
            "confrontation" => "Attempt a physical confrontation",
            "negotiation" => "Engage in complex debate",
            "investigation" => "Work through the puzzle systematically",
            "social_maneuvering" => "Make a bold social gambit",
            "crisis" => "Risk everything on a desperate gambit",
            "service_transaction" => "Attempt to bargain",
            "access_control" => "Force your way through",
            "information_gathering" => "Investigate on your own",
            "skill_demonstration" => "Attempt without preparation",
            "reputation_challenge" => "Challenge to duel",
            "emergency_aid" => "Risk improvised treatment",
            "administrative_procedure" => "Navigate red tape",
            "trade_dispute" => "Escalate to arbitration",
            "cultural_faux_pas" => "Defend your action",
            "recruitment" => "Counter-offer boldly",
            "rest_preparation" => "Force yourself to relax despite anxiety",
            "entering_private_space" => "Push through discomfort mentally",
            "departing_private_space" => "Force yourself to leave promptly",
            _ => "Accept the challenge"
        };
    }

    private static string GenerateFallbackActionText(SituationArchetype archetype)
    {
        return archetype.Id switch
        {
            "confrontation" => "Back down and submit",
            "negotiation" => "Accept unfavorable terms",
            "investigation" => "Give up and move on",
            "social_maneuvering" => "Exit awkwardly",
            "crisis" => "Flee the situation",
            "service_transaction" => "Leave without service",
            "access_control" => "Turn back",
            "information_gathering" => "Move on without answers",
            "skill_demonstration" => "Admit lack of skill",
            "reputation_challenge" => "Apologize and back down",
            "emergency_aid" => "Do nothing",
            "administrative_procedure" => "Abandon the process",
            "trade_dispute" => "Accept the loss",
            "cultural_faux_pas" => "Ignore and act oblivious",
            "recruitment" => "Refuse bluntly",
            "rest_preparation" => "Collapse from exhaustion immediately",
            "entering_private_space" => "Collapse immediately without preparation",
            "departing_private_space" => "Rush out without proper preparation",
            _ => "Accept poor outcome"
        };
    }
}
