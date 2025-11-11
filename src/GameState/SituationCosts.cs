/// <summary>
/// Resources player must pay to attempt situation
/// Transparent costs create resource competition and strategic choices
/// Board game pattern: X costs 5 wood, Y costs 3 wood - choose wisely
/// </summary>
public class SituationCosts
{
    /// <summary>
    /// Resolve consumed by this situation (Sir Brante Willpower equivalent)
    /// Competition: Universal consumable (0-30) shared by ALL situation types
    /// Recovery: Slow, limited sources (rest, special events)
    /// Strategic choice: Which situations are worth spending precious Resolve on?
    /// Tier correlation: Tier 0 = 0 Resolve, Tier 1 = 0-3, Tier 2 = 5-8, Tier 3 = 10-15, Tier 4 = 18-25
    /// </summary>
    public int Resolve { get; set; } = 0;

    /// <summary>
    /// Time segments consumed by this situation
    /// Competition: Limited time until deadline (obligation system)
    /// Strategic choice: Fast risky situation vs slow safe situation
    /// Example: Obligation A takes 3 segments, B takes 5 segments
    /// </summary>
    public int Time { get; set; } = 0;

    /// <summary>
    /// Focus consumed by Mental challenges
    /// Competition: Limited Focus pool (0-100), shared by ALL Mental situations
    /// Recovery: Rest actions, time passing
    /// Strategic choice: Multiple small obligations vs one large obligation
    /// Example: Obligation A costs 20 Focus, B costs 30 Focus
    /// Player has 50 Focus: Can do A+A, or A+B, or B+other
    /// </summary>
    public int Focus { get; set; } = 0;

    /// <summary>
    /// Stamina consumed by Physical challenges
    /// Competition: Limited Stamina pool (0-100), shared by ALL Physical situations
    /// Recovery: Rest actions, food, time passing
    /// Strategic choice: Force through scene vs find alternate route
    /// Example: Force Through costs 30 Stamina, Find Route costs 15 Focus
    /// Resource competition: Use Stamina or Focus?
    /// </summary>
    public int Stamina { get; set; } = 0;

    /// <summary>
    /// Coins spent on this situation (rare)
    /// Competition: Limited coins, needed for items, travel, obligations
    /// No recovery: Permanent spend
    /// Strategic choice: Pay for information vs investigate yourself
    /// Example: Bribe guard costs 10 Coins + 1 Time, Sneak past costs 20 Stamina + 3 Time
    /// </summary>
    public int Coins { get; set; } = 0;
}
