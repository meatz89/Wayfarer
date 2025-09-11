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
    public int DeadlineInSegments { get; set; }  // Segments until meeting deadline
    public StakeType Stakes { get; set; } = StakeType.REPUTATION;
    public string Reason { get; set; }  // Why they need to meet

    // Calculated properties for urgency levels based on segments
    public bool IsUrgent => DeadlineInSegments <= 4;  // ≤4 segments  
    public bool IsCritical => DeadlineInSegments <= 2;  // ≤2 segments

    // For UI display
    public int DeadlineInSegments_Display => DeadlineInSegments;

}