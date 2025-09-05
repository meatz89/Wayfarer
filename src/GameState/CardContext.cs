
// Card context for special behaviors
public class CardContext
{
    public ExchangeData ExchangeData { get; set; }
    public PromiseCardData PromiseData { get; set; }
    public bool GeneratesLetterOnSuccess { get; set; }
    public int RapportThreshold { get; set; } // For request/promise cards

    // Additional context properties  
    public string ExchangeRequest { get; set; }
    public string ObservationLocation { get; set; }
    public string ObservationSpot { get; set; }
    public string NPCName { get; set; }
    public PersonalityType Personality { get; set; }
    public PersonalityType NPCPersonality { get; set; }
    public ConnectionState ConnectionState { get; set; }
    public string UrgencyLevel { get; set; }
    public bool HasDeadline { get; set; }
    public int MinutesUntilDeadline { get; set; }
    public string ObservationType { get; set; }
    public string LetterId { get; set; }
    public string TargetNpcId { get; set; }
    public string ObservationId { get; set; }
    public string ObservationText { get; set; }
    public string ObservationDescription { get; set; }
    public string ExchangeName { get; set; }
    public Dictionary<ResourceType, int> ExchangeCost { get; set; }
    public Dictionary<ResourceType, int> ExchangeReward { get; set; }
    public string ObservationDecayState { get; set; }
    public string ObservationDecayDescription { get; set; }
    public bool IsOfferCard { get; set; }
    public bool GrantsToken { get; set; }
    public bool IsAcceptCard { get; set; }
    public bool IsDeclineCard { get; set; }
    public string OfferCardId { get; set; }
    public string CustomText { get; set; }
    public LetterDetails LetterDetails { get; set; }
    public List<ConnectionState> ValidStates { get; set; }
    public ExchangeOffer ExchangeOffer { get; set; }
}

public class LetterDetails
{
    public string RecipientId { get; set; }
    public string RecipientName { get; set; }
    public string Description { get; set; }
    public ConnectionType TokenType { get; set; }
    public StakeType Stakes { get; set; }
    public EmotionalFocus EmotionalFocus { get; set; }
}

public class EligibilityRequirements
{
    public List<ConnectionState> RequiredStates { get; set; } = new();
    public Dictionary<ConnectionType, int> RequiredTokens { get; set; } = new();
    public int MinTokens { get; set; }
}

public class NegotiationTerms
{
    public int BaseSuccessRate { get; set; }
    public TermDetails SuccessTerms { get; set; }
    public TermDetails FailureTerms { get; set; }
}

public class TermDetails
{
    public int DeadlineMinutes { get; set; }
    public int QueuePosition { get; set; }
    public int Payment { get; set; }
    public bool ForcesPositionOne { get; set; }
}

public class PromiseCardConfiguration
{
    public string CardId { get; set; }
    public EligibilityRequirements EligibilityRequirements { get; set; }
    public NegotiationTerms NegotiationTerms { get; set; }
    public LetterDetails LetterDetails { get; set; }
}
