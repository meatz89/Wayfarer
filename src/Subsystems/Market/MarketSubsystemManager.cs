
/// <summary>
/// Core market logic manager for all trading operations and market queries.
/// Implements dynamic pricing system with location-based arbitrage opportunities.
/// TWO PILLARS: Delegates resource mutations to RewardApplicationService
/// </summary>
public class MarketSubsystemManager
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;
    private readonly TimeManager _timeManager;
    private readonly RewardApplicationService _rewardApplicationService;

    public MarketSubsystemManager(
        GameWorld gameWorld,
        ItemRepository itemRepository,
        NPCRepository npcRepository,
        MessageSystem messageSystem,
        TimeManager timeManager,
        RewardApplicationService rewardApplicationService)
    {
        _gameWorld = gameWorld;
        _itemRepository = itemRepository;
        _npcRepository = npcRepository;
        _messageSystem = messageSystem;
        _timeManager = timeManager;
        _rewardApplicationService = rewardApplicationService;
    }

    /// <summary>
    /// Result of a trade operation
    /// HIGHLANDER: Object references only
    /// </summary>
    public class TradeResult
    {
        public bool Success { get; set; }
        public Item Item { get; set; }
        public Location Location { get; set; }
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
    /// HIGHLANDER: Object references only
    /// </summary>
    public class TradeRecommendation
    {
        public Item Item { get; set; }
        public TradeAction RecommendedAction { get; set; }
        public Location Location { get; set; }
        public int ExpectedProfit { get; set; }
        public string Reasoning { get; set; }
        public float Confidence { get; set; } // 0.0 to 1.0
    }

    /// <summary>
    /// Market summary for a location
    /// HIGHLANDER: Object references only
    /// </summary>
    public class MarketSummary
    {
        public Location Location { get; set; }
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
    /// HIGHLANDER: Accept Location object
    /// </summary>
    public bool IsMarketAvailable(Location location, TimeBlocks timeBlock)
    {
        if (location == null) return false;
        List<NPC> traders = GetTradersAtTime(location, timeBlock);
        return traders.Count > 0;
    }

    /// <summary>
    /// Get market availability status message
    /// HIGHLANDER: Accept Location object
    /// </summary>
    public string GetMarketAvailabilityStatus(Location location, TimeBlocks currentTime)
    {
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
    /// HIGHLANDER: Accept Location object
    /// </summary>
    public List<NPC> GetAvailableTraders(Location location, TimeBlocks timeBlock)
    {
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
    /// HIGHLANDER: Accept Item and Location objects, not string IDs
    /// </summary>
    private LocationPricing GetDynamicPricing(Location location, Item item)
    {
        if (item == null)
        {
            return new LocationPricing { BuyPrice = 0, SellPrice = 0, IsAvailable = false };
        }

        if (location == null)
        {
            return new LocationPricing { BuyPrice = 0, SellPrice = 0, IsAvailable = false };
        }

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        bool marketAvailable = IsMarketAvailable(location, currentTime);

        if (!marketAvailable)
        {
            return new LocationPricing { BuyPrice = 0, SellPrice = 0, IsAvailable = false };
        }

        LocationPricing pricing = new LocationPricing
        {
            IsAvailable = true,
            SupplyLevel = 1.0f
        };

        // Location purpose/role determine pricing (orthogonal properties replace capabilities)
        if (location != null)
        {
            // Check Location purpose/role to determine pricing
            if (location.Purpose == LocationPurpose.Commerce && location.Role == LocationRole.Hub)
            {
                // Markets have higher prices (competitive commercial environment)
                pricing.BuyPrice = item.BuyPrice + 1;
                pricing.SellPrice = item.SellPrice + 1;
            }
            else if (location.Role == LocationRole.Rest && location.Purpose == LocationPurpose.Commerce)
            {
                // Taverns/Inns have lower prices (casual trade, not primary business)
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
    /// HIGHLANDER: Accept Location and Item objects, not string IDs
    /// </summary>
    public int GetItemPrice(Location location, Item item, bool isBuyPrice)
    {
        LocationPricing pricing = GetDynamicPricing(location, item);
        if (!pricing.IsAvailable) return -1;
        return isBuyPrice ? pricing.BuyPrice : pricing.SellPrice;
    }

    /// <summary>
    /// Get all items available for purchase at a location
    /// HIGHLANDER: Accept Location object, not string ID
    /// </summary>
    public List<Item> GetAvailableItems(Location location)
    {
        List<Item> availableItems = new List<Item>();

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        if (!IsMarketAvailable(location, currentTime))
        {
            return availableItems;
        }

        List<Item> allItems = _itemRepository.GetAllItems();
        foreach (Item item in allItems)
        {
            LocationPricing pricing = GetDynamicPricing(location, item);
            if (pricing.IsAvailable && pricing.BuyPrice > 0)
            {
                // Create a copy with location-specific pricing
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
    /// HIGHLANDER: Accept Item and Location objects, not string IDs
    /// </summary>
    public bool CanBuyItem(Item item, Location location)
    {
        if (item == null || location == null) return false;

        // Check market availability
        if (!IsMarketAvailable(location, _timeManager.GetCurrentTimeBlock()))
            return false;

        // Check pricing
        int buyPrice = GetItemPrice(location, item, true);
        if (buyPrice <= 0) return false;

        // Check player resources
        Player player = _gameWorld.GetPlayer();
        if (player.Coins < buyPrice) return false;

        // Check inventory space
        return player.Inventory.CanAddItem(item);
    }

    /// <summary>
    /// Check if player can sell an item
    /// HIGHLANDER: Accept Item and Location objects, not string IDs
    /// </summary>
    public bool CanSellItem(Item item, Location location)
    {
        if (item == null || location == null) return false;

        // Check market availability
        if (!IsMarketAvailable(location, _timeManager.GetCurrentTimeBlock()))
            return false;

        Player player = _gameWorld.GetPlayer();

        // HIGHLANDER: Check if player has the item using object reference
        if (!player.Inventory.Contains(item))
            return false;

        // Check if Location buys this item
        int sellPrice = GetItemPrice(location, item, false);
        return sellPrice > 0;
    }

    /// <summary>
    /// Buy an item at a location
    /// HIGHLANDER: Accept Item and Location objects, not string IDs
    /// TWO PILLARS: Delegates resource mutations to RewardApplicationService
    /// </summary>
    public async Task<TradeResult> BuyItem(Item item, Location location)
    {
        if (item == null || location == null)
        {
            return new TradeResult
            {
                Success = false,
                ErrorReason = "Invalid item or location",
                Message = "Purchase failed: Invalid item or location"
            };
        }

        Player player = _gameWorld.GetPlayer();

        TradeResult result = new TradeResult
        {
            Item = item,
            Location = location,
            Action = TradeAction.Buy,
            CoinsBefore = player.Coins,
            HadItemBefore = player.Inventory.Contains(item)
        };

        // Check market availability
        if (!IsMarketAvailable(location, _timeManager.GetCurrentTimeBlock()))
        {
            result.Success = false;
            result.ErrorReason = "Market is closed at this time";
            return result;
        }

        // Get price
        int buyPrice = GetItemPrice(location, item, true);
        result.Price = buyPrice;

        // Attempt purchase
        bool success = false;
        if (buyPrice > 0 && player.Coins >= buyPrice && player.Inventory.CanAddItem(item))
        {
            // TWO PILLARS: Apply purchase via Consequence + ApplyConsequence
            Consequence purchaseCost = new Consequence
            {
                Coins = -buyPrice,
                Items = new List<Item> { item }
            };
            await _rewardApplicationService.ApplyConsequence(purchaseCost, null);
            success = true;
            _messageSystem.AddSystemMessage($"Bought {item.Name} for {buyPrice} coins", SystemMessageTypes.Success);
        }

        result.Success = success;
        result.CoinsAfter = player.Coins;
        result.HasItemAfter = player.Inventory.Contains(item);

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
            else if (!player.Inventory.CanAddItem(item))
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
    /// HIGHLANDER: Accept Item and Location objects, not string IDs
    /// TWO PILLARS: Delegates resource mutations to RewardApplicationService
    /// </summary>
    public async Task<TradeResult> SellItem(Item item, Location location)
    {
        if (item == null || location == null)
        {
            return new TradeResult
            {
                Success = false,
                ErrorReason = "Invalid item or location",
                Message = "Sale failed: Invalid item or location"
            };
        }

        Player player = _gameWorld.GetPlayer();

        TradeResult result = new TradeResult
        {
            Item = item,
            Location = location,
            Action = TradeAction.Sell,
            CoinsBefore = player.Coins,
            HadItemBefore = player.Inventory.Contains(item)
        };

        // Check market availability
        if (!IsMarketAvailable(location, _timeManager.GetCurrentTimeBlock()))
        {
            result.Success = false;
            result.ErrorReason = "Market is closed at this time";
            return result;
        }

        // Get price
        int sellPrice = GetItemPrice(location, item, false);
        result.Price = sellPrice;

        // Attempt sale
        bool success = false;
        // HIGHLANDER: Inventory.Contains(Item) accepts object, not string name
        if (sellPrice > 0 && player.Inventory.Contains(item))
        {
            // TWO PILLARS: Apply sale via single Consequence (remove item + gain coins)
            Consequence saleConsequence = new Consequence
            {
                Coins = sellPrice,
                ItemsToRemove = new List<Item> { item }
            };
            await _rewardApplicationService.ApplyConsequence(saleConsequence, null);
            success = true;
            _messageSystem.AddSystemMessage($"Sold {item.Name} for {sellPrice} coins", SystemMessageTypes.Success);
        }

        result.Success = success;
        result.CoinsAfter = player.Coins;
        result.HasItemAfter = player.Inventory.Contains(item);

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
    /// HIGHLANDER: Accept Location object, not string ID
    /// </summary>
    public List<MarketItem> GetAvailableMarketItems(Location location)
    {
        if (!IsMarketAvailable(location, _timeManager.GetCurrentTimeBlock()))
        {
            return new List<MarketItem>();
        }

        // Get items with location-specific pricing
        List<Item> items = GetAvailableItems(location);

        // Convert to MarketItem format
        return items.Select(item => new MarketItem
        {
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
    /// HIGHLANDER: Accept Location object, not string ID
    /// </summary>
    public List<TradeRecommendation> GetTradeRecommendations(Location currentLocation)
    {
        List<TradeRecommendation> recommendations = new List<TradeRecommendation>();
        Player player = _gameWorld.GetPlayer();

        if (currentLocation == null) return recommendations;

        // Recommend selling items that are more valuable here
        foreach (Item item in player.Inventory.GetAllItems())
        {
            int sellPriceHere = GetItemPrice(currentLocation, item, false);
            if (sellPriceHere <= 0) continue;

            // Check if this is a good place to sell (compare to all Locations)
            List<Location> allLocations = _gameWorld.Locations;
            int avgSellPrice = 0;
            int validLocations = 0;

            foreach (Location loc in allLocations)
            {
                int price = GetItemPrice(loc, item, false);
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
                        Item = item,
                        RecommendedAction = TradeAction.Sell,
                        Location = currentLocation,
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
            List<Item> availableItems = GetAvailableItems(currentLocation);

            foreach (Item item in availableItems)
            {
                if (!player.Inventory.CanAddItem(item)) continue;
                if (player.Coins < item.BuyPrice) continue;

                // Check profit potential
                int buyPriceHere = item.BuyPrice;
                List<Location> allLocations = _gameWorld.Locations;
                int maxSellPrice = 0;
                Location bestSellLocation = null;

                foreach (Location loc in allLocations)
                {
                    if (loc == currentLocation) continue;
                    int sellPrice = GetItemPrice(loc, item, false);
                    if (sellPrice > maxSellPrice)
                    {
                        maxSellPrice = sellPrice;
                        bestSellLocation = loc;
                    }
                }

                int profit = maxSellPrice - buyPriceHere;
                if (profit > 5) // Minimum profit threshold
                {
                    recommendations.Add(new TradeRecommendation
                    {
                        Item = item,
                        RecommendedAction = TradeAction.Buy,
                        Location = currentLocation,
                        ExpectedProfit = profit,
                        Reasoning = $"Can sell for {maxSellPrice} in {bestSellLocation?.Name ?? "elsewhere"}",
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
    /// HIGHLANDER: Accept Location object, not string ID
    /// </summary>
    public MarketSummary GetMarketSummary(Location location)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        Player player = _gameWorld.GetPlayer();

        MarketSummary summary = new MarketSummary
        {
            Location = location,
            IsOpen = IsMarketAvailable(location, currentTime)
        };

        if (summary.IsOpen)
        {
            List<NPC> traders = GetAvailableTraders(location, currentTime);
            summary.AvailableTraders = traders.Select(t => t.Name).ToList();

            List<Item> items = GetAvailableItems(location);
            summary.TotalItemsAvailable = items.Count;
            summary.AffordableItems = items.Count(i => i.BuyPrice <= player.Coins);

            // Count items profitable to sell
            foreach (Item item in player.Inventory.GetAllItems())
            {
                int sellPrice = GetItemPrice(location, item, false);
                if (sellPrice > 0)
                {
                    summary.ProfitableToSell++;
                }
            }

            summary.MarketStatus = $"Open with {summary.AvailableTraders.Count} trader(s)";
        }
        else
        {
            summary.MarketStatus = GetMarketAvailabilityStatus(location, currentTime);

            // Find next open time
            TimeBlocks[] futureTimes = { TimeBlocks.Morning, TimeBlocks.Midday,
                                    TimeBlocks.Afternoon, TimeBlocks.Evening };
            foreach (TimeBlocks time in futureTimes)
            {
                if (time <= currentTime) continue;
                if (IsMarketAvailable(location, time))
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

    // HIGHLANDER: GetLocationName and ValidateTradeConditions removed (dead code after refactoring)
}
