public class RouteImprovement
{
    public string Id { get; set; }
    public string RouteId { get; set; } // Which route this improvement applies to
    public string Name { get; set; }
    public string Description { get; set; }
    public int TimeReduction { get; set; }
    public int CostReduction { get; set; }
    public int StaminaReduction { get; set; }
}
