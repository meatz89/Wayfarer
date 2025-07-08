public class TradeManager
{
    private GameWorld gameWorld;
    private ItemRepository itemRepository;

    public TradeManager(GameWorld gameWorld, ItemRepository itemRepository)
    {
        this.gameWorld = gameWorld;
        this.itemRepository = itemRepository;
    }

    public List<Item> GetAvailableItems(string locationId, string spotId)
    {
        return itemRepository.GetItemsForLocation(locationId, spotId);
    }

    public bool CanBuyItem(Item item)
    {
        Player player = gameWorld.GetPlayer();
        bool hasEnoughMoney = player.Coins >= item.BuyPrice;
        bool hasInventorySpace = player.Inventory.HasFreeSlot();

        return hasEnoughMoney && hasInventorySpace;
    }

    public bool CanSellItem(string itemName)
    {
        return gameWorld.GetPlayer().Inventory.HasItem(itemName);
    }

    public void BuyItem(Item item)
    {
        Player player = gameWorld.GetPlayer();

        // Deduct cost
        player.Coins -= item.BuyPrice;

        // Add to inventory
        player.Inventory.AddItem(item.Name);
    }

    public void SellItem(Item item)
    {
        Player player = gameWorld.GetPlayer();

        // Add money
        player.Coins += item.SellPrice;

        // Remove from inventory
        player.Inventory.RemoveItem(item.Name);
    }

    public void DropItem(Item item)
    {
        Player player = gameWorld.GetPlayer();

        // Remove from inventory
        player.Inventory.RemoveItem(item.Name);
    }
}