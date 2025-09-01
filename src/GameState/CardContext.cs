
// Card context for special behaviors
public class CardContext
{
    public ExchangeData ExchangeData { get; set; }
    public PromiseCardData PromiseData { get; set; }
    public bool GeneratesLetterOnSuccess { get; set; }
    
    // Additional context properties  
    public string ExchangeRequest { get; set; }
    public string ObservationLocation { get; set; }
    public string ObservationSpot { get; set; }
    public string NPCName { get; set; }
    public PersonalityType Personality { get; set; }
    public PersonalityType NPCPersonality { get; set; }
    public EmotionalState EmotionalState { get; set; }
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
    public List<EmotionalState> ValidStates { get; set; }
    public ExchangeOffer ExchangeOffer { get; set; }
}
