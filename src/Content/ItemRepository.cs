public class ItemRepository
{
    private List<Item> _items = new List<Item>();
    private GameWorld gameWorld;

    public ItemRepository(GameWorld gameWorld)
    {
        this.gameWorld = gameWorld;
        // Load items from GameWorld instead of hardcoding
        _items = gameWorld.WorldState.Items ?? new List<Item>();
        
        // If no items are loaded, this means there's a problem with JSON loading
        if (!_items.Any())
        {
            Console.WriteLine("WARNING: No items loaded from GameWorld. JSON loading may have failed.");
        }
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

    public List<Item> GetItemsForLocation(string locationId, string spotId = null)
    {
        if (spotId != null)
        {
            return _items.Where(i => i.LocationId == locationId && i.SpotId == spotId).ToList();
        }
        else
        {
            return _items.Where(i => i.LocationId == locationId).ToList();
        }
    }
}