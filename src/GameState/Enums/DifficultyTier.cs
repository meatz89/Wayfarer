/// <summary>
/// Difficulty tiers for procedurally generated delivery jobs.
/// Translated to concrete payment amounts via DeliveryJobCatalog at parse time.
/// </summary>
public enum DifficultyTier
{
/// <summary>
/// Simple delivery - short route, low danger, low pay
/// Payment formula: RoomCost + TravelCost + 3 coin profit
/// </summary>
Simple,

/// <summary>
/// Moderate delivery - medium route, medium danger, medium pay
/// Payment formula: RoomCost + TravelCost + 5 coin profit
/// </summary>
Moderate,

/// <summary>
/// Dangerous delivery - long route, high danger, high pay
/// Payment formula: RoomCost + TravelCost + 10 coin profit
/// </summary>
Dangerous
}
