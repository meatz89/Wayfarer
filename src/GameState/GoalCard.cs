using System;

public class GoalCard : ConversationCard
{
    public GoalType GoalType { get; set; }
    public ObligationType CreatesObligation { get; set; }
    public SuccessTerms SuccessTerms { get; set; }
    public FailureTerms FailureTerms { get; set; }

    // Goal-specific properties
    public string GoalContext { get; set; } // Additional context for the goal
    public bool RequiresToken { get; set; } // Whether a specific token is needed
    public TokenType? RequiredToken { get; set; } // Which token is required

    public GoalCard()
    {
        Weight = 5; // Can be 5 or 6
        Difficulty = Difficulty.VeryHard; // 40%
        // Goal cards have both Fleeting and Opportunity properties
        Properties.Add(CardProperty.Fleeting);
        Properties.Add(CardProperty.Opportunity);
        // Set exhaust effect to end conversation
        ExhaustEffect = new CardEffect
        {
            Type = CardEffectType.EndConversation,
            Value = "goal_exhausted"
        };
    }

    // Helper to set weight within valid range
    public void SetWeight(int weight)
    {
        if (weight >= 5 && weight <= 6)
        {
            Weight = weight;
        }
        else
        {
            Weight = 5; // Default to minimum valid weight
        }
    }

    // Create a goal card with specific parameters
    public static GoalCard CreateLetterDelivery(string id, string name, string destination, int payment)
    {
        return new GoalCard
        {
            Id = id,
            Name = name,
            GoalType = GoalType.Letter,
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