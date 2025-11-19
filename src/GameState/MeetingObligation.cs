/// <summary>
/// Represents an obligation to meet with an NPC at a specific time.
/// Different from DeliveryObligation which is about delivering letters.
/// </summary>
public class MeetingObligation
{
    // HIGHLANDER: NO Id property - MeetingObligation identified by object reference
    // HIGHLANDER: Object reference ONLY, no RequesterId
    public NPC Requester { get; set; }  // NPC who requested the meeting
    public string Reason { get; set; }  // Why they need to meet

}