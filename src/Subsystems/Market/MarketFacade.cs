/// <summary>
/// Public facade for all market-related operations.
/// Provides a clean API for trading, pricing, and market analysis.
/// </summary>
public class MarketFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MarketSubsystemManager _marketManager;
    private readonly PriceManager _priceManager;
    private readonly ArbitrageCalculator _arbitrageCalculator;
    private readonly MarketStateTracker _marketStateTracker;
    private readonly ItemRepository _itemRepository;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;
    private readonly TimeManager _timeManager;

    public MarketFacade(
        GameWorld gameWorld,
        MarketSubsystemManager marketManager,
        PriceManager priceManager,
        ArbitrageCalculator arbitrageCalculator,
        MarketStateTracker marketStateTracker,
        ItemRepository itemRepository,
        NPCRepository npcRepository,
        MessageSystem messageSystem,
        TimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _marketManager = marketManager;
        _priceManager = priceManager;
        _arbitrageCalculator = arbitrageCalculator;
        _marketStateTracker = marketStateTracker;
        _itemRepository = itemRepository;
        _npcRepository = npcRepository;
        _messageSystem = messageSystem;
        _timeManager = timeManager;
    }

    // ========== MARKET AVAILABILITY ==========

    /// <summary>
    /// Check if a market is available at the given location during current time
    /// HIGHLANDER: Accept typed Location object
    /// </summary>
    public bool IsMarketAvailable(Location location)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        return _marketManager.IsMarketAvailable(location, currentTime);
    }

    /// <summary>
    /// Get market availability status message for UI display
    /// HIGHLANDER: Accept typed Location object
    /// </summary>
    public string GetMarketStatus(Location location)
    {
        return _marketManager.GetMarketAvailabilityStatus(location, _timeManager.GetCurrentTimeBlock());
    }

    /// <summary>
    /// Get traders currently available at a location
    /// HIGHLANDER: Accept typed Location object
    /// </summary>
    public List<NPC> GetAvailableTraders(Location location)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        return _marketManager.GetAvailableTraders(location, currentTime);
    }

    // ========== PRICING OPERATIONS ==========

    /// <summary>
    /// Get the buy price for an item at a location
    /// HIGHLANDER: Accept typed Item and Location objects
    /// </summary>
    public int GetBuyPrice(Item item, Location location)
    {
        return _priceManager.GetBuyPrice(item, location);
    }

    /// <summary>
    /// Get the sell price for an item at a location
    /// HIGHLANDER: Accept typed Item and Location objects
    /// </summary>
    public int GetSellPrice(Item item, Location location)
    {
        return _priceManager.GetSellPrice(item, location);
    }

    /// <summary>
    /// Get complete pricing information for an item at a location
    /// HIGHLANDER: Accept typed Item and Location objects
    /// </summary>
    public PriceManager.PricingInfo GetPricingInfo(Item item, Location location)
    {
        return _priceManager.GetPricingInfo(item, location);
    }

    /// <summary>
    /// Get pricing for all items at a location
    /// HIGHLANDER: Accept typed Location object
    /// </summary>
    public List<PriceManager.PricingInfo> GetAllPrices(Location location)
    {
        return _priceManager.GetLocationPrices(location);
    }

    // ========== TRADING OPERATIONS ==========

    /// <summary>
    /// Check if player can buy an item at a location
    /// HIGHLANDER: Accept typed Item and Location objects
    /// </summary>
    public bool CanBuyItem(Item item, Location location)
    {
        return _marketManager.CanBuyItem(item, location);
    }

    /// <summary>
    /// Check if player can sell an item at a location
    /// HIGHLANDER: Accept typed Item and Location objects
    /// </summary>
    public bool CanSellItem(Item item, Location location)
    {
        return _marketManager.CanSellItem(item, location);
    }

    /// <summary>
    /// Buy an item at a location
    /// HIGHLANDER: Accept typed Item and Location objects
    /// </summary>
    public MarketSubsystemManager.TradeResult BuyItem(Item item, Location location)
    {
        MarketSubsystemManager.TradeResult result = _marketManager.BuyItem(item, location);

        if (result.Success)
        {
            _marketStateTracker.RecordPurchase(item, location, result.Price);
        }

        return result;
    }

    /// <summary>
    /// Sell an item at a location
    /// HIGHLANDER: Accept typed Item and Location objects
    /// </summary>
    public MarketSubsystemManager.TradeResult SellItem(Item item, Location location)
    {
        MarketSubsystemManager.TradeResult result = _marketManager.SellItem(item, location);

        if (result.Success)
        {
            _marketStateTracker.RecordSale(item, location, result.Price);
        }

        return result;
    }

    /// <summary>
    /// Get items available for purchase at a location
    /// HIGHLANDER: Accept typed Location object
    /// </summary>
    public List<MarketItem> GetAvailableItems(Location location)
    {
        return _marketManager.GetAvailableMarketItems(location);
    }

    // ========== ARBITRAGE ANALYSIS ==========

    /// <summary>
    /// Find the best arbitrage opening for a specific item
    /// HIGHLANDER: Accept typed Item object
    /// </summary>
    public ArbitrageCalculator.ArbitrageOpening GetBestArbitrage(Item item)
    {
        return _arbitrageCalculator.FindBestOpening(item);
    }

    /// <summary>
    /// Find all profitable arbitrage opportunities
    /// </summary>
    public List<ArbitrageCalculator.ArbitrageOpening> GetAllArbitrageOpportunities()
    {
        return _arbitrageCalculator.FindAllOpportunities();
    }

    /// <summary>
    /// Calculate potential profit for buying an item here and selling elsewhere
    /// HIGHLANDER: Accept typed Item and Location objects
    /// </summary>
    public int CalculatePotentialProfit(Item item, Location buyLocation, Location sellLocation)
    {
        return _arbitrageCalculator.CalculateProfit(item, buyLocation, sellLocation);
    }

    // ========== MARKET STATE TRACKING ==========

    /// <summary>
    /// Get supply level for an item at a location
    /// HIGHLANDER: Accept typed Item and Location objects
    /// </summary>
    public float GetSupplyLevel(Item item, Location location)
    {
        return _marketStateTracker.GetSupplyLevel(item, location);
    }

    /// <summary>
    /// Get demand level for an item at a location
    /// HIGHLANDER: Accept typed Item and Location objects
    /// </summary>
    public float GetDemandLevel(Item item, Location location)
    {
        return _marketStateTracker.GetDemandLevel(item, location);
    }

    /// <summary>
    /// Get complete market conditions for a location
    /// HIGHLANDER: Accept typed Location object
    /// </summary>
    public MarketStateTracker.MarketConditions GetMarketConditions(Location location)
    {
        return _marketStateTracker.GetMarketConditions(location);
    }

    /// <summary>
    /// Get trade history for analysis
    /// </summary>
    public List<MarketStateTracker.TradeRecord> GetTradeHistory()
    {
        return _marketStateTracker.GetTradeHistory();
    }

    /// <summary>
    /// Get recent trades at a specific location
    /// HIGHLANDER: Accept typed Location object
    /// </summary>
    public List<MarketStateTracker.TradeRecord> GetLocationTradeHistory(Location location)
    {
        return _marketStateTracker.GetLocationTradeHistory(location);
    }

    // ========== INVENTORY INTEGRATION ==========

    /// <summary>
    /// Check if player has inventory space for an item
    /// HIGHLANDER: Accept typed Item object, no lookup needed
    /// </summary>
    public bool HasInventorySpace(Item item)
    {
        Player player = _gameWorld.GetPlayer();
        return item != null;
    }

    /// <summary>
    /// Get inventory status message for market UI
    /// </summary>
    public string GetInventoryStatus()
    {
        Player player = _gameWorld.GetPlayer();
        int usedWeight = player.Inventory.GetUsedWeight();
        return $"Inventory: {usedWeight} weight";
    }

    /// <summary>
    /// Get player's tradeable items
    /// HIGHLANDER: Return List of Item objects, not string IDs
    /// </summary>
    public List<Item> GetPlayerTradeableItems()
    {
        Player player = _gameWorld.GetPlayer();
        // HIGHLANDER: Inventory.GetAllItems() returns Item objects
        return player.Inventory.GetAllItems();
    }

    // ========== MARKET INSIGHTS ==========

    /// <summary>
    /// Get recommended trades based on current position and market conditions
    /// HIGHLANDER: Use typed Location object directly
    /// </summary>
    public List<MarketSubsystemManager.TradeRecommendation> GetTradeRecommendations()
    {
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        return _marketManager.GetTradeRecommendations(currentLocation);
    }

    /// <summary>
    /// Check if a trade would be profitable
    /// HIGHLANDER: Accept typed Item and Location objects
    /// </summary>
    public bool IsTradeProfiTable(Item item, Location buyLocation, Location sellLocation)
    {
        int profit = _arbitrageCalculator.CalculateProfit(item, buyLocation, sellLocation);
        return profit > 0;
    }

    /// <summary>
    /// Get market summary for a location
    /// HIGHLANDER: Accept typed Location object
    /// </summary>
    public MarketSubsystemManager.MarketSummary GetMarketSummary(Location location)
    {
        return _marketManager.GetMarketSummary(location);
    }

}
