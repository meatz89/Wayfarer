using System;
using System.Collections.Generic;

/// <summary>
/// Types of resources that can be exchanged
/// </summary>
public enum ResourceType
{
    Coins,
    Health,
    Attention,  // Daily attention points (replaces both Stamina and Concentration)
    Hunger,  // Setting hunger to specific value (e.g., 0 for "full")
    TrustToken,
    CommerceToken,
    StatusToken,
    ShadowToken,
    Item  // For item exchanges (uses ItemId field)
}

/// <summary>
/// A single resource requirement or reward
/// </summary>
public class ResourceExchange
{
    public ResourceType Type { get; init; }
    public int Amount { get; init; }
    public bool IsAbsolute { get; init; }  // true = set to value, false = modify by value
    public string ItemId { get; init; }  // For ResourceType.Item - specifies which item
    
    /// <summary>
    /// Get display text for this exchange
    /// </summary>
    public string GetDisplayText()
    {
        if (Type == ResourceType.Item)
        {
            return Amount > 1 ? $"{Amount}x {ItemId}" : ItemId;
        }
        else if (IsAbsolute)
        {
            return Type switch
            {
                ResourceType.Hunger => $"Hunger = {Amount}",
                _ => $"{Type} = {Amount}"
            };
        }
        else
        {
            var sign = Amount >= 0 ? "+" : "";
            return $"{Type} {sign}{Amount}";
        }
    }
}

/// <summary>
/// An exchange card represents a direct resource trade.
/// Used in Quick Exchange conversations (0 attention cost).
/// </summary>
public class ExchangeCard : ICard
{
    /// <summary>
    /// Unique identifier for this exchange
    /// </summary>
    public string Id { get; init; }
    
    /// <summary>
    /// Template type for narrative generation
    /// </summary>
    public string TemplateType { get; init; }
    
    /// <summary>
    /// NPC personality type (affects narrative framing)
    /// </summary>
    public PersonalityType NPCPersonality { get; init; }
    
    /// <summary>
    /// What the player must give
    /// </summary>
    public List<ResourceExchange> Cost { get; init; } = new();
    
    /// <summary>
    /// What the player receives
    /// </summary>
    public List<ResourceExchange> Reward { get; init; } = new();
    
    /// <summary>
    /// Base success rate for bartering (before token bonuses)
    /// </summary>
    public int BaseSuccessRate { get; init; } = 60;
    
    /// <summary>
    /// Cost if bartering succeeds (discounted price)
    /// </summary>
    public List<ResourceExchange> SuccessCost { get; init; }
    
    /// <summary>
    /// Cost if bartering fails (full price)
    /// </summary>
    public List<ResourceExchange> FailureCost { get; init; }
    
    /// <summary>
    /// Whether this exchange can be bartered (has different success/failure costs)
    /// </summary>
    public bool CanBarter => SuccessCost != null && FailureCost != null;
    
