public class ItemRepository
{
    private readonly GameWorld _gameWorld;

    public ItemRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
        
        if (_gameWorld.WorldState.Items == null)
        {
            _gameWorld.WorldState.Items = new List<Item>();
        }
        
        if (!_gameWorld.WorldState.Items.Any())
        {
            Console.WriteLine("WARNING: No items loaded from GameWorld. JSON loading may have failed.");
        }
    }

    #region Read Methods

    public Item GetItemById(string id)
    {
        return _gameWorld.WorldState.Items?.FirstOrDefault(i => i.Id == id);
    }

    public Item GetItemByName(string name)
    {
        return _gameWorld.WorldState.Items?.FirstOrDefault(i => i.Name == name);
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

    #endregion

    #region Write Methods

    public void AddItem(Item item)
    {
        if (_gameWorld.WorldState.Items == null)
        {
            _gameWorld.WorldState.Items = new List<Item>();
        }

        if (_gameWorld.WorldState.Items.Any(i => i.Id == item.Id))
        {
            throw new InvalidOperationException($"Item with ID '{item.Id}' already exists.");
        }

        _gameWorld.WorldState.Items.Add(item);
    }

    public void AddItems(IEnumerable<Item> items)
    {
        foreach (Item item in items)
        {
            AddItem(item);
        }
    }

    public bool RemoveItem(string id)
    {
        if (_gameWorld.WorldState.Items == null)
        {
            return false;
        }

        Item item = _gameWorld.WorldState.Items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            return _gameWorld.WorldState.Items.Remove(item);
        }

        return false;
    }

    public void UpdateItem(Item item)
    {
        if (_gameWorld.WorldState.Items == null)
        {
            throw new InvalidOperationException("No items collection exists.");
        }

        Item existingItem = _gameWorld.WorldState.Items.FirstOrDefault(i => i.Id == item.Id);
        if (existingItem == null)
        {
            throw new InvalidOperationException($"Item with ID '{item.Id}' not found.");
        }

        int index = _gameWorld.WorldState.Items.IndexOf(existingItem);
        _gameWorld.WorldState.Items[index] = item;
    }

    public void ClearAllItems()
    {
        if (_gameWorld.WorldState.Items == null)
        {
            _gameWorld.WorldState.Items = new List<Item>();
        }
        else
        {
            _gameWorld.WorldState.Items.Clear();
        }
    }

    #endregion
}