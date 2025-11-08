/// <summary>
/// Represents the cost structure for an exchange.
/// Supports multiple resource costs and token requirements.
/// All costs must be paid atomically - partial payment is not allowed.
/// </summary>
public class ExchangeCostStructure
{
    /// <summary>
    /// Resources that must be paid for this exchange.
    /// All resources must be available or the exchange cannot proceed.
    /// </summary>
    public List<ResourceAmount> Resources { get; set; } = new List<ResourceAmount>();

    /// <summary>
    /// Token requirements that gate access to this exchange.
    /// These are prerequisites, not consumed by the exchange.
    /// Key: ConnectionType (Trust/Diplomacy/Status/Shadow)
    /// Value: Minimum number of tokens required
    /// </summary>
    public Dictionary<ConnectionType, int> TokenRequirements { get; set; } = new Dictionary<ConnectionType, int>();

    /// <summary>
    /// Items that will be consumed (removed from inventory) by this exchange.
    /// PRINCIPLE 4: Items are resource costs, not boolean gates.
    /// If an exchange requires an item, it MUST consume it.
    /// </summary>
    public List<string> ConsumedItemIds { get; set; } = new List<string>();

    /// <summary>
    /// Checks if a player can afford all costs for this exchange.
    /// Does not check token requirements - those are prerequisites.
    /// </summary>
    public bool CanAfford(PlayerResourceState playerResources)
    {
        if (playerResources == null) return false;

        foreach (ResourceAmount cost in Resources)
        {
            switch (cost.Type)
            {
                case ResourceType.Coins:
                    if (playerResources.Coins < cost.Amount) return false;
                    break;
                case ResourceType.Health:
                    if (playerResources.Health < cost.Amount) return false;
                    break;
                case ResourceType.Hunger:
                    if (playerResources.Stamina < cost.Amount) return false;
                    break;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks if token requirements are met.
    /// Tokens are not consumed, just checked as prerequisites.
    /// </summary>
    public bool MeetsTokenRequirements(Dictionary<ConnectionType, int> playerTokens)
    {
        if (TokenRequirements == null || TokenRequirements.Count == 0)
            return true;

        foreach (KeyValuePair<ConnectionType, int> requirement in TokenRequirements)
        {
            if (!playerTokens.ContainsKey(requirement.Key) ||
                playerTokens[requirement.Key] < requirement.Value)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Gets a human-readable description of the costs.
    /// </summary>
    public string GetDescription()
    {
        List<string> parts = new List<string>();

        foreach (ResourceAmount resource in Resources)
        {
            parts.Add($"{resource.Amount} {resource.Type}");
        }

        foreach (string item in ConsumedItemIds)
        {
            parts.Add($"1x {item}");
        }

        return parts.Count > 0 ? string.Join(", ", parts) : "Free";
    }
}