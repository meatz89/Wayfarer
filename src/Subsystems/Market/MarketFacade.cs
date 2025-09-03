using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.MarketSubsystem
{
    /// <summary>
    /// Public facade for all market-related operations.
    /// Provides a clean API for trading, pricing, and market analysis.
    /// </summary>
    public class MarketFacade
    {
        private readonly GameWorld _gameWorld;
        private readonly MarketManager _legacyMarketManager;
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
            MarketManager legacyMarketManager,
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
            _legacyMarketManager = legacyMarketManager;
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
        /// </summary>
        public bool IsMarketAvailable(string locationId)
        {
            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
            return _marketManager.IsMarketAvailable(locationId, currentTime);
        }

        /// <summary>
        /// Get market availability status message for UI display
        /// </summary>
        public string GetMarketStatus(string locationId)
        {
            return _marketManager.GetMarketAvailabilityStatus(locationId, _timeManager.GetCurrentTimeBlock());
        }

        /// <summary>
        /// Get traders currently available at a location
        /// </summary>
        public List<NPC> GetAvailableTraders(string locationId)
        {
            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
            return _marketManager.GetAvailableTraders(locationId, currentTime);
        }

        // ========== PRICING OPERATIONS ==========

        /// <summary>
        /// Get the buy price for an item at a location
        /// </summary>
        public int GetBuyPrice(string itemId, string locationId)
        {
            return _priceManager.GetBuyPrice(itemId, locationId);
        }

        /// <summary>
        /// Get the sell price for an item at a location
        /// </summary>
        public int GetSellPrice(string itemId, string locationId)
        {
            return _priceManager.GetSellPrice(itemId, locationId);
        }

        /// <summary>
        /// Get complete pricing information for an item at a location
        /// </summary>
        public PriceManager.PricingInfo GetPricingInfo(string itemId, string locationId)
        {
            return _priceManager.GetPricingInfo(itemId, locationId);
        }

        /// <summary>
        /// Get pricing for all items at a location
        /// </summary>
        public List<PriceManager.PricingInfo> GetAllPrices(string locationId)
        {
            return _priceManager.GetLocationPrices(locationId);
        }

        // ========== TRADING OPERATIONS ==========

        /// <summary>
        /// Check if player can buy an item at a location
        /// </summary>
        public bool CanBuyItem(string itemId, string locationId)
        {
            return _marketManager.CanBuyItem(itemId, locationId);
        }

        /// <summary>
        /// Check if player can sell an item at a location
        /// </summary>
        public bool CanSellItem(string itemId, string locationId)
        {
            return _marketManager.CanSellItem(itemId, locationId);
        }

        /// <summary>
        /// Buy an item at a location
        /// </summary>
        public MarketSubsystemManager.TradeResult BuyItem(string itemId, string locationId)
        {
            MarketSubsystemManager.TradeResult result = _marketManager.BuyItem(itemId, locationId);

            if (result.Success)
            {
                // Track the purchase in market state
                _marketStateTracker.RecordPurchase(itemId, locationId, result.Price);
            }

            return result;
        }

        /// <summary>
        /// Sell an item at a location
        /// </summary>
        public MarketSubsystemManager.TradeResult SellItem(string itemId, string locationId)
        {
            MarketSubsystemManager.TradeResult result = _marketManager.SellItem(itemId, locationId);

            if (result.Success)
            {
                // Track the sale in market state
                _marketStateTracker.RecordSale(itemId, locationId, result.Price);
            }

            return result;
        }

        /// <summary>
        /// Get items available for purchase at a location
        /// </summary>
        public List<MarketItem> GetAvailableItems(string locationId)
        {
            return _marketManager.GetAvailableItems(locationId);
        }

        // ========== ARBITRAGE ANALYSIS ==========

        /// <summary>
        /// Find the best arbitrage opening for a specific item
        /// </summary>
        public ArbitrageCalculator.ArbitrageOpening GetBestArbitrage(string itemId)
        {
            return _arbitrageCalculator.FindBestOpening(itemId);
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
        /// </summary>
        public int CalculatePotentialProfit(string itemId, string buyLocation, string sellLocation)
        {
            return _arbitrageCalculator.CalculateProfit(itemId, buyLocation, sellLocation);
        }

        // ========== MARKET STATE TRACKING ==========

        /// <summary>
        /// Get supply level for an item at a location
        /// </summary>
        public float GetSupplyLevel(string itemId, string locationId)
        {
            return _marketStateTracker.GetSupplyLevel(itemId, locationId);
        }

        /// <summary>
        /// Get demand level for an item at a location
        /// </summary>
        public float GetDemandLevel(string itemId, string locationId)
        {
            return _marketStateTracker.GetDemandLevel(itemId, locationId);
        }

        /// <summary>
        /// Get complete market conditions for a location
        /// </summary>
        public MarketStateTracker.MarketConditions GetMarketConditions(string locationId)
        {
            return _marketStateTracker.GetMarketConditions(locationId);
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
        /// </summary>
        public List<MarketStateTracker.TradeRecord> GetLocationTradeHistory(string locationId)
        {
            return _marketStateTracker.GetLocationTradeHistory(locationId);
        }

        // ========== INVENTORY INTEGRATION ==========

        /// <summary>
        /// Check if player has inventory space for an item
        /// </summary>
        public bool HasInventorySpace(string itemId)
        {
            Player player = _gameWorld.GetPlayer();
            Item item = _itemRepository.GetItemById(itemId);
            return item != null && player.Inventory.CanAddItem(item, _itemRepository);
        }

        /// <summary>
        /// Get inventory status message for market UI
        /// </summary>
        public string GetInventoryStatus()
        {
            Player player = _gameWorld.GetPlayer();
            int usedSlots = player.Inventory.GetUsedSlots(_itemRepository);
            int maxSlots = player.Inventory.GetMaxSlots(_itemRepository);
            return $"Inventory: {usedSlots}/{maxSlots} slots";
        }

        /// <summary>
        /// Get player's tradeable items
        /// </summary>
        public List<string> GetPlayerTradeableItems()
        {
            Player player = _gameWorld.GetPlayer();
            return player.Inventory.GetItemIds();
        }

        // ========== MARKET INSIGHTS ==========

        /// <summary>
        /// Get recommended trades based on current position and market conditions
        /// </summary>
        public List<MarketSubsystemManager.TradeRecommendation> GetTradeRecommendations()
        {
            string currentLocation = _gameWorld.GetPlayer().CurrentLocationSpot?.LocationId;
            return _marketManager.GetTradeRecommendations(currentLocation);
        }

        /// <summary>
        /// Check if a trade would be profitable
        /// </summary>
        public bool IsTradeProfiTable(string itemId, string buyLocation, string sellLocation)
        {
            int profit = _arbitrageCalculator.CalculateProfit(itemId, buyLocation, sellLocation);
            return profit > 0;
        }

        /// <summary>
        /// Get market summary for a location
        /// </summary>
        public MarketSubsystemManager.MarketSummary GetMarketSummary(string locationId)
        {
            return _marketManager.GetMarketSummary(locationId);
        }

    }
}