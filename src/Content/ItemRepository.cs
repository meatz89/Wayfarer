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

        // Note: Items are loaded later in the initialization pipeline.
        // The repository is created early for dependency injection.
    }

    #region Read Methods

    public Item GetItemById(string id)
    {
        if (_gameWorld.WorldState.Items == null)
        {
            Console.WriteLine($"ERROR: Items collection is null in GetItemById({id})");
            throw new InvalidOperationException("Items collection not initialized - data loading failed");
        }
        return _gameWorld.WorldState.Items.FirstOrDefault(i => i.Id == id);
    }


    public List<Item> GetAllItems()
    {
        if (_gameWorld.WorldState.Items == null)
        {
            Console.WriteLine("ERROR: Items collection is null in GetAllItems");
            throw new InvalidOperationException("Items collection not initialized - data loading failed");
        }
        return _gameWorld.WorldState.Items;
    }


    #endregion

    #region Write Methods


    public void AddItems(IEnumerable<Item> items)
    {
        if (_gameWorld.WorldState.Items == null)
        {
            _gameWorld.WorldState.Items = new List<Item>();
        }

        foreach (Item item in items)
        {
            if (_gameWorld.WorldState.Items.Any(i => i.Id == item.Id))
            {
                throw new InvalidOperationException($"Item with ID '{item.Id}' already exists.");
            }

            _gameWorld.WorldState.Items.Add(item);
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



    #endregion
}