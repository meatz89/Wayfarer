public class MessageSystem
{
    private readonly GameWorld _gameWorld;

    public MessageSystem(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    public void AddSystemMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        // Different durations based on message importance
        int duration = type switch
        {
            SystemMessageTypes.Danger => 8000,   // 8 seconds for critical messages
            SystemMessageTypes.Warning => 6000,  // 6 seconds for warnings
            SystemMessageTypes.Success => 5000,  // 5 seconds for success
            SystemMessageTypes.Info => 4000,     // 4 seconds for info
            _ => 5000
        };

        SystemMessage systemMessage = new SystemMessage(message, type, duration);
        _gameWorld.SystemMessages.Add(systemMessage);
        // Also add to permanent event log
        _gameWorld.EventLog.Add(systemMessage);
    }
}