using System;
using System.Collections.Generic;

/// <summary>
/// Types of resources that can be exchanged
/// </summary>
public enum ResourceType
{
    Coins,
    Health,
    Stamina,
    Concentration,
    Hunger,  // Setting hunger to specific value (e.g., 0 for "full")
    TrustToken,
    CommerceToken,
    StatusToken,
    ShadowToken
}

/// <summary>
/// A single resource requirement or reward
/// </summary>
public class ResourceExchange
{
    public ResourceType Type { get; init; }
    public int Amount { get; init; }
    public bool IsAbsolute { get; init; }  // true = set to value, false = modify by value
    
    /// <summary>
    /// Get display text for this exchange
    /// </summary>
    public string GetDisplayText()
    {
        if (IsAbsolute)
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
public class ExchangeCard
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
    /// Maximum uses per day (usually 1)
    /// </summary>
    public int MaxUsesPerDay { get; init; } = 1;
    
    /// <summary>
    /// Current uses today
    /// </summary>
    public int UsesToday { get; set; }
    
    /// <summary>
    /// Last day this was used (for refresh tracking)
    /// </summary>
    public int LastUsedDay { get; set; } = -1;
    
    /// <summary>
    /// Mark this exchange as used for today
    /// </summary>
    public void MarkUsed(int currentDay)
    {
        if (currentDay > LastUsedDay)
        {
            // New day, reset counter
            LastUsedDay = currentDay;
            UsesToday = 1;
        }
        else
        {
            // Same day, increment counter
            UsesToday++;
        }
    }
    
    /// <summary>
    /// Whether this exchange is currently available
    /// </summary>
    public bool IsAvailable(int currentDay)
    {
        // Reset uses if it's a new day
        if (currentDay != LastUsedDay)
        {
            UsesToday = 0;
            LastUsedDay = currentDay;
        }
        
        return UsesToday < MaxUsesPerDay;
    }
    
    /// <summary>
    /// Mark this exchange as used
    /// </summary>
    public void MarkUsed()
    {
        UsesToday++;
    }
    
    /// <summary>
    /// Check if player can afford the cost
    /// </summary>
    public bool CanAfford(PlayerResourceState resources, TokenMechanicsManager tokens)
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
                case ResourceType.Stamina:
                    if (resources.Stamina < cost.Amount) return false;
                    break;
                case ResourceType.Concentration:
                    if (resources.Concentration < cost.Amount) return false;
                    break;
                case ResourceType.TrustToken:
                    if (tokens.GetTokenCount(ConnectionType.Trust) < cost.Amount) return false;
                    break;
                case ResourceType.CommerceToken:
                    if (tokens.GetTokenCount(ConnectionType.Commerce) < cost.Amount) return false;
                    break;
                case ResourceType.StatusToken:
                    if (tokens.GetTokenCount(ConnectionType.Status) < cost.Amount) return false;
                    break;
                case ResourceType.ShadowToken:
                    if (tokens.GetTokenCount(ConnectionType.Shadow) < cost.Amount) return false;
                    break;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Get narrative context based on NPC type and template
    /// </summary>
    public string GetNarrativeContext()
    {
        // This will be expanded based on NPC personality and template type
        // For now, return a simple description
        return TemplateType switch
        {
            "food" => NPCPersonality switch
            {
                PersonalityType.MERCANTILE => "purchase provisions",
                PersonalityType.DEVOTED => "share a meal",
                PersonalityType.STEADFAST => "trade for rations",
                _ => "exchange for food"
            },
            "healing" => NPCPersonality switch
            {
                PersonalityType.DEVOTED => "receive healing prayers",
                PersonalityType.MERCANTILE => "buy medical supplies",
                _ => "seek treatment"
            },
            "information" => NPCPersonality switch
            {
                PersonalityType.CUNNING => "trade secrets",
                _ => "exchange information"
            },
            "work" => NPCPersonality switch
            {
                PersonalityType.STEADFAST => "offer honest labor",
                PersonalityType.MERCANTILE => "negotiate a job",
                _ => "work for coins"
            },
            _ => "make an exchange"
        };
    }
}

/// <summary>
/// Factory for creating exchange cards based on NPC personality
/// </summary>
public static class ExchangeCardFactory
{
    public static List<ExchangeCard> CreateExchangeDeck(PersonalityType personality, string npcId)
    {
        var deck = new List<ExchangeCard>();
        
        switch (personality)
        {
            case PersonalityType.MERCANTILE:
                // Traders focus on coin exchanges
                deck.Add(new ExchangeCard
                {
                    Id = $"{npcId}_food_trade",
                    TemplateType = "food",
                    NPCPersonality = personality,
                    Cost = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 2 } },
                    Reward = new() { new ResourceExchange { Type = ResourceType.Hunger, Amount = 0, IsAbsolute = true } }
                });
                deck.Add(new ExchangeCard
                {
                    Id = $"{npcId}_health_trade",
                    TemplateType = "healing",
                    NPCPersonality = personality,
                    Cost = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 5 } },
                    Reward = new() { new ResourceExchange { Type = ResourceType.Health, Amount = 3 } }
                });
                break;
                
            case PersonalityType.DEVOTED:
                // Clergy focus on trust and healing
                deck.Add(new ExchangeCard
                {
                    Id = $"{npcId}_blessing",
                    TemplateType = "healing",
                    NPCPersonality = personality,
                    Cost = new() { new ResourceExchange { Type = ResourceType.TrustToken, Amount = 1 } },
                    Reward = new() { new ResourceExchange { Type = ResourceType.Health, Amount = 2 } }
                });
                deck.Add(new ExchangeCard
                {
                    Id = $"{npcId}_charity",
                    TemplateType = "food",
                    NPCPersonality = personality,
                    Cost = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 1 } },
                    Reward = new() { new ResourceExchange { Type = ResourceType.Hunger, Amount = 0, IsAbsolute = true } }
                });
                break;
                
            case PersonalityType.STEADFAST:
                // Workers offer labor exchanges
                deck.Add(new ExchangeCard
                {
                    Id = $"{npcId}_labor",
                    TemplateType = "work",
                    NPCPersonality = personality,
                    Cost = new() { new ResourceExchange { Type = ResourceType.Stamina, Amount = 3 } },
                    Reward = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 8 } }
                });
                break;
                
            case PersonalityType.CUNNING:
                // Spies trade information
                deck.Add(new ExchangeCard
                {
                    Id = $"{npcId}_information",
                    TemplateType = "information",
                    NPCPersonality = personality,
                    Cost = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 3 } },
                    Reward = new() { new ResourceExchange { Type = ResourceType.ShadowToken, Amount = 1 } }
                });
                break;
                
            case PersonalityType.PROUD:
                // Nobles have expensive but powerful exchanges
                deck.Add(new ExchangeCard
                {
                    Id = $"{npcId}_favor",
                    TemplateType = "favor",
                    NPCPersonality = personality,
                    Cost = new() { new ResourceExchange { Type = ResourceType.StatusToken, Amount = 2 } },
                    Reward = new() { new ResourceExchange { Type = ResourceType.Coins, Amount = 20 } }
                });
                break;
        }
        
        return deck;
    }
}