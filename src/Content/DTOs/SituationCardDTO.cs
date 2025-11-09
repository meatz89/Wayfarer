/// <summary>
/// DTO for SituationCards - tactical layer victory conditions
/// Defined inline within situations (not separate reusable entities)
/// Universal structure across all three challenge types
/// </summary>
public class SituationCardDTO
{
/// <summary>
/// Unique identifier for this situation card
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
/// Interpretation depends on Situation.systemType:
/// - Social: Momentum threshold
/// - Mental: Progress threshold
/// - Physical: Breakthrough threshold
/// </summary>
public int threshold { get; set; }

/// <summary>
/// Rewards granted when this victory condition is achieved
/// </summary>
public SituationCardRewardsDTO Rewards { get; set; }
}

/// <summary>
/// CreateObligation reward data (Resource-Based Pattern)
/// PRINCIPLE 4: No boolean gates - grants StoryCubes to PatronNPC
/// Generic delivery situations become visible when NPC.StoryCubes >= threshold
/// </summary>
public class CreateObligationRewardDTO
{
/// <summary>
/// NPC receiving StoryCubes (creates obligation with this patron)
/// </summary>
public string PatronNpcId { get; set; }

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
/// UnlockRouteSegment reward data - reveals hidden path in route segment
/// </summary>
public class RouteSegmentUnlockDTO
{
public string RouteId { get; set; }
public int SegmentPosition { get; set; }  // Which segment (1, 2, 3...)
public string PathId { get; set; }        // Which hidden path to reveal
}

/// <summary>
/// DTO for situation card rewards
/// Knowledge system eliminated - Understanding resource replaces Knowledge tokens
/// </summary>
public class SituationCardRewardsDTO
{
public int? Coins { get; set; }
public int? Progress { get; set; }
public int? Breakthrough { get; set; }
public string ObligationId { get; set; }
public string Item { get; set; }

// Cube rewards auto-applied to situation's context entity (Location/NPC/Route)
// Mental situations grant InvestigationCubes to their Location
// Social situations grant StoryCubes to their NPC
// Route completion grants ExplorationCubes to that Route
public int? InvestigationCubes { get; set; }  // +1-2, granted to situation's location
public int? StoryCubes { get; set; }          // +1-2, granted to situation's NPC
public int? ExplorationCubes { get; set; }    // +1, granted to situation's route

// Core Loop reward types (refinement spec lines 711-756)
public string EquipmentId { get; set; }  // GrantEquipment: Direct equipment grant
public CreateObligationRewardDTO CreateObligationData { get; set; }  // CreateObligation: NPC-commissioned work
public RouteSegmentUnlockDTO RouteSegmentUnlock { get; set; }  // UnlockRouteSegment: Reveal hidden path
}
