public class ItemContent
{
    public static Item CharmingPendant => new ItemBuilder()
        .WithName(ItemNames.CharmingPendant)
        .WithDescription("A beautifully crafted pendant that catches everyone's eye")
        .Build();

    public static Item WoodcuttersAxe => new ItemBuilder()
        .WithName(ItemNames.WoodcuttersAxe)
        .WithDescription("A nice axe for cutting trees")
        .Build();

    public static Item TorchLight => new ItemBuilder()
        .WithName(ItemNames.Torchlight)
        .WithDescription("A torchlight for nighttime gathering")
        .Build();

    public static Item CraftingApron => new ItemBuilder()
        .WithName(ItemNames.CraftingApron)
        .WithDescription("A sturdy apron that makes crafting easier")
        .Build();
}
