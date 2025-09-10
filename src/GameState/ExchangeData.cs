
// Exchange data for commerce cards
public class ExchangeData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ExchangeName { get; set; }
    public string Description { get; set; }
    public List<ResourceAmount> Costs { get; set; } = new();
    public List<ResourceAmount> Rewards { get; set; } = new();
    public int TrustRequirement { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool SingleUse { get; set; }

    // Additional properties used in CardDeckLoader
    public PersonalityType NPCPersonality { get; set; }
    public int BaseSuccessRate { get; set; }
    public bool CanBarter { get; set; }
    public string TemplateId { get; set; }
    
    // Exchange validation properties
    public string ExchangeId { get; set; }
    public int MinimumRelationshipTier { get; set; }
    public bool IsUnique { get; set; }
    public int TimesUsed { get; set; }
    public int MaxUses { get; set; }
    public List<string> RequiredDomains { get; set; }
    public List<TimeBlocks> TimeRestrictions { get; set; }
    public ConnectionType? RequiredTokenType { get; set; }
    public int MinimumTokensRequired { get; set; }
    public List<string> RequiredItems { get; set; }
    public bool RequiresPatience { get; set; }
    public ConnectionState? RequiredConnectionState { get; set; }
    
    // Exchange effects
    public bool GrantsTokens { get; set; }
    public string UnlocksExchangeId { get; set; }
    public string TriggerEvent { get; set; }
    public bool AffectsRelationship { get; set; }
    public bool ConsumesPatience { get; set; }
    public int PatienceCost { get; set; }
    public bool AdvancesTime { get; set; }
    public int TimeAdvancementHours { get; set; }
    public int FlowModifier { get; set; }
    public ConnectionState? ConnectionStateChange { get; set; }

    public bool CanAfford(Player player)
    {
        foreach (ResourceAmount cost in Costs)
        {
            switch (cost.Type)
            {
                case ResourceType.Coins:
                    if (player.Coins < cost.Amount) return false;
                    break;
                case ResourceType.Health:
                    if (player.Health < cost.Amount) return false;
                    break;
                case ResourceType.Hunger:
                    if (player.Hunger < cost.Amount) return false;
                    break;
            }
        }
        return true;
    }

    public bool CanAfford(PlayerResourceState playerResources, TokenMechanicsManager tokenManager, int currentAttention)
    {
        // Check if player can afford the exchange
        if (playerResources == null) return false;

        foreach (ResourceAmount cost in Costs)
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
                case ResourceType.Attention:
                    if (currentAttention < cost.Amount) return false;
                    break;
            }
        }
        return true;
    }

    public string GetNarrativeContext()
    {
        List<string> givesList = new List<string>();
        foreach (ResourceAmount cost in Costs)
        {
            givesList.Add($"{cost.Amount} {cost.Type.ToString().ToLower()}");
        }

        List<string> receivesList = new List<string>();
        foreach (ResourceAmount reward in Rewards)
        {
            receivesList.Add($"{reward.Amount} {reward.Type.ToString().ToLower()}");
        }

        return $"Trading {string.Join(", ", givesList)} for {string.Join(", ", receivesList)}";
    }

    public List<ResourceAmount> GetCostAsList()
    {
        return Costs;
    }

    public List<ResourceAmount> GetRewardAsList()
    {
        return Rewards;
    }
}
