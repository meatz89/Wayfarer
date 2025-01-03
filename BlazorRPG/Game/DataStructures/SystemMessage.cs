
// Create a proper class for system messages
public class SystemMessage
{
    public string Message { get; }
    public SystemMessageTypes Type { get; }

    public SystemMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        Message = message;
        Type = type;
    }
}
