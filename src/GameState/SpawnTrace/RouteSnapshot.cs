/// <summary>
/// Immutable snapshot of Route state at spawn time
/// Captures origin, destination, and travel properties for tracing
/// </summary>
public class RouteSnapshot
{
    public string OriginLocationName { get; set; }
    public string DestinationLocationName { get; set; }
    public TravelMethods Method { get; set; }
    public int BaseStaminaCost { get; set; }
    public int BaseCoinCost { get; set; }
    public List<TerrainCategory> TerrainCategories { get; set; }
}
