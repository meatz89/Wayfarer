using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.MarketSubsystem
{
    /// <summary>
    /// Core market logic manager that wraps and enhances the legacy MarketManager.
    /// Provides clean APIs for trading operations and market queries.
    /// </summary>
    public class MarketSubsystemManager
    {
        private readonly GameWorld _gameWorld;
        private readonly MarketManager _legacyMarketManager;
        private readonly ItemRepository _itemRepository;
        private readonly NPCRepository _npcRepository;
        private readonly MessageSystem _messageSystem;
        private readonly TimeManager _timeManager;

        public MarketSubsystemManager(
            GameWorld gameWorld,
            MarketManager legacyMarketManager,
            ItemRepository itemRepository,
            NPCRepository npcRepository,
            MessageSystem messageSystem,
            TimeManager timeManager)
        {
            _gameWorld = gameWorld;
            _legacyMarketManager = legacyMarketManager;
            _itemRepository = itemRepository;
            _npcRepository = npcRepository;
            _messageSystem = messageSystem;
            _timeManager = timeManager;
        }

        /// <summary>
        /// Result of a trade operation
        /// </summary>
        public class TradeResult
        {
            public bool Success { get; set; }
            public string ItemId { get; set; }
            public string ItemName { get; set; }
            public string LocationId { get; set; }
            public TradeAction Action { get; set; }
            public int Price { get; set; }
            public int CoinsBefore { get; set; }
            public int CoinsAfter { get; set; }
            public bool HadItemBefore { get; set; }
            public bool HasItemAfter { get; set; }
            public string Message { get; set; }
            public string ErrorReason { get; set; }
            public List<string> SystemMessages { get; set; } = new List<string>();
        }

        public enum TradeAction
        {
            Buy,
            Sell
        }

        /// <summary>
        /// Trade recommendation for strategic planning
        /// </summary>
        public class TradeRecommendation
        {
            public string ItemId { get; set; }
            public string ItemName { get; set; }
            public TradeAction RecommendedAction { get; set; }
            public string LocationId { get; set; }
            public string LocationName { get; set; }
            public int ExpectedProfit { get; set; }
            public string Reasoning { get; set; }
            public float Confidence { get; set; } // 0.0 to 1.0
        }

        /// <summary>
        /// Market summary for a location
        /// </summary>
        public class MarketSummary
        {
            public string LocationId { get; set; }
            public string LocationName { get; set; }
            public bool IsOpen { get; set; }
            public List<string> AvailableTraders { get; set; } = new List<string>();
            public int TotalItemsAvailable { get; set; }
            public int AffordableItems { get; set; }
            public int ProfitableToSell { get; set; }
            public string MarketStatus { get; set; }
            public TimeBlocks NextOpenTime { get; set; }
        }

        // ========== MARKET AVAILABILITY ==========

        /// <summary>
        /// Check if market is available at a location at a specific time
        /// </summary>
        public bool IsMarketAvailable(string locationId, TimeBlocks timeBlock)
        {
            List<NPC> traders = GetTradersAtTime(locationId, timeBlock);
            return traders.Count > 0;
        }

        /// <summary>
        /// Get market availability status message
        /// </summary>
        public string GetMarketAvailabilityStatus(string locationId, TimeBlocks currentTime)
        {
            List<NPC> currentTraders = GetTradersAtTime(locationId, currentTime);

            if (currentTraders.Count > 0)
            {
                string traderNames = string.Join(", ", currentTraders.Select(t => t.Name));
                return $"Market Open - Traders: {traderNames}";
            }

            // Find next available time
            List<NPC> allTraders = GetAllTraders(locationId);
            if (allTraders.Count == 0)
            {
                return "No traders at this location";
            }

            TimeBlocks[] allTimes = { TimeBlocks.Dawn, TimeBlocks.Morning, TimeBlocks.Afternoon,
                                     TimeBlocks.Evening, TimeBlocks.Night, TimeBlocks.LateNight };

            foreach (TimeBlocks time in allTimes)
            {
                if (time <= currentTime) continue; // Skip past and current times

                List<NPC> futureTraders = GetTradersAtTime(locationId, time);
                if (futureTraders.Count > 0)
                {
                    return $"Market closed - Opens at {time}";
                }
            }

            return "Market closed";
        }

        /// <summary>
        /// Get traders available at current time
        /// </summary>
        public List<NPC> GetAvailableTraders(string locationId, TimeBlocks timeBlock)
        {
            return GetTradersAtTime(locationId, timeBlock);
        }

        // ========== TRADING OPERATIONS ==========

        /// <summary>
        /// Check if player can buy an item
        /// </summary>
        public bool CanBuyItem(string itemId, string locationId)
        {
            // Check market availability
            if (!IsMarketAvailable(locationId, _timeManager.GetCurrentTimeBlock()))
                return false;

            // Check item availability and player resources
            return _legacyMarketManager.CanBuyItem(itemId, locationId);
        }

        /// <summary>
        /// Check if player can sell an item
        /// </summary>
        public bool CanSellItem(string itemId, string locationId)
        {
            // Check market availability
            if (!IsMarketAvailable(locationId, _timeManager.GetCurrentTimeBlock()))
                return false;

            Player player = _gameWorld.GetPlayer();

            // Check if player has the item
            if (!player.Inventory.HasItem(itemId))
                return false;

            // Check if location buys this item
            int sellPrice = _legacyMarketManager.GetItemPrice(locationId, itemId, false);
            return sellPrice > 0;
        }

        /// <summary>
        /// Buy an item at a location
        /// </summary>
        public TradeResult BuyItem(string itemId, string locationId)
        {
            Player player = _gameWorld.GetPlayer();
            Item item = _itemRepository.GetItemById(itemId);

            TradeResult result = new TradeResult
            {
                ItemId = itemId,
                ItemName = item?.Name ?? itemId,
                LocationId = locationId,
                Action = TradeAction.Buy,
                CoinsBefore = player.Coins,
                HadItemBefore = player.Inventory.HasItem(itemId)
            };

            // Check market availability
            if (!IsMarketAvailable(locationId, _timeManager.GetCurrentTimeBlock()))
            {
                result.Success = false;
                result.ErrorReason = "Market is closed at this time";
                return result;
            }

            // Get price
            int buyPrice = _legacyMarketManager.GetItemPrice(locationId, itemId, true);
            result.Price = buyPrice;

            // Attempt purchase through legacy manager
            bool success = _legacyMarketManager.BuyItem(itemId, locationId);

            result.Success = success;
            result.CoinsAfter = player.Coins;
            result.HasItemAfter = player.Inventory.HasItem(itemId);

            if (success)
            {
                result.Message = $"Successfully purchased {item?.Name ?? itemId} for {buyPrice} coins";
                result.SystemMessages.Add($"💰 Bought {item?.Name ?? itemId} for {buyPrice} coins");
                result.SystemMessages.Add($"Coins remaining: {player.Coins}");
            }
            else
            {
                if (player.Coins < buyPrice)
                {
                    result.ErrorReason = $"Insufficient funds (need {buyPrice}, have {player.Coins})";
                }
                else if (!player.Inventory.CanAddItem(item, _itemRepository))
                {
                    result.ErrorReason = "No inventory space available";
                }
                else
                {
                    result.ErrorReason = "Purchase failed";
                }
                result.Message = $"Failed to purchase {item?.Name ?? itemId}: {result.ErrorReason}";
            }

            return result;
        }

        /// <summary>
        /// Sell an item at a location
        /// </summary>
        public TradeResult SellItem(string itemId, string locationId)
        {
            Player player = _gameWorld.GetPlayer();
            Item item = _itemRepository.GetItemById(itemId);

            TradeResult result = new TradeResult
            {
                ItemId = itemId,
                ItemName = item?.Name ?? itemId,
                LocationId = locationId,
                Action = TradeAction.Sell,
                CoinsBefore = player.Coins,
                HadItemBefore = player.Inventory.HasItem(itemId)
            };

            // Check market availability
            if (!IsMarketAvailable(locationId, _timeManager.GetCurrentTimeBlock()))
            {
                result.Success = false;
                result.ErrorReason = "Market is closed at this time";
                return result;
            }

            // Get price
            int sellPrice = _legacyMarketManager.GetItemPrice(locationId, itemId, false);
            result.Price = sellPrice;

            // Attempt sale through legacy manager
            bool success = _legacyMarketManager.SellItem(itemId, locationId);

            result.Success = success;
            result.CoinsAfter = player.Coins;
            result.HasItemAfter = player.Inventory.HasItem(itemId);

            if (success)
            {
                result.Message = $"Successfully sold {item?.Name ?? itemId} for {sellPrice} coins";
                result.SystemMessages.Add($"💰 Sold {item?.Name ?? itemId} for {sellPrice} coins");
                result.SystemMessages.Add($"Total coins: {player.Coins}");
            }
            else
            {
                if (!result.HadItemBefore)
                {
                    result.ErrorReason = "You don't have this item";
                }
                else if (sellPrice <= 0)
                {
                    result.ErrorReason = "This item cannot be sold here";
                }
                else
                {
                    result.ErrorReason = "Sale failed";
                }
                result.Message = $"Failed to sell {item?.Name ?? itemId}: {result.ErrorReason}";
            }

            return result;
        }

        /// <summary>
        /// Get items available for purchase at a location
        /// </summary>
        public List<MarketItem> GetAvailableItems(string locationId)
        {
            if (!IsMarketAvailable(locationId, _timeManager.GetCurrentTimeBlock()))
            {
                return new List<MarketItem>();
            }

            // Use legacy manager to get items with location-specific pricing
            List<Item> items = _legacyMarketManager.GetAvailableItems(locationId);

            // Convert to MarketItem format
            return items.Select(item => new MarketItem
            {
                Id = item.Id,
                ItemId = item.Id,
                Name = item.Name,
                Price = item.BuyPrice,
                Stock = 10, // Default stock level
                Item = item,
                Categories = item.Categories
            }).ToList();
        }

        // ========== TRADE RECOMMENDATIONS ==========

        /// <summary>
        /// Get trade recommendations based on current position
        /// </summary>
        public List<TradeRecommendation> GetTradeRecommendations(string currentLocationId)
        {
            List<TradeRecommendation> recommendations = new List<TradeRecommendation>();
            Player player = _gameWorld.GetPlayer();

            // Recommend selling items that are more valuable here
            foreach (string itemId in player.Inventory.GetItemIds())
            {
                Item item = _itemRepository.GetItemById(itemId);
                if (item == null) continue;

                int sellPriceHere = _legacyMarketManager.GetItemPrice(currentLocationId, itemId, false);
                if (sellPriceHere <= 0) continue;

                // Check if this is a good place to sell
                List<Location> locations = _gameWorld.WorldState.locations ?? new List<Location>();
                int avgSellPrice = 0;
                int validLocations = 0;

                foreach (Location loc in locations)
                {
                    int price = _legacyMarketManager.GetItemPrice(loc.Id, itemId, false);
                    if (price > 0)
                    {
                        avgSellPrice += price;
                        validLocations++;
                    }
                }

                if (validLocations > 0)
                {
                    avgSellPrice /= validLocations;

                    if (sellPriceHere > avgSellPrice * 1.1f) // 10% above average
                    {
                        recommendations.Add(new TradeRecommendation
                        {
                            ItemId = itemId,
                            ItemName = item.Name,
                            RecommendedAction = TradeAction.Sell,
                            LocationId = currentLocationId,
                            LocationName = GetLocationName(currentLocationId),
                            ExpectedProfit = sellPriceHere - avgSellPrice,
                            Reasoning = $"Price here ({sellPriceHere}) is above average ({avgSellPrice})",
                            Confidence = Math.Min(1.0f, (float)(sellPriceHere - avgSellPrice) / avgSellPrice)
                        });
                    }
                }
            }

            // Recommend buying items that are cheap here
            if (player.Coins > 10) // Only if player has money to invest
            {
                List<Item> availableItems = _legacyMarketManager.GetAvailableItems(currentLocationId);

                foreach (Item item in availableItems)
                {
                    if (!player.Inventory.CanAddItem(item, _itemRepository)) continue;
                    if (player.Coins < item.BuyPrice) continue;

                    // Check profit potential
                    int buyPriceHere = item.BuyPrice;
                    List<Location> locations = _gameWorld.WorldState.locations ?? new List<Location>();
                    int maxSellPrice = 0;
                    string bestSellLocation = null;

                    foreach (Location loc in locations)
                    {
                        if (loc.Id == currentLocationId) continue;
                        int sellPrice = _legacyMarketManager.GetItemPrice(loc.Id, item.Id, false);
                        if (sellPrice > maxSellPrice)
                        {
                            maxSellPrice = sellPrice;
                            bestSellLocation = loc.Id;
                        }
                    }

                    int profit = maxSellPrice - buyPriceHere;
                    if (profit > 5) // Minimum profit threshold
                    {
                        recommendations.Add(new TradeRecommendation
                        {
                            ItemId = item.Id,
                            ItemName = item.Name,
                            RecommendedAction = TradeAction.Buy,
                            LocationId = currentLocationId,
                            LocationName = GetLocationName(currentLocationId),
                            ExpectedProfit = profit,
                            Reasoning = $"Can sell for {maxSellPrice} in {GetLocationName(bestSellLocation)}",
                            Confidence = Math.Min(1.0f, (float)profit / buyPriceHere)
                        });
                    }
                }
            }

            // Sort by expected profit
            return recommendations.OrderByDescending(r => r.ExpectedProfit).ToList();
        }

        // ========== MARKET SUMMARY ==========

        /// <summary>
        /// Get comprehensive market summary for a location
        /// </summary>
        public MarketSummary GetMarketSummary(string locationId)
        {
            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
            Player player = _gameWorld.GetPlayer();

            MarketSummary summary = new MarketSummary
            {
                LocationId = locationId,
                LocationName = GetLocationName(locationId),
                IsOpen = IsMarketAvailable(locationId, currentTime)
            };

            if (summary.IsOpen)
            {
                List<NPC> traders = GetAvailableTraders(locationId, currentTime);
                summary.AvailableTraders = traders.Select(t => t.Name).ToList();

                List<Item> items = _legacyMarketManager.GetAvailableItems(locationId);
                summary.TotalItemsAvailable = items.Count;
                summary.AffordableItems = items.Count(i => i.BuyPrice <= player.Coins);

                // Count items profitable to sell
                foreach (string itemId in player.Inventory.GetItemIds())
                {
                    int sellPrice = _legacyMarketManager.GetItemPrice(locationId, itemId, false);
                    if (sellPrice > 0)
                    {
                        summary.ProfitableToSell++;
                    }
                }

                summary.MarketStatus = $"Open with {summary.AvailableTraders.Count} trader(s)";
            }
            else
            {
                summary.MarketStatus = GetMarketAvailabilityStatus(locationId, currentTime);

                // Find next open time
                TimeBlocks[] futureTimes = { TimeBlocks.Dawn, TimeBlocks.Morning, TimeBlocks.Afternoon,
                                            TimeBlocks.Evening, TimeBlocks.Night, TimeBlocks.LateNight };
                foreach (TimeBlocks time in futureTimes)
                {
                    if (time <= currentTime) continue;
                    if (IsMarketAvailable(locationId, time))
                    {
                        summary.NextOpenTime = time;
                        break;
                    }
                }
            }

            return summary;
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Get all traders at a location (regardless of time)
        /// </summary>
        private List<NPC> GetAllTraders(string locationId)
        {
            return _npcRepository.GetNPCsForLocation(locationId)
                .Where(npc => npc.CanProvideService(ServiceTypes.Trade))
                .ToList();
        }

        /// <summary>
        /// Get traders available at a specific time
        /// </summary>
        private List<NPC> GetTradersAtTime(string locationId, TimeBlocks timeBlock)
        {
            return _npcRepository.GetNPCsForLocationAndTime(locationId, timeBlock)
                .Where(npc => npc.CanProvideService(ServiceTypes.Trade))
                .ToList();
        }

        /// <summary>
        /// Get location name by ID
        /// </summary>
        private string GetLocationName(string locationId)
        {
            Location location = _gameWorld.WorldState.locations?.FirstOrDefault(l => l.Id == locationId);
            return location?.Name ?? locationId;
        }

        /// <summary>
        /// Validate trade preconditions
        /// </summary>
        private bool ValidateTradeConditions(string locationId, out string error)
        {
            error = null;

            // Check time
            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
            if (!IsMarketAvailable(locationId, currentTime))
            {
                error = "Market is closed at this time";
                return false;
            }

            // Check player location
            Player player = _gameWorld.GetPlayer();
            if (player.CurrentLocationSpot?.LocationId != locationId)
            {
                error = "You must be at the location to trade";
                return false;
            }

            return true;
        }
    }
}