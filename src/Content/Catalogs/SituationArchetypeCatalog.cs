/// <summary>
/// ⚠️ PARSE-TIME ONLY CATALOGUE ⚠️
///
/// Defines 21 situation archetypes (5 core, 10 expanded, 6 specialized) for procedural choice generation.
/// Creates learnable mechanical patterns players recognize and prepare for.
/// Uses strongly-typed enums for compile-time validation.
///
/// CATALOGUE PATTERN COMPLIANCE:
/// - Called ONLY from SceneTemplateParser at PARSE TIME
/// - NEVER called from facades, managers, or runtime code
/// - NEVER imported except in parser classes
/// - Generates archetype definitions that parser uses to create ChoiceTemplates
/// - Translation happens ONCE at game initialization
///
/// ARCHITECTURE:
/// Parser reads SituationArchetypeType enum from DTO → Calls GetArchetype() → Receives archetype structure
/// → Parser generates 4 ChoiceTemplates from archetype → Stores in SituationTemplate
/// → Runtime queries Scene.Situations (situations embedded in scenes), NO catalogue calls
///
/// ARCHETYPE LIBRARY (21 patterns - see SituationArchetypeType enum):
/// Core (5): Confrontation, Negotiation, Investigation, SocialManeuvering, Crisis
/// Expanded (10): ServiceTransaction, AccessControl, InformationGathering, SkillDemonstration,
///                ReputationChallenge, EmergencyAid, AdministrativeProcedure, TradeDispute,
///                CulturalFauxPas, Recruitment
/// Specialized (6): RestPreparation, EnteringPrivateSpace, DepartingPrivateSpace,
///                  ServiceNegotiation, ServiceExecutionRest, ServiceDeparture
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
    /// Get archetype definition from strongly-typed enum.
    /// Called at parse time to generate choice structures.
    /// Compiler ensures exhaustiveness - no runtime unknown archetype errors.
    /// </summary>
    public static SituationArchetype GetArchetype(SituationArchetypeType archetypeType)
    {
        return archetypeType switch
        {
            SituationArchetypeType.Confrontation => CreateConfrontation(),
            SituationArchetypeType.Negotiation => CreateNegotiation(),
            SituationArchetypeType.Investigation => CreateInvestigation(),
            SituationArchetypeType.SocialManeuvering => CreateSocialManeuvering(),
            SituationArchetypeType.Crisis => CreateCrisis(),
            SituationArchetypeType.ServiceTransaction => CreateServiceTransaction(),
            SituationArchetypeType.AccessControl => CreateAccessControl(),
            SituationArchetypeType.InformationGathering => CreateInformationGathering(),
            SituationArchetypeType.SkillDemonstration => CreateSkillDemonstration(),
            SituationArchetypeType.ReputationChallenge => CreateReputationChallenge(),
            SituationArchetypeType.EmergencyAid => CreateEmergencyAid(),
            SituationArchetypeType.AdministrativeProcedure => CreateAdministrativeProcedure(),
            SituationArchetypeType.TradeDispute => CreateTradeDispute(),
            SituationArchetypeType.CulturalFauxPas => CreateCulturalFauxPas(),
            SituationArchetypeType.Recruitment => CreateRecruitment(),
            SituationArchetypeType.RestPreparation => CreateRestPreparation(),
            SituationArchetypeType.EnteringPrivateSpace => CreateEnteringPrivateSpace(),
            SituationArchetypeType.DepartingPrivateSpace => CreateDepartingPrivateSpace(),
            SituationArchetypeType.ServiceNegotiation => CreateServiceNegotiation(),
            SituationArchetypeType.ServiceExecutionRest => CreateServiceExecutionRest(),
            SituationArchetypeType.ServiceDeparture => CreateServiceDeparture(),
            _ => throw new InvalidOperationException($"Unhandled situation archetype type: {archetypeType}")
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
            Type = SituationArchetypeType.Confrontation,
            Name = "Confrontation",
            Domain = Domain.Authority,
            PrimaryStat = PlayerStatType.Authority,
            SecondaryStat = PlayerStatType.Authority, // Using Authority for both (Intimidation not in PlayerStatType)
            StatThreshold = 3,
            CoinCost = 15,
            ChallengeType = TacticalSystemType.Physical,
            DeckId = "physical_challenge",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.Negotiation,
            Name = "Negotiation",
            Domain = Domain.Economic,
            PrimaryStat = PlayerStatType.Diplomacy,
            SecondaryStat = PlayerStatType.Rapport,
            StatThreshold = 3,
            CoinCost = 15,
            ChallengeType = TacticalSystemType.Mental,
            DeckId = "mental_challenge",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.Investigation,
            Name = "Investigation",
            Domain = Domain.Mental,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Cunning,
            StatThreshold = 3,
            CoinCost = 10,
            ChallengeType = TacticalSystemType.Mental,
            DeckId = "mental_challenge",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.SocialManeuvering,
            Name = "Social Maneuvering",
            Domain = Domain.Social,
            PrimaryStat = PlayerStatType.Rapport,
            SecondaryStat = PlayerStatType.Cunning,
            StatThreshold = 3,
            CoinCost = 10,
            ChallengeType = TacticalSystemType.Social,
            DeckId = "friendly_chat",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.Crisis,
            Name = "Crisis",
            Domain = Domain.Physical, // Can appear in any domain, but Physical for default
            PrimaryStat = PlayerStatType.Authority, // Leadership in crisis
            SecondaryStat = PlayerStatType.Insight, // Alternative: Understanding the situation
            StatThreshold = 4, // Higher requirement for crisis
            CoinCost = 25, // Very expensive
            ChallengeType = TacticalSystemType.Physical, // Often physical danger
            DeckId = "physical_challenge",
            ResolveCost = 10,  // Sir Brante pattern: -10 for high-impact Crisis
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
            Type = SituationArchetypeType.ServiceTransaction,
            Name = "Service Transaction",
            Domain = Domain.Economic,
            PrimaryStat = PlayerStatType.None,
            SecondaryStat = PlayerStatType.None,
            StatThreshold = 0,
            CoinCost = 5,
            ChallengeType = TacticalSystemType.Mental,
            DeckId = "mental_challenge",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.AccessControl,
            Name = "Access Control",
            Domain = Domain.Authority,
            PrimaryStat = PlayerStatType.Authority,
            SecondaryStat = PlayerStatType.Cunning,
            StatThreshold = 3,
            CoinCost = 15,
            ChallengeType = TacticalSystemType.Physical,
            DeckId = "physical_challenge",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.InformationGathering,
            Name = "Information Gathering",
            Domain = Domain.Social,
            PrimaryStat = PlayerStatType.Rapport,
            SecondaryStat = PlayerStatType.Insight,
            StatThreshold = 3,
            CoinCost = 8,
            ChallengeType = TacticalSystemType.Social,
            DeckId = "friendly_chat",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.SkillDemonstration,
            Name = "Skill Demonstration",
            Domain = Domain.Economic,
            PrimaryStat = PlayerStatType.Diplomacy,
            SecondaryStat = PlayerStatType.Insight,
            StatThreshold = 3,
            CoinCost = 12,
            ChallengeType = TacticalSystemType.Mental,
            DeckId = "mental_challenge",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.ReputationChallenge,
            Name = "Reputation Challenge",
            Domain = Domain.Social,
            PrimaryStat = PlayerStatType.Authority,
            SecondaryStat = PlayerStatType.Diplomacy,
            StatThreshold = 3,
            CoinCost = 10,
            ChallengeType = TacticalSystemType.Social,
            DeckId = "friendly_chat",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.EmergencyAid,
            Name = "Emergency Aid",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Authority,
            StatThreshold = 3,
            CoinCost = 20,
            ChallengeType = TacticalSystemType.Physical,
            DeckId = "physical_challenge",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.AdministrativeProcedure,
            Name = "Administrative Procedure",
            Domain = Domain.Authority,
            PrimaryStat = PlayerStatType.Diplomacy,
            SecondaryStat = PlayerStatType.Insight,
            StatThreshold = 3,
            CoinCost = 12,
            ChallengeType = TacticalSystemType.Mental,
            DeckId = "mental_challenge",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.TradeDispute,
            Name = "Trade Dispute",
            Domain = Domain.Economic,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Diplomacy,
            StatThreshold = 3,
            CoinCost = 15,
            ChallengeType = TacticalSystemType.Mental,
            DeckId = "mental_challenge",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.CulturalFauxPas,
            Name = "Cultural Faux Pas",
            Domain = Domain.Social,
            PrimaryStat = PlayerStatType.Rapport,
            SecondaryStat = PlayerStatType.Insight,
            StatThreshold = 3,
            CoinCost = 10,
            ChallengeType = TacticalSystemType.Social,
            DeckId = "desperate_request",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.Recruitment,
            Name = "Recruitment",
            Domain = Domain.Social,
            PrimaryStat = PlayerStatType.Cunning,
            SecondaryStat = PlayerStatType.Diplomacy,
            StatThreshold = 3,
            CoinCost = 8,
            ChallengeType = TacticalSystemType.Social,
            DeckId = "desperate_request",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.RestPreparation,
            Name = "Rest Preparation",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Authority,
            StatThreshold = 3,
            CoinCost = 8,
            ChallengeType = TacticalSystemType.Mental,
            DeckId = "mental_challenge",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.EnteringPrivateSpace,
            Name = "Entering Private Space",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Authority,
            StatThreshold = 3,
            CoinCost = 8,
            ChallengeType = TacticalSystemType.Mental,
            DeckId = "mental_challenge",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.DepartingPrivateSpace,
            Name = "Departing Private Space",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Cunning,
            StatThreshold = 2,
            CoinCost = 5,
            ChallengeType = TacticalSystemType.Mental,
            DeckId = "mental_challenge",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.ServiceNegotiation,
            Name = "Service Negotiation",
            Category = ArchetypeCategory.ServiceNegotiation,  // Enum-based routing
            Domain = Domain.Economic,
            PrimaryStat = PlayerStatType.Rapport,
            SecondaryStat = PlayerStatType.Diplomacy,
            StatThreshold = 3,
            CoinCost = 5,
            ChallengeType = TacticalSystemType.Social,
            DeckId = "friendly_chat",
            ResolveCost = 5,  // Sir Brante pattern: -5 standard cost
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
            Type = SituationArchetypeType.ServiceExecutionRest,
            Name = "Service Execution (Rest)",
            Category = ArchetypeCategory.ServiceExecutionRest,  // Enum-based routing
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.None,
            SecondaryStat = PlayerStatType.None,
            StatThreshold = 0,
            CoinCost = 0,
            ChallengeType = TacticalSystemType.Physical,  // Rest is Physical domain (no actual challenge)
            DeckId = string.Empty,  // No challenge deck - all rest choices succeed
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
            Type = SituationArchetypeType.ServiceDeparture,
            Name = "Service Departure",
            Category = ArchetypeCategory.ServiceDeparture,  // Enum-based routing
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Cunning,
            StatThreshold = 2,
            CoinCost = 0,
            ChallengeType = TacticalSystemType.Mental,
            DeckId = "mental_challenge",
            ResolveCost = 0,
            FallbackTimeCost = 1
        };
    }

    /// <summary>
    /// CONTEXT-AWARE CHOICE GENERATION (for service archetypes)
    ///
    /// Generates choices with context-driven scaling:
    /// - service_negotiation: Scales stat thresholds by NPC.Demeanor, costs by Service.Quality
    /// - service_execution_rest: Generates 4 rest choices with contextual rewards (NOT archetype pattern)
    /// - service_departure: Generates 2 departure choices (NOT archetype pattern)
    ///
    /// For other archetypes, delegates to GenerateChoiceTemplates().
    /// </summary>
    public static List<ChoiceTemplate> GenerateChoiceTemplatesWithContext(
        SituationArchetype archetype,
        string situationTemplateId,
        GenerationContext context)
    {
        // Enum-based routing for specialized choice generation (no ID string matching)
        switch (archetype.Category)
        {
            case ArchetypeCategory.ServiceNegotiation:
                return GenerateServiceNegotiationChoices(archetype, situationTemplateId, context);

            case ArchetypeCategory.ServiceExecutionRest:
                return GenerateServiceExecutionRestChoices(situationTemplateId, context);

            case ArchetypeCategory.ServiceDeparture:
                return GenerateServiceDepartureChoices(situationTemplateId, context);

            case ArchetypeCategory.Standard:
            default:
                // Standard 4-choice pattern
                return GenerateChoiceTemplates(archetype, situationTemplateId);
        }
    }

    /// <summary>
    /// Generate 4 standard choices from archetype with UNIVERSAL PROPERTY SCALING.
    ///
    /// Context parameter enables universal scaling:
    /// - StatThreshold scales by PowerDynamic (Dominant 0.6x, Equal 1.0x, Submissive 1.4x)
    /// - CoinCost scales by Quality (Basic 0.6x, Standard 1.0x, Premium 1.6x, Luxury 2.4x)
    ///
    /// If context null, uses archetype base values (no scaling).
    /// ALL 18 base archetypes benefit from universal scaling.
    /// </summary>
    public static List<ChoiceTemplate> GenerateChoiceTemplates(
        SituationArchetype archetype,
        string situationTemplateId,
        GenerationContext context = null)
    {
        // Scale by universal properties if context provided
        int scaledStatThreshold = archetype.StatThreshold;
        int scaledCoinCost = archetype.CoinCost;

        if (context != null)
        {
            // Adjust stat threshold by PowerDynamic (easier if dominant, harder if submissive)
            scaledStatThreshold = context.Power switch
            {
                PowerDynamic.Dominant => archetype.StatThreshold - 2,
                PowerDynamic.Equal => archetype.StatThreshold,
                PowerDynamic.Submissive => archetype.StatThreshold + 2,
                _ => archetype.StatThreshold
            };

            // Adjust coin cost by Quality (cheaper if basic, expensive if luxury)
            scaledCoinCost = context.Quality switch
            {
                Quality.Basic => archetype.CoinCost - 3,
                Quality.Standard => archetype.CoinCost,
                Quality.Premium => archetype.CoinCost + 5,
                Quality.Luxury => archetype.CoinCost + 10,
                _ => archetype.CoinCost
            };

            // Adjust by NpcDemeanor for additional nuance
            if (context.NpcDemeanor == NPCDemeanor.Hostile)
            {
                scaledStatThreshold = scaledStatThreshold + 2; // Hostile NPCs harder to influence
            }
            else if (context.NpcDemeanor == NPCDemeanor.Friendly)
            {
                scaledStatThreshold = scaledStatThreshold - 2; // Friendly NPCs easier
            }
        }

        List<ChoiceTemplate> choices = new List<ChoiceTemplate>();

        ChoiceTemplate statGatedChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_stat",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = GenerateStatGatedActionText(archetype),
            RequirementFormula = CreateStatRequirement(archetype, scaledStatThreshold),
            Consequence = new Consequence(),
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(statGatedChoice);

        ChoiceTemplate moneyChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_money",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = GenerateMoneyActionText(archetype),
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence { Coins = -scaledCoinCost },
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(moneyChoice);

        // Create consequence first, then derive requirement from it (Sir Brante dual-nature encapsulated)
        Consequence challengeConsequence = archetype.ResolveCost > 0
            ? new Consequence { Resolve = -archetype.ResolveCost }
            : new Consequence();

        ChoiceTemplate challengeChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_challenge",
            PathType = ChoicePathType.Challenge,
            ActionTextTemplate = GenerateChallengeActionText(archetype),
            RequirementFormula = CompoundRequirement.CreateForConsequence(challengeConsequence),
            Consequence = challengeConsequence,
            ActionType = ChoiceActionType.StartChallenge,
            ChallengeId = null,
            ChallengeType = archetype.ChallengeType,
            DeckId = archetype.DeckId
        };
        choices.Add(challengeChoice);

        ChoiceTemplate fallbackChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_fallback",
            PathType = ChoicePathType.Fallback,
            ActionTextTemplate = GenerateFallbackActionText(archetype),
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence { TimeSegments = archetype.FallbackTimeCost },
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(fallbackChoice);

        return choices;
    }

    /// <summary>
    /// Create stat requirement with scaled threshold.
    /// Threshold parameter allows universal property scaling.
    /// </summary>
    public static CompoundRequirement CreateStatRequirement(SituationArchetype archetype, int scaledThreshold)
    {
        CompoundRequirement requirement = new CompoundRequirement();

        OrPath primaryPath = CreateOrPathForStat(archetype.PrimaryStat, scaledThreshold);
        requirement.OrPaths.Add(primaryPath);

        if (archetype.SecondaryStat != archetype.PrimaryStat)
        {
            OrPath secondaryPath = CreateOrPathForStat(archetype.SecondaryStat, scaledThreshold);
            requirement.OrPaths.Add(secondaryPath);
        }

        return requirement;
    }

    /// <summary>
    /// Create an OrPath with the appropriate explicit stat property set.
    /// Uses Explicit Property Principle - each stat has its own named property.
    /// </summary>
    public static OrPath CreateOrPathForStat(PlayerStatType stat, int threshold)
    {
        OrPath path = new OrPath { Label = $"{stat} {threshold}+" };

        switch (stat)
        {
            case PlayerStatType.Insight:
                path.InsightRequired = threshold;
                break;
            case PlayerStatType.Rapport:
                path.RapportRequired = threshold;
                break;
            case PlayerStatType.Authority:
                path.AuthorityRequired = threshold;
                break;
            case PlayerStatType.Diplomacy:
                path.DiplomacyRequired = threshold;
                break;
            case PlayerStatType.Cunning:
                path.CunningRequired = threshold;
                break;
        }

        return path;
    }

    private static string GenerateStatGatedActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Assert authority and take command",
            SituationArchetypeType.Negotiation => "Negotiate favorable terms",
            SituationArchetypeType.Investigation => "Deduce the solution through analysis",
            SituationArchetypeType.SocialManeuvering => "Read the social dynamics and navigate skillfully",
            SituationArchetypeType.Crisis => "Take decisive action with expertise",
            SituationArchetypeType.ServiceTransaction => "Use your expertise",
            SituationArchetypeType.AccessControl => "Present credentials",
            SituationArchetypeType.InformationGathering => "Ask the right questions",
            SituationArchetypeType.SkillDemonstration => "Demonstrate your competence",
            SituationArchetypeType.ReputationChallenge => "Defend your honor",
            SituationArchetypeType.EmergencyAid => "Apply expert treatment",
            SituationArchetypeType.AdministrativeProcedure => "Navigate bureaucracy skillfully",
            SituationArchetypeType.TradeDispute => "Leverage your position",
            SituationArchetypeType.CulturalFauxPas => "Apologize gracefully",
            SituationArchetypeType.Recruitment => "Negotiate terms",
            SituationArchetypeType.RestPreparation => "Optimize rest conditions",
            SituationArchetypeType.EnteringPrivateSpace => "Thoroughly inspect and optimize the space",
            SituationArchetypeType.DepartingPrivateSpace => "Systematically prepare and check everything",
            _ => "Use your expertise"
        };
    }

    private static string GenerateMoneyActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Pay off the opposition",
            SituationArchetypeType.Negotiation => "Pay the premium price",
            SituationArchetypeType.Investigation => "Hire an expert or pay for information",
            SituationArchetypeType.SocialManeuvering => "Offer a generous gift",
            SituationArchetypeType.Crisis => "Pay for emergency solution",
            SituationArchetypeType.ServiceTransaction => "Pay the asking price",
            SituationArchetypeType.AccessControl => "Bribe the gatekeeper",
            SituationArchetypeType.InformationGathering => "Buy the information",
            SituationArchetypeType.SkillDemonstration => "Hire someone to vouch",
            SituationArchetypeType.ReputationChallenge => "Offer compensation",
            SituationArchetypeType.EmergencyAid => "Pay for premium care",
            SituationArchetypeType.AdministrativeProcedure => "Expedite with payment",
            SituationArchetypeType.TradeDispute => "Offer settlement",
            SituationArchetypeType.CulturalFauxPas => "Offer gift as amends",
            SituationArchetypeType.Recruitment => "Buy time",
            SituationArchetypeType.RestPreparation => "Use comfort items for better rest",
            SituationArchetypeType.EnteringPrivateSpace => "Request comfort amenities",
            SituationArchetypeType.DepartingPrivateSpace => "Leave generous gratuity for staff",
            _ => "Pay to resolve"
        };
    }

    private static string GenerateChallengeActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Attempt a physical confrontation",
            SituationArchetypeType.Negotiation => "Engage in complex debate",
            SituationArchetypeType.Investigation => "Work through the puzzle systematically",
            SituationArchetypeType.SocialManeuvering => "Make a bold social gambit",
            SituationArchetypeType.Crisis => "Risk everything on a desperate gambit",
            SituationArchetypeType.ServiceTransaction => "Attempt to bargain",
            SituationArchetypeType.AccessControl => "Force your way through",
            SituationArchetypeType.InformationGathering => "Investigate on your own",
            SituationArchetypeType.SkillDemonstration => "Attempt without preparation",
            SituationArchetypeType.ReputationChallenge => "Challenge to duel",
            SituationArchetypeType.EmergencyAid => "Risk improvised treatment",
            SituationArchetypeType.AdministrativeProcedure => "Navigate red tape",
            SituationArchetypeType.TradeDispute => "Escalate to arbitration",
            SituationArchetypeType.CulturalFauxPas => "Defend your action",
            SituationArchetypeType.Recruitment => "Counter-offer boldly",
            SituationArchetypeType.RestPreparation => "Force yourself to relax despite anxiety",
            SituationArchetypeType.EnteringPrivateSpace => "Push through discomfort mentally",
            SituationArchetypeType.DepartingPrivateSpace => "Force yourself to leave promptly",
            _ => "Accept the challenge"
        };
    }

    private static string GenerateFallbackActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Back down and submit",
            SituationArchetypeType.Negotiation => "Accept unfavorable terms",
            SituationArchetypeType.Investigation => "Give up and move on",
            SituationArchetypeType.SocialManeuvering => "Exit awkwardly",
            SituationArchetypeType.Crisis => "Flee the situation",
            SituationArchetypeType.ServiceTransaction => "Leave without service",
            SituationArchetypeType.AccessControl => "Turn back",
            SituationArchetypeType.InformationGathering => "Move on without answers",
            SituationArchetypeType.SkillDemonstration => "Admit lack of skill",
            SituationArchetypeType.ReputationChallenge => "Apologize and back down",
            SituationArchetypeType.EmergencyAid => "Do nothing",
            SituationArchetypeType.AdministrativeProcedure => "Abandon the process",
            SituationArchetypeType.TradeDispute => "Accept the loss",
            SituationArchetypeType.CulturalFauxPas => "Ignore and act oblivious",
            SituationArchetypeType.Recruitment => "Refuse bluntly",
            SituationArchetypeType.RestPreparation => "Collapse from exhaustion immediately",
            SituationArchetypeType.EnteringPrivateSpace => "Collapse immediately without preparation",
            SituationArchetypeType.DepartingPrivateSpace => "Rush out without proper preparation",
            _ => "Accept poor outcome"
        };
    }

    /// <summary>
    /// Generate service_negotiation choices with context-aware scaling.
    /// Scales stat thresholds by NPC.Demeanor and coin costs by Service.Quality.
    /// Returns 4 choices with EMPTY Consequence (SceneArchetypeCatalog enriches them).
    /// </summary>
    private static List<ChoiceTemplate> GenerateServiceNegotiationChoices(
        SituationArchetype archetype,
        string situationTemplateId,
        GenerationContext context)
    {
        // Adjust stat threshold by NPC demeanor
        int scaledStatThreshold = context.NpcDemeanor switch
        {
            NPCDemeanor.Friendly => archetype.StatThreshold - 2,  // Easier
            NPCDemeanor.Neutral => archetype.StatThreshold,       // Baseline
            NPCDemeanor.Hostile => archetype.StatThreshold + 2,   // Harder
            _ => archetype.StatThreshold
        };

        // Adjust coin cost by quality (universal property)
        int scaledCoinCost = context.Quality switch
        {
            Quality.Basic => archetype.CoinCost - 3,    // 2 coins (5-3)
            Quality.Standard => archetype.CoinCost,     // 5 coins
            Quality.Premium => archetype.CoinCost + 5,  // 10 coins (5+5)
            Quality.Luxury => archetype.CoinCost + 10,  // 15 coins (5+10)
            _ => archetype.CoinCost
        };

        List<ChoiceTemplate> choices = new List<ChoiceTemplate>();

        // Choice 1: Rapport-gated (free if you have relationship)
        CompoundRequirement rapportReq = new CompoundRequirement();
        rapportReq.OrPaths.Add(CreateOrPathForStat(PlayerStatType.Rapport, scaledStatThreshold));

        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_stat",
            PathType = ChoicePathType.InstantSuccess,  // Stat-gated instant success
            ActionTextTemplate = "Leverage your rapport",
            RequirementFormula = rapportReq,
            Consequence = new Consequence(),  // Empty, enriched by scene archetype
            ActionType = ChoiceActionType.Instant
        });

        // Choice 2: Pay coins (scaled by quality)
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_money",
            PathType = ChoicePathType.InstantSuccess,  // Money-gated instant success
            ActionTextTemplate = $"Pay {scaledCoinCost} coins for the service",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence { Coins = -scaledCoinCost },  // Empty, enriched by scene archetype
            ActionType = ChoiceActionType.Instant
        });

        // Choice 3: Challenge (negotiate better terms)
        // Create consequence first, then derive requirement from it (Sir Brante dual-nature encapsulated)
        Consequence negotiationChallengeConsequence = archetype.ResolveCost > 0
            ? new Consequence { Resolve = -archetype.ResolveCost }
            : new Consequence();

        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_challenge",
            PathType = ChoicePathType.Challenge,
            ActionTextTemplate = "Attempt to negotiate better terms",
            RequirementFormula = CompoundRequirement.CreateForConsequence(negotiationChallengeConsequence),
            Consequence = negotiationChallengeConsequence,
            ActionType = ChoiceActionType.StartChallenge,
            ChallengeType = archetype.ChallengeType,
            DeckId = archetype.DeckId
        });

        // Choice 4: Decline
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_fallback",
            PathType = ChoicePathType.Fallback,  // Fallback path
            ActionTextTemplate = "Politely decline",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence(),
            ActionType = ChoiceActionType.Instant
        });

        return choices;
    }

    /// <summary>
    /// Generate service_execution_rest choices with context-aware restoration scaling.
    /// Returns 4 rest choices that all advance to next morning.
    /// Restoration amounts scale by Environment.Quality using tier-based explicit values.
    /// </summary>
    private static List<ChoiceTemplate> GenerateServiceExecutionRestChoices(
        string situationTemplateId,
        GenerationContext context)
    {
        // Tier-based restoration values (Basic/Standard/Premium)
        // Balanced restoration: moderate recovery for all resources
        int balancedHealth = context.Environment switch
        {
            EnvironmentQuality.Basic => 15,
            EnvironmentQuality.Standard => 30,
            EnvironmentQuality.Premium => 45,
            _ => 30
        };
        int balancedStamina = context.Environment switch
        {
            EnvironmentQuality.Basic => 15,
            EnvironmentQuality.Standard => 30,
            EnvironmentQuality.Premium => 45,
            _ => 30
        };
        int balancedFocus = context.Environment switch
        {
            EnvironmentQuality.Basic => 10,
            EnvironmentQuality.Standard => 21,
            EnvironmentQuality.Premium => 31,
            _ => 21
        };

        // Physical focus: high health, low focus
        int physicalHealth = context.Environment switch
        {
            EnvironmentQuality.Basic => 25,
            EnvironmentQuality.Standard => 50,
            EnvironmentQuality.Premium => 75,
            _ => 50
        };
        int physicalStamina = context.Environment switch
        {
            EnvironmentQuality.Basic => 10,
            EnvironmentQuality.Standard => 20,
            EnvironmentQuality.Premium => 30,
            _ => 20
        };
        int physicalFocus = context.Environment switch
        {
            EnvironmentQuality.Basic => 3,
            EnvironmentQuality.Standard => 7,
            EnvironmentQuality.Premium => 10,
            _ => 7
        };

        // Mental focus: low health, high focus
        int mentalHealth = context.Environment switch
        {
            EnvironmentQuality.Basic => 5,
            EnvironmentQuality.Standard => 10,
            EnvironmentQuality.Premium => 15,
            _ => 10
        };
        int mentalStamina = context.Environment switch
        {
            EnvironmentQuality.Basic => 10,
            EnvironmentQuality.Standard => 20,
            EnvironmentQuality.Premium => 30,
            _ => 20
        };
        int mentalFocus = context.Environment switch
        {
            EnvironmentQuality.Basic => 17,
            EnvironmentQuality.Standard => 35,
            EnvironmentQuality.Premium => 52,
            _ => 35
        };

        // Special: balanced + slight bonus + buff
        int specialHealth = context.Environment switch
        {
            EnvironmentQuality.Basic => 13,
            EnvironmentQuality.Standard => 27,
            EnvironmentQuality.Premium => 40,
            _ => 27
        };
        int specialStamina = context.Environment switch
        {
            EnvironmentQuality.Basic => 13,
            EnvironmentQuality.Standard => 27,
            EnvironmentQuality.Premium => 40,
            _ => 27
        };
        int specialFocus = context.Environment switch
        {
            EnvironmentQuality.Basic => 10,
            EnvironmentQuality.Standard => 21,
            EnvironmentQuality.Premium => 31,
            _ => 21
        };

        List<ChoiceTemplate> choices = new List<ChoiceTemplate>();

        // Choice 1: Balanced restoration
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_balanced",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = "Sleep peacefully",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence
            {
                Health = balancedHealth,
                Stamina = balancedStamina,
                Focus = balancedFocus,
                AdvanceToDay = DayAdvancement.NextDay,
                AdvanceToBlock = TimeBlocks.Morning
            },
            ActionType = ChoiceActionType.Instant
        });

        // Choice 2: Physical focus
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_physical",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = "Rest deeply",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence
            {
                Health = physicalHealth,
                Stamina = physicalStamina,
                Focus = physicalFocus,
                AdvanceToDay = DayAdvancement.NextDay,
                AdvanceToBlock = TimeBlocks.Morning
            },
            ActionType = ChoiceActionType.Instant
        });

        // Choice 3: Mental focus
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_mental",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = "Meditate before sleeping",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence
            {
                Health = mentalHealth,
                Stamina = mentalStamina,
                Focus = mentalFocus,
                AdvanceToDay = DayAdvancement.NextDay,
                AdvanceToBlock = TimeBlocks.Morning
            },
            ActionType = ChoiceActionType.Instant
        });

        // Choice 4: Special (balanced + buff)
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_special",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = "Dream vividly",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence
            {
                Health = specialHealth,
                Stamina = specialStamina,
                Focus = specialFocus,
                AdvanceToDay = DayAdvancement.NextDay,
                AdvanceToBlock = TimeBlocks.Morning,
                StateApplications = new List<StateApplication>
            {
                new StateApplication
                {
                    StateType = StateType.Inspired,
                    Apply = true,
                    DurationSegments = 4
                }
            }
            },
            ActionType = ChoiceActionType.Instant
        });

        return choices;
    }

    /// <summary>
    /// Generate service_departure choices (only 2, not 4).
    /// Universal buff granted for careful departure (Focused).
    /// Returns choices with PARTIAL Consequence (SceneArchetypeCatalog adds cleanup).
    /// </summary>
    private static List<ChoiceTemplate> GenerateServiceDepartureChoices(
        string situationTemplateId,
        GenerationContext context)
    {
        // Universal buff for careful preparation (applies to all activity types)
        StateType buffType = StateType.Focused;

        List<ChoiceTemplate> choices = new List<ChoiceTemplate>();

        // Choice 1: Leave immediately (no cost, no buff)
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_immediate",
            PathType = ChoicePathType.Fallback,  // Quick exit, minimal benefit
            ActionTextTemplate = "Leave immediately",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence(),  // Empty, enriched with cleanup by scene archetype
            ActionType = ChoiceActionType.Instant
        });

        // Choice 2: Gather carefully (costs 1 segment, grants buff)
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_careful",
            PathType = ChoicePathType.InstantSuccess,  // Careful preparation, grants buff
            ActionTextTemplate = "Gather your belongings carefully",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence
            {
                TimeSegments = 1,
                StateApplications = new List<StateApplication>
            {
                new StateApplication
                {
                    StateType = buffType,
                    Apply = true,
                    DurationSegments = 4
                }
            }
            },
            ActionType = ChoiceActionType.Instant
        });

        return choices;
    }
}
