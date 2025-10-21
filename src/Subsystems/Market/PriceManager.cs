using System;
using System.Collections.Generic;
using System.Linq;

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
    /// </summary>
    public class PricingInfo
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string VenueId { get; set; }
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
    /// </summary>
    public int GetBuyPrice(string itemId, string venueId)
    {
        PricingInfo pricing = GetPricingInfo(itemId, venueId);
        return pricing.IsAvailable ? pricing.AdjustedBuyPrice : -1;
    }

    /// <summary>
    /// Get sell price for an item at a location
    /// </summary>
    public int GetSellPrice(string itemId, string venueId)
    {
        PricingInfo pricing = GetPricingInfo(itemId, venueId);
        return pricing.IsAvailable ? pricing.AdjustedSellPrice : -1;
    }

    /// <summary>
    /// Get complete pricing information for an item at a location
    /// </summary>
    public PricingInfo GetPricingInfo(string itemId, string venueId)
    {
        Item item = _itemRepository.GetItemById(itemId);
        if (item == null)
        {
            return new PricingInfo
            {
                ItemId = itemId,
                IsAvailable = false,
                PriceExplanation = "Item not found"
            };
        }

        PricingInfo pricing = new PricingInfo
        {
            ItemId = itemId,
            ItemName = item.Name,
            VenueId = venueId,
            BaseBuyPrice = item.BuyPrice,
            BaseSellPrice = item.SellPrice,
            IsAvailable = true
        };

        // Calculate modifiers
        pricing.SupplyModifier = CalculateSupplyModifier(itemId, venueId);
        pricing.DemandModifier = CalculateDemandModifier(itemId, venueId);
        pricing.LocationModifier = CalculateLocationModifier(itemId, venueId);

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

        // Generate explanation
        pricing.PriceExplanation = GeneratePriceExplanation(pricing);

        return pricing;
    }

    /// <summary>
    /// Calculate supply-based price modifier
    /// </summary>
    private float CalculateSupplyModifier(string itemId, string venueId)
    {
        float supplyLevel = _marketStateTracker.GetSupplyLevel(itemId, venueId);

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
    /// </summary>
    private float CalculateDemandModifier(string itemId, string venueId)
    {
        float demandLevel = _marketStateTracker.GetDemandLevel(itemId, venueId);

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
    /// Calculate location-based price modifier
    /// </summary>
    private float CalculateLocationModifier(string itemId, string venueId)
    {
        Venue venue = _gameWorld.Venues.FirstOrDefault(l => l.Id == venueId);
        if (venue == null)
            throw new InvalidOperationException($"Venue not found: {venueId}");

        Item item = _itemRepository.GetItemById(itemId);
        if (item == null)
            throw new InvalidOperationException($"Item not found: {itemId}");

        float modifier = 1.0f;

        // Venue type affects prices
        switch (venueId)
        {
            case "town_square":
                // Town has slightly higher prices for most goods
                modifier = 1.1f;
                // But lower prices for common items
                if (item.Categories.Contains(ItemCategory.Hunger) ||
                    item.Categories.Contains(ItemCategory.Materials))
                {
                    modifier = 0.95f;
                }
                break;

            case "dusty_flagon":
                // Tavern has lower general prices
                modifier = 0.9f;
                // But higher prices for food and drink
                if (item.Categories.Contains(ItemCategory.Hunger))
                {
                    modifier = 1.15f;
                }
                break;

            case "old_mill":
                // Mill has good prices for materials and tools
                if (item.Categories.Contains(ItemCategory.Materials) ||
                    item.Categories.Contains(ItemCategory.Tools))
                {
                    modifier = 0.85f;
                }
                else
                {
                    modifier = 1.05f;
                }
                break;

            case "merchant_quarter":
                // Merchant quarter has competitive prices for trade goods
                if (item.Categories.Contains(ItemCategory.Trade_Goods) ||
                    item.Categories.Contains(ItemCategory.Valuables))
                {
                    modifier = 0.9f;
                }
                else
                {
                    modifier = 1.0f;
                }
                break;

            case "harbor":
                // Harbor has good prices for water-related items
                if (item.Categories.Contains(ItemCategory.Water_Transport))
                {
                    modifier = 0.8f;
                }
                else
                {
                    modifier = 1.1f;
                }
                break;

            default:
                modifier = 1.0f;
                break;
        }

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
    /// </summary>
    public List<PricingInfo> GetLocationPrices(string venueId)
    {
        List<PricingInfo> prices = new List<PricingInfo>();
        List<Item> allItems = _itemRepository.GetAllItems();

        foreach (Item item in allItems)
        {
            PricingInfo pricing = GetPricingInfo(item.Id, venueId);
            if (pricing.IsAvailable)
            {
                prices.Add(pricing);
            }
        }

        return prices;
    }

    /// <summary>
    /// Get price comparison for an item across all locations
    /// </summary>
    public List<PricingInfo> GetItemPriceComparison(string itemId)
    {
        List<PricingInfo> prices = new List<PricingInfo>();
        List<Venue> locations = _gameWorld.Venues;

        foreach (Venue venue in locations)
        {
            PricingInfo pricing = GetPricingInfo(itemId, venue.Id);
            if (pricing.IsAvailable)
            {
                prices.Add(pricing);
            }
        }

        return prices.OrderBy(p => p.AdjustedBuyPrice).ToList();
    }

    /// <summary>
    /// Find items with best profit margins at a location
    /// </summary>
    public List<PricingInfo> GetHighMarginItems(string venueId, int topN = 5)
    {
        List<PricingInfo> prices = GetLocationPrices(venueId);

        return prices
            .OrderByDescending(p => (float)(p.AdjustedSellPrice - p.AdjustedBuyPrice) / p.AdjustedBuyPrice)
            .Take(topN)
            .ToList();
    }

    // ========== PRICE PREDICTIONS ==========

    /// <summary>
    /// Predict future price based on current trends
    /// </summary>
    public int PredictFuturePrice(string itemId, string venueId, bool isBuyPrice)
    {
        PricingInfo current = GetPricingInfo(itemId, venueId);
        if (!current.IsAvailable) return -1;

        // Get market conditions
        MarketStateTracker.MarketConditions conditions = _marketStateTracker.GetMarketConditions(venueId);

        float trendModifier = 1.0f;

        // If item is trending, prices might increase
        if (conditions.TrendingItems.Contains(itemId))
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
    /// </summary>
    public float CalculatePriceVolatility(string itemId)
    {
        List<PricingInfo> prices = GetItemPriceComparison(itemId);
        if (prices.Count < 2) return 0;

        float avgPrice = (float)prices.Average(p => p.AdjustedBuyPrice);
        float variance = (float)(prices.Sum(p => Math.Pow(p.AdjustedBuyPrice - avgPrice, 2)) / prices.Count);
        float stdDev = (float)Math.Sqrt(variance);

        return stdDev / avgPrice; // Coefficient of variation
    }

    // ========== SPECIAL PRICING RULES ==========

    /// <summary>
    /// Apply special event pricing (festivals, shortages, etc.)
    /// </summary>
    public void ApplyEventPricing(string eventType, string venueId)
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
                    float currentDemand = _marketStateTracker.GetDemandLevel(item.Id, venueId);
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
    /// </summary>
    public int CalculateBulkPrice(string itemId, string venueId, int quantity)
    {
        int singlePrice = GetBuyPrice(itemId, venueId);
        if (singlePrice <= 0) return -1;

        float discount = 1.0f;
        if (quantity >= 10)
            discount = 0.9f; // 10% discount for 10+ items
        else if (quantity >= 5)
            discount = 0.95f; // 5% discount for 5+ items

        return (int)Math.Ceiling(singlePrice * quantity * discount);
    }
}
