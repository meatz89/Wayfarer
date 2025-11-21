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
        {
            throw new InvalidOperationException("Items collection not initialized - data loading failed");
        }
        // HIGHLANDER: Item.Id deleted, use Name as natural key
        return _gameWorld.Items.FirstOrDefault(i => i.Name == id);
    }

    public List<Item> GetAllItems()
    {
        if (_gameWorld.Items == null)
        {
            throw new InvalidOperationException("Items collection not initialized - data loading failed");
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
            // ADR-007: Use object equality instead of string matching
            if (_gameWorld.Items.Contains(item))
            {
                throw new InvalidOperationException($"Item '{item.Name}' already exists.");
            }

            _gameWorld.Items.Add(item);
        }
    }

    #endregion
}