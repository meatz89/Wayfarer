/// <summary>
/// Tracks market conditions, inventory levels, and trade history
/// to provide insights into supply, demand, and market dynamics.
/// DDR-007: Uses tier-based enums instead of percentages
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

    /// <summary>
    /// DDR-007: Discrete supply tiers with explicit price adjustments
    /// Replaces percentage-based supply levels
    /// </summary>
    public enum SupplyTier
    {
        SevereShortagePlus12 = 0,  // +12 coins - critical shortage
        LowSupplyPlus9 = 1,        // +9 coins - supply issues
        BelowNormalPlus6 = 2,      // +6 coins - below normal
        SlightlyLowPlus3 = 3,      // +3 coins - slightly constrained
        Normal = 4,                 // 0 coins - baseline
        SlightSurplusMinus3 = 5,   // -3 coins - minor oversupply
        MajorSurplusMinus6 = 6     // -6 coins - significant oversupply
    }

    /// <summary>
    /// DDR-007: Discrete demand tiers with explicit price adjustments
    /// Replaces percentage-based demand levels
    /// </summary>
    public enum DemandTier
    {
        NoDemandMinus6 = 0,        // -6 coins - nobody wants this
        VeryLowMinus4 = 1,         // -4 coins - very few buyers
        LowDemandMinus3 = 2,       // -3 coins - below average interest
        BelowNormalMinus1 = 3,     // -1 coin - slightly slow
        Normal = 4,                 // 0 coins - baseline
        HighDemandPlus2 = 5,       // +2 coins - strong interest
        VeryHighPlus4 = 6          // +4 coins - everyone wants this
    }

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
    /// DDR-007: Tier-based enums instead of percentage integers
    /// </summary>
    public class MarketMetrics
    {
        public Venue Venue { get; set; }
        public Item Item { get; set; }
        public SupplyTier Supply { get; set; } = SupplyTier.Normal;
        public DemandTier Demand { get; set; } = DemandTier.Normal;
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
    /// DDR-007: Tier counts instead of percentage indices
    /// </summary>
    public class MarketConditions
    {
        public Venue Venue { get; set; }
        public int TotalItems { get; set; }
        public int ScarcityItems { get; set; } // Items with supply tier below Normal
        public int AbundantItems { get; set; } // Items with supply tier above Normal
        public int HighDemandItems { get; set; } // Items with demand tier above Normal
        public int LowDemandItems { get; set; } // Items with demand tier below Normal
        public List<Item> TrendingItems { get; set; } = new List<Item>(); // Recently traded items
        public SupplyTier OverallSupply { get; set; } = SupplyTier.Normal; // DDR-007: Tier-based
        public DemandTier OverallDemand { get; set; } = DemandTier.Normal; // DDR-007: Tier-based
    }

    // ========== SUPPLY & DEMAND TRACKING ==========

    /// <summary>
    /// Get supply tier for an item at a venue
    /// HIGHLANDER: Accept typed objects, use object references
    /// DDR-007: Returns tier enum, not percentage
    /// </summary>
    public SupplyTier GetSupplyTier(Item item, Location location)
    {
        MarketMetrics metrics = GetOrCreateMetrics(location.Venue, item);
        return metrics.Supply;
    }

    /// <summary>
    /// Get demand tier for an item at a venue
    /// HIGHLANDER: Accept typed objects, use object references
    /// DDR-007: Returns tier enum, not percentage
    /// </summary>
    public DemandTier GetDemandTier(Item item, Location location)
    {
        MarketMetrics metrics = GetOrCreateMetrics(location.Venue, item);
        return metrics.Demand;
    }

    /// <summary>
    /// DDR-007: Convert supply tier to flat coin adjustment
    /// </summary>
    public static int GetSupplyAdjustment(SupplyTier tier)
    {
        return tier switch
        {
            SupplyTier.SevereShortagePlus12 => 12,
            SupplyTier.LowSupplyPlus9 => 9,
            SupplyTier.BelowNormalPlus6 => 6,
            SupplyTier.SlightlyLowPlus3 => 3,
            SupplyTier.Normal => 0,
            SupplyTier.SlightSurplusMinus3 => -3,
            SupplyTier.MajorSurplusMinus6 => -6,
            _ => 0
        };
    }

    /// <summary>
    /// DDR-007: Convert demand tier to flat coin adjustment
    /// </summary>
    public static int GetDemandAdjustment(DemandTier tier)
    {
        return tier switch
        {
            DemandTier.NoDemandMinus6 => -6,
            DemandTier.VeryLowMinus4 => -4,
            DemandTier.LowDemandMinus3 => -3,
            DemandTier.BelowNormalMinus1 => -1,
            DemandTier.Normal => 0,
            DemandTier.HighDemandPlus2 => 2,
            DemandTier.VeryHighPlus4 => 4,
            _ => 0
        };
    }

    /// <summary>
    /// Update supply tier based on market activity
    /// HIGHLANDER: Accept object references, no string parameters
    /// DDR-007: Tier transitions, not percentage math
    /// </summary>
    private void UpdateSupplyTier(Venue venue, Item item, TradeType tradeType)
    {
        MarketMetrics metrics = GetOrCreateMetrics(venue, item);

        if (tradeType == TradeType.Purchase)
        {
            // Player bought item - supply decreases (shift tier toward shortage)
            metrics.Supply = ShiftSupplyTierDown(metrics.Supply);
        }
        else if (tradeType == TradeType.Sale)
        {
            // Player sold item - supply increases (shift tier toward surplus)
            metrics.Supply = ShiftSupplyTierUp(metrics.Supply);
        }
    }

    /// <summary>
    /// Update demand tier based on market activity
    /// HIGHLANDER: Accept object references, no string parameters
    /// DDR-007: Tier transitions, not percentage math
    /// </summary>
    private void UpdateDemandTier(Venue venue, Item item, TradeType tradeType)
    {
        MarketMetrics metrics = GetOrCreateMetrics(venue, item);

        if (tradeType == TradeType.Purchase)
        {
            // Player bought item - demand increases (shift tier toward high demand)
            metrics.Demand = ShiftDemandTierUp(metrics.Demand);
        }
        else if (tradeType == TradeType.Sale)
        {
            // Player sold item - demand might decrease slightly
            // Only shift down every 2nd sale (slower effect)
            if (metrics.RecentSales > 0 && metrics.RecentSales % 2 == 0)
            {
                metrics.Demand = ShiftDemandTierDown(metrics.Demand);
            }
        }
    }

    /// <summary>
    /// DDR-007: Shift supply tier toward shortage (one step)
    /// </summary>
    private static SupplyTier ShiftSupplyTierDown(SupplyTier current)
    {
        return current switch
        {
            SupplyTier.MajorSurplusMinus6 => SupplyTier.SlightSurplusMinus3,
            SupplyTier.SlightSurplusMinus3 => SupplyTier.Normal,
            SupplyTier.Normal => SupplyTier.SlightlyLowPlus3,
            SupplyTier.SlightlyLowPlus3 => SupplyTier.BelowNormalPlus6,
            SupplyTier.BelowNormalPlus6 => SupplyTier.LowSupplyPlus9,
            SupplyTier.LowSupplyPlus9 => SupplyTier.SevereShortagePlus12,
            _ => current // Already at minimum
        };
    }

    /// <summary>
    /// DDR-007: Shift supply tier toward surplus (one step)
    /// </summary>
    private static SupplyTier ShiftSupplyTierUp(SupplyTier current)
    {
        return current switch
        {
            SupplyTier.SevereShortagePlus12 => SupplyTier.LowSupplyPlus9,
            SupplyTier.LowSupplyPlus9 => SupplyTier.BelowNormalPlus6,
            SupplyTier.BelowNormalPlus6 => SupplyTier.SlightlyLowPlus3,
            SupplyTier.SlightlyLowPlus3 => SupplyTier.Normal,
            SupplyTier.Normal => SupplyTier.SlightSurplusMinus3,
            SupplyTier.SlightSurplusMinus3 => SupplyTier.MajorSurplusMinus6,
            _ => current // Already at maximum
        };
    }

    /// <summary>
    /// DDR-007: Shift demand tier toward high demand (one step)
    /// </summary>
    private static DemandTier ShiftDemandTierUp(DemandTier current)
    {
        return current switch
        {
            DemandTier.NoDemandMinus6 => DemandTier.VeryLowMinus4,
            DemandTier.VeryLowMinus4 => DemandTier.LowDemandMinus3,
            DemandTier.LowDemandMinus3 => DemandTier.BelowNormalMinus1,
            DemandTier.BelowNormalMinus1 => DemandTier.Normal,
            DemandTier.Normal => DemandTier.HighDemandPlus2,
            DemandTier.HighDemandPlus2 => DemandTier.VeryHighPlus4,
            _ => current // Already at maximum
        };
    }

    /// <summary>
    /// DDR-007: Shift demand tier toward low demand (one step)
    /// </summary>
    private static DemandTier ShiftDemandTierDown(DemandTier current)
    {
        return current switch
        {
            DemandTier.VeryHighPlus4 => DemandTier.HighDemandPlus2,
            DemandTier.HighDemandPlus2 => DemandTier.Normal,
            DemandTier.Normal => DemandTier.BelowNormalMinus1,
            DemandTier.BelowNormalMinus1 => DemandTier.LowDemandMinus3,
            DemandTier.LowDemandMinus3 => DemandTier.VeryLowMinus4,
            DemandTier.VeryLowMinus4 => DemandTier.NoDemandMinus6,
            _ => current // Already at minimum
        };
    }

    // ========== TRADE RECORDING ==========

    /// <summary>
    /// Record a purchase transaction
    /// HIGHLANDER: Accept typed objects, pass objects throughout
    /// DDR-007: Updates tier-based supply/demand
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
        UpdateSupplyTier(location.Venue, item, TradeType.Purchase);
        UpdateDemandTier(location.Venue, item, TradeType.Purchase);

        MarketMetrics metrics = GetOrCreateMetrics(location.Venue, item);
        metrics.RecentPurchases++;
        metrics.LastTradeTime = DateTime.Now;
        UpdateAveragePrice(metrics, price);
    }

    /// <summary>
    /// Record a sale transaction
    /// HIGHLANDER: Accept typed objects, pass objects throughout
    /// DDR-007: Updates tier-based supply/demand
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
        UpdateSupplyTier(location.Venue, item, TradeType.Sale);
        UpdateDemandTier(location.Venue, item, TradeType.Sale);

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
    /// DDR-007: Tier-based aggregation, no percentages
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
            // Return default conditions if no data (Normal tiers by default)
            return conditions;
        }

        int supplyScore = 0;
        int demandScore = 0;

        foreach (MarketMetrics metrics in venueMetrics)
        {
            conditions.TotalItems++;

            // DDR-007: Use tier comparisons, not numeric thresholds
            if (metrics.Supply < SupplyTier.Normal) conditions.ScarcityItems++;
            if (metrics.Supply > SupplyTier.Normal) conditions.AbundantItems++;
            if (metrics.Demand > DemandTier.Normal) conditions.HighDemandItems++;
            if (metrics.Demand < DemandTier.Normal) conditions.LowDemandItems++;

            // Accumulate tier ordinals for average calculation
            supplyScore += (int)metrics.Supply;
            demandScore += (int)metrics.Demand;

            // Items traded in last hour are trending
            if (metrics.LastTradeTime > DateTime.Now.AddHours(-1))
            {
                conditions.TrendingItems.Add(metrics.Item);
            }
        }

        if (conditions.TotalItems > 0)
        {
            // DDR-007: Integer division to get average tier ordinal
            int avgSupplyOrdinal = supplyScore / conditions.TotalItems;
            int avgDemandOrdinal = demandScore / conditions.TotalItems;

            // Clamp to valid enum ranges
            avgSupplyOrdinal = Math.Max(0, Math.Min(6, avgSupplyOrdinal));
            avgDemandOrdinal = Math.Max(0, Math.Min(6, avgDemandOrdinal));

            conditions.OverallSupply = (SupplyTier)avgSupplyOrdinal;
            conditions.OverallDemand = (DemandTier)avgDemandOrdinal;
        }

        return conditions;
    }

    /// <summary>
    /// Get items with best profit margins at a venue
    /// HIGHLANDER: Accept typed objects, return objects
    /// HIGHLANDER: topN REQUIRED - caller specifies how many items to return
    /// DDR-007: Tier-based filtering
    /// </summary>
    public List<Item> GetHighMarginItems(Location location, int topN)
    {
        Venue venue = location.Venue;

        // DDR-007: High margin = high demand tier + low supply tier
        return _marketMetrics
            .Where(m => m.Venue == venue &&
                        m.Demand >= DemandTier.HighDemandPlus2 &&
                        m.Supply <= SupplyTier.SlightlyLowPlus3)
            .OrderByDescending(m => (int)m.Demand - (int)m.Supply)
            .Take(topN)
            .Select(m => m.Item)
            .ToList();
    }

    /// <summary>
    /// Get items that are oversupplied at a venue
    /// HIGHLANDER: Accept typed objects, return objects
    /// DDR-007: Tier-based filtering
    /// </summary>
    public List<Item> GetOversuppliedItems(Location location)
    {
        Venue venue = location.Venue;

        // DDR-007: Oversupplied = supply surplus tier + normal or below demand
        return _marketMetrics
            .Where(m => m.Venue == venue &&
                        m.Supply >= SupplyTier.SlightSurplusMinus3 &&
                        m.Demand <= DemandTier.Normal)
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
    /// HIGHLANDER: count REQUIRED - caller specifies how many trades to return
    /// </summary>
    public List<TradeRecord> GetRecentTrades(int count)
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
    /// DDR-007: Tier-based normalization toward Normal
    /// </summary>
    public void SimulateMarketEvolution()
    {
        // Gradually normalize supply and demand tiers toward Normal
        foreach (MarketMetrics metrics in _marketMetrics)
        {
            // Supply trends toward Normal (one step)
            if (metrics.Supply > SupplyTier.Normal)
                metrics.Supply = ShiftSupplyTierDown(metrics.Supply);
            else if (metrics.Supply < SupplyTier.Normal)
                metrics.Supply = ShiftSupplyTierUp(metrics.Supply);

            // Demand trends toward Normal (one step)
            if (metrics.Demand > DemandTier.Normal)
                metrics.Demand = ShiftDemandTierDown(metrics.Demand);
            else if (metrics.Demand < DemandTier.Normal)
                metrics.Demand = ShiftDemandTierUp(metrics.Demand);
        }
    }
}
