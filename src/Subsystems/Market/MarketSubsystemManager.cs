using Wayfarer.GameState.Enums;

/// <summary>
/// Core market logic manager for all trading operations and market queries.
/// Implements dynamic pricing system with location-based arbitrage opportunities.
/// </summary>
public class MarketSubsystemManager
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;
    private readonly TimeManager _timeManager;

    public MarketSubsystemManager(
        GameWorld gameWorld,
        ItemRepository itemRepository,
        NPCRepository npcRepository,
        MessageSystem messageSystem,
        TimeManager timeManager)
    {
        _gameWorld = gameWorld;
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
        public string VenueId { get; set; }
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
        public string VenueId { get; set; }
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
        public string VenueId { get; set; }
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
    /// Check if market is available at a Venue at a specific time
    /// </summary>
    public bool IsMarketAvailable(string venueId, TimeBlocks timeBlock)
    {
        List<NPC> traders = GetTradersAtTime(venueId, timeBlock);
        return traders.Count > 0;
    }

    /// <summary>
    /// Get market availability status message
    /// </summary>
    public string GetMarketAvailabilityStatus(string venueId, TimeBlocks currentTime)
    {
        List<NPC> currentTraders = GetTradersAtTime(venueId, currentTime);

        if (currentTraders.Count > 0)
        {
            string traderNames = string.Join(", ", currentTraders.Select(t => t.Name));
            return $"Market Open - Traders: {traderNames}";
        }

        // Find next available time
        List<NPC> allTraders = GetAllTraders(venueId);
        if (allTraders.Count == 0)
        {
            return "No traders at this location";
        }

        TimeBlocks[] allTimes = { TimeBlocks.Morning, TimeBlocks.Midday,
                                 TimeBlocks.Afternoon, TimeBlocks.Evening };

        foreach (TimeBlocks time in allTimes)
        {
            if (time <= currentTime) continue; // Skip past and current times

            List<NPC> futureTraders = GetTradersAtTime(venueId, time);
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
    public List<NPC> GetAvailableTraders(string venueId, TimeBlocks timeBlock)
    {
        return GetTradersAtTime(venueId, timeBlock);
    }

    // ========== PRICING LOGIC ==========

    private class LocationPricing
    {
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public bool IsAvailable { get; set; }
        public float SupplyLevel { get; set; }
    }

    /// <summary>
    /// Get dynamic pricing for an item at a location
    /// </summary>
    private LocationPricing GetDynamicPricing(string venueId, string itemId)
    {
        Item item = _itemRepository.GetItemById(itemId);
        if (item == null)
        {
            return new LocationPricing { BuyPrice = 0, SellPrice = 0, IsAvailable = false };
        }

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        bool marketAvailable = IsMarketAvailable(venueId, currentTime);

        if (!marketAvailable)
        {
            return new LocationPricing { BuyPrice = 0, SellPrice = 0, IsAvailable = false };
        }

        LocationPricing pricing = new LocationPricing
        {
            IsAvailable = true,
            SupplyLevel = 1.0f
        };

        // Location-specific pricing logic (uses strongly-typed VenueType enum)
        Venue venue = _gameWorld.Venues.FirstOrDefault(v => v.Id == venueId);
        if (venue != null)
        {
            switch (venue.Type)
            {
                case VenueType.Market:
                    // Markets have higher prices (competitive commercial environment)
                    pricing.BuyPrice = item.BuyPrice + 1;
                    pricing.SellPrice = item.SellPrice + 1;
                    break;
                case VenueType.Tavern:
                    // Taverns have lower prices (casual trade, not primary business)
                    pricing.BuyPrice = Math.Max(1, item.BuyPrice - 1);
                    pricing.SellPrice = Math.Max(1, item.SellPrice - 1);
                    break;
                default:
                    // Other venue types use base prices
                    pricing.BuyPrice = item.BuyPrice;
                    pricing.SellPrice = item.SellPrice;
                    break;
            }
        }
        else
        {
            // Venue not found, use base prices
            pricing.BuyPrice = item.BuyPrice;
            pricing.SellPrice = item.SellPrice;
        }

        // Ensure buy price is always higher than sell price
        int minimumBuyPrice = (int)Math.Ceiling(pricing.SellPrice * 1.15);
        if (pricing.BuyPrice < minimumBuyPrice)
        {
            pricing.BuyPrice = minimumBuyPrice;
        }

        return pricing;
    }

    /// <summary>
    /// Get the price for a specific item at a specific location
    /// </summary>
    public int GetItemPrice(string venueId, string itemId, bool isBuyPrice)
    {
        LocationPricing pricing = GetDynamicPricing(venueId, itemId);
        if (!pricing.IsAvailable) return -1;
        return isBuyPrice ? pricing.BuyPrice : pricing.SellPrice;
    }

    /// <summary>
    /// Get all items available for purchase at a location
    /// </summary>
    public List<Item> GetAvailableItems(string venueId)
    {
        List<Item> availableItems = new List<Item>();

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        if (!IsMarketAvailable(venueId, currentTime))
        {
            return availableItems;
        }

        List<Item> allItems = _itemRepository.GetAllItems();
        foreach (Item item in allItems)
        {
            LocationPricing pricing = GetDynamicPricing(venueId, item.Id);
            if (pricing.IsAvailable && pricing.BuyPrice > 0)
            {
                // Create a copy with location-specific pricing
                Item pricedItem = new Item
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    InitiativeCost = item.InitiativeCost,
                    BuyPrice = pricing.BuyPrice,
                    SellPrice = pricing.SellPrice,
                    Categories = item.Categories,
                    Weight = item.Weight,
                    Size = item.Size
                };
                availableItems.Add(pricedItem);
            }
        }

        return availableItems;
    }

    // ========== TRADING OPERATIONS ==========

    /// <summary>
    /// Check if player can buy an item
    /// </summary>
    public bool CanBuyItem(string itemId, string venueId)
    {
        // Check market availability
        if (!IsMarketAvailable(venueId, _timeManager.GetCurrentTimeBlock()))
            return false;

        // Check item availability
        Item item = _itemRepository.GetItemById(itemId);
        if (item == null) return false;

        // Check pricing
        int buyPrice = GetItemPrice(venueId, itemId, true);
        if (buyPrice <= 0) return false;

        // Check player resources
        Player player = _gameWorld.GetPlayer();
        if (player.Coins < buyPrice) return false;

        // Check inventory space
        return player.Inventory.CanAddItem(item, _itemRepository);
    }

    /// <summary>
    /// Check if player can sell an item
    /// </summary>
    public bool CanSellItem(string itemId, string venueId)
    {
        // Check market availability
        if (!IsMarketAvailable(venueId, _timeManager.GetCurrentTimeBlock()))
            return false;

        Player player = _gameWorld.GetPlayer();

        // Check if player has the item
        if (!player.Inventory.HasItem(itemId))
            return false;

        // Check if Venue buys this item
        int sellPrice = GetItemPrice(venueId, itemId, false);
        return sellPrice > 0;
    }

    /// <summary>
    /// Buy an item at a location
    /// </summary>
    public TradeResult BuyItem(string itemId, string venueId)
    {
        Player player = _gameWorld.GetPlayer();
        Item item = _itemRepository.GetItemById(itemId);

        TradeResult result = new TradeResult
        {
            ItemId = itemId,
            ItemName = item.Name,
            VenueId = venueId,
            Action = TradeAction.Buy,
            CoinsBefore = player.Coins,
            HadItemBefore = player.Inventory.HasItem(itemId)
        };

        // Check market availability
        if (!IsMarketAvailable(venueId, _timeManager.GetCurrentTimeBlock()))
        {
            result.Success = false;
            result.ErrorReason = "Market is closed at this time";
            return result;
        }

        // Get price
        int buyPrice = GetItemPrice(venueId, itemId, true);
        result.Price = buyPrice;

        // Attempt purchase
        bool success = false;
        if (buyPrice > 0 && player.Coins >= buyPrice && player.Inventory.CanAddItem(item, _itemRepository))
        {
            player.AddCoins(-buyPrice);
            player.Inventory.AddItem(itemId);
            success = true;
            _messageSystem.AddSystemMessage($"Bought {item.Name} for {buyPrice} coins", SystemMessageTypes.Success);
        }

        result.Success = success;
        result.CoinsAfter = player.Coins;
        result.HasItemAfter = player.Inventory.HasItem(itemId);

        if (success)
        {
            result.Message = $"Successfully purchased {item.Name} for {buyPrice} coins";
            result.SystemMessages.Add($"💰 Bought {item.Name} for {buyPrice} coins");
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
            result.Message = $"Failed to purchase {item.Name}: {result.ErrorReason}";
        }

        return result;
    }

    /// <summary>
    /// Sell an item at a location
    /// </summary>
    public TradeResult SellItem(string itemId, string venueId)
    {
        Player player = _gameWorld.GetPlayer();
        Item item = _itemRepository.GetItemById(itemId);

        TradeResult result = new TradeResult
        {
            ItemId = itemId,
            ItemName = item.Name,
            VenueId = venueId,
            Action = TradeAction.Sell,
            CoinsBefore = player.Coins,
            HadItemBefore = player.Inventory.HasItem(itemId)
        };

        // Check market availability
        if (!IsMarketAvailable(venueId, _timeManager.GetCurrentTimeBlock()))
        {
            result.Success = false;
            result.ErrorReason = "Market is closed at this time";
            return result;
        }

        // Get price
        int sellPrice = GetItemPrice(venueId, itemId, false);
        result.Price = sellPrice;

        // Attempt sale
        bool success = false;
        if (sellPrice > 0 && player.Inventory.HasItem(itemId))
        {
            player.Inventory.RemoveItem(itemId);
            player.AddCoins(sellPrice);
            success = true;
            _messageSystem.AddSystemMessage($"Sold {item.Name} for {sellPrice} coins", SystemMessageTypes.Success);
        }

        result.Success = success;
        result.CoinsAfter = player.Coins;
        result.HasItemAfter = player.Inventory.HasItem(itemId);

        if (success)
        {
            result.Message = $"Successfully sold {item.Name} for {sellPrice} coins";
            result.SystemMessages.Add($"💰 Sold {item.Name} for {sellPrice} coins");
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
            result.Message = $"Failed to sell {item.Name}: {result.ErrorReason}";
        }

        return result;
    }

    /// <summary>
    /// Get market items with UI-friendly information for purchase at a location
    /// </summary>
    public List<MarketItem> GetAvailableMarketItems(string venueId)
    {
        if (!IsMarketAvailable(venueId, _timeManager.GetCurrentTimeBlock()))
        {
            return new List<MarketItem>();
        }

        // Get items with location-specific pricing
        List<Item> items = GetAvailableItems(venueId);

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
    public List<TradeRecommendation> GetTradeRecommendations(string currentVenueId)
    {
        List<TradeRecommendation> recommendations = new List<TradeRecommendation>();
        Player player = _gameWorld.GetPlayer();

        // Recommend selling items that are more valuable here
        foreach (string itemId in player.Inventory.GetItemIds())
        {
            Item item = _itemRepository.GetItemById(itemId);
            if (item == null) continue;

            int sellPriceHere = GetItemPrice(currentVenueId, itemId, false);
            if (sellPriceHere <= 0) continue;

            // Check if this is a good place to sell
            List<Venue> locations = _gameWorld.Venues;
            int avgSellPrice = 0;
            int validLocations = 0;

            foreach (Venue loc in locations)
            {
                int price = GetItemPrice(loc.Id, itemId, false);
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
                        VenueId = currentVenueId,
                        LocationName = GetLocationName(currentVenueId),
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
            List<Item> availableItems = GetAvailableItems(currentVenueId);

            foreach (Item item in availableItems)
            {
                if (!player.Inventory.CanAddItem(item, _itemRepository)) continue;
                if (player.Coins < item.BuyPrice) continue;

                // Check profit potential
                int buyPriceHere = item.BuyPrice;
                List<Venue> locations = _gameWorld.Venues;
                int maxSellPrice = 0;
                string bestSellLocation = null;

                foreach (Venue loc in locations)
                {
                    if (loc.Id == currentVenueId) continue;
                    int sellPrice = GetItemPrice(loc.Id, item.Id, false);
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
                        VenueId = currentVenueId,
                        LocationName = GetLocationName(currentVenueId),
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
    public MarketSummary GetMarketSummary(string venueId)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        Player player = _gameWorld.GetPlayer();

        MarketSummary summary = new MarketSummary
        {
            VenueId = venueId,
            LocationName = GetLocationName(venueId),
            IsOpen = IsMarketAvailable(venueId, currentTime)
        };

        if (summary.IsOpen)
        {
            List<NPC> traders = GetAvailableTraders(venueId, currentTime);
            summary.AvailableTraders = traders.Select(t => t.Name).ToList();

            List<Item> items = GetAvailableItems(venueId);
            summary.TotalItemsAvailable = items.Count;
            summary.AffordableItems = items.Count(i => i.BuyPrice <= player.Coins);

            // Count items profitable to sell
            foreach (string itemId in player.Inventory.GetItemIds())
            {
                int sellPrice = GetItemPrice(venueId, itemId, false);
                if (sellPrice > 0)
                {
                    summary.ProfitableToSell++;
                }
            }

            summary.MarketStatus = $"Open with {summary.AvailableTraders.Count} trader(s)";
        }
        else
        {
            summary.MarketStatus = GetMarketAvailabilityStatus(venueId, currentTime);

            // Find next open time
            TimeBlocks[] futureTimes = { TimeBlocks.Morning, TimeBlocks.Midday,
                                        TimeBlocks.Afternoon, TimeBlocks.Evening };
            foreach (TimeBlocks time in futureTimes)
            {
                if (time <= currentTime) continue;
                if (IsMarketAvailable(venueId, time))
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
    /// Get all traders at a Venue (regardless of time)
    /// </summary>
    private List<NPC> GetAllTraders(string venueId)
    {
        return _npcRepository.GetNPCsForLocation(venueId)
            .Where(npc => npc.CanProvideService(ServiceTypes.Trade))
            .ToList();
    }

    /// <summary>
    /// Get traders available at a specific time
    /// </summary>
    private List<NPC> GetTradersAtTime(string venueId, TimeBlocks timeBlock)
    {
        return _npcRepository.GetNPCsForLocationAndTime(venueId, timeBlock)
            .Where(npc => npc.CanProvideService(ServiceTypes.Trade))
            .ToList();
    }

    /// <summary>
    /// Get Venue name by ID
    /// </summary>
    private string GetLocationName(string venueId)
    {
        Venue venue = _gameWorld.Venues.FirstOrDefault(l => l.Id == venueId);
        return venue.Name;
    }

    /// <summary>
    /// Validate trade preconditions
    /// </summary>
    private bool ValidateTradeConditions(string venueId, out string error)
    {
        error = "";

        // Check time
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        if (!IsMarketAvailable(venueId, currentTime))
        {
            error = "Market is closed at this time";
            return false;
        }

        // Check player location
        Player player = _gameWorld.GetPlayer();
        if (_gameWorld.GetPlayerCurrentLocation().VenueId != venueId)
        {
            error = "You must be at the Venue to trade";
            return false;
        }

        return true;
    }
}
