public class ItemBuilder
{
    private ItemNames itemName;
    private string description;
    private List<ActionModifier> modifiers = new();

    public ItemBuilder WithName(ItemNames itemName)
    {
        this.itemName = itemName;
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
        buildModifier(builder);
        modifiers.AddRange(builder.Build());
        return this;
    }

    public Item Build()
    {
        return new Item
        {
            Name = this.itemName,
            Description = this.description,
            ActionModifiers = modifiers
        };
    }

}
