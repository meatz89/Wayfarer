public class RouteImprovement
{
    // HIGHLANDER: NO Id property - RouteImprovement identified by object reference
    // HIGHLANDER: Object reference ONLY, no RouteId
    public RouteOption Route { get; set; } // Which route this improvement applies to
    public string Name { get; set; }
    public string Description { get; set; }
    public int TimeReduction { get; set; }
    public int CostReduction { get; set; }
    public int StaminaReduction { get; set; }
}
