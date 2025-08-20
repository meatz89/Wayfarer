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
    ProvideContext
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
}