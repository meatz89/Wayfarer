/// <summary>
/// Resources player must pay to attempt goal
/// Transparent costs create resource competition and strategic choices
/// Board game pattern: X costs 5 wood, Y costs 3 wood - choose wisely
/// </summary>
public class GoalCosts
{
    /// <summary>
    /// Time segments consumed by this goal
    /// Competition: Limited time until deadline (obligation system)
    /// Strategic choice: Fast risky goal vs slow safe goal
    /// Example: Investigation A takes 3 segments, B takes 5 segments
    /// </summary>
    public int Time { get; set; } = 0;

    /// <summary>
    /// Focus consumed by Mental challenges
    /// Competition: Limited Focus pool (0-100), shared by ALL Mental goals
    /// Recovery: Rest actions, time passing
    /// Strategic choice: Multiple small investigations vs one large investigation
    /// Example: Investigation A costs 20 Focus, B costs 30 Focus
    /// Player has 50 Focus: Can do A+A, or A+B, or B+other
    /// </summary>
    public int Focus { get; set; } = 0;

    /// <summary>
    /// Stamina consumed by Physical challenges
    /// Competition: Limited Stamina pool (0-100), shared by ALL Physical goals
    /// Recovery: Rest actions, food, time passing
    /// Strategic choice: Force through obstacle vs find alternate route
    /// Example: Force Through costs 30 Stamina, Find Route costs 15 Focus
    /// Resource competition: Use Stamina or Focus?
    /// </summary>
    public int Stamina { get; set; } = 0;

    /// <summary>
    /// Coins spent on this goal (rare)
    /// Competition: Limited coins, needed for items, travel, obligations
    /// No recovery: Permanent spend
    /// Strategic choice: Pay for information vs investigate yourself
    /// Example: Bribe guard costs 10 Coins + 1 Time, Sneak past costs 20 Stamina + 3 Time
    /// </summary>
    public int Coins { get; set; } = 0;
}
