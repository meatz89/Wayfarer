public class ItemRepository
{
    private WorldState _worldState;

    public ItemRepository(GameWorld gameWorld)
    {
        _worldState = gameWorld.WorldState;
    }

    internal Item GetItemByName(string itemId)
    {
        throw new NotImplementedException();
    }
}