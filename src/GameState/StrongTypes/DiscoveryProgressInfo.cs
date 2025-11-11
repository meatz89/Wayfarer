public class DiscoveryProgressInfo
{
    public int Discovered { get; init; }
    public int Total { get; init; }

    public DiscoveryProgressInfo(int discovered, int total)
    {
        Discovered = discovered;
        Total = total;
    }
}