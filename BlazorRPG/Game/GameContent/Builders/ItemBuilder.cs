public class ItemBuilder
{
    private ItemNames itemName;
    private ItemTypes itemType;
    private string description;
    private List<ActionModifier> modifiers = new();

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

    public ItemBuilder WithActionModifier(Action<ActionModifierBuilder> buildModifier)
    {
        ActionModifierBuilder builder = new();

        builder.WithSource(itemName.ToString());

        buildModifier(builder);
        modifiers.AddRange(builder.Build());
        return this;
    }

    public Item Build()
    {
        return new Item(this.itemName.ToString(), this.itemType);
    }

}
