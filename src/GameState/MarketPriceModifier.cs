/// <summary>
/// Represents an override to the calculated market price for a specific item at a specific location.
/// When set, the override price is used instead of the dynamically calculated price based on supply/demand.
/// Used for testing, quest rewards, merchant relationships, and special market conditions.
/// HIGHLANDER: Object references only, no string identifiers
/// SENTINEL: Null override = use calculated price, explicit value = override to that price
/// </summary>
public class MarketPriceModifier
{
    /// <summary>
    /// The item whose price is being modified.
    /// HIGHLANDER: Direct object reference, not string ID
    /// </summary>
    public Item Item { get; set; }

    /// <summary>
    /// The location where the price override applies.
    /// HIGHLANDER: Direct object reference, not string ID
    /// </summary>
    public Location Location { get; set; }

    /// <summary>
    /// Override for the buy price (what the player pays to purchase this item).
    /// SENTINEL: If null, the standard calculated buy price is used.
    /// If set, this exact price is used regardless of supply/demand calculations.
    /// </summary>
    public int? BuyPriceOverride { get; set; }

    /// <summary>
    /// Override for the sell price (what the player receives when selling this item).
    /// SENTINEL: If null, the standard calculated sell price is used.
    /// If set, this exact price is used regardless of supply/demand calculations.
    /// </summary>
    public int? SellPriceOverride { get; set; }
}
