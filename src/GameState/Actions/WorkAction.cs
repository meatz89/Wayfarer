/// <summary>
/// Represents a work action that consumes an entire time block (4 segments) to generate coins.
/// Output is affected by hunger level: coins = base - floor(hunger/25)
/// </summary>
public class WorkAction
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public WorkType Type { get; set; }
    public int BaseCoins { get; set; }
    public string VenueId { get; set; }
    public string SpotId { get; set; }

    // Optional requirements
    public int? RequiredTokens { get; set; }
    public ConnectionType? RequiredTokenType { get; set; }
    public string RequiredPermit { get; set; }

    // Additional benefits for service work
    public int? HungerReduction { get; set; }
    public int? HealthRestore { get; set; }
    public string GrantedItem { get; set; }
}

public enum WorkType
{
    Standard,   // Basic work, 5 coins base
    Enhanced,   // Better work with requirements, 6-7 coins base
    Service     // Work that provides benefits beyond coins, 3-4 coins base
}