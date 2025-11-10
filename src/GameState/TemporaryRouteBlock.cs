/// <summary>
/// Tracks temporary route block with expiration
/// Replacement for Dictionary<string, int> TemporaryRouteBlocks
/// </summary>
public class TemporaryRouteBlock
{
public string RouteId { get; set; }
public int UnblockDay { get; set; }
}
