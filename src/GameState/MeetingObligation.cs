using System;

/// <summary>
/// Represents an obligation to meet with an NPC at a specific time.
/// Different from DeliveryObligation which is about delivering letters.
/// </summary>
public class MeetingObligation
{
    public string Id { get; set; }
    public string RequesterId { get; set; }  // NPC who requested the meeting
    public string RequesterName { get; set; }
    public int DeadlineInMinutes { get; set; }  // Minutes until meeting deadline
    public StakeType Stakes { get; set; } = StakeType.REPUTATION;
    public string Reason { get; set; }  // Why they need to meet
    
    // Calculated properties for urgency levels
    public bool IsUrgent => DeadlineInMinutes < 360;  // <6 hours
    public bool IsCritical => DeadlineInMinutes < 180;  // <3 hours
    
    // For UI display
    public int DeadlineInHours => (int)Math.Ceiling(DeadlineInMinutes / 60.0);
    
    // Calculate minutes remaining (for compatibility with existing systems)
    public int MinutesUntilDeadline => DeadlineInMinutes;
}