/// <summary>
/// Context for commerce/exchange conversations
/// Contains exchange-specific data without dictionaries
/// </summary>
public class CommerceContext : ConversationContextBase
{
    public List<ExchangeData> AvailableExchanges { get; set; }
    public ExchangeData SelectedExchange { get; set; }
    public bool CanAffordSelectedExchange { get; set; }
    public List<ResourceAmount> PlayerResources { get; set; }
    public string ExchangeName { get; set; }
    public string ExchangeDescription { get; set; }
    public List<ResourceAmount> ExchangeCosts { get; set; }
    public List<ResourceAmount> ExchangeRewards { get; set; }
    public int TrustRequirement { get; set; }
    public PersonalityType NPCPersonality { get; set; }
    public int BaseSuccessRate { get; set; }
    public bool CanBarter { get; set; }

    public void SetNPCPersonality(PersonalityType personality)
    {
        NPCPersonality = personality;
        // Additional personality-specific logic can be added here
    }

    public CommerceContext()
    {
        Type = ConversationType.Commerce;
        AvailableExchanges = new List<ExchangeData>();
        PlayerResources = new List<ResourceAmount>();
        ExchangeCosts = new List<ResourceAmount>();
        ExchangeRewards = new List<ResourceAmount>();
    }

    public void SetSelectedExchange(ExchangeData exchange)
    {
        SelectedExchange = exchange;
        if (exchange != null)
        {
            ExchangeName = exchange.Name;
            ExchangeDescription = exchange.Description;
            ExchangeCosts = exchange.Costs;
            ExchangeRewards = exchange.Rewards;
            TrustRequirement = exchange.TrustRequirement;
            NPCPersonality = exchange.NPCPersonality;
            BaseSuccessRate = exchange.BaseSuccessRate;
            CanBarter = exchange.CanBarter;
        }
    }

    public bool CanAffordExchange(ExchangeData exchange, PlayerResourceState playerResourceState)
    {
        if (exchange == null || playerResourceState == null) return false;

        foreach (ResourceAmount cost in exchange.Costs)
        {
            switch (cost.Type)
            {
                case ResourceType.Coins:
                    if (playerResourceState.Coins < cost.Amount) return false;
                    break;
                case ResourceType.Health:
                    if (playerResourceState.Health < cost.Amount) return false;
                    break;
                case ResourceType.Hunger:
                    if (playerResourceState.Stamina < cost.Amount) return false;
                    break;
                case ResourceType.Attention:
                    if (AttentionSpent + cost.Amount > playerResourceState.MaxConcentration) return false;
                    break;
            }
        }
        return true;
    }
}