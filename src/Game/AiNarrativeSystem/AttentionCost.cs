/// <summary>
/// Represents the attention cost for a conversation choice in the literary UI system.
/// Replaces the old FocusCost mechanic.
/// </summary>
public class AttentionCost
{
    public int Amount { get; private set; }

    public AttentionCost(int amount)
    {
        Amount = Math.Max(0, Math.Min(amount, 3)); // Clamp between 0-3
    }

    // For JSON serialization
    public object ToJsonObject()
    {
        return new { Amount = Amount };
    }

    /// <summary>
    /// Get narrative description of the cost
    /// </summary>
    public string GetDescription()
    {
        return Amount switch
        {
            0 => "A simple response",
            1 => "Requires focus",
            2 => "Demands deep concentration",
            3 => "Exhausts your full attention",
            _ => "Unknown effort"
        };
    }
}