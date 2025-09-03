
// Exchange data for commerce cards
public class ExchangeData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ExchangeName { get; set; }
    public string Description { get; set; }
    public Dictionary<string, int> PlayerGives { get; set; } = new();
    public Dictionary<string, int> PlayerReceives { get; set; } = new();
    public Dictionary<ResourceType, int> Cost { get; set; } = new();
    public Dictionary<ResourceType, int> Reward { get; set; } = new();
    public int TrustRequirement { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool SingleUse { get; set; }

    // Additional properties used in CardDeckLoader
    public PersonalityType NPCPersonality { get; set; }
    public int BaseSuccessRate { get; set; }
    public bool CanBarter { get; set; }
    public string TemplateId { get; set; }

    public bool CanAfford(Player player)
    {
        foreach (KeyValuePair<ResourceType, int> cost in Cost)
        {
            switch (cost.Key)
            {
                case ResourceType.Coins:
                    if (player.Coins < cost.Value) return false;
                    break;
                case ResourceType.Health:
                    if (player.Health < cost.Value) return false;
                    break;
                case ResourceType.Hunger:
                    if (player.Hunger < cost.Value) return false;
                    break;
            }
        }
        return true;
    }

    public bool CanAfford(PlayerResourceState playerResources, TokenMechanicsManager tokenManager, int currentAttention)
    {
        // Check if player can afford the exchange
        if (playerResources == null) return false;

        foreach (KeyValuePair<ResourceType, int> cost in Cost)
        {
            switch (cost.Key)
            {
                case ResourceType.Coins:
                    if (playerResources.Coins < cost.Value) return false;
                    break;
                case ResourceType.Health:
                    if (playerResources.Health < cost.Value) return false;
                    break;
                case ResourceType.Hunger:
                    if (playerResources.Stamina < cost.Value) return false;
                    break;
                case ResourceType.Attention:
                    if (currentAttention < cost.Value) return false;
                    break;
            }
        }
        return true;
    }

    public string GetNarrativeContext()
    {
        List<string> givesList = new List<string>();
        foreach (KeyValuePair<string, int> kv in PlayerGives)
        {
            givesList.Add($"{kv.Value} {kv.Key}");
        }

        List<string> receivesList = new List<string>();
        foreach (KeyValuePair<string, int> kv in PlayerReceives)
        {
            receivesList.Add($"{kv.Value} {kv.Key}");
        }

        return $"Trading {string.Join(", ", givesList)} for {string.Join(", ", receivesList)}";
    }

    public List<ResourceExchange> GetCostAsList()
    {
        List<ResourceExchange> list = new List<ResourceExchange>();
        foreach (KeyValuePair<ResourceType, int> kv in Cost)
        {
            list.Add(new ResourceExchange { ResourceType = kv.Key, Amount = kv.Value });
        }
        return list;
    }

    public List<ResourceExchange> GetRewardAsList()
    {
        List<ResourceExchange> list = new List<ResourceExchange>();
        foreach (KeyValuePair<ResourceType, int> kv in Reward)
        {
            list.Add(new ResourceExchange { ResourceType = kv.Key, Amount = kv.Value });
        }
        return list;
    }
}
