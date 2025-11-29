
/// <summary>
/// Manages price calculations with supply/demand dynamics.
/// Provides deterministic pricing based on Venue characteristics and market conditions.
/// </summary>
public class PriceManager
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;
    private readonly MarketStateTracker _marketStateTracker;

    // Price adjustment factors (basis points: 10000 = 1.0x)
    private const int MIN_PRICE_MULTIPLIER_BP = 5000;  // 0.5x
    private const int MAX_PRICE_MULTIPLIER_BP = 25000; // 2.5x
    private const int BUY_SELL_SPREAD_PERCENT = 15;    // 15% spread between buy and sell prices

    public PriceManager(
        GameWorld gameWorld,
        ItemRepository itemRepository,
        MarketStateTracker marketStateTracker)
    {
        _gameWorld = gameWorld;
        _itemRepository = itemRepository;
        _marketStateTracker = marketStateTracker;
    }

    /// <summary>
    /// Complete pricing information for an item at a location
    /// HIGHLANDER: Object references only
    /// </summary>
    public class PricingInfo
    {
        public Item Item { get; set; }
        public Location Location { get; set; }
        public int BaseBuyPrice { get; set; }
        public int BaseSellPrice { get; set; }
        public int AdjustedBuyPrice { get; set; }
        public int AdjustedSellPrice { get; set; }
        public int SupplyModifierBP { get; set; }     // Basis points (10000 = 1.0x)
        public int DemandModifierBP { get; set; }     // Basis points (10000 = 1.0x)
        public int LocationModifierBP { get; set; }   // Basis points (10000 = 1.0x)
        public int FinalModifierBP { get; set; }      // Basis points (10000 = 1.0x)
        public bool IsAvailable { get; set; }
        public string PriceExplanation { get; set; }
    }

    // ========== PRICE CALCULATIONS ==========

    /// <summary>
    /// Get buy price for an item at a location
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    public int GetBuyPrice(Item item, Location location)
    {
        if (item == null || location == null)
            return -1;

        PricingInfo pricing = GetPricingInfo(item, location);
        return pricing.IsAvailable ? pricing.AdjustedBuyPrice : -1;
    }

    /// <summary>
    /// Get sell price for an item at a location
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    public int GetSellPrice(Item item, Location location)
    {
        if (item == null || location == null)
            return -1;

        PricingInfo pricing = GetPricingInfo(item, location);
        return pricing.IsAvailable ? pricing.AdjustedSellPrice : -1;
    }

    /// <summary>
    /// Get complete pricing information for an item at a location
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    public PricingInfo GetPricingInfo(Item item, Location location)
    {
        if (item == null)
        {
            return new PricingInfo
            {
                IsAvailable = false,
                PriceExplanation = "Item not found"
            };
        }

        if (location == null)
        {
            return new PricingInfo
            {
                Item = item,
                IsAvailable = false,
                PriceExplanation = "Location not found"
            };
        }

        PricingInfo pricing = new PricingInfo
        {
            Item = item,
            Location = location,
            BaseBuyPrice = item.BuyPrice,
            BaseSellPrice = item.SellPrice,
            IsAvailable = true
        };

        // Calculate modifiers (in basis points)
        pricing.SupplyModifierBP = CalculateSupplyModifier(item, location);
        pricing.DemandModifierBP = CalculateDemandModifier(item, location);
        pricing.LocationModifierBP = CalculateLocationModifier(item, location);

        // Combine modifiers (multiply basis points: BP1 * BP2 / 10000, then * BP3 / 10000)
        int combinedBP = pricing.SupplyModifierBP * pricing.DemandModifierBP / 10000;
        combinedBP = combinedBP * pricing.LocationModifierBP / 10000;
        pricing.FinalModifierBP = Math.Max(MIN_PRICE_MULTIPLIER_BP, Math.Min(MAX_PRICE_MULTIPLIER_BP, combinedBP));

        // Apply modifiers to prices (basis points)
        pricing.AdjustedBuyPrice = pricing.BaseBuyPrice * pricing.FinalModifierBP / 10000;
        pricing.AdjustedSellPrice = pricing.BaseSellPrice * pricing.FinalModifierBP / 10000;

        // Ensure buy price is always higher than sell price (add percentage spread)
        int minBuyPrice = pricing.AdjustedSellPrice * (100 + BUY_SELL_SPREAD_PERCENT) / 100;
        if (pricing.AdjustedBuyPrice < minBuyPrice)
        {
            pricing.AdjustedBuyPrice = minBuyPrice;
        }

        // Check for price overrides (quest rewards, testing, special market conditions)
        // Overrides bypass ALL calculations including spread enforcement
        // HIGHLANDER: GameWorld.MarketPriceModifiers is single source of truth for overrides
        // SENTINEL: Null override = use calculated price, explicit value = override to that price
        MarketPriceModifier modifier = _gameWorld.MarketPriceModifiers
            .FirstOrDefault(m => m.Item == item && m.Location == location);

        if (modifier != null)
        {
            if (modifier.BuyPriceOverride.HasValue)
            {
                pricing.AdjustedBuyPrice = modifier.BuyPriceOverride.Value;
            }

            if (modifier.SellPriceOverride.HasValue)
            {
                pricing.AdjustedSellPrice = modifier.SellPriceOverride.Value;
            }
        }

        // Generate explanation
        pricing.PriceExplanation = GeneratePriceExplanation(pricing);

        return pricing;
    }

    /// <summary>
    /// Calculate supply-based price modifier (returns basis points: 10000 = 1.0x)
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    private int CalculateSupplyModifier(Item item, Location location)
    {
        int supplyPercent = _marketStateTracker.GetSupplyLevel(item, location);

        // Low supply = higher prices, high supply = lower prices
        // Supply 50% = 1.3x price (13000 BP), Supply 100% = 1.0x price (10000 BP), Supply 200% = 0.7x price (7000 BP)
        if (supplyPercent < 100)
        {
            // 10000 + (100 - supplyPercent) * 60 = range from 16000 BP (0%) to 10000 BP (100%)
            return 10000 + (100 - supplyPercent) * 60;
        }
        else
        {
            // 10000 - (supplyPercent - 100) * 15 = range from 10000 BP (100%) to 8500 BP (200%)
            return Math.Max(7000, 10000 - (supplyPercent - 100) * 15);
        }
    }

    /// <summary>
    /// Calculate demand-based price modifier (returns basis points: 10000 = 1.0x)
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    private int CalculateDemandModifier(Item item, Location location)
    {
        int demandPercent = _marketStateTracker.GetDemandLevel(item, location);

        // High demand = higher prices, low demand = lower prices
        // Demand 50% = 0.85x price (8500 BP), Demand 100% = 1.0x price (10000 BP), Demand 200% = 1.2x price (12000 BP)
        if (demandPercent < 100)
        {
            // 10000 - (100 - demandPercent) * 30 = range from 7000 BP (0%) to 10000 BP (100%)
            return 10000 - (100 - demandPercent) * 30;
        }
        else
        {
            // 10000 + (demandPercent - 100) * 20 = range from 10000 BP (100%) to 12000 BP (200%)
            return Math.Min(12000, 10000 + (demandPercent - 100) * 20);
        }
    }

    /// <summary>
    /// Calculate location-based price modifier based on Location properties (returns basis points: 10000 = 1.0x)
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    private int CalculateLocationModifier(Item item, Location location)
    {
        int modifierBP = 10000; // 1.0x

        // Location purpose/role determine pricing (orthogonal properties replace capabilities)
        // Check in priority order (most specific first)

        // Market-purpose locations - higher prices for most goods
        if (location.Purpose == LocationPurpose.Commerce && location.Role == LocationRole.Hub)
        {
            modifierBP = 11000; // 1.1x
            // But lower prices for common items (food/materials)
            if (item.Categories.Contains(ItemCategory.Hunger) ||
                item.Categories.Contains(ItemCategory.Materials))
            {
                modifierBP = 9500; // 0.95x
            }
        }
        // Rest-role locations (taverns/inns) - lower general prices, higher food prices
        else if (location.Role == LocationRole.Rest && location.Purpose == LocationPurpose.Commerce)
        {
            modifierBP = 9000; // 0.9x
            // Higher prices for food and drink
            if (item.Categories.Contains(ItemCategory.Hunger))
            {
                modifierBP = 11500; // 1.15x
            }
        }
        // Commercial-purpose locations (workshops, etc.) - good prices for tools/materials
        else if (location.Purpose == LocationPurpose.Commerce)
        {
            // Good prices for materials and tools
            if (item.Categories.Contains(ItemCategory.Materials) ||
                item.Categories.Contains(ItemCategory.Tools))
            {
                modifierBP = 8500; // 0.85x
            }
            else
            {
                modifierBP = 10500; // 1.05x
            }
        }
        // High-tier locations - higher prices for trade goods
        else if (location.Tier >= 3)
        {
            // Competitive prices for trade goods and valuables
            if (item.Categories.Contains(ItemCategory.Trade_Goods) ||
                item.Categories.Contains(ItemCategory.Valuables))
            {
                modifierBP = 9000; // 0.9x
            }
            else
            {
                modifierBP = 10000; // 1.0x
            }
        }
        // NOTE: Water-adjacent location pricing removed - LocationSetting doesn't include Water
        // Future: Could use LocationEnvironment or specific tags for water-adjacent locations

        return modifierBP;
    }

    /// <summary>
    /// Generate human-readable price explanation
    /// </summary>
    private string GeneratePriceExplanation(PricingInfo pricing)
    {
        List<string> factors = new List<string>();

        // Check supply (9000 BP = 0.9x, 11000 BP = 1.1x)
        if (pricing.SupplyModifierBP < 9000)
            factors.Add("abundant supply");
        else if (pricing.SupplyModifierBP > 11000)
            factors.Add("scarce supply");

        // Check demand (9000 BP = 0.9x, 11000 BP = 1.1x)
        if (pricing.DemandModifierBP < 9000)
            factors.Add("low demand");
        else if (pricing.DemandModifierBP > 11000)
            factors.Add("high demand");

        // Check location (9500 BP = 0.95x, 10500 BP = 1.05x)
        if (pricing.LocationModifierBP < 9500)
            factors.Add("favorable location");
        else if (pricing.LocationModifierBP > 10500)
            factors.Add("premium location");

        if (factors.Count == 0)
            return "Standard market pricing";

        return "Price affected by: " + string.Join(", ", factors);
    }

    // ========== BULK PRICE OPERATIONS ==========

    /// <summary>
    /// Get prices for all items at a location
    /// HIGHLANDER: Accept Location object
    /// </summary>
    public List<PricingInfo> GetLocationPrices(Location location)
    {
        List<PricingInfo> prices = new List<PricingInfo>();
        List<Item> allItems = _itemRepository.GetAllItems();

        foreach (Item item in allItems)
        {
            PricingInfo pricing = GetPricingInfo(item, location);
            if (pricing.IsAvailable)
            {
                prices.Add(pricing);
            }
        }

        return prices;
    }

    /// <summary>
    /// Get price comparison for an item across all locations (checks all Locations in world)
    /// HIGHLANDER: Accept Item object
    /// </summary>
    public List<PricingInfo> GetItemPriceComparison(Item item)
    {
        List<PricingInfo> prices = new List<PricingInfo>();
        List<Location> allLocations = _gameWorld.Locations;

        foreach (Location location in allLocations)
        {
            PricingInfo pricing = GetPricingInfo(item, location);
            if (pricing.IsAvailable)
            {
                prices.Add(pricing);
            }
        }

        return prices.OrderBy(p => p.AdjustedBuyPrice).ToList();
    }

    /// <summary>
    /// Find items with best profit margins at a location
    /// HIGHLANDER: Accept Location object
    /// </summary>
    public List<PricingInfo> GetHighMarginItems(Location location, int topN = 5)
    {
        List<PricingInfo> prices = GetLocationPrices(location);

        return prices
            .OrderByDescending(p => (p.AdjustedSellPrice - p.AdjustedBuyPrice) * 100 / p.AdjustedBuyPrice)
            .Take(topN)
            .ToList();
    }

    // ========== PRICE PREDICTIONS ==========

    /// <summary>
    /// Predict future price based on current trends
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    public int PredictFuturePrice(Item item, Location location, bool isBuyPrice)
    {
        PricingInfo current = GetPricingInfo(item, location);
        if (!current.IsAvailable) return -1;

        // Get market conditions
        MarketStateTracker.MarketConditions conditions = _marketStateTracker.GetMarketConditions(location);

        int trendModifierBP = 10000; // 1.0x

        // If item is trending, prices might increase (5% increase = 10500 BP)
        if (conditions.TrendingItems.Contains(item))
        {
            trendModifierBP = 10500; // 1.05x
        }

        // Apply trend to current price
        if (isBuyPrice)
        {
            return current.AdjustedBuyPrice * trendModifierBP / 10000;
        }
        else
        {
            return current.AdjustedSellPrice * trendModifierBP / 10000;
        }
    }

    /// <summary>
    /// Calculate price volatility for an item (returns coefficient of variation as percentage)
    /// HIGHLANDER: Accept Item object
    /// </summary>
    public int CalculatePriceVolatility(Item item)
    {
        List<PricingInfo> prices = GetItemPriceComparison(item);
        if (prices.Count < 2) return 0;

        int avgPrice = prices.Sum(p => p.AdjustedBuyPrice) / prices.Count;
        int sumSquaredDiff = prices.Sum(p => {
            int diff = p.AdjustedBuyPrice - avgPrice;
            return diff * diff;
        });
        int variance = sumSquaredDiff / prices.Count;
        int stdDev = (int)Math.Sqrt(variance);

        // Return coefficient of variation as percentage
        return avgPrice > 0 ? stdDev * 100 / avgPrice : 0;
    }

    // ========== SPECIAL PRICING RULES ==========

    /// <summary>
    /// Apply special event pricing (festivals, shortages, etc.)
    /// HIGHLANDER: Accept Location object
    /// </summary>
    public void ApplyEventPricing(string eventType, Location location)
    {
        switch (eventType)
        {
            case "festival":
                // Hunger and luxury items more expensive during festivals
                // This would update the market state tracker's demand levels
                List<Item> items = _itemRepository.GetAllItems();
                foreach (Item item in items.Where(i =>
                    i.Categories.Contains(ItemCategory.Hunger) ||
                    i.Categories.Contains(ItemCategory.Luxury_Items)))
                {
                    // Increase demand during festival
                    int currentDemand = _marketStateTracker.GetDemandLevel(item, location);
                    // Note: Would need to add SetDemandLevel method to MarketStateTracker
                }
                break;

            case "shortage":
                // Specific items become scarce
                // This would update supply levels in market state tracker
                break;

            case "merchant_arrival":
                // New merchant increases supply
                // This would update supply levels in market state tracker
                break;
        }
    }

    /// <summary>
    /// Calculate bulk discount for multiple purchases
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    public int CalculateBulkPrice(Item item, Location location, int quantity)
    {
        int singlePrice = GetBuyPrice(item, location);
        if (singlePrice <= 0) return -1;

        int discountPercent = 100; // No discount (100%)
        if (quantity >= 10)
            discountPercent = 90;  // 10% discount for 10+ items
        else if (quantity >= 5)
            discountPercent = 95;  // 5% discount for 5+ items

        return singlePrice * quantity * discountPercent / 100;
    }
}
