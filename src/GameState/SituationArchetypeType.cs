/// <summary>
/// Strongly-typed enum for situation archetype IDs.
/// Enables compile-time validation instead of runtime string matching.
/// Used by SituationArchetypeCatalog to generate choice structures.
/// </summary>
public enum SituationArchetypeType
{
    // ==================== CORE (5) ====================

    /// <summary>
    /// Authority challenges, physical barriers, intimidation moments
    /// Primary stat: Authority
    /// </summary>
    Confrontation,

    /// <summary>
    /// Price disputes, deal-making, compromise seeking
    /// Primary stat: Diplomacy
    /// </summary>
    Negotiation,

    /// <summary>
    /// Mysteries, puzzles, information gathering, deduction
    /// Primary stat: Insight
    /// </summary>
    Investigation,

    /// <summary>
    /// Reputation management, relationship building
    /// Primary stat: Rapport
    /// </summary>
    SocialManeuvering,

    /// <summary>
    /// Emergencies, high-stakes moments
    /// Higher thresholds, test player preparation
    /// </summary>
    Crisis,

    // ==================== EXPANDED (10) ====================

    /// <summary>
    /// Paying for services, economic exchanges
    /// </summary>
    ServiceTransaction,

    /// <summary>
    /// Gatekeepers, locked doors, restricted areas
    /// </summary>
    AccessControl,

    /// <summary>
    /// Rumors, gossip, local knowledge
    /// </summary>
    InformationGathering,

    /// <summary>
    /// Proving competence, showing credentials
    /// </summary>
    SkillDemonstration,

    /// <summary>
    /// Defending honor, responding to accusations
    /// </summary>
    ReputationChallenge,

    /// <summary>
    /// Medical crisis, rescue situations
    /// </summary>
    EmergencyAid,

    /// <summary>
    /// Bureaucracy, permits, official processes
    /// </summary>
    AdministrativeProcedure,

    /// <summary>
    /// Disagreements over goods, quality, terms
    /// </summary>
    TradeDispute,

    /// <summary>
    /// Social blunders, tradition violations
    /// </summary>
    CulturalFauxPas,

    /// <summary>
    /// Join requests, commitment decisions
    /// </summary>
    Recruitment,

    // ==================== SPECIALIZED (6) ====================

    /// <summary>
    /// Rest preparation: choosing how to rest
    /// </summary>
    RestPreparation,

    /// <summary>
    /// Entering private space: accessing secured areas
    /// </summary>
    EnteringPrivateSpace,

    /// <summary>
    /// Departing private space: leaving secured areas
    /// </summary>
    DepartingPrivateSpace,

    /// <summary>
    /// Service negotiation: securing services (inn booking, etc.)
    /// Specialized choice generation for commerce contexts
    /// </summary>
    ServiceNegotiation,

    /// <summary>
    /// Service execution rest: using secured service (staying at inn)
    /// Different recovery approaches
    /// </summary>
    ServiceExecutionRest,

    /// <summary>
    /// Service departure: leaving service venue
    /// Organize belongings, return key, etc.
    /// </summary>
    ServiceDeparture
}
