
// Extension methods for display
public static class ResourceExtensions
{
    public static string GetDisplayText(this KeyValuePair<ResourceType, int> resourcePair)
    {
        string resourceName;
        if (resourcePair.Key == ResourceType.Coins) resourceName = "coins";
        else if (resourcePair.Key == ResourceType.Health) resourceName = "health";
        else if (resourcePair.Key == ResourceType.Hunger) resourceName = "hunger";
        else if (resourcePair.Key == ResourceType.Hunger) resourceName = "food";
        else if (resourcePair.Key == ResourceType.TrustToken) resourceName = "trust tokens";
        else if (resourcePair.Key == ResourceType.CommerceToken) resourceName = "commerce tokens";
        else if (resourcePair.Key == ResourceType.StatusToken) resourceName = "status tokens";
        else if (resourcePair.Key == ResourceType.ShadowToken) resourceName = "shadow tokens";
        else if (resourcePair.Key == ResourceType.Item) resourceName = "items";
        else resourceName = resourcePair.Key.ToString().ToLower();

        return $"{resourcePair.Value} {resourceName}";
    }
}
