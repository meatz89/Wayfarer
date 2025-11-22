/// <summary>
/// Fluent builder for creating Item test data.
/// Provides sensible defaults and chainable configuration methods.
/// Eliminates hard-coded test fixtures - generates data programmatically.
/// </summary>
public class ItemBuilder
{
    private string _name;
    private int _weight;
    private int _buyPrice;
    private int _sellPrice;
    private SizeCategory _size;
    private List<ItemCategory> _categories;
    private string _description;

    public ItemBuilder()
    {
        // Sensible defaults for minimal test setup
        _name = GenerateUniqueName("Item");
        _weight = 2;
        _buyPrice = 10;
        _sellPrice = 8;
        _size = SizeCategory.Small;
        _categories = new List<ItemCategory>();
        _description = "Test item";
    }

    public ItemBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ItemBuilder WithWeight(int weight)
    {
        _weight = weight;
        return this;
    }

    public ItemBuilder WithBuyPrice(int buyPrice)
    {
        _buyPrice = buyPrice;
        return this;
    }

    public ItemBuilder WithSellPrice(int sellPrice)
    {
        _sellPrice = sellPrice;
        return this;
    }

    public ItemBuilder WithSize(SizeCategory size)
    {
        _size = size;
        return this;
    }

    public ItemBuilder WithCategory(ItemCategory category)
    {
        if (!_categories.Contains(category))
        {
            _categories.Add(category);
        }
        return this;
    }

    public ItemBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public Item Build()
    {
        Item item = new Item
        {
            Name = _name,
            Weight = _weight,
            BuyPrice = _buyPrice,
            SellPrice = _sellPrice,
            Size = _size,
            Categories = new List<ItemCategory>(_categories),
            Description = _description
        };
        return item;
    }

    // Convenience factory methods for common scenarios
    public static Item Simple()
    {
        return new ItemBuilder().Build();
    }

    public static Item Expensive()
    {
        return new ItemBuilder()
            .WithBuyPrice(100)
            .WithSellPrice(80)
            .WithName(GenerateUniqueName("Expensive"))
            .Build();
    }

    public static Item Heavy()
    {
        return new ItemBuilder()
            .WithWeight(5)
            .WithSize(SizeCategory.Large)
            .WithName(GenerateUniqueName("Heavy"))
            .Build();
    }

    public static Item Lightweight()
    {
        return new ItemBuilder()
            .WithWeight(1)
            .WithSize(SizeCategory.Tiny)
            .WithName(GenerateUniqueName("Light"))
            .Build();
    }

    public static Item TradeGood()
    {
        return new ItemBuilder()
            .WithCategory(ItemCategory.Trade_Goods)
            .WithWeight(2)
            .WithBuyPrice(15)
            .WithSellPrice(12)
            .WithName(GenerateUniqueName("TradeGood"))
            .Build();
    }

    public static Item Luxury()
    {
        return new ItemBuilder()
            .WithCategory(ItemCategory.Luxury_Items)
            .WithWeight(1)
            .WithBuyPrice(200)
            .WithSellPrice(150)
            .WithName(GenerateUniqueName("Luxury"))
            .Build();
    }

    private static string GenerateUniqueName(string prefix)
    {
        return $"{prefix}_{Guid.NewGuid().ToString().Substring(0, 8)}";
    }
}
