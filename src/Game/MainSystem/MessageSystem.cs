public class MessageSystem
{
    private readonly GameWorld _gameWorld;
    
    public MessageSystem(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    public void AddSystemMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        _gameWorld.SystemMessages.Add(new SystemMessage(message, type));
    }
}