    /// <summary>
    /// Check if player can afford the cost
    /// </summary>
    public bool CanAfford(PlayerResourceState resources, TokenMechanicsManager tokens, AttentionManager currentAttention = null)
    {
        foreach (var cost in Cost)
        {
            switch (cost.Type)
            {
                case ResourceType.Coins:
                    if (resources.Coins < cost.Amount) return false;
                    break;
                case ResourceType.Health:
                    if (resources.Health < cost.Amount) return false;
                    break;
                case ResourceType.Attention:
                    // Attention is managed by TimeBlockAttentionManager
                    if (currentAttention != null && currentAttention.Current < cost.Amount)
                        return false;
                    break;
                case ResourceType.TrustToken:
                case ResourceType.CommerceToken:
                case ResourceType.StatusToken:
                case ResourceType.ShadowToken:
                    var tokenType = cost.Type switch
                    {
                        ResourceType.TrustToken => ConnectionType.Trust,
                        ResourceType.CommerceToken => ConnectionType.Commerce,
                        ResourceType.StatusToken => ConnectionType.Status,
                        ResourceType.ShadowToken => ConnectionType.Shadow,
                        _ => ConnectionType.Trust
                    };
                    // Check if player has enough tokens of this type
                    if (tokens?.GetTokenCount(tokenType) < cost.Amount)
                        return false;
                    break;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Convert to ConversationCard for hand display
    /// </summary>
    public ConversationCard ToConversationCard()
    {
        // Generate display text
        var displayName = GetExchangeName();
        var costText = GetExchangeCostDisplay();
        var rewardText = GetExchangeRewardDisplay();
        
        return new ConversationCard
        {
            Id = Id,
            Template = CardTemplateType.Exchange,
            Context = new CardContext
            {
                NPCPersonality = NPCPersonality,
                ExchangeData = this
            },
            Type = CardType.Commerce,
            Persistence = PersistenceType.Fleeting,
            Weight = 0, // Exchanges are free to play
            BaseComfort = 0,
            DisplayName = displayName,
            Description = $"Cost: {costText} → Reward: {rewardText}",
            SuccessRate = 100 // Exchanges always succeed if affordable
        };
    }
    
    private string GetExchangeName()
    {
        // Generate proper exchange names based on template type
        return TemplateType switch
        {
            "food" => "Buy Travel Provisions",
            "healing" => "Purchase Medicine",
            "information" => "Information Trade",
            "work" => "Help Inventory Stock",
            "favor" => "Noble Favor",
            "lodging" => "Rest at the Inn",
            _ => "Make Exchange"
        };
    }
    
    private string GetExchangeCostDisplay()
    {
        if (Cost == null || !Cost.Any())
            return "Free";
            
        var costParts = Cost.Select(c => c.GetDisplayText());
        return string.Join(", ", costParts);
    }
    
    private string GetExchangeRewardDisplay()
    {
        if (Reward == null || !Reward.Any())
            return "Nothing";
            
        var rewardParts = Reward.Select(r => r.GetDisplayText());
        return string.Join(", ", rewardParts);
    }
}

/// <summary>
/// Factory for creating exchange cards based on NPC personality and context
/// </summary>
public static class ExchangeCardFactory
{
    /// <summary>
    /// Create exchange deck based on NPC personality and location spot properties
    /// </summary>
    public static List<ExchangeCard> CreateExchangeDeck(
        PersonalityType personality, 
        string npcId,
        List<string> spotDomainTags = null)
    {
        var deck = new List<ExchangeCard>();
        Console.WriteLine($"[DEBUG ExchangeCardFactory] Creating exchange deck for {npcId} with personality {personality}");
        if (spotDomainTags != null)
        {
            Console.WriteLine($"[DEBUG ExchangeCardFactory] SpotDomainTags: [{string.Join(", ", spotDomainTags)}]");
        }
        else
        {
            Console.WriteLine($"[DEBUG ExchangeCardFactory] SpotDomainTags: NULL");
        }
        
        // Different personalities offer different types of exchanges
        switch (personality)
        {
            case PersonalityType.MERCANTILE:
                // Marcus's specific exchanges from POC plan
                // Buy Travel Provisions: 3 coins → Hunger = 0 (Repeatable)
                deck.Add(new ExchangeCard
                {
                    Id = $"{npcId}_food_trade",
                    TemplateType = "food",
                    NPCPersonality = personality,
                    Cost = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 3 } },
                    Reward = new() { new ResourceExchange { Type = ResourceType.Hunger, Amount = 0, IsAbsolute = true } }
                });
                // Buy Medicine: 5 coins → Health +20 (Repeatable)
                deck.Add(new ExchangeCard
                {
                    Id = $"{npcId}_health_trade",
                    TemplateType = "healing",
                    NPCPersonality = personality,
                    Cost = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 5 } },
                    Reward = new() { new ResourceExchange { Type = ResourceType.Health, Amount = 20 } }
                });
                // Purchase Fine Silk: 15 coins → Fine Silk item
                deck.Add(new ExchangeCard
                {
                    Id = $"{npcId}_silk_trade",
                    TemplateType = "luxury",
                    NPCPersonality = personality,
                    Cost = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 15 } },
                    Reward = new() { new ResourceExchange { Type = ResourceType.Item, Amount = 1, ItemId = "fine_silk" } }
                });
                // Help Inventory Stock: 3 attention → 8 coins (advances time)
                deck.Add(new ExchangeCard
                {
                    Id = $"{npcId}_work_trade",
                    TemplateType = "work",
                    NPCPersonality = personality,
                    Cost = new() { new ResourceExchange { Type = ResourceType.Attention, Amount = 3 } },
                    Reward = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 8 } }
                });
                Console.WriteLine($"[DEBUG ExchangeCardFactory] Added {deck.Count} cards for MERCANTILE personality");
                break;
                
            case PersonalityType.STEADFAST:
                // STEADFAST NPCs at Hospitality locations offer rest exchanges (like innkeepers)
                bool isHospitalitySpot = spotDomainTags?.Contains("Hospitality") ?? false;
                
                if (isHospitalitySpot)
                {
                    // Stay the Night: 5 coins → Full rest (calculated by hunger formula)
                    deck.Add(new ExchangeCard
                    {
                        Id = $"{npcId}_stay_night",
                        TemplateType = "lodging",
                        NPCPersonality = personality,
                        Cost = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 5 } },
                        Reward = new() { 
                            new ResourceExchange { Type = ResourceType.Attention, Amount = 7, IsAbsolute = true },
                            new ResourceExchange { Type = ResourceType.Hunger, Amount = 0, IsAbsolute = true },
                            new ResourceExchange { Type = ResourceType.Health, Amount = 20 }
                        }
                    });
                    
                    // Quick Nap: 2 coins → +3 attention
                    deck.Add(new ExchangeCard
                    {
                        Id = $"{npcId}_quick_nap",
                        TemplateType = "lodging",
                        NPCPersonality = personality,
                        Cost = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 2 } },
                        Reward = new() { new ResourceExchange { Type = ResourceType.Attention, Amount = 3 } }
                    });
                    
                    Console.WriteLine($"[DEBUG ExchangeCardFactory] Added {deck.Count} rest exchange cards for STEADFAST NPC at Hospitality location");
                }
                else
                {
                    Console.WriteLine($"[DEBUG ExchangeCardFactory] STEADFAST NPC {npcId} is not at a Hospitality location - no exchanges");
                }
                break;
                
            case PersonalityType.DEVOTED:
                // DEVOTED NPCs at Hospitality locations might offer charitable rest/food
                bool isHospitalityDevoted = spotDomainTags?.Contains("Hospitality") ?? false;
                
                if (isHospitalityDevoted)
                {
                    // Charitable Meal: 2 coins → Hunger = 0  
                    deck.Add(new ExchangeCard
                    {
                        Id = $"{npcId}_charitable_meal",
                        TemplateType = "food",
                        NPCPersonality = personality,
                        Cost = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 2 } },
                        Reward = new() { new ResourceExchange { Type = ResourceType.Hunger, Amount = 0, IsAbsolute = true } }
                    });
                    
                    // Pilgrim's Rest: 3 coins → +5 attention + Hunger = 0
                    deck.Add(new ExchangeCard
                    {
                        Id = $"{npcId}_pilgrim_rest",
                        TemplateType = "lodging",
                        NPCPersonality = personality,
                        Cost = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 3 } },
                        Reward = new() { 
                            new ResourceExchange { Type = ResourceType.Attention, Amount = 5 },
                            new ResourceExchange { Type = ResourceType.Hunger, Amount = 0, IsAbsolute = true }
                        }
                    });
                    
                    Console.WriteLine($"[DEBUG ExchangeCardFactory] Added {deck.Count} hospitality exchange cards for DEVOTED NPC");
                }
                break;
                
            default:
                // Other personality types have no exchange decks
                Console.WriteLine($"[DEBUG ExchangeCardFactory] NPC {npcId} with personality {personality} has no exchange deck");
                return new List<ExchangeCard>();
        }
        
        Console.WriteLine($"[DEBUG ExchangeCardFactory] Final deck for {npcId} has {deck.Count} cards");
        return deck;
    }
    
}