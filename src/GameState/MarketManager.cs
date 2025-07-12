using Wayfarer.Game.MainSystem;

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
    private readonly ContractProgressionService _contractProgressionService;

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
                        ContractProgressionService contractProgressionService)
    {
        _gameWorld = gameWorld;
        _locationSystem = locationSystem;
        _itemRepository = itemRepository;
        _contractProgressionService = contractProgressionService;
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
        if (pricing.BuyPrice <= pricing.SellPrice)
        {
            pricing.BuyPrice = pricing.SellPrice + 1;
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

        // Get all items from ItemRepository and create location-specific versions
        List<Item> allItems = _itemRepository.GetAllItems();

        foreach (Item baseItem in allItems)
        {
            LocationPricing pricing = GetDynamicPricing(locationId, baseItem.Id);
            if (pricing.IsAvailable)
            {
                Item item = CreateItemWithLocationPricing(baseItem.Id, locationId);
                availableItems.Add(item);
            }
        }

        return availableItems;
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
            Categories = baseItem?.Categories ?? new List<EquipmentCategory>()
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
            _ => weight > 4 ? "extremely heavy" : "light"
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
        bool hasInventorySpace = player.Inventory.HasFreeSlot();

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

        player.Coins -= buyPrice;
        player.Inventory.AddItem(itemId);

        // Check for contract progression based on purchase
        _contractProgressionService.CheckMarketProgression(itemId, locationId, TransactionType.Buy, 1, buyPrice, player);

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

        player.Inventory.RemoveItem(itemId);
        player.Coins += sellPrice;

        // Check for contract progression based on sale
        _contractProgressionService.CheckMarketProgression(itemId, locationId, TransactionType.Sell, 1, sellPrice, player);

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
        var prices = new List<MarketPriceInfo>();
        
        // Get all locations
        List<Location> locations = _gameWorld.WorldState.locations ?? new List<Location>();
        
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
}