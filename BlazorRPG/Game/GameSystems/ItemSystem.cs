public class ItemSystem
{
    private readonly GameState gameState;
    private readonly List<Item> allItems;

    public ItemSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.allItems = contentProvider.GetItems();
    }

    public Item GetItemFromName(ItemNames names)
    {
        Item? item = allItems.FirstOrDefault(i => i.Name == names);
        return item;
    }
}