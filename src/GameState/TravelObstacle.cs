public enum ObstacleType
{
    Physical,
    Environmental,
    Social
}

public class TravelObstacle
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ObstacleType Type { get; set; }
    public string VenueId { get; set; }
    public string RouteId { get; set; }

    public List<ObstacleApproach> Approaches { get; set; } = new List<ObstacleApproach>();
}

public class ObstacleApproach
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public int StaminaRequired { get; set; }
    public Dictionary<PlayerStatType, int> StatRequirements { get; set; } = new Dictionary<PlayerStatType, int>();

    public ObstacleOutcome SuccessOutcome { get; set; }
    public ObstacleOutcome FailureOutcome { get; set; }

    public int SuccessProbability { get; set; } = 100;

    public bool CanUseApproach(Player player, ItemRepository itemRepository)
    {
        // Equipment system eliminated - no equipment gates
        // PRINCIPLE 4: Equipment reduces costs/difficulty, never gates visibility

        if (player.Stamina < StaminaRequired)
        {
            return false;
        }

        foreach (KeyValuePair<PlayerStatType, int> statReq in StatRequirements)
        {
            if (player.Stats.GetLevel(statReq.Key) < statReq.Value)
            {
                return false;
            }
        }

        return true;
    }
}

public class ObstacleOutcome
{
    public string Description { get; set; }

    public int TimeSegmentCost { get; set; } = 1;
    public int StaminaCost { get; set; }
    public int HealthChange { get; set; }

    // Knowledge system eliminated - no knowledge rewards

    public RouteImprovement RouteImprovement { get; set; }
}
