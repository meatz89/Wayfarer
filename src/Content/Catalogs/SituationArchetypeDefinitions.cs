/// <summary>
/// Archetype definitions for SituationArchetypeCatalog.
/// Each method creates a SituationArchetype with defined mechanical properties.
///
/// COMPOSITION: Extracted from SituationArchetypeCatalog for file size compliance.
/// Called exclusively by SituationArchetypeCatalog.GetArchetype().
/// </summary>
internal static class SituationArchetypeDefinitions
{
    /// <summary>
    /// CONFRONTATION archetype
    /// Domain: Authority | Primary: Authority | Challenge: Physical
    /// When Used: Authority challenges, physical barriers, intimidation moments
    /// </summary>
    internal static SituationArchetype CreateConfrontation()
    {
        return new SituationArchetype
        {
            Type = SituationArchetypeType.Confrontation,
            Name = "Confrontation",
            Domain = Domain.Authority,
            PrimaryStat = PlayerStatType.Authority,
            SecondaryStat = PlayerStatType.Authority,
            StatThreshold = 3,
            CoinCost = 15,
            ChallengeType = TacticalSystemType.Physical,
            ChallengeDeckName = "physical_challenge",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Demanding
        };
    }

    /// <summary>
    /// NEGOTIATION archetype
    /// Domain: Economic | Primary: Diplomacy | Challenge: Mental
    /// When Used: Price disputes, deal-making, compromise seeking
    /// </summary>
    internal static SituationArchetype CreateNegotiation()
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
            ChallengeDeckName = "mental_challenge",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// INVESTIGATION archetype
    /// Domain: Mental | Primary: Insight | Challenge: Mental
    /// When Used: Mysteries, puzzles, information gathering, deduction
    /// </summary>
    internal static SituationArchetype CreateInvestigation()
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
            ChallengeDeckName = "mental_challenge",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// SOCIAL MANEUVERING archetype
    /// Domain: Social | Primary: Rapport | Challenge: Social
    /// When Used: Reputation management, relationship building, social hierarchy
    /// </summary>
    internal static SituationArchetype CreateSocialManeuvering()
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
            ChallengeDeckName = "friendly_chat",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// CRISIS archetype
    /// Domain: Physical | Primary: Authority | Challenge: Physical
    /// When Used: Emergencies, high-stakes moments, moral dilemmas, scene climaxes
    /// </summary>
    internal static SituationArchetype CreateCrisis()
    {
        return new SituationArchetype
        {
            Type = SituationArchetypeType.Crisis,
            Name = "Crisis",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.Authority,
            SecondaryStat = PlayerStatType.Insight,
            StatThreshold = 4,
            CoinCost = 25,
            ChallengeType = TacticalSystemType.Physical,
            ChallengeDeckName = "physical_challenge",
            ResolveCost = 10,
            FallbackTimeCost = 2,
            Intensity = ArchetypeIntensity.Demanding
        };
    }

