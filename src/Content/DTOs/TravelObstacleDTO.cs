
public class TravelObstacleDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string VenueId { get; set; }
    public string RouteId { get; set; }

    public List<ObstacleApproachDTO> Approaches { get; set; }
}

public class ObstacleApproachDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int SuccessProbability { get; set; }

    public EquipmentRequirementDTO EquipmentRequirements { get; set; }
    public int StaminaRequired { get; set; }
    public Dictionary<string, int> StatRequirements { get; set; }

    public ObstacleOutcomeDTO SuccessOutcome { get; set; }
    public ObstacleOutcomeDTO FailureOutcome { get; set; }
}

public class ObstacleOutcomeDTO
{
    public string Description { get; set; }
    public int TimeSegmentCost { get; set; } = 1;
    public int StaminaCost { get; set; }
    public int HealthChange { get; set; }

    // Knowledge system eliminated

    public RouteImprovementDTO RouteImprovement { get; set; }
}

public class RouteImprovementDTO
{
    public string Description { get; set; }
    public int TimeReduction { get; set; }
    public int StaminaReduction { get; set; }
}
