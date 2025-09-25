public class ItemDefinition
{
    public static List<Item> GetAllItems()
    {
        return new List<Item>
        {
            new Item { Name = "Herbs", Focus = 1, BuyPrice = 2, SellPrice = 1 },
            new Item { Name = "Tools", Focus = 3, BuyPrice = 8, SellPrice = 4 },
            new Item { Name = "Rare Book", Focus = 1, BuyPrice = 15, SellPrice = 8 },
            new Item { Name = "Rope", Focus = 2, BuyPrice = 6, SellPrice = 3, Categories = new List<ItemCategory> { ItemCategory.Climbing_Equipment } },
            new Item { Name = "Merchant Papers", Focus = 0, BuyPrice = 10, SellPrice = 5, Categories = new List<ItemCategory> { ItemCategory.Special_Access } },
            new Item { Name = "Lantern", Focus = 1, BuyPrice = 4, SellPrice = 2, Categories = new List<ItemCategory> { ItemCategory.Light_Source } },
            new Item { Name = "Iron Ingots", Focus = 6, BuyPrice = 5, SellPrice = 15, Weight = 2 },
        };
    }
}
