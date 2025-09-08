// Strongly-typed resource amount to replace Dictionary<ResourceType, int>
public class ResourceAmount
{
    public ResourceType Type { get; set; }
    public int Amount { get; set; }

    public ResourceAmount() { }

    public ResourceAmount(ResourceType type, int amount)
    {
        Type = type;
        Amount = amount;
    }

    public override string ToString()
    {
        return $"{Amount} {Type.ToString().ToLower()}";
    }
}