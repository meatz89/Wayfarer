public class SystemMessage
{
    public string Message { get; }
    public SystemMessageTypes Type { get; }
    public DateTime Timestamp { get; }
    public bool IsNew { get; set; }
    public DateTime ExpiresAt { get; }
    public int DurationMilliseconds { get; }

    public SystemMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info, int durationMs = 5000)
    {
        Message = message;
        Type = type;
        Timestamp = DateTime.Now;
        IsNew = false;
        DurationMilliseconds = durationMs;
        ExpiresAt = Timestamp.AddMilliseconds(durationMs);
    }

    public bool IsExpired => DateTime.Now > ExpiresAt;
}
