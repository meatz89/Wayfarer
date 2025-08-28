/// <summary>
/// Types of resources in the game
/// </summary>
public enum ResourceType
{
    Coins,
    Health,
    Hunger,
    Attention,
    TrustToken,
    CommerceToken,
    StatusToken,
    ShadowToken,
    Item
}

/// <summary>
/// Represents a resource cost or reward in exchanges
/// </summary>
public class ResourceExchange
{
    /// <summary>
    /// Type of resource
    /// </summary>
    public ResourceType ResourceType { get; init; }
    
    /// <summary>
    /// Amount of the resource (positive for gain, negative for cost)
    /// </summary>
    public int Amount { get; init; }
    
    /// <summary>
    /// Whether this is an absolute value (vs additive)
    /// For example: "Health = 10" vs "Health +3"
    /// </summary>
    public bool IsAbsolute { get; init; }
    
    /// <summary>
    /// Item ID for Item resource type exchanges
    /// </summary>
    public string ItemId { get; init; }
    
    /// <summary>
    /// Display format for UI (e.g., "3 Coins", "Health = 10", "+2 Favor")
    /// </summary>
    public string GetDisplayText()
    {
        if (IsAbsolute)
        {
            return $"{ResourceType} = {Amount}";
        }
        
        var prefix = Amount >= 0 ? "+" : "";
        return $"{prefix}{Amount} {ResourceType}";
    }
}