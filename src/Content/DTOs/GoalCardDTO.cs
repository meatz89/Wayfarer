using System.Collections.Generic;

/// <summary>
/// DTO for GoalCards - tactical layer victory conditions
/// Defined inline within goals (not separate reusable entities)
/// Universal structure across all three challenge types
/// </summary>
public class GoalCardDTO
{
    /// <summary>
    /// Unique identifier for this goal card
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of this victory condition
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of this victory condition
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Universal threshold required to achieve this victory condition
    /// Interpretation depends on Goal.systemType:
    /// - Social: Momentum threshold
    /// - Mental: Progress threshold
    /// - Physical: Breakthrough threshold
    /// </summary>
    public int threshold { get; set; }

    /// <summary>
    /// Rewards granted when this victory condition is achieved
    /// </summary>
    public GoalCardRewardsDTO Rewards { get; set; }
}

/// <summary>
/// DTO for goal card rewards
/// </summary>
public class GoalCardRewardsDTO
{
    public int? Coins { get; set; }
    public int? Progress { get; set; }
    public int? Breakthrough { get; set; }
    public string ObligationId { get; set; }
    public string Item { get; set; }
    public List<string> Knowledge { get; set; } = new List<string>();
    public Dictionary<string, int> Tokens { get; set; } = new Dictionary<string, int>();
}
