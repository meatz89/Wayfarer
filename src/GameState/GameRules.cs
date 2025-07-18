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
    public const int STAMINA_COST_DELIVER = 1;     // Letter delivery
    public const int STAMINA_RECOVERY_REST = 3;    // Rest action recovery
    public const int STAMINA_RECOVERY_SLEEP = 6;   // Full night's sleep
    
    // Resource Competition: Fixed Hour Costs  
    public const int HOUR_COST_ACTION = 1;         // Most actions
    public const int HOUR_COST_TRAVEL = 2;         // Travel between locations
    public const int HOUR_COST_DEEP_ACTION = 2;    // Deep discussions, hard labor
    
    // Resource Competition: Token Thresholds
    public const int TOKENS_STRANGER_THRESHOLD = 0;    // No interaction
    public const int TOKENS_BASIC_THRESHOLD = 3;       // Basic letters offered
    public const int TOKENS_QUALITY_THRESHOLD = 5;     // Better letters offered
    public const int TOKENS_PREMIUM_THRESHOLD = 8;     // Premium letters & routes
}
