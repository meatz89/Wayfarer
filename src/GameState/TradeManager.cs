public class TradeManager
{
    private GameWorld gameWorld;
    private ItemRepository itemRepository;
    private MarketManager marketManager;

    public TradeManager(GameWorld gameWorld, ItemRepository itemRepository, MarketManager marketManager)
    {
        this.gameWorld = gameWorld;
        this.itemRepository = itemRepository;
        this.marketManager = marketManager;
    }

    public List<Item> GetAvailableItems(string locationId, string spotId)
    {
        // Use MarketManager for dynamic pricing instead of static repository data
        return marketManager.GetAvailableItems(locationId);
    }

    public bool CanBuyItem(Item item)
    {
        // Get current location for location-aware pricing
        string currentLocationId = gameWorld.CurrentLocation.Id;
        
        // Delegate to MarketManager for location-aware pricing
        return marketManager.CanBuyItem(item.Id ?? item.Name, currentLocationId);
    }

    public bool CanSellItem(string itemName)
    {
        return gameWorld.GetPlayer().Inventory.HasItem(itemName);
    }

    public void BuyItem(Item item)
    {
        // Get current location for location-aware pricing
        string currentLocationId = gameWorld.CurrentLocation.Id;
        
        // Delegate to MarketManager for location-aware pricing and transactions
        marketManager.BuyItem(item.Id ?? item.Name, currentLocationId);
    }

    public void SellItem(Item item)
    {
        // Get current location for location-aware pricing
        string currentLocationId = gameWorld.CurrentLocation.Id;
        
        // Delegate to MarketManager for location-aware pricing and transactions
        marketManager.SellItem(item.Name, currentLocationId);
    }

    public void DropItem(Item item)
    {
        Player player = gameWorld.GetPlayer();

        // Remove from inventory
        player.Inventory.RemoveItem(item.Name);
    }
}