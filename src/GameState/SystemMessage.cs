public class SystemMessage
{
    public string Message { get; }
    public SystemMessageTypes Type { get; }
    public MessageCategory? Category { get; }
    public DateTime Timestamp { get; }
    public bool IsNew { get; set; }
    public DateTime ExpiresAt { get; }
    public int DurationMilliseconds { get; }

    public SystemMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info, MessageCategory? category = null, int durationMs = 5000)
    {
        Message = message;
        Type = type;
        Category = category;
        Timestamp = DateTime.Now;
        IsNew = false;
        DurationMilliseconds = durationMs;
        ExpiresAt = Timestamp.AddMilliseconds(durationMs);
    }

    public bool IsExpired { get; set; } = false; // Now manually controlled, not time-based
}
