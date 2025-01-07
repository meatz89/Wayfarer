public class ActionResultMessages
{
    // Store the actual outcomes that were applied
    public List<Outcome> Outcomes { get; init; } = new();
    // Store any system messages (like "Night falls...")
    public List<SystemMessage> SystemMessages { get; init; } = new();
}