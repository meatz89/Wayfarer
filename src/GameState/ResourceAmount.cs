// Strongly-typed resource amount to replace Dictionary<ResourceType, int>
public class ResourceAmount
{
public ResourceType Type { get; set; }
public int Amount { get; set; }
public string ItemId { get; set; } // For Item type resources

public ResourceAmount() { }

public ResourceAmount(ResourceType type, int amount)
{
    Type = type;
    Amount = amount;
}

public override string ToString()
{
    if (Type == ResourceType.Item && !string.IsNullOrEmpty(ItemId))
    {
        return $"{Amount} {ItemId}";
    }
    return $"{Amount} {Type.ToString().ToLower()}";
}
}