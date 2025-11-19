// Strongly-typed resource amount to replace Dictionary<ResourceType, int>
// HIGHLANDER: Stores Item object for Item-type resources
public class ResourceAmount
{
    public ResourceType Type { get; set; }
    public int Amount { get; set; }
    public Item Item { get; set; } // For Item type resources (typed object, not string ID)

    public ResourceAmount() { }

    public ResourceAmount(ResourceType type, int amount)
    {
        Type = type;
        Amount = amount;
    }

    public override string ToString()
    {
        if (Type == ResourceType.Item && Item != null)
        {
            return $"{Amount} {Item.Name}"; // Extract .Name for display only
        }
        return $"{Amount} {Type.ToString().ToLower()}";
    }
}