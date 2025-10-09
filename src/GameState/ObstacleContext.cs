using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Context object for travel obstacle UI rendering.
/// Contains all data needed to display and interact with obstacles.
/// </summary>
public class ObstacleContext
{
    /// <summary>
    /// The obstacle encountered.
    /// </summary>
    public TravelObstacle Obstacle { get; set; }

    /// <summary>
    /// Player instance for checking requirements.
    /// </summary>
    public Player Player { get; set; }

    /// <summary>
    /// Item repository for equipment requirement checks.
    /// </summary>
    public ItemRepository ItemRepository { get; set; }

    /// <summary>
    /// The route being traveled (optional context).
    /// </summary>
    public RouteOption Route { get; set; }

    /// <summary>
    /// Current Venue information.
    /// </summary>
    public LocationInfo LocationInfo { get; set; }

    /// <summary>
    /// Current time block.
    /// </summary>
    public TimeBlocks CurrentTimeBlock { get; set; }

    /// <summary>
    /// Gets approaches that are currently available to the player.
    /// Filters based on knowledge, equipment, stats, and stamina requirements.
    /// </summary>
    public List<ObstacleApproach> GetAvailableApproaches()
    {
        if (Obstacle?.Approaches == null)
            return new List<ObstacleApproach>();

        return Obstacle.Approaches
            .Where(a => a.CanUseApproach(Player, ItemRepository))
            .ToList();
    }

    /// <summary>
    /// Gets approaches that are unavailable with reasons why.
    /// </summary>
    public List<(ObstacleApproach approach, List<string> reasons)> GetUnavailableApproaches()
    {
        if (Obstacle?.Approaches == null)
            return new List<(ObstacleApproach, List<string>)>();

        List<(ObstacleApproach, List<string>)> result = new List<(ObstacleApproach, List<string>)>();

        foreach (ObstacleApproach approach in Obstacle.Approaches)
        {
            if (!approach.CanUseApproach(Player, ItemRepository))
            {
                List<string> reasons = GetApproachBlockReasons(approach);
                result.Add((approach, reasons));
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the reasons why an approach is blocked.
    /// </summary>
    public List<string> GetApproachBlockReasons(ObstacleApproach approach)
    {
        List<string> reasons = new List<string>();

        if (approach.KnowledgeRequirements != null && !approach.KnowledgeRequirements.MeetsRequirements(Player.Knowledge))
        {
            List<string> missing = approach.KnowledgeRequirements.GetMissingRequirements(Player.Knowledge);
            reasons.AddRange(missing.Select(m => $"Missing knowledge: {m}"));
        }

        if (approach.EquipmentRequirements != null && !approach.EquipmentRequirements.MeetsRequirements(Player, ItemRepository))
        {
            List<string> missing = approach.EquipmentRequirements.GetMissingRequirements(Player, ItemRepository);
            reasons.AddRange(missing.Select(m => $"Missing equipment: {m}"));
        }

        if (Player.Stamina < approach.StaminaRequired)
        {
            reasons.Add($"Stamina: {approach.StaminaRequired} required, {Player.Stamina} current");
        }

        foreach (KeyValuePair<PlayerStatType, int> statReq in approach.StatRequirements)
        {
            int currentLevel = Player.Stats.GetLevel(statReq.Key);
            if (currentLevel < statReq.Value)
            {
                reasons.Add($"{statReq.Key}: Level {statReq.Value} required, Level {currentLevel} current");
            }
        }

        return reasons;
    }

    /// <summary>
    /// Gets a preview of what an approach might lead to.
    /// Shows success/failure outcomes with probabilities.
    /// </summary>
    public ObstacleApproachPreview GetApproachPreview(ObstacleApproach approach)
    {
        if (approach == null)
            return null;

        return new ObstacleApproachPreview
        {
            ApproachId = approach.Id,
            ApproachName = approach.Name,
            CanUseApproach = approach.CanUseApproach(Player, ItemRepository),
            BlockReasons = GetApproachBlockReasons(approach),
            StaminaCost = approach.StaminaRequired,
            SuccessProbability = approach.SuccessProbability,
            SuccessOutcome = approach.SuccessOutcome != null ? new ObstacleOutcomePreview
            {
                Description = approach.SuccessOutcome.Description,
                TimeSegmentCost = approach.SuccessOutcome.TimeSegmentCost,
                StaminaCost = approach.SuccessOutcome.StaminaCost,
                HealthChange = approach.SuccessOutcome.HealthChange,
                KnowledgeGained = approach.SuccessOutcome.KnowledgeGained,
                HasRouteImprovement = approach.SuccessOutcome.RouteImprovement != null
            } : null,
            FailureOutcome = approach.FailureOutcome != null ? new ObstacleOutcomePreview
            {
                Description = approach.FailureOutcome.Description,
                TimeSegmentCost = approach.FailureOutcome.TimeSegmentCost,
                StaminaCost = approach.FailureOutcome.StaminaCost,
                HealthChange = approach.FailureOutcome.HealthChange,
                KnowledgeGained = approach.FailureOutcome.KnowledgeGained,
                HasRouteImprovement = approach.FailureOutcome.RouteImprovement != null
            } : null
        };
    }
}

/// <summary>
/// Preview of an obstacle approach for UI display.
/// </summary>
public class ObstacleApproachPreview
{
    public string ApproachId { get; set; }
    public string ApproachName { get; set; }
    public bool CanUseApproach { get; set; }
    public List<string> BlockReasons { get; set; } = new List<string>();
    public int StaminaCost { get; set; }
    public int SuccessProbability { get; set; }
    public ObstacleOutcomePreview SuccessOutcome { get; set; }
    public ObstacleOutcomePreview FailureOutcome { get; set; }
}

/// <summary>
/// Preview of a possible obstacle outcome.
/// </summary>
public class ObstacleOutcomePreview
{
    public string Description { get; set; }
    public int TimeSegmentCost { get; set; }
    public int StaminaCost { get; set; }
    public int HealthChange { get; set; }
    public List<string> KnowledgeGained { get; set; } = new List<string>();
    public bool HasRouteImprovement { get; set; }
}
