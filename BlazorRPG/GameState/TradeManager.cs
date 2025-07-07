public class TradeManager
{
    public List<Item> GetAvailableItems(GameWorld gameWorld)
    {
        Location currentLocation = gameWorld.Locations.Find(l => l.Id == gameWorld.CurrentLocation.Id);
        if (currentLocation == null || currentLocation.MarketItems == null)
        {
            return new List<Item>();
        }

        return currentLocation.MarketItems.ToList();
    }

    public bool CanBuyItem(GameWorld gameWorld, Item item)
    {
        bool hasEnoughMoney = gameWorld.PlayerCoins >= item.BuyPrice;
        bool hasInventorySpace = Array.IndexOf(gameWorld.PlayerInventory.ItemSlots, null) != -1;

        return hasEnoughMoney && hasInventorySpace;
    }

    public bool CanSellItem(GameWorld gameWorld, string itemName)
    {
        return Array.IndexOf(gameWorld.PlayerInventory.ItemSlots, itemName) != -1;
    }

    public void BuyItem(GameWorld gameWorld, Item item)
    {
        // Find empty inventory slot
        int emptySlot = Array.IndexOf(gameWorld.PlayerInventory.ItemSlots, null);
        if (emptySlot == -1)
        {
            return; // No empty slot (should never happen if CanBuyItem was checked)
        }

        // Deduct cost and add to inventory
        gameWorld.PlayerCoins -= item.BuyPrice;
        gameWorld.PlayerInventory.ItemSlots[emptySlot] = item.Name;
    }

    public void SellItem(GameWorld gameWorld, Item item)
    {
        // Find the item in inventory
        int itemSlot = Array.IndexOf(gameWorld.PlayerInventory.ItemSlots, item.Name);
        if (itemSlot == -1)
        {
            return; // Item not found (should never happen if CanSellItem was checked)
        }

        // Add money and remove from inventory
        gameWorld.PlayerCoins += item.SellPrice;
        gameWorld.PlayerInventory.ItemSlots[itemSlot] = null;
    }

    public void DropItem(GameWorld gameWorld, int inventorySlot)
    {
        if (inventorySlot >= 0 && inventorySlot < gameWorld.PlayerInventory.ItemSlots.Length)
        {
            gameWorld.PlayerInventory.ItemSlots[inventorySlot] = null;
        }
    }
}
