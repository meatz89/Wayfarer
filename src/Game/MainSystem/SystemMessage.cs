public class SystemMessage
{
    public string Message { get; }
    public SystemMessageTypes Type { get; }
    public DateTime Timestamp { get; }
    public bool IsNew { get; set; }

    public SystemMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        Message = message;
        Type = type;
        Timestamp = DateTime.Now;
        IsNew = false;
    }
}
