/// <summary>
/// Represents an obligation to meet with an NPC at a specific time.
/// Different from DeliveryObligation which is about delivering letters.
/// </summary>
public class MeetingObligation
{
public string Id { get; set; }
public string RequesterId { get; set; }  // NPC who requested the meeting
public string RequesterName { get; set; }
public string Reason { get; set; }  // Why they need to meet

}