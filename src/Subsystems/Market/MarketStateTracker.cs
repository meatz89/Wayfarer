/// <summary>
/// Tracks market conditions, inventory levels, and trade history
/// to provide insights into supply, demand, and market dynamics.
/// </summary>
public class MarketStateTracker
{
private readonly GameWorld _gameWorld;
private readonly ItemRepository _itemRepository;

// Track supply and demand per Venue per item
private Dictionary<string, Dictionary<string, MarketMetrics>> _marketMetrics;

// Track trade history for trend analysis
private List<TradeRecord> _tradeHistory;

// Maximum trade history to keep
private const int MAX_TRADE_HISTORY = 100;

public MarketStateTracker(GameWorld gameWorld, ItemRepository itemRepository)
{
    _gameWorld = gameWorld;
    _itemRepository = itemRepository;
    _marketMetrics = new Dictionary<string, Dictionary<string, MarketMetrics>>();
    _tradeHistory = new List<TradeRecord>();
}

/// <summary>
/// Represents supply and demand metrics for an item at a location
/// </summary>
public class MarketMetrics
{
    public float SupplyLevel { get; set; } = 1.0f; // 0.5 = scarce, 1.0 = normal, 2.0 = abundant
    public float DemandLevel { get; set; } = 1.0f; // 0.5 = low demand, 1.0 = normal, 2.0 = high demand
    public int RecentPurchases { get; set; }
    public int RecentSales { get; set; }
    public DateTime LastTradeTime { get; set; }
    public int AveragePrice { get; set; }
}

/// <summary>
/// Record of a completed trade
/// </summary>
public class TradeRecord
{
    public DateTime Timestamp { get; set; }
    public string VenueId { get; set; }
    public string ItemId { get; set; }
    public TradeType Type { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }
    public string TraderId { get; set; }
}

public enum TradeType
{
    Purchase,
    Sale
}

/// <summary>
/// Market conditions summary for a location
/// </summary>
public class MarketConditions
{
    public string VenueId { get; set; }
    public int TotalItems { get; set; }
    public int ScarcityItems { get; set; } // Items with low supply
    public int AbundantItems { get; set; } // Items with high supply
    public int HighDemandItems { get; set; }
    public int LowDemandItems { get; set; }
    public List<string> TrendingItems { get; set; } // Recently traded items
    public float OverallSupplyIndex { get; set; } // Average supply level
    public float OverallDemandIndex { get; set; } // Average demand level
}

// ========== SUPPLY & DEMAND TRACKING ==========

/// <summary>
/// Get supply level for an item at a location
/// </summary>
public float GetSupplyLevel(string itemId, string venueId)
{
    MarketMetrics metrics = GetOrCreateMetrics(venueId, itemId);
    return metrics.SupplyLevel;
}

/// <summary>
/// Get demand level for an item at a location
/// </summary>
public float GetDemandLevel(string itemId, string venueId)
{
    MarketMetrics metrics = GetOrCreateMetrics(venueId, itemId);
    return metrics.DemandLevel;
}

/// <summary>
/// Update supply level based on market activity
/// </summary>
private void UpdateSupplyLevel(string venueId, string itemId, TradeType tradeType)
{
    MarketMetrics metrics = GetOrCreateMetrics(venueId, itemId);

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
/// </summary>
private void UpdateDemandLevel(string venueId, string itemId, TradeType tradeType)
{
    MarketMetrics metrics = GetOrCreateMetrics(venueId, itemId);

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
/// </summary>
public void RecordPurchase(string itemId, string venueId, int price)
{
    TradeRecord record = new TradeRecord
    {
        Timestamp = DateTime.Now,
        VenueId = venueId,
        ItemId = itemId,
        Type = TradeType.Purchase,
        Price = price,
        Quantity = 1,
        TraderId = _gameWorld.GetPlayer().Name
    };

    AddTradeRecord(record);
    UpdateSupplyLevel(venueId, itemId, TradeType.Purchase);
    UpdateDemandLevel(venueId, itemId, TradeType.Purchase);

    MarketMetrics metrics = GetOrCreateMetrics(venueId, itemId);
    metrics.RecentPurchases++;
    metrics.LastTradeTime = DateTime.Now;
    UpdateAveragePrice(metrics, price);
}

/// <summary>
/// Record a sale transaction
/// </summary>
public void RecordSale(string itemId, string venueId, int price)
{
    TradeRecord record = new TradeRecord
    {
        Timestamp = DateTime.Now,
        VenueId = venueId,
        ItemId = itemId,
        Type = TradeType.Sale,
        Price = price,
        Quantity = 1,
        TraderId = _gameWorld.GetPlayer().Name
    };

    AddTradeRecord(record);
    UpdateSupplyLevel(venueId, itemId, TradeType.Sale);
    UpdateDemandLevel(venueId, itemId, TradeType.Sale);

    MarketMetrics metrics = GetOrCreateMetrics(venueId, itemId);
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
/// Get complete market conditions for a location
/// </summary>
public MarketConditions GetMarketConditions(string venueId)
{
    MarketConditions conditions = new MarketConditions
    {
        VenueId = venueId,
        TrendingItems = new List<string>()
    };

    if (!_marketMetrics.ContainsKey(venueId))
    {
        // Return default conditions if no data
        conditions.OverallSupplyIndex = 1.0f;
        conditions.OverallDemandIndex = 1.0f;
        return conditions;
    }

    Dictionary<string, MarketMetrics> locationMetrics = _marketMetrics[venueId];

    float totalSupply = 0;
    float totalDemand = 0;

    foreach (KeyValuePair<string, MarketMetrics> kvp in locationMetrics)
    {
        MarketMetrics metrics = kvp.Value;

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
            conditions.TrendingItems.Add(kvp.Key);
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
/// Get items with best profit margins at a location
/// </summary>
public List<string> GetHighMarginItems(string venueId, int topN = 5)
{
    if (!_marketMetrics.ContainsKey(venueId))
        return new List<string>();

    return _marketMetrics[venueId]
        .Where(kvp => kvp.Value.DemandLevel > 1.2f && kvp.Value.SupplyLevel < 0.8f)
        .OrderByDescending(kvp => kvp.Value.DemandLevel / kvp.Value.SupplyLevel)
        .Take(topN)
        .Select(kvp => kvp.Key)
        .ToList();
}

/// <summary>
/// Get items that are oversupplied at a location
/// </summary>
public List<string> GetOversuppliedItems(string venueId)
{
    if (!_marketMetrics.ContainsKey(venueId))
        return new List<string>();

    return _marketMetrics[venueId]
        .Where(kvp => kvp.Value.SupplyLevel > 1.5f && kvp.Value.DemandLevel < 1.0f)
        .Select(kvp => kvp.Key)
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
/// Get trade history for a specific location
/// </summary>
public List<TradeRecord> GetLocationTradeHistory(string venueId)
{
    return _tradeHistory.Where(t => t.VenueId == venueId).ToList();
}

/// <summary>
/// Get trade history for a specific item
/// </summary>
public List<TradeRecord> GetItemTradeHistory(string itemId)
{
    return _tradeHistory.Where(t => t.ItemId == itemId).ToList();
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
/// Calculate trade volume for a Venue in a time period
/// </summary>
public int GetTradeVolume(string venueId, TimeSpan period)
{
    DateTime cutoff = DateTime.Now - period;
    return _tradeHistory
        .Where(t => t.VenueId == venueId && t.Timestamp > cutoff)
        .Sum(t => t.Quantity);
}

/// <summary>
/// Calculate total trade value for a Venue in a time period
/// </summary>
public int GetTradeValue(string venueId, TimeSpan period)
{
    DateTime cutoff = DateTime.Now - period;
    return _tradeHistory
        .Where(t => t.VenueId == venueId && t.Timestamp > cutoff)
        .Sum(t => t.Price * t.Quantity);
}

// ========== HELPER METHODS ==========

/// <summary>
/// Get or create metrics for a location-item pair
/// </summary>
private MarketMetrics GetOrCreateMetrics(string venueId, string itemId)
{
    if (!_marketMetrics.ContainsKey(venueId))
    {
        _marketMetrics[venueId] = new Dictionary<string, MarketMetrics>();
    }

    if (!_marketMetrics[venueId].ContainsKey(itemId))
    {
        _marketMetrics[venueId][itemId] = new MarketMetrics();
    }

    return _marketMetrics[venueId][itemId];
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
    foreach (Dictionary<string, MarketMetrics> locationMetrics in _marketMetrics.Values)
    {
        foreach (MarketMetrics metrics in locationMetrics.Values)
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
}
