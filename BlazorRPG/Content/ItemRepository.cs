public class ItemRepository
{
    private List<Item> _items = new List<Item>();

    public ItemRepository()
    {
        // Initialize with standard items
        _items = new List<Item>
        {
            new Item { Id = "herbs", Name = "Herbs", Weight = 1, BuyPrice = 2, SellPrice = 1 },
            new Item { Id = "tools", Name = "Tools", Weight = 3, BuyPrice = 8, SellPrice = 4 },
            new Item { Id = "rare_book", Name = "Rare Book", Weight = 1, BuyPrice = 15, SellPrice = 8 },
            new Item { Id = "rope", Name = "Rope", Weight = 2, BuyPrice = 6, SellPrice = 3,
                       EnabledRouteTypes = new List<string> { "MountainPass" } },
            new Item { Id = "merchant_papers", Name = "Merchant Papers", Weight = 0, BuyPrice = 10, SellPrice = 5,
                       EnabledRouteTypes = new List<string> { "TradeCaravan" } },
            new Item { Id = "lantern", Name = "Lantern", Weight = 1, BuyPrice = 4, SellPrice = 2,
                       EnabledRouteTypes = new List<string> { "NightTravel" } },
            new Item { Id = "iron_ingots", Name = "Iron Ingots", Weight = 6, BuyPrice = 5, SellPrice = 15, InventorySlots = 2 },
        };
    }

    public Item GetItemById(string id)
    {
        return _items.FirstOrDefault(i => i.Id == id);
    }

    public Item GetItemByName(string name)
    {
        return _items.FirstOrDefault(i => i.Name == name);
    }

    public List<Item> GetAllItems()
    {
        return _items;
    }
}