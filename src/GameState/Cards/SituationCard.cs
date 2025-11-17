/// <summary>
/// SituationCard - tactical layer victory condition
/// Universal across all three challenge types (Social/Mental/Physical)
/// Defines WHEN victory occurs (momentum threshold and rewards)
/// Defined inline within situations (not separate reusable entities)
/// </summary>
public class SituationCard
{
    // HIGHLANDER: NO Id property - SituationCard identified by object reference

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
    /// Interpretation depends on Situation.systemType:
    /// - Social: Momentum threshold
    /// - Mental: Progress threshold
    /// - Physical: Breakthrough threshold
    /// </summary>
    public int threshold { get; set; }

    /// <summary>
    /// Rewards granted when this victory condition is achieved
    /// </summary>
    public SituationCardRewards Rewards { get; set; }

    /// <summary>
    /// Whether this situation card has been achieved
    /// </summary>
    public bool IsAchieved { get; set; } = false;
}

/// <summary>
/// Runtime data for CreateObligation reward (Resource-Based Pattern)
/// PRINCIPLE 4: No boolean gates - grants StoryCubes to PatronNPC
/// Generic delivery situations become visible when NPC.StoryCubes >= threshold
/// </summary>
public class CreateObligationReward
{
    /// <summary>
    /// NPC receiving StoryCubes (creates obligation with this patron)
    /// HIGHLANDER: Object reference ONLY, no PatronNpcId
    /// </summary>
    public NPC PatronNpc { get; set; }

    /// <summary>
    /// Number of StoryCubes granted to patron (typically 2-5)
    /// Enables visibility of generic delivery situations when threshold met
    /// </summary>
    public int StoryCubesGranted { get; set; }

    /// <summary>
    /// Coins rewarded when delivery situation completes
    /// Stored on NPC for generic situation reward calculation
    /// </summary>
    public int RewardCoins { get; set; }
}

/// <summary>
/// Runtime data for RouteSegmentUnlock reward
/// </summary>
public class RouteSegmentUnlock
{
    // HIGHLANDER: Object reference ONLY, no RouteId
    public RouteOption Route { get; set; }
    public int SegmentPosition { get; set; }
    // HIGHLANDER: Object reference ONLY, no PathId
    public PathCard Path { get; set; }
}

/// <summary>
/// Rewards for achieving a situation card
/// Knowledge system eliminated - Understanding resource replaces Knowledge tokens
/// </summary>
public class SituationCardRewards
{
    public int? Coins { get; set; }
    public int? Progress { get; set; }
    public int? Breakthrough { get; set; }
    // HIGHLANDER: Object reference ONLY, no ObligationId
    public Obligation Obligation { get; set; }
    // HIGHLANDER: Object reference ONLY, no Item ID
    public Item Item { get; set; }

    // Cube rewards (strong typing, auto-applied to situation's context)
    public int? InvestigationCubes { get; set; }
    public int? StoryCubes { get; set; }
    public int? ExplorationCubes { get; set; }

    // Core Loop reward types
    public CreateObligationReward CreateObligationData { get; set; }
    public RouteSegmentUnlock RouteSegmentUnlock { get; set; }
}
