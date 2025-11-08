
public class TravelSceneDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string VenueId { get; set; }
    public string RouteId { get; set; }

    public List<SceneApproachDTO> Approaches { get; set; }
}

public class SceneApproachDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int SuccessProbability { get; set; }

    public int StaminaRequired { get; set; }
    public Dictionary<string, int> StatRequirements { get; set; }

    public SceneOutcomeDTO SuccessOutcome { get; set; }
    public SceneOutcomeDTO FailureOutcome { get; set; }
}

public class SceneOutcomeDTO
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
