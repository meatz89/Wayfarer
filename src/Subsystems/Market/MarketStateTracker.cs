/// <summary>
/// Tracks market conditions, inventory levels, and trade history
/// to provide insights into supply, demand, and market dynamics.
/// </summary>
public class MarketStateTracker
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;

    // Track supply and demand per Venue per item
    private List<MarketMetrics> _marketMetrics;

    // Track trade history for trend analysis
    private List<TradeRecord> _tradeHistory;

    // Maximum trade history to keep
    private const int MAX_TRADE_HISTORY = 100;

    public MarketStateTracker(GameWorld gameWorld, ItemRepository itemRepository)
    {
        _gameWorld = gameWorld;
        _itemRepository = itemRepository;
        _marketMetrics = new List<MarketMetrics>();
        _tradeHistory = new List<TradeRecord>();
    }

    /// <summary>
    /// Represents supply and demand metrics for an item at a location
    /// HIGHLANDER: Object references, no string IDs
    /// </summary>
    public class MarketMetrics
    {
        public Venue Venue { get; set; }
        public Item Item { get; set; }
        public float SupplyLevel { get; set; } = 1.0f; // 0.5 = scarce, 1.0 = normal, 2.0 = abundant
        public float DemandLevel { get; set; } = 1.0f; // 0.5 = low demand, 1.0 = normal, 2.0 = high demand
        public int RecentPurchases { get; set; }
        public int RecentSales { get; set; }
        public DateTime LastTradeTime { get; set; }
        public int AveragePrice { get; set; }
    }

    /// <summary>
    /// Record of a completed trade
    /// HIGHLANDER: Object references, no string IDs
    /// Single-player game: No Trader property needed (always THE player)
    /// </summary>
    public class TradeRecord
    {
        public DateTime Timestamp { get; set; }
        public Venue Venue { get; set; }
        public Item Item { get; set; }
        public TradeType Type { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
    }

    public enum TradeType
    {
        Purchase,
        Sale
    }

    /// <summary>
    /// Market conditions summary for a location
    /// HIGHLANDER: Object references, no string IDs
    /// </summary>
    public class MarketConditions
    {
        public Venue Venue { get; set; }
        public int TotalItems { get; set; }
        public int ScarcityItems { get; set; } // Items with low supply
        public int AbundantItems { get; set; } // Items with high supply
        public int HighDemandItems { get; set; }
        public int LowDemandItems { get; set; }
        public List<Item> TrendingItems { get; set; } = new List<Item>(); // Recently traded items
        public float OverallSupplyIndex { get; set; } // Average supply level
        public float OverallDemandIndex { get; set; } // Average demand level
    }

    // ========== SUPPLY & DEMAND TRACKING ==========

    /// <summary>
    /// Get supply level for an item at a venue
    /// HIGHLANDER: Accept typed objects, use object references
    /// </summary>
    public float GetSupplyLevel(Item item, Location location)
    {
        MarketMetrics metrics = GetOrCreateMetrics(location.Venue, item);
        return metrics.SupplyLevel;
    }

    /// <summary>
    /// Get demand level for an item at a venue
    /// HIGHLANDER: Accept typed objects, use object references
    /// </summary>
    public float GetDemandLevel(Item item, Location location)
    {
        MarketMetrics metrics = GetOrCreateMetrics(location.Venue, item);
        return metrics.DemandLevel;
    }

    /// <summary>
    /// Update supply level based on market activity
    /// HIGHLANDER: Accept object references, no string parameters
    /// </summary>
    private void UpdateSupplyLevel(Venue venue, Item item, TradeType tradeType)
    {
        MarketMetrics metrics = GetOrCreateMetrics(venue, item);

        if (tradeType == TradeType.Purchase)
        {
            // Player bought item - supply decreases slightly
            metrics.SupplyLevel = Math.Max(0.3f, metrics.SupplyLevel - 0.1f);
        }
        else if (tradeType == TradeType.Sale)
        {
            // Player sold item - supply increases slightly
            metrics.SupplyLevel = Math.Min(3.0f, metrics.SupplyLevel + 0.1f);
        }
    }

    /// <summary>
    /// Update demand level based on market activity
    /// HIGHLANDER: Accept object references, no string parameters
    /// </summary>
    private void UpdateDemandLevel(Venue venue, Item item, TradeType tradeType)
    {
        MarketMetrics metrics = GetOrCreateMetrics(venue, item);

        if (tradeType == TradeType.Purchase)
        {
            // Player bought item - demand increases (others might want it too)
            metrics.DemandLevel = Math.Min(3.0f, metrics.DemandLevel + 0.05f);
        }
        else if (tradeType == TradeType.Sale)
        {
            // Player sold item - demand might decrease slightly
            metrics.DemandLevel = Math.Max(0.3f, metrics.DemandLevel - 0.02f);
        }
    }

    // ========== TRADE RECORDING ==========

    /// <summary>
    /// Record a purchase transaction
    /// HIGHLANDER: Accept typed objects, pass objects throughout
    /// </summary>
    public void RecordPurchase(Item item, Location location, int price)
    {
        TradeRecord record = new TradeRecord
        {
            Timestamp = DateTime.Now,
            Venue = location.Venue,
            Item = item,
            Type = TradeType.Purchase,
            Price = price,
            Quantity = 1
        };

        AddTradeRecord(record);
        UpdateSupplyLevel(location.Venue, item, TradeType.Purchase);
        UpdateDemandLevel(location.Venue, item, TradeType.Purchase);

        MarketMetrics metrics = GetOrCreateMetrics(location.Venue, item);
        metrics.RecentPurchases++;
        metrics.LastTradeTime = DateTime.Now;
        UpdateAveragePrice(metrics, price);
    }

    /// <summary>
    /// Record a sale transaction
    /// HIGHLANDER: Accept typed objects, pass objects throughout
    /// </summary>
    public void RecordSale(Item item, Location location, int price)
    {
        TradeRecord record = new TradeRecord
        {
            Timestamp = DateTime.Now,
            Venue = location.Venue,
            Item = item,
            Type = TradeType.Sale,
            Price = price,
            Quantity = 1
        };

        AddTradeRecord(record);
        UpdateSupplyLevel(location.Venue, item, TradeType.Sale);
        UpdateDemandLevel(location.Venue, item, TradeType.Sale);

        MarketMetrics metrics = GetOrCreateMetrics(location.Venue, item);
        metrics.RecentSales++;
        metrics.LastTradeTime = DateTime.Now;
        UpdateAveragePrice(metrics, price);
    }

    /// <summary>
    /// Add a trade record and maintain history limit
    /// </summary>
    private void AddTradeRecord(TradeRecord record)
    {
        _tradeHistory.Add(record);

        // Keep only recent history
        if (_tradeHistory.Count > MAX_TRADE_HISTORY)
        {
            _tradeHistory.RemoveAt(0);
        }
    }

    /// <summary>
    /// Update average price based on new trade
    /// </summary>
    private void UpdateAveragePrice(MarketMetrics metrics, int newPrice)
    {
        if (metrics.AveragePrice == 0)
        {
            metrics.AveragePrice = newPrice;
        }
        else
        {
            // Moving average
            metrics.AveragePrice = (metrics.AveragePrice * 3 + newPrice) / 4;
        }
    }

    // ========== MARKET ANALYSIS ==========

    /// <summary>
    /// Get complete market conditions for a venue
    /// HIGHLANDER: Accept typed objects, use object equality
    /// </summary>
    public MarketConditions GetMarketConditions(Location location)
    {
        Venue venue = location.Venue;

        MarketConditions conditions = new MarketConditions
        {
            Venue = venue,
            TrendingItems = new List<Item>()
        };

        List<MarketMetrics> venueMetrics = _marketMetrics
            .Where(m => m.Venue == venue)
            .ToList();

        if (venueMetrics.Count == 0)
        {
            // Return default conditions if no data
            conditions.OverallSupplyIndex = 1.0f;
            conditions.OverallDemandIndex = 1.0f;
            return conditions;
        }

        float totalSupply = 0;
        float totalDemand = 0;

        foreach (MarketMetrics metrics in venueMetrics)
        {
            conditions.TotalItems++;
            totalSupply += metrics.SupplyLevel;
            totalDemand += metrics.DemandLevel;

            if (metrics.SupplyLevel < 0.7f) conditions.ScarcityItems++;
            if (metrics.SupplyLevel > 1.5f) conditions.AbundantItems++;
            if (metrics.DemandLevel > 1.3f) conditions.HighDemandItems++;
            if (metrics.DemandLevel < 0.7f) conditions.LowDemandItems++;

            // Items traded in last hour are trending
            if (metrics.LastTradeTime > DateTime.Now.AddHours(-1))
            {
                conditions.TrendingItems.Add(metrics.Item);
            }
        }

        if (conditions.TotalItems > 0)
        {
            conditions.OverallSupplyIndex = totalSupply / conditions.TotalItems;
            conditions.OverallDemandIndex = totalDemand / conditions.TotalItems;
        }
        else
        {
            conditions.OverallSupplyIndex = 1.0f;
            conditions.OverallDemandIndex = 1.0f;
        }

        return conditions;
    }

    /// <summary>
    /// Get items with best profit margins at a venue
    /// HIGHLANDER: Accept typed objects, return objects
    /// </summary>
    public List<Item> GetHighMarginItems(Location location, int topN = 5)
    {
        Venue venue = location.Venue;

        return _marketMetrics
            .Where(m => m.Venue == venue && m.DemandLevel > 1.2f && m.SupplyLevel < 0.8f)
            .OrderByDescending(m => m.DemandLevel / m.SupplyLevel)
            .Take(topN)
            .Select(m => m.Item)
            .ToList();
    }

    /// <summary>
    /// Get items that are oversupplied at a venue
    /// HIGHLANDER: Accept typed objects, return objects
    /// </summary>
    public List<Item> GetOversuppliedItems(Location location)
    {
        Venue venue = location.Venue;

        return _marketMetrics
            .Where(m => m.Venue == venue && m.SupplyLevel > 1.5f && m.DemandLevel < 1.0f)
            .Select(m => m.Item)
            .ToList();
    }

    // ========== TRADE HISTORY ==========

    /// <summary>
    /// Get complete trade history
    /// </summary>
    public List<TradeRecord> GetTradeHistory()
    {
        return new List<TradeRecord>(_tradeHistory);
    }

    /// <summary>
    /// Get trade history for a specific venue
    /// HIGHLANDER: Accept typed objects, use object equality
    /// </summary>
    public List<TradeRecord> GetLocationTradeHistory(Location location)
    {
        Venue venue = location.Venue;
        return _tradeHistory.Where(t => t.Venue == venue).ToList();
    }

    /// <summary>
    /// Get trade history for a specific item
    /// HIGHLANDER: Accept typed objects, use object equality
    /// </summary>
    public List<TradeRecord> GetItemTradeHistory(Item item)
    {
        return _tradeHistory.Where(t => t.Item == item).ToList();
    }

    /// <summary>
    /// Get recent trades (last N trades)
    /// </summary>
    public List<TradeRecord> GetRecentTrades(int count = 10)
    {
        return _tradeHistory
            .OrderByDescending(t => t.Timestamp)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Calculate trade volume for a venue in a time period
    /// HIGHLANDER: Accept typed objects, use object equality
    /// </summary>
    public int GetTradeVolume(Location location, TimeSpan period)
    {
        Venue venue = location.Venue;

        DateTime cutoff = DateTime.Now - period;
        return _tradeHistory
            .Where(t => t.Venue == venue && t.Timestamp > cutoff)
            .Sum(t => t.Quantity);
    }

    /// <summary>
    /// Calculate total trade value for a venue in a time period
    /// HIGHLANDER: Accept typed objects, use object equality
    /// </summary>
    public int GetTradeValue(Location location, TimeSpan period)
    {
        Venue venue = location.Venue;

        DateTime cutoff = DateTime.Now - period;
        return _tradeHistory
            .Where(t => t.Venue == venue && t.Timestamp > cutoff)
            .Sum(t => t.Price * t.Quantity);
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Get or create metrics for a venue-item pair
    /// HIGHLANDER: Object equality, no string comparisons
    /// </summary>
    private MarketMetrics GetOrCreateMetrics(Venue venue, Item item)
    {
        MarketMetrics metrics = _marketMetrics.FirstOrDefault(m => m.Venue == venue && m.Item == item);

        if (metrics == null)
        {
            metrics = new MarketMetrics
            {
                Venue = venue,
                Item = item
            };
            _marketMetrics.Add(metrics);
        }

        return metrics;
    }

    /// <summary>
    /// Reset market metrics (for testing or new game)
    /// </summary>
    public void ResetMetrics()
    {
        _marketMetrics.Clear();
        _tradeHistory.Clear();
    }

    /// <summary>
    /// Simulate market evolution over time (called periodically)
    /// </summary>
    public void SimulateMarketEvolution()
    {
        // Gradually normalize supply and demand levels
        foreach (MarketMetrics metrics in _marketMetrics)
        {
            // Supply trends toward normal
            if (metrics.SupplyLevel > 1.0f)
                metrics.SupplyLevel = Math.Max(1.0f, metrics.SupplyLevel - 0.02f);
            else if (metrics.SupplyLevel < 1.0f)
                metrics.SupplyLevel = Math.Min(1.0f, metrics.SupplyLevel + 0.02f);

            // Demand trends toward normal
            if (metrics.DemandLevel > 1.0f)
                metrics.DemandLevel = Math.Max(1.0f, metrics.DemandLevel - 0.01f);
            else if (metrics.DemandLevel < 1.0f)
                metrics.DemandLevel = Math.Min(1.0f, metrics.DemandLevel + 0.01f);
        }
    }
}
