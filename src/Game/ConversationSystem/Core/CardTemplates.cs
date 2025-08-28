using System;

/// <summary>
/// Categorical card template types - frontend maps these to actual text
/// </summary>
public enum CardTemplateType
{
    // Generic templates
    SimpleGreeting,
    CasualInquiry,
    ActiveListening,
    
    // Trust templates
    OfferHelp,
    SharePersonal,
    MakePromise,
    ExpressEmpathy,
    
    // Commerce templates  
    DiscussBusiness,
    ProposeDeal,
    NegotiateTerms,
    OfferWork,
    
    // Status templates
    AcknowledgePosition,
    ShowRespect,
    DiscussPolitics,
    MentionConnections,
    
    // Shadow templates
    ShareSecret,
    ImplyKnowledge,
    OfferInformation,
    RequestDiscretion,
    
    // State change templates
    CalmnessAttempt,
    OpeningUp,
    ShowingTension,
    BecomingEager,
    GuardedApproach,
    WarmGreeting,
    TenseComment,
    EagerEngagement,
    OverwhelmedResponse,
    DesperatePlea,
    
    // Crisis templates
    DesperateRequest,
    UrgentPlea,
    LastChance,
    
    // Letter/Obligation templates
    MentionLetter,
    DeliverLetter,
    DiscussObligation,
    NegotiateDeadline,
    
    // Observation templates
    MentionObservation,
    ShareInformation,
    ProvideContext,
    ShareUrgentNews,
    MentionOpportunity,
    HintAtSecret,
    ExpressCuriosity,
    SuggestAction,
    MakeCasualObservation,
    
    // Additional crisis templates
    MakeDesperatePromise,
    OfferEverything,
    TakeImmediateAction,
    CalmTheSituation,
    ActDecisively,
    
    // Additional state templates
    ProvideReassurance,
    AskDirectQuestion,
    RevealSecret,
    ExpressVulnerability,
    
    // Exchange templates
    Exchange,
    ExchangeOffer,      // The display-only offer card from NPC
    ExchangeAccept,     // Player's accept response card
    ExchangeDecline,    // Player's decline response card
    SimpleExchange,     // Simple instant trade template
    
    // Special templates
    GoalCard,           // Goal card template
    ObservationShare,   // Observation sharing template
    
    // Additional comfort templates
    OfferAssistance,    // Offer to help with problems
    PersonalStory,      // Share personal experience
    CalmingResponse     // Attempt to calm someone
}

/// <summary>
/// Context data for card template - strongly typed categorical data
/// </summary>
public class CardContext
{
    /// <summary>
    /// NPC personality driving the card
    /// </summary>
    public PersonalityType Personality { get; init; }
    
    /// <summary>
    /// Current emotional state
    /// </summary>
    public EmotionalState EmotionalState { get; init; }
    
    /// <summary>
    /// Urgency level (0-3)
    /// </summary>
    public int UrgencyLevel { get; init; }
    
    /// <summary>
    /// Whether this relates to a deadline
    /// </summary>
    public bool HasDeadline { get; init; }
    
    /// <summary>
    /// Minutes until deadline if applicable
    /// </summary>
    public int? MinutesUntilDeadline { get; init; }
    
    /// <summary>
    /// Observation type if observation card
    /// </summary>
    public ObservationType? ObservationType { get; init; }
    
    /// <summary>
    /// Specific letter ID if letter-related
    /// </summary>
    public string LetterId { get; init; }
    
    /// <summary>
    /// Target NPC for observation/letter
    /// </summary>
    public string TargetNpcId { get; init; }
    
    /// <summary>
    /// NPC name for display
    /// </summary>
    public string NPCName { get; init; }
    
    /// <summary>
    /// NPC personality for exchange cards
    /// </summary>
    public PersonalityType NPCPersonality { get; init; }
    
    /// <summary>
    /// Exchange card data for exchange cards
    /// </summary>
    public ExchangeCard ExchangeData { get; init; }
    
    /// <summary>
    /// Observation ID if observation card
    /// </summary>
    public string ObservationId { get; init; }
    
    /// <summary>
    /// Observation text for display
    /// </summary>
    public string ObservationText { get; init; }
    
    /// <summary>
    /// Observation description for narrative
    /// </summary>
    public string ObservationDescription { get; init; }
    
    /// <summary>
    /// Exchange name for display (e.g., "Buy Travel Provisions")
    /// </summary>
    public string ExchangeName { get; init; }
    
    /// <summary>
    /// Exchange cost display (e.g., "3 Coins")
    /// </summary>
    public string ExchangeCost { get; init; }
    
    /// <summary>
    /// Exchange reward display (e.g., "Hunger = 0")
    /// </summary>
    public string ExchangeReward { get; init; }
    
    /// <summary>
    /// Observation decay state (Fresh/Stale/Expired)
    /// </summary>
    public ObservationDecayState? ObservationDecayState { get; init; }
    
    /// <summary>
    /// Observation decay description for UI (e.g., "Fresh (1.2h remaining)")
    /// </summary>
    public string ObservationDecayDescription { get; init; }
    
    /// <summary>
    /// Special flag for Crisis letters that generate letters on success
    /// </summary>
    public bool GeneratesLetterOnSuccess { get; init; }
    
    /// <summary>
    /// Flag for offer cards that can't be selected (display only)
    /// </summary>
    public bool IsOfferCard { get; init; }
    
    /// <summary>
    /// Whether this card grants a token on success
    /// </summary>
    public bool GrantsToken { get; init; }
    
    /// <summary>
    /// Flag for accept response cards in exchanges
    /// </summary>
    public bool IsAcceptCard { get; init; }
    
    /// <summary>
    /// Flag for decline response cards in exchanges
    /// </summary>
    public bool IsDeclineCard { get; init; }
    
    /// <summary>
    /// Reference to the original offer card ID for response cards
    /// </summary>
    public string OfferCardId { get; init; }
    
    /// <summary>
    /// Custom text for special cards
    /// </summary>
    public string CustomText { get; init; }
    
    /// <summary>
    /// Letter details for delivery cards
    /// </summary>
    public string LetterDetails { get; init; }
    
    /// <summary>
    /// Valid states for goal cards (which states allow this goal)
    /// </summary>
    public List<EmotionalState> ValidStates { get; init; } = new();
    
    /// <summary>
    /// Exchange offer for exchange cards (e.g., "3 food")
    /// </summary>
    public string ExchangeOffer { get; init; }
    
    /// <summary>
    /// Exchange request for exchange cards (e.g., "5 coins")
    /// </summary>
    public string ExchangeRequest { get; init; }
    
    /// <summary>
    /// Observation location for observation cards
    /// </summary>
    public string ObservationLocation { get; init; }
    
    /// <summary>
    /// Observation spot for observation cards
    /// </summary>
    public string ObservationSpot { get; init; }
}