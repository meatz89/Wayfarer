/// <summary>
/// Tracks visit count for a specific location
/// Replacement for Dictionary<string, int> LocationVisitCounts
/// </summary>
public class LocationVisitCount
{
    public string LocationId { get; set; }
    public int Count { get; set; }
}
