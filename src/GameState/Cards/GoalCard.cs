using System.Collections.Generic;

/// <summary>
/// GoalCard - tactical layer victory condition
/// Universal across all three challenge types (Social/Mental/Physical)
/// Defines WHEN victory occurs (momentum threshold and rewards)
/// Defined inline within goals (not separate reusable entities)
/// </summary>
public class GoalCard
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
    public GoalCardRewards Rewards { get; set; }

    /// <summary>
    /// Whether this goal card has been achieved
    /// </summary>
    public bool IsAchieved { get; set; } = false;
}

/// <summary>
/// Runtime data for CreateObligation reward
/// </summary>
public class CreateObligationReward
{
    public string PatronNpcId { get; set; }
    public string DestinationLocationId { get; set; }
    public List<string> RequiredGoalIds { get; set; } = new List<string>();
    public int DeadlineSegment { get; set; }
    public int RewardCoins { get; set; }
    public int RewardStoryTokens { get; set; }
}

/// <summary>
/// Runtime data for RouteSegmentUnlock reward
/// </summary>
public class RouteSegmentUnlock
{
    public string RouteId { get; set; }
    public int SegmentPosition { get; set; }
    public string PathId { get; set; }
}

/// <summary>
/// Rewards for achieving a goal card
/// Knowledge system eliminated - Understanding resource replaces Knowledge tokens
/// </summary>
public class GoalCardRewards
{
    public int? Coins { get; set; }
    public int? Progress { get; set; }
    public int? Breakthrough { get; set; }
    public string ObligationId { get; set; }
    public string Item { get; set; }

    // Cube rewards (strong typing, auto-applied to goal's context)
    public int? InvestigationCubes { get; set; }
    public int? StoryCubes { get; set; }
    public int? ExplorationCubes { get; set; }

    // Core Loop reward types
    public string EquipmentId { get; set; }
    public CreateObligationReward CreateObligationData { get; set; }
    public RouteSegmentUnlock RouteSegmentUnlock { get; set; }

    public ObstaclePropertyReduction ObstacleReduction { get; set; }
}
