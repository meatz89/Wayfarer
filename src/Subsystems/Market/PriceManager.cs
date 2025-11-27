
/// <summary>
/// Manages price calculations with supply/demand dynamics.
/// Provides deterministic pricing based on Venue characteristics and market conditions.
/// </summary>
public class PriceManager
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;
    private readonly MarketStateTracker _marketStateTracker;

    // Price adjustment factors
    private const float MIN_PRICE_MULTIPLIER = 0.5f;
    private const float MAX_PRICE_MULTIPLIER = 2.5f;
    private const float BUY_SELL_SPREAD = 0.15f; // 15% spread between buy and sell prices

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
        public float SupplyModifier { get; set; }
        public float DemandModifier { get; set; }
        public float LocationModifier { get; set; }
        public float FinalModifier { get; set; }
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

        // Calculate modifiers
        pricing.SupplyModifier = CalculateSupplyModifier(item, location);
        pricing.DemandModifier = CalculateDemandModifier(item, location);
        pricing.LocationModifier = CalculateLocationModifier(item, location);

        // Combine modifiers
        pricing.FinalModifier = pricing.SupplyModifier * pricing.DemandModifier * pricing.LocationModifier;
        pricing.FinalModifier = Math.Max(MIN_PRICE_MULTIPLIER, Math.Min(MAX_PRICE_MULTIPLIER, pricing.FinalModifier));

        // Apply modifiers to prices
        pricing.AdjustedBuyPrice = (int)Math.Ceiling(pricing.BaseBuyPrice * pricing.FinalModifier);
        pricing.AdjustedSellPrice = (int)Math.Floor(pricing.BaseSellPrice * pricing.FinalModifier);

        // Ensure buy price is always higher than sell price
        int minBuyPrice = (int)Math.Ceiling(pricing.AdjustedSellPrice * (1 + BUY_SELL_SPREAD));
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
    /// Calculate supply-based price modifier
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    private float CalculateSupplyModifier(Item item, Location location)
    {
        float supplyLevel = _marketStateTracker.GetSupplyLevel(item, location);

        // Low supply = higher prices, high supply = lower prices
        // Supply 0.5 = 1.3x price, Supply 1.0 = 1.0x price, Supply 2.0 = 0.7x price
        if (supplyLevel < 1.0f)
        {
            return 1.0f + (1.0f - supplyLevel) * 0.6f;
        }
        else
        {
            return 1.0f - (supplyLevel - 1.0f) * 0.15f;
        }
    }

    /// <summary>
    /// Calculate demand-based price modifier
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    private float CalculateDemandModifier(Item item, Location location)
    {
        float demandLevel = _marketStateTracker.GetDemandLevel(item, location);

        // High demand = higher prices, low demand = lower prices
        // Demand 0.5 = 0.85x price, Demand 1.0 = 1.0x price, Demand 2.0 = 1.2x price
        if (demandLevel < 1.0f)
        {
            return 1.0f - (1.0f - demandLevel) * 0.3f;
        }
        else
        {
            return 1.0f + (demandLevel - 1.0f) * 0.2f;
        }
    }

    /// <summary>
    /// Calculate location-based price modifier based on Location properties
    /// HIGHLANDER: Accept Item and Location objects
    /// </summary>
    private float CalculateLocationModifier(Item item, Location location)
    {
        float modifier = 1.0f;

        // Location purpose/role determine pricing (orthogonal properties replace capabilities)
        // Check in priority order (most specific first)

        // Market-purpose locations - higher prices for most goods
        if (location.Purpose == LocationPurpose.Commerce && location.Role == LocationRole.Hub)
        {
            modifier = 1.1f;
            // But lower prices for common items (food/materials)
            if (item.Categories.Contains(ItemCategory.Hunger) ||
                item.Categories.Contains(ItemCategory.Materials))
            {
                modifier = 0.95f;
            }
        }
        // Rest-role locations (taverns/inns) - lower general prices, higher food prices
        else if (location.Role == LocationRole.Rest && location.Purpose == LocationPurpose.Commerce)
        {
            modifier = 0.9f;
            // Higher prices for food and drink
            if (item.Categories.Contains(ItemCategory.Hunger))
            {
                modifier = 1.15f;
            }
        }
        // Commercial-purpose locations (workshops, etc.) - good prices for tools/materials
        else if (location.Purpose == LocationPurpose.Commerce)
        {
            // Good prices for materials and tools
            if (item.Categories.Contains(ItemCategory.Materials) ||
                item.Categories.Contains(ItemCategory.Tools))
            {
                modifier = 0.85f;
            }
            else
            {
                modifier = 1.05f;
            }
        }
        // High-tier locations - higher prices for trade goods
        else if (location.Tier >= 3)
        {
            // Competitive prices for trade goods and valuables
            if (item.Categories.Contains(ItemCategory.Trade_Goods) ||
                item.Categories.Contains(ItemCategory.Valuables))
            {
                modifier = 0.9f;
            }
            else
            {
                modifier = 1.0f;
            }
        }
        // NOTE: Water-adjacent location pricing removed - LocationSetting doesn't include Water
        // Future: Could use LocationEnvironment or specific tags for water-adjacent locations

        return modifier;
    }

    /// <summary>
    /// Generate human-readable price explanation
    /// </summary>
    private string GeneratePriceExplanation(PricingInfo pricing)
    {
        List<string> factors = new List<string>();

        if (pricing.SupplyModifier < 0.9f)
            factors.Add("abundant supply");
        else if (pricing.SupplyModifier > 1.1f)
            factors.Add("scarce supply");

        if (pricing.DemandModifier < 0.9f)
            factors.Add("low demand");
        else if (pricing.DemandModifier > 1.1f)
            factors.Add("high demand");

        if (pricing.LocationModifier < 0.95f)
            factors.Add("favorable location");
        else if (pricing.LocationModifier > 1.05f)
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
            .OrderByDescending(p => (float)(p.AdjustedSellPrice - p.AdjustedBuyPrice) / p.AdjustedBuyPrice)
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

        float trendModifier = 1.0f;

        // If item is trending, prices might increase
        if (conditions.TrendingItems.Contains(item))
        {
            trendModifier = 1.05f;
        }

        // Apply trend to current price
        if (isBuyPrice)
        {
            return (int)Math.Ceiling(current.AdjustedBuyPrice * trendModifier);
        }
        else
        {
            return (int)Math.Floor(current.AdjustedSellPrice * trendModifier);
        }
    }

    /// <summary>
    /// Calculate price volatility for an item
    /// HIGHLANDER: Accept Item object
    /// </summary>
    public float CalculatePriceVolatility(Item item)
    {
        List<PricingInfo> prices = GetItemPriceComparison(item);
        if (prices.Count < 2) return 0;

        float avgPrice = (float)prices.Average(p => p.AdjustedBuyPrice);
        float variance = (float)(prices.Sum(p => Math.Pow(p.AdjustedBuyPrice - avgPrice, 2)) / prices.Count);
        float stdDev = (float)Math.Sqrt(variance);

        return stdDev / avgPrice; // Coefficient of variation
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
                    float currentDemand = _marketStateTracker.GetDemandLevel(item, location);
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

        float discount = 1.0f;
        if (quantity >= 10)
            discount = 0.9f; // 10% discount for 10+ items
        else if (quantity >= 5)
            discount = 0.95f; // 5% discount for 5+ items

        return (int)Math.Ceiling(singlePrice * quantity * discount);
    }
}
