/// <summary>
/// Represents a work action that consumes an entire time block (4 segments) to generate coins.
/// Output is affected by hunger level: coins = base - floor(hunger/25)
/// </summary>
public class WorkAction
{
    // HIGHLANDER: NO Id property - WorkAction identified by object reference
    public string Name { get; set; }
    public string Description { get; set; }
    public WorkType Type { get; set; }
    public int BaseCoins { get; set; }
    // HIGHLANDER: Object references ONLY, no VenueId or LocationId
    public Venue Venue { get; set; }
    public Location Location { get; set; }

    // Optional requirements
    public int? RequiredTokens { get; set; }
    public ConnectionType? RequiredTokenType { get; set; }
    // HIGHLANDER: Object reference ONLY, no RequiredPermit ID
    public Item RequiredPermit { get; set; }

    // Additional benefits for service work
    public int? HungerReduction { get; set; }
    public int? HealthRestore { get; set; }
    // HIGHLANDER: Object reference ONLY, no GrantedItem ID
    public Item GrantedItem { get; set; }
}

public enum WorkType
{
    Standard,   // Basic work, 5 coins base
    Enhanced,   // Better work with requirements, 6-7 coins base
    Service     // Work that provides benefits beyond coins, 3-4 coins base
}