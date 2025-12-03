
/// <summary>
/// Manages price calculations with supply/demand dynamics.
/// Provides deterministic pricing based on Venue characteristics and market conditions.
/// </summary>
public class PriceManager
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;
    private readonly MarketStateTracker _marketStateTracker;

    // Price adjustment limits (flat coin adjustments)
    private const int MIN_TOTAL_ADJUSTMENT = -10;  // Maximum price reduction
    private const int MAX_TOTAL_ADJUSTMENT = 20;   // Maximum price increase
    private const int BUY_SELL_SPREAD = 3;         // Fixed 3-coin spread between buy and sell prices (DDR-007)

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
        public int SupplyAdjustment { get; set; }     // Flat coin adjustment from supply
        public int DemandAdjustment { get; set; }     // Flat coin adjustment from demand
        public int LocationAdjustment { get; set; }   // Flat coin adjustment from location
        public int TotalAdjustment { get; set; }      // Sum of all adjustments
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

        // Calculate adjustments (flat coin values)
        pricing.SupplyAdjustment = CalculateSupplyAdjustment(item, location);
        pricing.DemandAdjustment = CalculateDemandAdjustment(item, location);
        pricing.LocationAdjustment = CalculateLocationAdjustment(item, location);

        // Combine adjustments additively (DDR-007 compliance)
        int totalAdjustment = pricing.SupplyAdjustment + pricing.DemandAdjustment + pricing.LocationAdjustment;
        pricing.TotalAdjustment = Math.Max(MIN_TOTAL_ADJUSTMENT, Math.Min(MAX_TOTAL_ADJUSTMENT, totalAdjustment));

        // Apply adjustments to prices (additive)
        pricing.AdjustedBuyPrice = pricing.BaseBuyPrice + pricing.TotalAdjustment;
        pricing.AdjustedSellPrice = pricing.BaseSellPrice + pricing.TotalAdjustment;

        // Ensure buy price is always higher than sell price (DDR-007: fixed spread)
        int minBuyPrice = pricing.AdjustedSellPrice + BUY_SELL_SPREAD;
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
    /// Calculate supply-based price adjustment (returns flat coin adjustment)
    /// HIGHLANDER: Accept Item and Location objects
    /// DDR-007: Tier-based lookup replaces percentage calculations
    /// </summary>
    private int CalculateSupplyAdjustment(Item item, Location location)
    {
        int supplyPercent = _marketStateTracker.GetSupplyLevel(item, location);

        // Low supply = higher prices, high supply = lower prices
        // DDR-007: Tier-based lookup (no percentage math)
        if (supplyPercent < 10) return 12;       // Severe shortage: +12 coins
        else if (supplyPercent < 30) return 9;   // Low supply: +9 coins
        else if (supplyPercent < 60) return 6;   // Below normal: +6 coins
        else if (supplyPercent < 90) return 3;   // Slightly low: +3 coins
        else if (supplyPercent <= 110) return 0; // Normal: no adjustment
        else if (supplyPercent < 150) return -3; // Slight surplus: -3 coins
        else return -6;                          // Major surplus: -6 coins
    }

    /// <summary>
    /// Calculate demand-based price adjustment (returns flat coin adjustment)
    /// HIGHLANDER: Accept Item and Location objects
    /// DDR-007: Tier-based lookup replaces percentage calculations
    /// </summary>
    private int CalculateDemandAdjustment(Item item, Location location)
    {
        int demandPercent = _marketStateTracker.GetDemandLevel(item, location);

        // High demand = higher prices, low demand = lower prices
        // DDR-007: Tier-based lookup (no percentage math)
        if (demandPercent < 10) return -6;       // No demand: -6 coins
        else if (demandPercent < 30) return -4;  // Very low demand: -4 coins
        else if (demandPercent < 60) return -3;  // Low demand: -3 coins
        else if (demandPercent < 90) return -1;  // Below normal: -1 coin
        else if (demandPercent <= 110) return 0; // Normal: no adjustment
        else if (demandPercent < 150) return 2;  // High demand: +2 coins
        else return 4;                           // Very high demand: +4 coins
    }

    /// <summary>
    /// Calculate location-based price adjustment based on Location properties (returns flat coin adjustment)
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    private int CalculateLocationAdjustment(Item item, Location location)
    {
        int adjustment = 0; // 0 coins

        // Location purpose/role determine pricing (orthogonal properties replace capabilities)
        // Check in priority order (most specific first)

        // Market-purpose locations - higher prices for most goods
        if (location.Purpose == LocationPurpose.Commerce && location.Role == LocationRole.Hub)
        {
            adjustment = 2; // +2 coins
            // But lower prices for common items (food/materials)
            if (item.Categories.Contains(ItemCategory.Hunger) ||
                item.Categories.Contains(ItemCategory.Materials))
            {
                adjustment = -1; // -1 coin
            }
        }
        // Rest-role locations (taverns/inns) - lower general prices, higher food prices
        else if (location.Role == LocationRole.Rest && location.Purpose == LocationPurpose.Commerce)
        {
            adjustment = -2; // -2 coins
            // Higher prices for food and drink
            if (item.Categories.Contains(ItemCategory.Hunger))
            {
                adjustment = 3; // +3 coins
            }
        }
        // Commercial-purpose locations (workshops, etc.) - good prices for tools/materials
        else if (location.Purpose == LocationPurpose.Commerce)
        {
            // Good prices for materials and tools
            if (item.Categories.Contains(ItemCategory.Materials) ||
                item.Categories.Contains(ItemCategory.Tools))
            {
                adjustment = -3; // -3 coins
            }
            else
            {
                adjustment = 1; // +1 coin
            }
        }
        // High-difficulty locations - higher prices for trade goods
        else if (location.Difficulty >= 2)
        {
            // Competitive prices for trade goods and valuables
            if (item.Categories.Contains(ItemCategory.Trade_Goods) ||
                item.Categories.Contains(ItemCategory.Valuables))
            {
                adjustment = -2; // -2 coins
            }
            else
            {
                adjustment = 0; // 0 coins
            }
        }
        // NOTE: Water-adjacent location pricing removed - LocationSetting doesn't include Water
        // Future: Could use LocationEnvironment or specific tags for water-adjacent locations

        return adjustment;
    }

    /// <summary>
    /// Generate human-readable price explanation
    /// </summary>
    private string GeneratePriceExplanation(PricingInfo pricing)
    {
        List<string> factors = new List<string>();

        // Check supply (negative = abundant, positive = scarce)
        if (pricing.SupplyAdjustment < -2)
            factors.Add("abundant supply");
        else if (pricing.SupplyAdjustment > 3)
            factors.Add("scarce supply");

        // Check demand (negative = low, positive = high)
        if (pricing.DemandAdjustment < -2)
            factors.Add("low demand");
        else if (pricing.DemandAdjustment > 2)
            factors.Add("high demand");

        // Check location (negative = favorable, positive = premium)
        if (pricing.LocationAdjustment < -1)
            factors.Add("favorable location");
        else if (pricing.LocationAdjustment > 1)
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
    /// HIGHLANDER: topN REQUIRED - caller specifies how many items to return
    /// </summary>
    public List<PricingInfo> GetHighMarginItems(Location location, int topN)
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

        int trendAdjustment = 0; // 0 coins

        // If item is trending, prices might increase
        if (conditions.TrendingItems.Contains(item))
        {
            trendAdjustment = 2; // +2 coins
        }

        // Apply trend to current price
        if (isBuyPrice)
        {
            return current.AdjustedBuyPrice + trendAdjustment;
        }
        else
        {
            return current.AdjustedSellPrice + trendAdjustment;
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
    /// Calculate bulk discount for multiple purchases (DDR-007: flat per-item discount)
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    public int CalculateBulkPrice(Item item, Location location, int quantity)
    {
        int singlePrice = GetBuyPrice(item, location);
        if (singlePrice <= 0) return -1;

        int perItemDiscount = 0; // No discount
        if (quantity >= 10)
            perItemDiscount = 2;  // -2 coins per item for 10+ items
        else if (quantity >= 5)
            perItemDiscount = 1;  // -1 coin per item for 5+ items

        int discountedPrice = Math.Max(1, singlePrice - perItemDiscount);
        return discountedPrice * quantity;
    }
}
