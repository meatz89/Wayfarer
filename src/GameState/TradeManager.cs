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


    public bool CanSellItem(string itemName)
    {
        return gameWorld.GetPlayer().Inventory.HasItem(itemName);
    }



    public void BuyItem(string itemId, string locationId)
    {
        // Get item from repository
        Item item = itemRepository.GetItemById(itemId);
        if (item == null) return;

        // Delegate to MarketManager for location-aware pricing and transactions
        marketManager.BuyItem(itemId, locationId);
    }

    public void SellItem(string itemId, string locationId)
    {
        // Get item from repository
        Item item = itemRepository.GetItemById(itemId);
        if (item == null) return;

        // Delegate to MarketManager for location-aware pricing and transactions
        marketManager.SellItem(itemId, locationId);
    }

    public void DropItem(Item item)
    {
        Player player = gameWorld.GetPlayer();

        // Remove from inventory
        player.Inventory.RemoveItem(item.Name);
    }
}