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

    public RequestCard()
    {
        Focus = 5; // Can be 5 or 6
        Difficulty = Difficulty.VeryHard; // 40%
        // Request cards have both Impulse and Opening properties
        Properties.Add(CardProperty.Impulse);
        Properties.Add(CardProperty.Opening);
        // Set exhaust effect to end conversation
        ExhaustEffect = new CardEffect
        {
            Type = CardEffectType.EndConversation,
            Value = "request_exhausted"
        };
    }

    // Helper to set focus within valid range
    public void SetFocus(int focus)
    {
        if (focus >= 5 && focus <= 6)
        {
            Focus = focus;
        }
        else
        {
            Focus = 5; // Default to minimum valid focus
        }
    }

    // Create a request card with specific parameters
    public static RequestCard CreateLetterDelivery(string id, string name, string destination, int payment)
    {
        return new RequestCard
        {
            Id = id,
            Name = name,
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