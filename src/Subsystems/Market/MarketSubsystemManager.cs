
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
    /// Check if market is available at a Location at a specific time
    /// </summary>
    public bool IsMarketAvailable(string locationId, TimeBlocks timeBlock)
    {
        Location location = _gameWorld.GetLocation(locationId);
        if (location == null) return false;
        List<NPC> traders = GetTradersAtTime(location, timeBlock);
        return traders.Count > 0;
    }

    /// <summary>
    /// Get market availability status message
    /// </summary>
    public string GetMarketAvailabilityStatus(string locationId, TimeBlocks currentTime)
    {
        Location location = _gameWorld.GetLocation(locationId);
        if (location == null) return "Location not found";

        List<NPC> currentTraders = GetTradersAtTime(location, currentTime);

        if (currentTraders.Count > 0)
        {
            string traderNames = string.Join(", ", currentTraders.Select(t => t.Name));
            return $"Market Open - Traders: {traderNames}";
        }

        // Find next available time
        List<NPC> allTraders = GetAllTraders(location);
        if (allTraders.Count == 0)
        {
            return "No traders at this location";
        }

        TimeBlocks[] allTimes = { TimeBlocks.Morning, TimeBlocks.Midday,
                             TimeBlocks.Afternoon, TimeBlocks.Evening };

        foreach (TimeBlocks time in allTimes)
        {
            if (time <= currentTime) continue; // Skip past and current times

            List<NPC> futureTraders = GetTradersAtTime(location, time);
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
        Location location = _gameWorld.GetLocation(locationId);
        if (location == null) return new List<NPC>();
        return GetTradersAtTime(location, timeBlock);
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
    /// Get dynamic pricing for an item at a location (based on Location properties, NOT Venue type)
    /// </summary>
    private LocationPricing GetDynamicPricing(string locationId, string itemId)
    {
        Item item = _itemRepository.GetItemById(itemId);
        if (item == null)
        {
            return new LocationPricing { BuyPrice = 0, SellPrice = 0, IsAvailable = false };
        }

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        bool marketAvailable = IsMarketAvailable(locationId, currentTime);

        if (!marketAvailable)
        {
            return new LocationPricing { BuyPrice = 0, SellPrice = 0, IsAvailable = false };
        }

        LocationPricing pricing = new LocationPricing
        {
            IsAvailable = true,
            SupplyLevel = 1.0f
        };

        // Location properties determine pricing (NOT Venue type)
        Location location = _gameWorld.GetLocation(locationId);
        if (location != null)
        {
            // Check Location properties to determine pricing
            if (location.LocationProperties.Contains(LocationPropertyType.Market))
            {
                // Markets have higher prices (competitive commercial environment)
                pricing.BuyPrice = item.BuyPrice + 1;
                pricing.SellPrice = item.SellPrice + 1;
            }
            else if (location.LocationProperties.Contains(LocationPropertyType.Tavern))
            {
                // Taverns have lower prices (casual trade, not primary business)
                pricing.BuyPrice = Math.Max(1, item.BuyPrice - 1);
                pricing.SellPrice = Math.Max(1, item.SellPrice - 1);
            }
            else
            {
                // Other location types use base prices
                pricing.BuyPrice = item.BuyPrice;
                pricing.SellPrice = item.SellPrice;
            }
        }
        else
        {
            // Location not found, use base prices
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
    public int GetItemPrice(string locationId, string itemId, bool isBuyPrice)
    {
        LocationPricing pricing = GetDynamicPricing(locationId, itemId);
        if (!pricing.IsAvailable) return -1;
        return isBuyPrice ? pricing.BuyPrice : pricing.SellPrice;
    }

    /// <summary>
    /// Get all items available for purchase at a location
    /// </summary>
    public List<Item> GetAvailableItems(string locationId)
    {
        List<Item> availableItems = new List<Item>();

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        if (!IsMarketAvailable(locationId, currentTime))
        {
            return availableItems;
        }

        List<Item> allItems = _itemRepository.GetAllItems();
        foreach (Item item in allItems)
        {
            // ADR-007: Use Name instead of deleted Id
            LocationPricing pricing = GetDynamicPricing(locationId, item.Name);
            if (pricing.IsAvailable && pricing.BuyPrice > 0)
            {
                // Create a copy with location-specific pricing
                // ADR-007: No Id property (Name is natural key)
                Item pricedItem = new Item
                {
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
    public bool CanBuyItem(string itemId, string locationId)
    {
        // Check market availability
        if (!IsMarketAvailable(locationId, _timeManager.GetCurrentTimeBlock()))
            return false;

        // Check item availability
        Item item = _itemRepository.GetItemById(itemId);
        if (item == null) return false;

        // Check pricing
        int buyPrice = GetItemPrice(locationId, itemId, true);
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
    public bool CanSellItem(string itemId, string locationId)
    {
        // Check market availability
        if (!IsMarketAvailable(locationId, _timeManager.GetCurrentTimeBlock()))
            return false;

        Player player = _gameWorld.GetPlayer();

        // Check if player has the item
        if (!player.Inventory.HasItem(itemId))
            return false;

        // Check if Location buys this item
        int sellPrice = GetItemPrice(locationId, itemId, false);
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
            ItemName = item.Name,
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
        int buyPrice = GetItemPrice(locationId, itemId, true);
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
            result.SystemMessages.Add($"Bought {item.Name} for {buyPrice} coins");
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
    public TradeResult SellItem(string itemId, string locationId)
    {
        Player player = _gameWorld.GetPlayer();
        Item item = _itemRepository.GetItemById(itemId);

        TradeResult result = new TradeResult
        {
            ItemId = itemId,
            ItemName = item.Name,
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
        int sellPrice = GetItemPrice(locationId, itemId, false);
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
            result.SystemMessages.Add($"Sold {item.Name} for {sellPrice} coins");
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
    public List<MarketItem> GetAvailableMarketItems(string locationId)
    {
        if (!IsMarketAvailable(locationId, _timeManager.GetCurrentTimeBlock()))
        {
            return new List<MarketItem>();
        }

        // Get items with location-specific pricing
        List<Item> items = GetAvailableItems(locationId);

        // Convert to MarketItem format
        // ADR-007: Use Name instead of deleted Id
        return items.Select(item => new MarketItem
        {
            Id = item.Name,
            ItemId = item.Name,
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

            int sellPriceHere = GetItemPrice(currentLocationId, itemId, false);
            if (sellPriceHere <= 0) continue;

            // Check if this is a good place to sell (compare to all Locations)
            List<Location> allLocations = _gameWorld.Locations;
            int avgSellPrice = 0;
            int validLocations = 0;

            foreach (Location loc in allLocations)
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
            List<Item> availableItems = GetAvailableItems(currentLocationId);

            foreach (Item item in availableItems)
            {
                if (!player.Inventory.CanAddItem(item, _itemRepository)) continue;
                if (player.Coins < item.BuyPrice) continue;

                // Check profit potential
                int buyPriceHere = item.BuyPrice;
                List<Location> allLocations = _gameWorld.Locations;
                int maxSellPrice = 0;
                string bestSellLocation = null;

                foreach (Location loc in allLocations)
                {
                    // ADR-007: Use Name instead of deleted Id
                    if (loc.Name == currentLocationId) continue;
                    int sellPrice = GetItemPrice(loc.Name, item.Name, false);
                    if (sellPrice > maxSellPrice)
                    {
                        maxSellPrice = sellPrice;
                        bestSellLocation = loc.Name;
                    }
                }

                int profit = maxSellPrice - buyPriceHere;
                if (profit > 5) // Minimum profit threshold
                {
                    // ADR-007: Use Name instead of deleted Id
                    recommendations.Add(new TradeRecommendation
                    {
                        ItemId = item.Name,
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

            List<Item> items = GetAvailableItems(locationId);
            summary.TotalItemsAvailable = items.Count;
            summary.AffordableItems = items.Count(i => i.BuyPrice <= player.Coins);

            // Count items profitable to sell
            foreach (string itemId in player.Inventory.GetItemIds())
            {
                int sellPrice = GetItemPrice(locationId, itemId, false);
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
            TimeBlocks[] futureTimes = { TimeBlocks.Morning, TimeBlocks.Midday,
                                    TimeBlocks.Afternoon, TimeBlocks.Evening };
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
    /// Get all traders at a Location (regardless of time)
    /// </summary>
    private List<NPC> GetAllTraders(Location location)
    {
        return _npcRepository.GetNPCsForLocation(location)
            .Where(npc => npc.Profession == Professions.Merchant)
            .ToList();
    }

    /// <summary>
    /// Get traders available at a specific time
    /// </summary>
    private List<NPC> GetTradersAtTime(Location location, TimeBlocks timeBlock)
    {
        return _npcRepository.GetNPCsForLocationAndTime(location, timeBlock)
            .Where(npc => npc.Profession == Professions.Merchant)
            .ToList();
    }

    /// <summary>
    /// Get Location name by ID
    /// </summary>
    private string GetLocationName(string locationId)
    {
        Location location = _gameWorld.GetLocation(locationId);
        return location?.Name ?? "Unknown Location";
    }

    /// <summary>
    /// Validate trade preconditions
    /// </summary>
    private bool ValidateTradeConditions(string locationId, out string error)
    {
        error = "";

        // Check time
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        if (!IsMarketAvailable(locationId, currentTime))
        {
            error = "Market is closed at this time";
            return false;
        }

        // Check player location
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();

        // Resolve locationId to Location object (template boundary)
        Location targetLocation = _gameWorld.GetLocation(locationId);
        if (currentLocation == null || targetLocation == null || currentLocation != targetLocation)
        {
            error = "You must be at the Location to trade";
            return false;
        }

        return true;
    }
}
