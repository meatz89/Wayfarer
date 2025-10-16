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
/// CreateObligation reward data - creates NPCCommissioned obligation with deadline
/// </summary>
public class CreateObligationRewardDTO
{
    public string PatronNpcId { get; set; }
    public string DestinationLocationId { get; set; }
    public List<string> RequiredGoalIds { get; set; } = new List<string>();
    public int DeadlineSegment { get; set; }  // 8, 12, or 16
    public int RewardCoins { get; set; }
    public int RewardStoryTokens { get; set; }  // Story cubes for patron NPC
}

/// <summary>
/// UnlockRouteSegment reward data - reveals hidden path in route segment
/// </summary>
public class RouteSegmentUnlockDTO
{
    public string RouteId { get; set; }
    public int SegmentPosition { get; set; }  // Which segment (1, 2, 3...)
    public string PathId { get; set; }        // Which hidden path to reveal
}

/// <summary>
/// DTO for goal card rewards
/// Knowledge system eliminated - Understanding resource replaces Knowledge tokens
/// </summary>
public class GoalCardRewardsDTO
{
    public int? Coins { get; set; }
    public int? Progress { get; set; }
    public int? Breakthrough { get; set; }
    public string ObligationId { get; set; }
    public string Item { get; set; }

    // Cube rewards auto-applied to goal's context entity (Location/NPC/Route)
    // Mental goals grant InvestigationCubes to their Location
    // Social goals grant StoryCubes to their NPC
    // Route completion grants ExplorationCubes to that Route
    public int? InvestigationCubes { get; set; }  // +1-2, granted to goal's location
    public int? StoryCubes { get; set; }          // +1-2, granted to goal's NPC
    public int? ExplorationCubes { get; set; }    // +1, granted to goal's route

    // Core Loop reward types (refinement spec lines 711-756)
    public string EquipmentId { get; set; }  // GrantEquipment: Direct equipment grant
    public CreateObligationRewardDTO CreateObligationData { get; set; }  // CreateObligation: NPC-commissioned work
    public RouteSegmentUnlockDTO RouteSegmentUnlock { get; set; }  // UnlockRouteSegment: Reveal hidden path

    public ObstaclePropertyReductionDTO ObstacleReduction { get; set; }
}
