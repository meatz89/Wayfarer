public class SystemMessage
{
    public string Message { get; }
    public SystemMessageTypes Type { get; }
    public MessageCategory? Category { get; }
    public DateTime Timestamp { get; }
    public bool IsNew { get; set; }
    public DateTime ExpiresAt { get; }
    public int DurationMilliseconds { get; }

    /// <summary>
    /// HIGHLANDER: All parameters required - caller must specify type, category, and duration
    /// </summary>
    public SystemMessage(string message, SystemMessageTypes type, MessageCategory? category, int durationMs)
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
