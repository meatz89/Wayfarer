using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Parses exchange DTOs into ExchangeCard entities.
/// Converts JSON exchange definitions into strongly-typed domain objects.
/// </summary>
public static class ExchangeParser
{
    /// <summary>
    /// Parse a single ExchangeDTO into an ExchangeCard
    /// </summary>
    public static ExchangeCard ParseExchange(ExchangeDTO dto, string npcId = null)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        ExchangeCard card = new ExchangeCard
        {
            Id = dto.Id,
            Name = dto.Name ?? $"Exchange {dto.Id}",
            Description = GenerateDescription(dto),
            NpcId = npcId ?? string.Empty,
            
            // Default to trade type
            ExchangeType = DetermineExchangeType(dto),
            
            // Parse cost structure
            Cost = ParseCostStructure(dto),
            
            // Parse reward structure
            Reward = ParseRewardStructure(dto),
            
            // Default properties
            SingleUse = false,
            SuccessRate = 100,
            IsCompleted = false
        };

        return card;
    }

    /// <summary>
    /// Parse multiple exchange DTOs
    /// </summary>
    public static List<ExchangeCard> ParseExchanges(List<ExchangeDTO> dtos, string npcId = null)
    {
        if (dtos == null || dtos.Count == 0)
            return new List<ExchangeCard>();

        return dtos.Select(dto => ParseExchange(dto, npcId)).ToList();
    }

    /// <summary>
    /// Generate a description from the exchange DTO
    /// </summary>
    private static string GenerateDescription(ExchangeDTO dto)
    {
        return $"Trade {dto.GiveAmount} {dto.GiveCurrency} for {dto.ReceiveAmount} {dto.ReceiveCurrency}";
    }

    /// <summary>
    /// Determine the exchange type based on DTO properties
    /// </summary>
    private static ExchangeType DetermineExchangeType(ExchangeDTO dto)
    {
        // Simple heuristics for exchange type
        if (dto.GiveCurrency == "coins")
            return ExchangeType.Purchase;
        
        // Default to trade
        return ExchangeType.Trade;
    }

    /// <summary>
    /// Parse the cost structure from DTO
    /// </summary>
    private static ExchangeCostStructure ParseCostStructure(ExchangeDTO dto)
    {
        ExchangeCostStructure cost = new ExchangeCostStructure();

        // Convert give currency to resource type
        ResourceType? resourceType = ParseResourceType(dto.GiveCurrency);
        if (resourceType != null && dto.GiveAmount > 0)
        {
            cost.Resources.Add(new ResourceAmount
            {
                Type = resourceType.Value,
                Amount = dto.GiveAmount
            });
        }

        // Add token gates if present
        if (dto.TokenGate != null && dto.TokenGate.Count > 0)
        {
            cost.TokenRequirements = new Dictionary<ConnectionType, int>();
            foreach (var kvp in dto.TokenGate)
            {
                if (Enum.TryParse<ConnectionType>(kvp.Key, true, out ConnectionType tokenType))
                {
                    cost.TokenRequirements[tokenType] = kvp.Value;
                }
            }
        }

        return cost;
    }

    /// <summary>
    /// Parse the reward structure from DTO
    /// </summary>
    private static ExchangeRewardStructure ParseRewardStructure(ExchangeDTO dto)
    {
        ExchangeRewardStructure reward = new ExchangeRewardStructure();

        // Convert receive currency to resource type
        ResourceType? resourceType = ParseResourceType(dto.ReceiveCurrency);
        if (resourceType != null && dto.ReceiveAmount > 0)
        {
            reward.Resources.Add(new ResourceAmount
            {
                Type = resourceType.Value,
                Amount = dto.ReceiveAmount
            });
        }

        return reward;
    }

    /// <summary>
    /// Parse resource type from string
    /// </summary>
    private static ResourceType? ParseResourceType(string currency)
    {
        if (string.IsNullOrEmpty(currency))
            return null;

        return currency.ToLower() switch
        {
            "coins" or "coin" => ResourceType.Coins,
            "food" => ResourceType.Hunger,  // Food maps to Hunger resource
            "health" => ResourceType.Health,
            "attention" or "stamina" => ResourceType.Attention,
            "trust" => ResourceType.TrustToken,
            "commerce" => ResourceType.CommerceToken,
            "status" => ResourceType.StatusToken,
            "shadow" => ResourceType.ShadowToken,
            _ => null
        };
    }

    /// <summary>
    /// Create exchange cards for an NPC based on their personality type
    /// </summary>
    public static List<ExchangeCard> CreateDefaultExchangesForNPC(NPC npc)
    {
        List<ExchangeCard> exchanges = new List<ExchangeCard>();

        // Only mercantile NPCs get default exchanges
        if (npc.PersonalityType != PersonalityType.MERCANTILE)
            return exchanges;

        // Create some default exchanges based on NPC profession
        switch (npc.Profession)
        {
            case Professions.Merchant:
                exchanges.Add(new ExchangeCard
                {
                    Id = $"{npc.ID}_food_purchase",
                    Name = "Buy Food",
                    Description = "Purchase provisions from the merchant",
                    NpcId = npc.ID,
                    ExchangeType = ExchangeType.Purchase,
                    Cost = new ExchangeCostStructure 
                    { 
                        Resources = new List<ResourceAmount>
                        {
                            new ResourceAmount { Type = ResourceType.Coins, Amount = 5 }
                        }
                    },
                    Reward = new ExchangeRewardStructure 
                    { 
                        Resources = new List<ResourceAmount>
                        {
                            new ResourceAmount { Type = ResourceType.Hunger, Amount = 2 }
                        }
                    },
                    SuccessRate = 100
                });
                break;
                
            case Professions.Innkeeper:
                exchanges.Add(new ExchangeCard
                {
                    Id = $"{npc.ID}_rest_service",
                    Name = "Rest at Inn",
                    Description = "Pay for a comfortable rest",
                    NpcId = npc.ID,
                    ExchangeType = ExchangeType.Service,
                    Cost = new ExchangeCostStructure 
                    { 
                        Resources = new List<ResourceAmount>
                        {
                            new ResourceAmount { Type = ResourceType.Coins, Amount = 10 }
                        }
                    },
                    Reward = new ExchangeRewardStructure 
                    { 
                        Resources = new List<ResourceAmount>
                        {
                            new ResourceAmount { Type = ResourceType.Health, Amount = 3 },
                            new ResourceAmount { Type = ResourceType.Attention, Amount = 2 }
                        }
                    },
                    SuccessRate = 100
                });
                break;
        }

        return exchanges;
    }
}