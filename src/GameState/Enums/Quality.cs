
/// <summary>
/// UNIVERSAL categorical property: Quality/tier of goods, services, locations, items
///
/// Derivation: Location.Difficulty (0 = Basic, 1 = Standard, 2 = Premium, 3+ = Luxury)
/// Location.Difficulty = hex distance from world center / 5 (arc42 §8.28)
///
/// Scales costs and benefits across ALL domains:
/// - Services: Cost multiplier (Basic 0.6x, Standard 1.0x, Premium 1.6x, Luxury 2.4x)
/// - Items: Value and effectiveness
/// - Locations: Amenities and restoration power
/// - Equipment: Damage/defense bonuses
///
/// Replaces narrow ServiceQuality enum with universal tier system.
/// </summary>
public enum Quality
{
    Basic,      // Tier 1: Cheap, functional, minimal (0.6x cost, 0.8x benefit)
    Standard,   // Tier 2: Average, reliable, ordinary (1.0x cost, 1.0x benefit)
    Premium,    // Tier 3: High quality, comfortable, notable (1.6x cost, 1.4x benefit)
    Luxury      // Tier 4+: Exceptional, opulent, finest (2.4x cost, 2.0x benefit)
}
