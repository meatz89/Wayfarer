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
/// Runtime data for CreateObligation reward (Resource-Based Pattern)
/// PRINCIPLE 4: No boolean gates - grants StoryCubes to PatronNPC
/// Generic delivery goals become visible when NPC.StoryCubes >= threshold
/// </summary>
public class CreateObligationReward
{
    /// <summary>
    /// NPC receiving StoryCubes (creates obligation with this patron)
    /// </summary>
    public string PatronNpcId { get; set; }

    /// <summary>
    /// Number of StoryCubes granted to patron (typically 2-5)
    /// Enables visibility of generic delivery goals when threshold met
    /// </summary>
    public int StoryCubesGranted { get; set; }

    /// <summary>
    /// Coins rewarded when delivery goal completes
    /// Stored on NPC for generic goal reward calculation
    /// </summary>
    public int RewardCoins { get; set; }
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
