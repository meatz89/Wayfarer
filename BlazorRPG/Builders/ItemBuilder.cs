public class ItemBuilder
{
    private ItemNames itemName;
    private ItemTypes itemType;
    private string description;

    public ItemBuilder WithName(ItemNames itemName)
    {
        this.itemName = itemName;
        return this;
    }

    public ItemBuilder WithItemType(ItemTypes itemType)
    {
        this.itemType = itemType;
        return this;
    }

    public ItemBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public Item Build()
    {
        return new Item(this.itemName.ToString(), this.itemType);
    }

}
