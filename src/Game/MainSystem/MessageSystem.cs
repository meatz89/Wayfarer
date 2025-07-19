public class MessageSystem
{
    private List<SystemMessage> currentMessages = new();

    public void AddSystemMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        currentMessages.Add(new SystemMessage(message, type));
    }

    public List<SystemMessage> GetAndClearMessages()
    {
        var messages = new List<SystemMessage>(currentMessages);
        currentMessages.Clear();
        return messages;
    }
}