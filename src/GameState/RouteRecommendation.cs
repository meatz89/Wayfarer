
/// <summary>
/// Represents an optimized route recommendation based on player resources and strategy.
/// Provides justification for the recommendation and efficiency metrics.
/// </summary>
public class RouteRecommendation
{
    public RouteOption RecommendedRoute { get; set; }
    public string Justification { get; set; }
    public int EfficiencyScore { get; set; }
    public OptimizationStrategy Strategy { get; set; }
    public string ResourceAnalysis { get; set; }
    public string AlternativeOptions { get; set; }

    public RouteRecommendation(RouteOption route, OptimizationStrategy strategy)
    {
        RecommendedRoute = route;
        Strategy = strategy;
        Justification = "";
        ResourceAnalysis = "";
        AlternativeOptions = "";
    }
}