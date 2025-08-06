using System;

namespace Wayfarer.GameState;

/// <summary>
/// Represents a system message for the event log and activity feed.
/// </summary>
public class SystemMessage
{
    public string Message { get; set; }
    public MessageType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsMajorEvent { get; set; }
    public object Data { get; set; }
}

public enum MessageType
{
    General,
    Resource,
    Inventory,
    Time,
    Location,
    Quest,
    Combat,
    Social,
    DayTransition,
    Error,
    Warning,
    Success
}