using System;

public class RequestCard : ConversationCard
{
    public RequestType RequestType { get; set; }
    public ObligationType CreatesObligation { get; set; }
    public SuccessTerms SuccessTerms { get; set; }
    public FailureTerms FailureTerms { get; set; }

    // Request-specific properties
    public string RequestContext { get; set; } // Additional context for the request
    public bool RequiresToken { get; set; } // Whether a specific token is needed
    public TokenType? RequiredToken { get; set; } // Which token is required
    
    // NEW: Rapport threshold for activation
    public int RapportThreshold { get; set; } = 5; // Default threshold

    public RequestCard()
    {
        // NEW: Request cards have NO focus cost
        Focus = 0;
        
        // NEW: Request cards ALWAYS succeed (100% success rate)
        Difficulty = Difficulty.VeryEasy; // Will override to 100% in play logic
        
        // NEW: Request cards are Persistent (never exhaust)
        Properties.Add(CardProperty.Persistent);
        
        // Request cards still end conversation when played
        SuccessEffect = new CardEffect
        {
            Type = CardEffectType.EndConversation,
            Value = "request_accepted"
        };
    }

    // Helper to check if request card can be played
    public bool IsPlayable(int currentRapport)
    {
        return currentRapport >= RapportThreshold;
    }

    // Create a request card with specific parameters
    public static RequestCard CreateLetterDelivery(string id, string name, string destination, int payment)
    {
        return new RequestCard
        {
            Id = id,
            Description = name,
            RequestType = RequestType.Letter,
            CreatesObligation = ObligationType.Promise,
            SuccessTerms = new SuccessTerms
            {
                DeadlineHours = 48,
                QueuePosition = 0, // Flexible
                Payment = payment,
                DestinationLocation = destination
            },
            FailureTerms = new FailureTerms
            {
                DeadlineHours = 24,
                QueuePosition = 1, // Forced first
                Payment = -payment / 2, // Penalty
                ReputationLoss = 1
            }
        };
    }
}