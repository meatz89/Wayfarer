public class ItemDefinition
{
    public static List<Item> GetAllItems()
    {
        return new List<Item>
        {
            new Item { Name = "Herbs", Weight = 1, BuyPrice = 2, SellPrice = 1 },
            new Item { Name = "Tools", Weight = 3, BuyPrice = 8, SellPrice = 4 },
            new Item { Name = "Rare Book", Weight = 1, BuyPrice = 15, SellPrice = 8 },
            new Item { Name = "Rope", Weight = 2, BuyPrice = 6, SellPrice = 3, EnabledRouteTypes = new List<string> { "MountainPass" } },
            new Item { Name = "Merchant Papers", Weight = 0, BuyPrice = 10, SellPrice = 5, EnabledRouteTypes = new List<string> { "TradeCaravan" } },
            new Item { Name = "Lantern", Weight = 1, BuyPrice = 4, SellPrice = 2, EnabledRouteTypes = new List<string> { "NightTravel" } },
            new Item { Name = "Iron Ingots", Weight = 6, BuyPrice = 5, SellPrice = 15, InventorySlots = 2 },
        };
    }
}
