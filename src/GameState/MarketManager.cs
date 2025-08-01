using Wayfarer.GameState.Constants;

/// <summary>
/// Manages location-specific item pricing and trading operations.
/// Implements dynamic pricing system that creates arbitrage opportunities
/// between different locations for strategic trading gameplay.
/// </summary>
public class MarketManager
{
    private readonly GameWorld _gameWorld;
    private readonly LocationSystem _locationSystem;
    private readonly ItemRepository _itemRepository;
    private readonly NPCRepository _npcRepository;
    private readonly LocationRepository _locationRepository;
    private readonly ITimeManager _timeManager;
    private readonly MessageSystem _messageSystem;

    /// <summary>
    /// Represents pricing information for an item at a specific location
    /// </summary>
    public class LocationPricing
    {
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public float SupplyLevel { get; set; } // 0.5 = scarce, 1.0 = normal, 2.0 = abundant
        public bool IsAvailable { get; set; }
    }

    public MarketManager(GameWorld gameWorld, LocationSystem locationSystem, ItemRepository itemRepository,
                        NPCRepository npcRepository, LocationRepository locationRepository, MessageSystem messageSystem,
                        ITimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _locationSystem = locationSystem;
        _itemRepository = itemRepository;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _timeManager = timeManager;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Get dynamic pricing for an item at a location
    /// Calculates pricing based on location and item properties from GameWorld
    /// </summary>
    private LocationPricing GetDynamicPricing(string locationId, string itemId)
    {
        // Get base item data from ItemRepository
        Item item = _itemRepository.GetItemById(itemId);
        if (item == null)
        {
            return new LocationPricing { BuyPrice = 0, SellPrice = 0, IsAvailable = false };
        }

        // Check if market is available based on NPC schedules
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        bool marketAvailable = IsMarketAvailableAtLocation(locationId, currentTime);

        if (!marketAvailable)
        {
            return new LocationPricing { BuyPrice = 0, SellPrice = 0, IsAvailable = false };
        }

        // Calculate location-specific pricing dynamically
        LocationPricing pricing = new LocationPricing
        {
            IsAvailable = true,
            SupplyLevel = 1.0f
        };

        // Location-specific pricing logic (replace hardcoded with dynamic calculation)
        switch (locationId)
        {
            case "town_square":
                pricing.BuyPrice = item.BuyPrice + 1; // Slightly more expensive in town
                pricing.SellPrice = item.SellPrice + 1; // Higher sell price in town
                break;
            case "dusty_flagon":
                pricing.BuyPrice = Math.Max(1, item.BuyPrice - 1); // Cheaper at tavern
                pricing.SellPrice = Math.Max(1, item.SellPrice - 1); // Lower sell price at tavern
                break;
            default:
                pricing.BuyPrice = item.BuyPrice;
                pricing.SellPrice = item.SellPrice;
                break;
        }

        // Ensure buy price is always higher than sell price for valid trading
        // Maintain at least 15% spread for economic gameplay (buy price 15% higher than sell price)
        int minimumBuyPrice = (int)Math.Ceiling(pricing.SellPrice * 1.15);
        if (pricing.BuyPrice < minimumBuyPrice)
        {
            pricing.BuyPrice = minimumBuyPrice;
        }

        return pricing;
    }

    /// <summary>
    /// Get the price for a specific item at a specific location.
    /// Calculates pricing dynamically from GameWorld data.
    /// </summary>
    /// <param name="locationId">The location ID</param>
    /// <param name="itemId">The item ID</param>
    /// <param name="isBuyPrice">True for buy price, false for sell price</param>
    /// <returns>Price in coins, or -1 if item not available at location</returns>
    public int GetItemPrice(string locationId, string itemId, bool isBuyPrice)
    {
        LocationPricing pricing = GetDynamicPricing(locationId, itemId);
        if (!pricing.IsAvailable) return -1;

        return isBuyPrice ? pricing.BuyPrice : pricing.SellPrice;
    }

    /// <summary>
    /// Get all items available for purchase at a specific location with location-specific pricing.
    /// </summary>
    /// <param name="locationId">The location ID</param>
    /// <returns>List of items with location-specific pricing applied</returns>
    public List<Item> GetAvailableItems(string locationId)
    {
        List<Item> availableItems = new List<Item>();

        // Check market availability once upfront
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        bool marketAvailable = IsMarketAvailableAtLocation(locationId, currentTime);

        if (!marketAvailable)
        {
            return availableItems; // Return empty list if market is closed
        }

        // Get all items from ItemRepository
        List<Item> allItems = _itemRepository.GetAllItems();

        foreach (Item baseItem in allItems)
        {
            // Calculate location-specific pricing directly
            LocationPricing pricing = CalculateLocationPricing(baseItem, locationId);

            // Create item with location-specific pricing
            Item locationItem = new Item
            {
                Id = baseItem.Id,
                Name = baseItem.Name,
                BuyPrice = pricing.BuyPrice,
                SellPrice = pricing.SellPrice,
                IsAvailable = true, // We know it's available since market is open
                Weight = baseItem.Weight,
                InventorySlots = baseItem.InventorySlots,
                LocationId = locationId,
                Description = baseItem.Description,
                Categories = baseItem.Categories
            };

            availableItems.Add(locationItem);
        }

        return availableItems;
    }

    /// <summary>
    /// Calculate location-specific pricing for an item (simplified version)
    /// </summary>
    private LocationPricing CalculateLocationPricing(Item baseItem, string locationId)
    {
        LocationPricing pricing = new LocationPricing
        {
            IsAvailable = true,
            SupplyLevel = 1.0f
        };

        // Location-specific pricing logic
        switch (locationId)
        {
            case "town_square":
                pricing.BuyPrice = baseItem.BuyPrice + 1;
                pricing.SellPrice = baseItem.SellPrice + 1;
                break;
            case "dusty_flagon":
                pricing.BuyPrice = Math.Max(1, baseItem.BuyPrice - 1);
                pricing.SellPrice = Math.Max(1, baseItem.SellPrice - 1);
                break;
            default:
                pricing.BuyPrice = baseItem.BuyPrice;
                pricing.SellPrice = baseItem.SellPrice;
                break;
        }

        // Ensure buy price is always higher than sell price
        if (pricing.BuyPrice <= pricing.SellPrice)
        {
            pricing.BuyPrice = pricing.SellPrice + 1;
        }

        return pricing;
    }

    /// <summary>
    /// Create an item instance with location-specific pricing applied.
    /// </summary>
    private Item CreateItemWithLocationPricing(string itemId, string locationId)
    {
        LocationPricing pricing = GetDynamicPricing(locationId, itemId);
        Item? baseItem = _itemRepository.GetItemById(itemId);

        return new Item
        {
            Id = itemId,
            Name = baseItem?.Name ?? itemId,
            BuyPrice = pricing.BuyPrice,
            SellPrice = pricing.SellPrice,
            IsAvailable = pricing.IsAvailable,
            Weight = baseItem?.Weight ?? GetDefaultWeight(itemId),
            InventorySlots = baseItem?.InventorySlots ?? 1,
            LocationId = locationId,
            Description = baseItem?.Description ?? GetDefaultDescription(itemId),
            Categories = baseItem?.Categories ?? new List<ItemCategory>()
        };
    }

    /// <summary>
    /// Get default weight for known items (temporary implementation)
    /// </summary>
    private int GetDefaultWeight(string itemId)
    {
        return itemId switch
        {
            "herbs" => 1,
            "tools" => 3,
            "rope" => 2,
            "food" => 1,
            _ => 1
        };
    }

    /// <summary>
    /// Get default description for known items (temporary implementation)
    /// </summary>
    private string GetDefaultDescription(string itemId)
    {
        return itemId switch
        {
            "herbs" => "Common herbs useful for cooking and basic remedies.",
            "tools" => "Basic tools for various trades and repairs.",
            "rope" => "Strong hemp rope, essential for climbing and securing loads.",
            "food" => "Fresh provisions for sustenance during travel.",
            _ => $"A valuable {itemId} for trade."
        };
    }

    /// <summary>
    /// Get default weight description for known items (temporary implementation)
    /// </summary>
    private string GetDefaultWeightDescription(string itemId)
    {
        int weight = GetDefaultWeight(itemId);
        return weight switch
        {
            0 => "weightless",
            1 => "light",
            2 => "medium",
            3 => "heavy",
            4 => "very heavy",
            _ => weight > GameConstants.Inventory.HEAVY_ITEM_WEIGHT_THRESHOLD ? "extremely heavy" : "light"
        };
    }


    /// <summary>
    /// Check if player can buy a specific item at a specific location.
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="locationId">The location ID</param>
    /// <returns>True if player can afford and has inventory space</returns>
    public bool CanBuyItem(string itemId, string locationId)
    {
        Player player = _gameWorld.GetPlayer();
        int buyPrice = GetItemPrice(locationId, itemId, true);

        if (buyPrice <= 0) return false; // Item not available

        bool hasEnoughMoney = player.Coins >= buyPrice;

        // Check if inventory has space for this item considering its size
        Item item = _itemRepository.GetItemById(itemId);
        bool hasInventorySpace = item != null && player.Inventory.CanAddItem(item, _itemRepository);

        return hasEnoughMoney && hasInventorySpace;
    }

    /// <summary>
    /// Buy an item at a specific location using location-specific pricing.
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="locationId">The location ID</param>
    /// <returns>True if purchase successful</returns>
    public bool BuyItem(string itemId, string locationId)
    {
        if (!CanBuyItem(itemId, locationId)) return false;

        Player player = _gameWorld.GetPlayer();
        int buyPrice = GetItemPrice(locationId, itemId, true);
        Item item = _itemRepository.GetItemById(itemId);

        // Show the transaction happening
        _messageSystem.AddSystemMessage(
            $"💰 Purchasing {item.Name} for {buyPrice} coins...",
            SystemMessageTypes.Info
        );

        player.Coins -= buyPrice;

        // Use size-aware inventory method
        bool success = player.Inventory.AddItemWithSizeCheck(item, _itemRepository);
        if (!success)
        {
            // Refund if inventory addition failed
            player.Coins += buyPrice;
            _messageSystem.AddSystemMessage(
                $"❌ Purchase failed - no room in inventory! Coins refunded.",
                SystemMessageTypes.Warning
            );
            return false;
        }

        // Success narrative
        _messageSystem.AddSystemMessage(
            $"✅ Purchased {item.Name} for {buyPrice} coins.",
            SystemMessageTypes.Success
        );

        _messageSystem.AddSystemMessage(
            $"  • Remaining coins: {player.Coins}",
            SystemMessageTypes.Info
        );

        // Trading takes time
        _timeManager.AdvanceTime(1); // 1 hour for trading


        return true;
    }

    /// <summary>
    /// Sell an item at a specific location using location-specific pricing.
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="locationId">The location ID</param>
    /// <returns>True if sale successful</returns>
    public bool SellItem(string itemId, string locationId)
    {
        Player player = _gameWorld.GetPlayer();

        if (!player.Inventory.HasItem(itemId)) return false;

        int sellPrice = GetItemPrice(locationId, itemId, false);
        if (sellPrice <= 0) return false; // Cannot sell here

        // Get item for narrative context
        Item item = _itemRepository.GetItemById(itemId);

        // Show the transaction
        _messageSystem.AddSystemMessage(
            $"💰 Selling {item.Name} for {sellPrice} coins...",
            SystemMessageTypes.Info
        );

        player.Inventory.RemoveItem(itemId);
        player.Coins += sellPrice;

        // Success narrative
        _messageSystem.AddSystemMessage(
            $"✅ Sold {item.Name} for {sellPrice} coins.",
            SystemMessageTypes.Success
        );

        _messageSystem.AddSystemMessage(
            $"  • Total coins: {player.Coins}",
            SystemMessageTypes.Info
        );

        // Trading takes time
        _timeManager.AdvanceTime(1); // 1 hour for trading


        return true;
    }

    // ===== QUERY METHODS FOR STATE INSPECTION =====
    // These methods provide market data inspection capabilities for both production and testing

    /// <summary>
    /// Get current market prices for an item at all locations.
    /// Useful for UI trading screens and testing market scenarios.
    /// </summary>
    public List<MarketPriceInfo> GetItemMarketPrices(string itemId)
    {
        List<MarketPriceInfo> prices = new List<MarketPriceInfo>();

        // Get all locations
        List<Location> locations = _locationRepository.GetAllLocations() ?? new List<Location>();

        foreach (Location location in locations)
        {
            LocationPricing pricing = GetDynamicPricing(location.Id, itemId);

            if (pricing.IsAvailable) // Only include locations that have this item
            {
                prices.Add(new MarketPriceInfo
                {
                    LocationId = location.Id,
                    LocationName = location.Name,
                    ItemId = itemId,
                    BuyPrice = pricing.BuyPrice,
                    SellPrice = pricing.SellPrice,
                    CanBuy = CanBuyItem(itemId, location.Id),
                    SupplyLevel = pricing.SupplyLevel
                });
            }
        }

        return prices;
    }

    /// <summary>
    /// Try to execute a buy action and return detailed result.
    /// Provides better error handling and state tracking than the boolean BuyItem method.
    /// </summary>
    public TradeActionResult TryBuyItem(string itemId, string locationId)
    {
        try
        {
            Player player = _gameWorld.GetPlayer();
            int coinsBefore = player.Coins;
            bool hadItemBefore = player.Inventory.HasItem(itemId);

            // Get price before attempting purchase
            int buyPrice = GetItemPrice(locationId, itemId, true);

            // Attempt the purchase
            bool success = BuyItem(itemId, locationId);

            if (success)
            {
                int coinsAfter = player.Coins;
                bool hasItemAfter = player.Inventory.HasItem(itemId);

                return new TradeActionResult
                {
                    Success = true,
                    Action = "buy",
                    ItemId = itemId,
                    LocationId = locationId,
                    CoinsBefore = coinsBefore,
                    CoinsAfter = coinsAfter,
                    CoinsChanged = coinsAfter - coinsBefore,
                    HadItemBefore = hadItemBefore,
                    HasItemAfter = hasItemAfter,
                    ErrorMessage = null,
                    TransactionPrice = buyPrice
                };
            }
            else
            {
                return new TradeActionResult
                {
                    Success = false,
                    Action = "buy",
                    ItemId = itemId,
                    LocationId = locationId,
                    ErrorMessage = "Purchase failed - insufficient funds or item not available"
                };
            }
        }
        catch (Exception ex)
        {
            return new TradeActionResult
            {
                Success = false,
                Action = "buy",
                ItemId = itemId,
                LocationId = locationId,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Try to execute a sell action and return detailed result.
    /// Provides better error handling and state tracking than the boolean SellItem method.
    /// </summary>
    public TradeActionResult TrySellItem(string itemId, string locationId)
    {
        try
        {
            Player player = _gameWorld.GetPlayer();
            int coinsBefore = player.Coins;
            bool hadItemBefore = player.Inventory.HasItem(itemId);

            // Get price before attempting sale
            int sellPrice = GetItemPrice(locationId, itemId, false);

            // Attempt the sale
            bool success = SellItem(itemId, locationId);

            if (success)
            {
                int coinsAfter = player.Coins;
                bool hasItemAfter = player.Inventory.HasItem(itemId);

                return new TradeActionResult
                {
                    Success = true,
                    Action = "sell",
                    ItemId = itemId,
                    LocationId = locationId,
                    CoinsBefore = coinsBefore,
                    CoinsAfter = coinsAfter,
                    CoinsChanged = coinsAfter - coinsBefore,
                    HadItemBefore = hadItemBefore,
                    HasItemAfter = hasItemAfter,
                    ErrorMessage = null,
                    TransactionPrice = sellPrice
                };
            }
            else
            {
                return new TradeActionResult
                {
                    Success = false,
                    Action = "sell",
                    ItemId = itemId,
                    LocationId = locationId,
                    ErrorMessage = "Sale failed - item not in inventory or not sellable at this location"
                };
            }
        }
        catch (Exception ex)
        {
            return new TradeActionResult
            {
                Success = false,
                Action = "sell",
                ItemId = itemId,
                LocationId = locationId,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get profit potential for an item across all locations.
    /// Useful for UI trading analysis and testing arbitrage scenarios.
    /// </summary>
    public MarketArbitrageInfo GetArbitrageOpportunities(string itemId)
    {
        List<MarketPriceInfo> allPrices = GetItemMarketPrices(itemId);

        if (allPrices.Count < 2)
        {
            return new MarketArbitrageInfo
            {
                ItemId = itemId,
                BestBuyLocation = null,
                BestSellLocation = null,
                MaxProfit = 0,
                AllPrices = allPrices
            };
        }

        // Find best buy location (lowest buy price)
        MarketPriceInfo bestBuy = allPrices.Where(p => p.CanBuy).OrderBy(p => p.BuyPrice).FirstOrDefault();

        // Find best sell location (highest sell price)
        MarketPriceInfo bestSell = allPrices.OrderByDescending(p => p.SellPrice).FirstOrDefault();

        int maxProfit = (bestBuy != null && bestSell != null) ? bestSell.SellPrice - bestBuy.BuyPrice : 0;

        return new MarketArbitrageInfo
        {
            ItemId = itemId,
            BestBuyLocation = bestBuy,
            BestSellLocation = bestSell,
            MaxProfit = maxProfit,
            AllPrices = allPrices
        };
    }

    /// <summary>
    /// Get current inventory status for market display
    /// </summary>
    public string GetInventoryStatusForMarket()
    {
        Player player = _gameWorld.GetPlayer();
        Inventory inventory = player.Inventory;

        int usedSlots = inventory.GetUsedSlots(_itemRepository);
        int maxSlots = inventory.GetMaxSlots(_itemRepository);

        return $"Inventory: {usedSlots}/{maxSlots} slots used";
    }

    /// <summary>
    /// Check if item can fit in inventory with size considerations
    /// </summary>
    public string GetInventoryConstraintMessage(string itemId)
    {
        Player player = _gameWorld.GetPlayer();
        Item item = _itemRepository.GetItemById(itemId);

        if (item == null) return "Item not found";

        if (player.Inventory.CanAddItem(item, _itemRepository))
        {
            return $"Can fit ({item.GetRequiredSlots()} slot{(item.GetRequiredSlots() > 1 ? "s" : "")} required)";
        }
        else
        {
            int usedSlots = player.Inventory.GetUsedSlots(_itemRepository);
            int maxSlots = player.Inventory.GetMaxSlots(_itemRepository);
            int requiredSlots = item.GetRequiredSlots();

            return $"Cannot fit: needs {requiredSlots} slot{(requiredSlots > 1 ? "s" : "")}, only {maxSlots - usedSlots} available";
        }
    }

    /// <summary>
    /// Check if market trading is available at a location during the current time
    /// </summary>
    private bool IsMarketAvailableAtLocation(string locationId, TimeBlocks currentTime)
    {
        // Get NPCs who provide Trade services at this location
        List<NPC> tradeNPCs = _npcRepository.GetNPCsForLocationAndTime(locationId, currentTime)
            .Where(npc => npc.CanProvideService(ServiceTypes.Commerce))
            .ToList();

        Console.WriteLine($"[DEBUG] IsMarketAvailableAtLocation: location={locationId}, time={currentTime}, tradeNPCs={tradeNPCs.Count}");

        // Market is available if at least one trade NPC is present
        bool available = tradeNPCs.Any();
        Console.WriteLine($"[DEBUG] Market available at {locationId}: {available}");
        return available;
    }

    /// <summary>
    /// Get market availability status for UI display
    /// </summary>
    public string GetMarketAvailabilityStatus(string locationId)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        bool isAvailable = IsMarketAvailableAtLocation(locationId, currentTime);

        if (isAvailable)
        {
            return "Market Open";
        }
        else
        {
            // Find when market will be available next
            List<NPC> tradeNPCs = _npcRepository.GetNPCsForLocation(locationId)
                .Where(npc => npc.CanProvideService(ServiceTypes.Commerce))
                .ToList();

            if (!tradeNPCs.Any())
            {
                return "No traders at this location";
            }

            // Find next available time
            List<TimeBlocks> allTimes = new List<TimeBlocks> { TimeBlocks.Dawn, TimeBlocks.Morning, TimeBlocks.Afternoon, TimeBlocks.Evening, TimeBlocks.Night };
            foreach (TimeBlocks time in allTimes)
            {
                if (tradeNPCs.Any(npc => npc.IsAvailable(time)))
                {
                    return $"Market opens at {time.ToString().Replace('_', ' ')}";
                }
            }

            return "Market closed";
        }
    }

    /// <summary>
    /// Get detailed market status with trader information
    /// </summary>
    public string GetDetailedMarketStatus(string locationId)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        List<NPC> tradeNPCs = _npcRepository.GetNPCsForLocation(locationId)
            .Where(npc => npc.CanProvideService(ServiceTypes.Commerce))
            .ToList();

        if (!tradeNPCs.Any())
        {
            return "No traders at this location";
        }

        List<NPC> currentlyAvailable = tradeNPCs.Where(npc => npc.IsAvailable(currentTime)).ToList();

        if (currentlyAvailable.Any())
        {
            string traderNames = string.Join(", ", currentlyAvailable.Select(npc => npc.Name));
            return $"Market Open - Traders available: {traderNames}";
        }
        else
        {
            return "Market Closed - No traders available right now";
        }
    }

    /// <summary>
    /// Get all traders at a location regardless of availability
    /// </summary>
    public List<NPC> GetAllTraders(string locationId)
    {
        return _npcRepository.GetNPCsForLocation(locationId)
            .Where(npc => npc.CanProvideService(ServiceTypes.Commerce))
            .ToList();
    }

    /// <summary>
    /// Get currently available traders at a location
    /// </summary>
    public List<NPC> GetCurrentTraders(string locationId)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        return _npcRepository.GetNPCsForLocationAndTime(locationId, currentTime)
            .Where(npc => npc.CanProvideService(ServiceTypes.Commerce))
            .ToList();
    }
}