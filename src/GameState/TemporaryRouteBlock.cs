/// <summary>
/// Tracks temporary route block with expiration
/// HIGHLANDER: Stores RouteOption object reference, not string name
/// </summary>
public class TemporaryRouteBlock
{
    public RouteOption Route { get; set; }
    public int UnblockDay { get; set; }
}
