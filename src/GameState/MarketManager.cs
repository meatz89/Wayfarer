/// <summary>
/// Manages location-specific item pricing and trading operations.
/// Implements dynamic pricing system that creates arbitrage opportunities
/// between different locations for strategic trading gameplay.
/// </summary>
public class MarketManager
{
    private GameWorld gameWorld;
    private LocationSystem LocationSystem;
    private Dictionary<string, Dictionary<string, LocationPricing>> locationPricing;

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

    public MarketManager(GameWorld gameWorld, LocationSystem locationSystem)
    {
        this.gameWorld = gameWorld;
        LocationSystem = locationSystem;
        InitializeLocationPricing();
    }

    /// <summary>
    /// Initialize location-specific pricing for all items at all locations.
    /// Creates arbitrage opportunities between locations.
    /// </summary>
    private void InitializeLocationPricing()
    {
        locationPricing = new Dictionary<string, Dictionary<string, LocationPricing>>
        {
            ["town_square"] = new Dictionary<string, LocationPricing>
            {
                ["herbs"] = new LocationPricing { BuyPrice = 3, SellPrice = 2, SupplyLevel = 1.2f, IsAvailable = true },
                ["tools"] = new LocationPricing { BuyPrice = 8, SellPrice = 6, SupplyLevel = 0.8f, IsAvailable = true },
                ["rope"] = new LocationPricing { BuyPrice = 7, SellPrice = 4, SupplyLevel = 0.9f, IsAvailable = true },
                ["food"] = new LocationPricing { BuyPrice = 4, SellPrice = 2, SupplyLevel = 0.7f, IsAvailable = true }
            },
            ["dusty_flagon"] = new Dictionary<string, LocationPricing>
            {
                ["herbs"] = new LocationPricing { BuyPrice = 2, SellPrice = 4, SupplyLevel = 1.8f, IsAvailable = true },
                ["food"] = new LocationPricing { BuyPrice = 1, SellPrice = 3, SupplyLevel = 2.0f, IsAvailable = true },
                ["tools"] = new LocationPricing { BuyPrice = 12, SellPrice = 8, SupplyLevel = 0.3f, IsAvailable = true },
                ["rope"] = new LocationPricing { BuyPrice = 9, SellPrice = 5, SupplyLevel = 0.5f, IsAvailable = false }
            }
        };
    }

    /// <summary>
    /// Get the price for a specific item at a specific location.
    /// </summary>
    /// <param name="locationId">The location ID</param>
    /// <param name="itemId">The item ID</param>
    /// <param name="isBuyPrice">True for buy price, false for sell price</param>
    /// <returns>Price in coins, or -1 if item not available at location</returns>
    public int GetItemPrice(string locationId, string itemId, bool isBuyPrice)
    {
        if (locationPricing.ContainsKey(locationId) && 
            locationPricing[locationId].ContainsKey(itemId))
        {
            LocationPricing pricing = locationPricing[locationId][itemId];
            if (!pricing.IsAvailable) return -1; // Item not available
            
            return isBuyPrice ? pricing.BuyPrice : pricing.SellPrice;
        }
        return -1; // Item not found
    }

    /// <summary>
    /// Get all items available for purchase at a specific location with location-specific pricing.
    /// </summary>
    /// <param name="locationId">The location ID</param>
    /// <returns>List of items with location-specific pricing applied</returns>
    public List<Item> GetAvailableItems(string locationId)
    {
        List<Item> availableItems = new List<Item>();
        
        if (locationPricing.ContainsKey(locationId))
        {
            foreach (KeyValuePair<string, LocationPricing> itemPricing in locationPricing[locationId])
            {
                if (itemPricing.Value.IsAvailable)
                {
                    Item item = CreateItemWithLocationPricing(itemPricing.Key, locationId);
                    availableItems.Add(item);
                }
            }
        }
        
        return availableItems;
    }

    /// <summary>
    /// Create an item instance with location-specific pricing applied.
    /// </summary>
    private Item CreateItemWithLocationPricing(string itemId, string locationId)
    {
        LocationPricing pricing = locationPricing[locationId][itemId];
        
        return new Item
        {
            Id = itemId,
            Name = itemId, // For now, use ID as name
            BuyPrice = pricing.BuyPrice,
            SellPrice = pricing.SellPrice,
            IsAvailable = pricing.IsAvailable,
            Weight = GetDefaultWeight(itemId),
            InventorySlots = 1,
            LocationId = locationId,
            Description = GetDefaultDescription(itemId)
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
    /// Calculate arbitrage opportunities between two locations.
    /// </summary>
    /// <param name="fromLocation">Location to buy from</param>
    /// <param name="toLocation">Location to sell to</param>
    /// <returns>List of profitable arbitrage opportunities, sorted by profit descending</returns>
    public List<ArbitrageOpportunity> GetArbitrageOpportunities(string fromLocation, string toLocation)
    {
        List<ArbitrageOpportunity> opportunities = new List<ArbitrageOpportunity>();
        
        if (!locationPricing.ContainsKey(fromLocation) || !locationPricing.ContainsKey(toLocation))
            return opportunities;
            
        foreach (string itemId in locationPricing[fromLocation].Keys)
        {
            if (locationPricing[toLocation].ContainsKey(itemId))
            {
                int buyPrice = GetItemPrice(fromLocation, itemId, true);
                int sellPrice = GetItemPrice(toLocation, itemId, false);
                
                if (buyPrice > 0 && sellPrice > 0 && sellPrice > buyPrice)
                {
                    opportunities.Add(new ArbitrageOpportunity
                    {
                        ItemId = itemId,
                        BuyPrice = buyPrice,
                        SellPrice = sellPrice,
                        Profit = sellPrice - buyPrice,
                        ProfitMargin = ((float)(sellPrice - buyPrice) / buyPrice) * 100
                    });
                }
            }
        }
        
        return opportunities.OrderByDescending(o => o.Profit).ToList();
    }

    /// <summary>
    /// Check if player can buy a specific item at a specific location.
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="locationId">The location ID</param>
    /// <returns>True if player can afford and has inventory space</returns>
    public bool CanBuyItem(string itemId, string locationId)
    {
        Player player = gameWorld.GetPlayer();
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
        
        Player player = gameWorld.GetPlayer();
        int buyPrice = GetItemPrice(locationId, itemId, true);
        
        player.Coins -= buyPrice;
        player.Inventory.AddItem(itemId);
        
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
        Player player = gameWorld.GetPlayer();
        
        if (!player.Inventory.HasItem(itemId)) return false;
        
        int sellPrice = GetItemPrice(locationId, itemId, false);
        if (sellPrice <= 0) return false; // Cannot sell here
        
        player.Inventory.RemoveItem(itemId);
        player.Coins += sellPrice;
        
        return true;
    }

    /// <summary>
    /// Legacy method for backward compatibility - delegates to new location-aware method
    /// </summary>
    public bool CanBuyItem(Item item)
    {
        Player player = gameWorld.GetPlayer();

        // Check if player has enough money
        if (player.Coins < item.BuyPrice) return false;

        // Check if player has enough inventory space
        int freeSlots = player.GetMaxItemCapacity() - player.Inventory.ItemSlots.Count(i => i != null);
        if (freeSlots < item.InventorySlots) return false;

        return true;
    }

    /// <summary>
    /// Legacy method for backward compatibility - delegates to new location-aware method
    /// </summary>
    public void BuyItem(Item item)
    {
        Player player = gameWorld.GetPlayer();

        // Deduct cost
        player.Coins -= item.BuyPrice;
    }
}