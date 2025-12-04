public class MessageSystem
{
    private readonly GameWorld _gameWorld;

    public MessageSystem(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// HIGHLANDER: type and category required - caller must be explicit about message type
    /// </summary>
    public void AddSystemMessage(string message, SystemMessageTypes type, MessageCategory? category)
    {
        // Different durations based on message importance
        int duration = type switch
        {
            SystemMessageTypes.Danger => 15000,   // 15 seconds for critical messages
            SystemMessageTypes.Warning => 12000,  // 12 seconds for warnings
            SystemMessageTypes.Success => 10000,  // 10 seconds for success
            SystemMessageTypes.Tutorial => 12000, // 12 seconds for tutorial
            SystemMessageTypes.Info => 8000,     // 8 seconds for info
            _ => 10000
        };

        SystemMessage systemMessage = new SystemMessage(message, type, category, duration);
        _gameWorld.SystemMessages.Add(systemMessage);
        // Also add to permanent event log
        _gameWorld.EventLog.Add(systemMessage);
    }

    /// <summary>
    /// Get all current system messages
    /// </summary>
    public List<SystemMessage> GetMessages()
    {
        return _gameWorld.SystemMessages.ToList();
    }

    /// <summary>
    /// Clear all current system messages
    /// </summary>
    public void ClearMessages()
    {
        _gameWorld.SystemMessages.Clear();
    }
}