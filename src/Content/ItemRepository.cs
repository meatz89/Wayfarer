public class ItemRepository
{
    private readonly GameWorld _gameWorld;

    public ItemRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
        
        if (_gameWorld.WorldState.Items == null || !_gameWorld.WorldState.Items.Any())
        {
            Console.WriteLine("WARNING: No items loaded from GameWorld. JSON loading may have failed.");
        }
    }

    public Item GetItemById(string id)
    {
        return _gameWorld.WorldState.Items?.FirstOrDefault(i => i.Id == id);
    }

    public Item GetItemByName(string name)
    {
        return _gameWorld.WorldState.Items?.FirstOrDefault(i => i.Name == name);
    }

    public Item GetItem(string itemId)
    {
        return GetItemById(itemId);
    }

    public List<Item> GetAllItems()
    {
        return _gameWorld.WorldState.Items ?? new List<Item>();
    }

    public List<Item> GetItemsForLocation(string locationId, string spotId = null)
    {
        var items = _gameWorld.WorldState.Items ?? new List<Item>();
        if (spotId != null)
        {
            return items.Where(i => i.LocationId == locationId && i.SpotId == spotId).ToList();
        }
        else
        {
            return items.Where(i => i.LocationId == locationId).ToList();
        }
    }
}