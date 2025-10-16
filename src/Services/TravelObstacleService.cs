using System;
using System.Linq;

public class TravelObstacleService
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;
    private readonly TimeManager _timeManager;

    public TravelObstacleService(GameWorld gameWorld, ItemRepository itemRepository, TimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _itemRepository = itemRepository;
        _timeManager = timeManager;
    }

    public List<ObstacleApproach> GetAvailableApproaches(Player player, TravelObstacle obstacle)
    {
        return obstacle.Approaches.Where(a => a.CanUseApproach(player, _itemRepository)).ToList();
    }

    public List<(ObstacleApproach approach, List<string> reasons)> GetUnavailableApproaches(Player player, TravelObstacle obstacle)
    {
        List<(ObstacleApproach, List<string>)> result = new List<(ObstacleApproach, List<string>)>();

        foreach (ObstacleApproach approach in obstacle.Approaches)
        {
            if (!approach.CanUseApproach(player, _itemRepository))
            {
                List<string> reasons = new List<string>();

                // EquipmentRequirement system eliminated - PRINCIPLE 4: Equipment reduces costs, never gates visibility
                // Knowledge system eliminated - no knowledge requirements

                if (player.Stamina < approach.StaminaRequired)
                {
                    reasons.Add($"Stamina: {approach.StaminaRequired} required");
                }

                foreach (KeyValuePair<PlayerStatType, int> statReq in approach.StatRequirements)
                {
                    int currentLevel = player.Stats.GetLevel(statReq.Key);
                    if (currentLevel < statReq.Value)
                    {
                        reasons.Add($"{statReq.Key}: Level {statReq.Value} required");
                    }
                }

                result.Add((approach, reasons));
            }
        }

        return result;
    }

    public ObstacleAttemptResult AttemptObstacle(Player player, TravelObstacle obstacle, ObstacleApproach approach, string routeId)
    {
        ObstacleAttemptResult result = new ObstacleAttemptResult
        {
            ObstacleId = obstacle.Id,
            ApproachId = approach.Id
        };

        Random rng = new Random();
        int roll = rng.Next(0, 100);
        bool success = roll < approach.SuccessProbability;

        ObstacleOutcome outcome = success ? approach.SuccessOutcome : approach.FailureOutcome;

        result.Success = success;
        result.Description = outcome.Description;

        player.Stamina -= outcome.StaminaCost;
        result.StaminaCost = outcome.StaminaCost;

        if (outcome.TimeSegmentCost > 0)
        {
            _timeManager.AdvanceSegments(outcome.TimeSegmentCost);
            result.TimeConsumed = outcome.TimeSegmentCost;
        }

        player.ModifyHealth(outcome.HealthChange);
        result.HealthChange = outcome.HealthChange;

        // Knowledge system eliminated - no knowledge rewards

        if (success && outcome.RouteImprovement != null)
        {
            ApplyRouteImprovement(routeId, outcome.RouteImprovement);
            result.RouteImproved = true;
            result.ImprovementDescription = outcome.RouteImprovement.Description;
        }

        return result;
    }

    private void ApplyRouteImprovement(string routeId, RouteImprovement improvement)
    {
        if (!_gameWorld.RouteImprovements.ContainsKey(routeId))
        {
            _gameWorld.RouteImprovements[routeId] = new List<RouteImprovement>();
        }

        _gameWorld.RouteImprovements[routeId].Add(improvement);
    }

    public List<RouteImprovement> GetRouteImprovements(string routeId)
    {
        if (_gameWorld.RouteImprovements.TryGetValue(routeId, out List<RouteImprovement> improvements))
        {
            return improvements;
        }
        return new List<RouteImprovement>();
    }
}

public class ObstacleAttemptResult
{
    public string ObstacleId { get; set; }
    public string ApproachId { get; set; }
    public bool Success { get; set; }
    public string Description { get; set; }

    public int TimeConsumed { get; set; }
    public int StaminaCost { get; set; }
    public int HealthChange { get; set; }

    // Knowledge system eliminated - Understanding resource replaces Knowledge tokens

    public bool RouteImproved { get; set; }
    public string ImprovementDescription { get; set; }
}
