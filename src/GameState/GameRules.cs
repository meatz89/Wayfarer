public class GameRules
{
    public static GameRules StandardRuleset = new GameRules
    {
    };

    public Professions PlayerArchetype = Professions.Soldier;
    public string Background = string.Empty;
    public string Name = "Wayfarer";

    // Resource Competition: Fixed Stamina Costs
    public const int STAMINA_COST_TRAVEL = 2;      // Per route segment
    public const int STAMINA_COST_WORK = 2;        // Physical labor
    public const int STAMINA_COST_DELIVER = 1;     // DeliveryObligation delivery
    public const int STAMINA_RECOVERY_REST = 3;    // Rest action recovery
    public const int STAMINA_RECOVERY_SLEEP = 6;   // Full night's sleep

    // Resource Competition: Fixed Hour Costs  
    public const int HOUR_COST_ACTION = 1;         // Most actions
    public const int HOUR_COST_TRAVEL = 2;         // Travel between locations
    public const int HOUR_COST_DEEP_ACTION = 2;    // Deep discussions, hard labor

    // Resource Competition: Token Thresholds
    public const int TOKENS_STRANGER_THRESHOLD = 0;    // No interaction
    public const int TOKENS_BASIC_THRESHOLD = 1;       // Basic letters offered (1-2 tokens)
    public const int TOKENS_QUALITY_THRESHOLD = 3;     // Quality letters offered (3-4 tokens)
    public const int TOKENS_PREMIUM_THRESHOLD = 5;     // Premium letters & routes (5+ tokens)

    // Standing Obligations: Crisis Card Rewards
    public const int CRISIS_CARD_TOKEN_REWARD = 3;     // Immediate token gain from crisis cards (US-8.1)
    public const int OBLIGATION_BREAKING_PENALTY = 5;  // Token loss for breaking obligations (US-8.3)

    // Conversation Comfort Thresholds
    public const double COMFORT_MAINTAIN_THRESHOLD = 0.5;    // Comfort ≥ Patience/2: Maintain relationship
    public const double COMFORT_LETTER_THRESHOLD = 1.0;      // Comfort ≥ Patience: DeliveryObligation becomes available
    public const double COMFORT_PERFECT_THRESHOLD = 1.5;     // Comfort ≥ Patience × 1.5: Perfect conversation bonus

    // Attention Refresh System (Epic 9)
    public const int ATTENTION_REFRESH_QUICK_DRINK_COST = 1;  // Quick drink: 1 coin = +1 attention
    public const int ATTENTION_REFRESH_FULL_MEAL_COST = 3;    // Full meal: 3 coins = +2 attention  
    public const int ATTENTION_REFRESH_QUICK_DRINK_POINTS = 1;
    public const int ATTENTION_REFRESH_FULL_MEAL_POINTS = 2;
    public const int ATTENTION_REFRESH_MAX_TOTAL = 7;         // Maximum attention possible (base 5 + 2 meal)
}