    /// <summary>
    /// SERVICE TRANSACTION archetype
    /// Domain: Economic | Primary: None | Challenge: Mental
    /// When Used: Paying for services (lodging, food, healing, ferry passage)
    /// </summary>
    internal static SituationArchetype CreateServiceTransaction()
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
            ChallengeDeckName = "mental_challenge",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// ACCESS CONTROL archetype
    /// Domain: Authority | Primary: Authority | Challenge: Physical
    /// When Used: Getting past gatekeepers, locked doors, restricted areas
    /// </summary>
    internal static SituationArchetype CreateAccessControl()
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
            ChallengeDeckName = "physical_challenge",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// INFORMATION GATHERING archetype
    /// Domain: Social | Primary: Rapport | Challenge: Social
    /// When Used: Rumors, news, local knowledge, gossip networks
    /// </summary>
    internal static SituationArchetype CreateInformationGathering()
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
            ChallengeDeckName = "friendly_chat",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// SKILL DEMONSTRATION archetype
    /// Domain: Economic | Primary: Diplomacy | Challenge: Mental
    /// When Used: Proving competence, showing credentials, demonstrating expertise
    /// </summary>
    internal static SituationArchetype CreateSkillDemonstration()
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
            ChallengeDeckName = "mental_challenge",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// REPUTATION CHALLENGE archetype
    /// Domain: Social | Primary: Authority | Challenge: Social
    /// When Used: Someone questioning your standing, honor, or claims
    /// </summary>
    internal static SituationArchetype CreateReputationChallenge()
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
            ChallengeDeckName = "friendly_chat",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Demanding
        };
    }

    /// <summary>
    /// EMERGENCY AID archetype
    /// Domain: Physical | Primary: Insight | Challenge: Physical
    /// When Used: Medical crisis, immediate danger to others, rescue situations
    /// </summary>
    internal static SituationArchetype CreateEmergencyAid()
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
            ChallengeDeckName = "physical_challenge",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Demanding
        };
    }

    /// <summary>
    /// ADMINISTRATIVE PROCEDURE archetype
    /// Domain: Authority | Primary: Diplomacy | Challenge: Mental
    /// When Used: Bureaucracy, paperwork, permits, official processes
    /// </summary>
    internal static SituationArchetype CreateAdministrativeProcedure()
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
            ChallengeDeckName = "mental_challenge",
            ResolveCost = 5,
            FallbackTimeCost = 2,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// TRADE DISPUTE archetype
    /// Domain: Economic | Primary: Insight | Challenge: Mental
    /// When Used: Disagreement over goods, prices, quality, contract terms
    /// </summary>
    internal static SituationArchetype CreateTradeDispute()
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
            ChallengeDeckName = "mental_challenge",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// CULTURAL FAUX PAS archetype
    /// Domain: Social | Primary: Rapport | Challenge: Social
    /// When Used: Social blunder, tradition violation, etiquette breach
    /// </summary>
    internal static SituationArchetype CreateCulturalFauxPas()
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
            ChallengeDeckName = "desperate_request",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// RECRUITMENT archetype
    /// Domain: Social | Primary: Cunning | Challenge: Social
    /// When Used: Someone wants you to join, work for them, commit to cause
    /// </summary>
    internal static SituationArchetype CreateRecruitment()
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
            ChallengeDeckName = "desperate_request",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// REST PREPARATION archetype
    /// Domain: Physical | Primary: Insight | Challenge: Mental
    /// When Used: Preparing to rest in private space, optimizing recovery
    /// </summary>
    internal static SituationArchetype CreateRestPreparation()
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
            ChallengeDeckName = "mental_challenge",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// ENTERING PRIVATE SPACE archetype
    /// Domain: Physical | Primary: Insight | Challenge: Mental
    /// When Used: First entry into private room/space player has rented/unlocked
    /// </summary>
    internal static SituationArchetype CreateEnteringPrivateSpace()
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
            ChallengeDeckName = "mental_challenge",
            ResolveCost = 5,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// DEPARTING PRIVATE SPACE archetype
    /// Domain: Physical | Primary: Insight | Challenge: Mental
    /// When Used: Leaving lodging after rest
    /// </summary>
    internal static SituationArchetype CreateDepartingPrivateSpace()
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
            ChallengeDeckName = "mental_challenge",
            ResolveCost = 5,
            FallbackTimeCost = 2,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// SERVICE NEGOTIATION archetype (REUSABLE)
    /// Domain: Economic | Primary: Rapport | Challenge: Social
    /// When Used: Negotiating access to any service (lodging, bathing, healing)
    /// EXPENSE archetype - player pays coins for service
    /// </summary>
    internal static SituationArchetype CreateServiceNegotiation()
    {
        return new SituationArchetype
        {
            Type = SituationArchetypeType.ServiceNegotiation,
            Name = "Service Negotiation",
            Domain = Domain.Economic,
            PrimaryStat = PlayerStatType.Rapport,
            SecondaryStat = PlayerStatType.Diplomacy,
            StatThreshold = 3,
            CoinCost = 5,
            CoinReward = 0,
            ChallengeType = TacticalSystemType.Social,
            ChallengeDeckName = "friendly_chat",
            ResolveCost = 5,
            FallbackTimeCost = 0,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// CONTRACT NEGOTIATION archetype (REUSABLE)
    /// Domain: Economic | Primary: Diplomacy | Challenge: Social
    /// When Used: Accepting work contracts (delivery jobs, courier work)
    /// INCOME archetype - player earns coins from contract
    /// </summary>
    internal static SituationArchetype CreateContractNegotiation()
    {
        return new SituationArchetype
        {
            Type = SituationArchetypeType.ContractNegotiation,
            Name = "Contract Negotiation",
            Domain = Domain.Economic,
            PrimaryStat = PlayerStatType.Diplomacy,
            SecondaryStat = PlayerStatType.Insight,
            StatThreshold = 3,
            CoinCost = 0,
            CoinReward = 10,
            ChallengeType = TacticalSystemType.Social,
            ChallengeDeckName = "friendly_chat",
            ResolveCost = 5,
            FallbackTimeCost = 0,
            Intensity = ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// SERVICE EXECUTION REST archetype (REUSABLE)
    /// Domain: Physical | Primary: None (Recovery) | Challenge: Physical
    /// When Used: Using service in private space (lodging, bathing, healing)
    /// All choices succeed - different resource restoration options
    /// </summary>
    internal static SituationArchetype CreateServiceExecutionRest()
    {
        return new SituationArchetype
        {
            Type = SituationArchetypeType.ServiceExecutionRest,
            Name = "Service Execution (Rest)",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.None,
            SecondaryStat = PlayerStatType.None,
            StatThreshold = 0,
            CoinCost = 0,
            ChallengeType = TacticalSystemType.Physical,
            ChallengeDeckName = string.Empty,
            ResolveCost = 0,
            FallbackTimeCost = 0,
            Intensity = ArchetypeIntensity.Recovery
        };
    }

    /// <summary>
    /// SERVICE DEPARTURE archetype (REUSABLE)
    /// Domain: Physical | Primary: Insight | Challenge: Mental
    /// When Used: Leaving service space (lodging, bathing, healing)
    /// </summary>
    internal static SituationArchetype CreateServiceDeparture()
    {
        return new SituationArchetype
        {
            Type = SituationArchetypeType.ServiceDeparture,
            Name = "Service Departure",
            Domain = Domain.Physical,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Cunning,
            StatThreshold = 2,
            CoinCost = 0,
            ChallengeType = TacticalSystemType.Mental,
            ChallengeDeckName = "mental_challenge",
            ResolveCost = 0,
            FallbackTimeCost = 1,
            Intensity = ArchetypeIntensity.Recovery
        };
    }

    // ==================== PEACEFUL ARCHETYPES ====================

    /// <summary>
    /// MEDITATION AND REFLECTION archetype
    /// Domain: Mental | Primary: Insight | Challenge: Mental
    /// When Used: Player needs mental recovery, quiet contemplation
    /// Recovery-focused: No Resolve cost, no stat requirements
    /// </summary>
    internal static SituationArchetype CreateMeditationAndReflection()
    {
        return new SituationArchetype
        {
            Type = SituationArchetypeType.MeditationAndReflection,
            Name = "Meditation and Reflection",
            Domain = Domain.Mental,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Cunning,
            StatThreshold = 0,
            CoinCost = 0,
            ChallengeType = TacticalSystemType.Mental,
            ChallengeDeckName = string.Empty,
            ResolveCost = 0,
            FallbackTimeCost = 0,
            Intensity = ArchetypeIntensity.Recovery
        };
    }

    /// <summary>
    /// LOCAL CONVERSATION archetype
    /// Domain: Social | Primary: Rapport | Challenge: Social
    /// When Used: Player needs social recovery, casual interaction
    /// Recovery-focused: No Resolve cost, no stat requirements
    /// </summary>
    internal static SituationArchetype CreateLocalConversation()
    {
        return new SituationArchetype
        {
            Type = SituationArchetypeType.LocalConversation,
            Name = "Local Conversation",
            Domain = Domain.Social,
            PrimaryStat = PlayerStatType.Rapport,
            SecondaryStat = PlayerStatType.Diplomacy,
            StatThreshold = 0,
            CoinCost = 0,
            ChallengeType = TacticalSystemType.Social,
            ChallengeDeckName = string.Empty,
            ResolveCost = 0,
            FallbackTimeCost = 0,
            Intensity = ArchetypeIntensity.Recovery
        };
    }

    /// <summary>
    /// STUDY IN LIBRARY archetype
    /// Domain: Mental | Primary: Insight | Challenge: Mental
    /// When Used: Player needs intellectual recovery, knowledge acquisition
    /// Recovery-focused: No Resolve cost, no stat requirements
    /// </summary>
    internal static SituationArchetype CreateStudyInLibrary()
    {
        return new SituationArchetype
        {
            Type = SituationArchetypeType.StudyInLibrary,
            Name = "Study in Library",
            Domain = Domain.Mental,
            PrimaryStat = PlayerStatType.Insight,
            SecondaryStat = PlayerStatType.Authority,
            StatThreshold = 0,
            CoinCost = 0,
            ChallengeType = TacticalSystemType.Mental,
            ChallengeDeckName = string.Empty,
            ResolveCost = 0,
            FallbackTimeCost = 0,
            Intensity = ArchetypeIntensity.Recovery
        };
    }
}
