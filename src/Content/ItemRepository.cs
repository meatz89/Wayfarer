public class ItemRepository
{
    private readonly GameWorld _gameWorld;

    public ItemRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;

        if (_gameWorld.Items == null)
        {
            _gameWorld.Items = new List<Item>();
        }

        // Note: Items are loaded later in the initialization pipeline.
        // The repository is created early for dependency injection.
    }

    #region Read Methods

    public Item GetItemById(string id)
    {
        if (_gameWorld.Items == null)
        {throw new InvalidOperationException("Items collection not initialized - data loading failed");
        }
        return _gameWorld.Items.FirstOrDefault(i => i.Id == id);
    }

    public List<Item> GetAllItems()
    {
        if (_gameWorld.Items == null)
        {throw new InvalidOperationException("Items collection not initialized - data loading failed");
        }
        return _gameWorld.Items;
    }

    #endregion

    #region Write Methods

    public void AddItems(IEnumerable<Item> items)
    {
        if (_gameWorld.Items == null)
        {
            _gameWorld.Items = new List<Item>();
        }

        foreach (Item item in items)
        {
            if (_gameWorld.Items.Any(i => i.Id == item.Id))
            {
                throw new InvalidOperationException($"Item with ID '{item.Id}' already exists.");
            }

            _gameWorld.Items.Add(item);
        }
    }

    public bool RemoveItem(string id)
    {
        if (_gameWorld.Items == null)
        {
            return false;
        }

        Item item = _gameWorld.Items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            return _gameWorld.Items.Remove(item);
        }

        return false;
    }

    #endregion
}