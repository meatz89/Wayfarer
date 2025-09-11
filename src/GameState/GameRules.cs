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

    // Resource Competition: Fixed Segment Costs  
    public const int SEGMENT_COST_ACTION = 2;         // Most actions (1 hour = 2 segments)
    public const int SEGMENT_COST_TRAVEL = 4;         // Travel between locations (2 hours = 4 segments)
    public const int SEGMENT_COST_DEEP_ACTION = 4;    // Deep discussions, hard labor (2 hours = 4 segments)

    // Resource Competition: Token Thresholds (for letters/rewards, not card availability)
    public const int TOKENS_STRANGER_THRESHOLD = 0;    // No interaction
    public const int TOKENS_BASIC_THRESHOLD = 1;       // Basic letters offered (1-2 tokens)
    public const int TOKENS_QUALITY_THRESHOLD = 3;     // Quality letters offered (3-4 tokens)
    public const int TOKENS_PREMIUM_THRESHOLD = 5;     // Premium letters & routes (5+ tokens)

    public const int OBLIGATION_BREAKING_PENALTY = 5;  // Token loss for breaking obligations (US-8.3)

    // Conversation Flow Thresholds
    public const double FLOW_MAINTAIN_THRESHOLD = 0.5;    // Flow ≥ Patience/2: Maintain relationship
    public const double FLOW_LETTER_THRESHOLD = 1.0;      // Flow ≥ Patience: DeliveryObligation becomes available
    public const double FLOW_PERFECT_THRESHOLD = 1.5;     // Flow ≥ Patience × 1.5: Perfect conversation bonus

    // Attention Refresh System (Epic 9)
    public const int ATTENTION_REFRESH_QUICK_DRINK_COST = 1;  // Quick drink: 1 coin = +1 attention
    public const int ATTENTION_REFRESH_FULL_MEAL_COST = 3;    // Full meal: 3 coins = +2 attention  
    public const int ATTENTION_REFRESH_QUICK_DRINK_POINTS = 1;
    public const int ATTENTION_REFRESH_FULL_MEAL_POINTS = 2;
    public const int ATTENTION_REFRESH_MAX_TOTAL = 12;        // Maximum attention possible (base 10 + 2 meal)

    // Conversation Depth Progression
    public const int DEPTH_ADVANCE_THRESHOLD_1 = 5;          // Surface to Personal (5 flow)
    public const int DEPTH_ADVANCE_THRESHOLD_2 = 10;         // Personal to Intimate (10 flow)
    public const int DEPTH_ADVANCE_THRESHOLD_3 = 15;         // Intimate to Deep (15 flow)
    public const int MAX_CONVERSATION_DEPTH = 3;             // Maximum depth level
}
