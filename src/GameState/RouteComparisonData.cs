/// <summary>
/// Represents a comprehensive analysis of a single route option for informed travel decisions.
/// Includes cost calculations, efficiency scoring, and resource requirements.
/// </summary>
public class RouteComparisonData
{
public RouteOption Route { get; set; }
public string CostBenefitAnalysis { get; set; }
public int TotalCost { get; set; }
public int AdjustedStaminaCost { get; set; }
public int FocusPenalty { get; set; }
public double EfficiencyScore { get; set; }
public bool CanAfford { get; set; }
public string ArrivalTime { get; set; }
public string ResourceBreakdown { get; set; }
public string Recommendation { get; set; }

public RouteComparisonData(RouteOption route)
{
    Route = route;
    CostBenefitAnalysis = "";
    ResourceBreakdown = "";
    Recommendation = "";
    ArrivalTime = "";
}
}
