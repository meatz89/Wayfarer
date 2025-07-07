public class MarketManager
{
    private GameWorld gameWorld;
    private LocationSystem LocationSystem;

    public MarketManager(GameWorld gameWorld, LocationSystem locationSystem)
    {
        this.gameWorld = gameWorld;
        LocationSystem = locationSystem;
    }

    public List<TradeItem> GetAvailableItems(string locationId)
    {
        Location location = LocationSystem.GetLocation(locationId);
        return location.MarketItems.Where(i => i.IsAvailable).ToList();
    }

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

    public void BuyItem(Item item)
    {
        Player player = gameWorld.GetPlayer();

        // Deduct cost
        player.Coins -= item.BuyPrice;
    }
}