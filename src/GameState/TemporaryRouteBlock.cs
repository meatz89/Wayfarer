/// <summary>
/// Tracks temporary route block with expiration
/// HIGHLANDER: Uses RouteName (RouteOption.Name) as natural key, not Id
/// </summary>
public class TemporaryRouteBlock
{
    public string RouteName { get; set; }
    public int UnblockDay { get; set; }
}
