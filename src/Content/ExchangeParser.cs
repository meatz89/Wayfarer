/// <summary>
/// Parses exchange DTOs into ExchangeCard entities.
/// Converts JSON exchange definitions into strongly-typed domain objects.
/// </summary>
public static class ExchangeParser
{
    /// <summary>
    /// Parse a single ExchangeDTO into an ExchangeCard
    /// </summary>
    public static ExchangeCard ParseExchange(ExchangeDTO dto, string npcId, GameWorld gameWorld)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // VALIDATION: Name is REQUIRED field
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"Exchange '{dto.Id}' missing required field 'name'");

        // Verify NPC exists and resolve object reference
        NPC npc = gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            throw new InvalidOperationException(
                $"Exchange '{dto.Id}' references unknown NPC '{npcId}'");
        }

        ExchangeCard card = new ExchangeCard
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = GenerateDescription(dto),
            // HIGHLANDER Sub-Pattern A: Store both ID (for persistence) and Object (for runtime)
            NpcId = npcId,  // From JSON, enables save/load
            Npc = npc,  // Resolved once at parse-time, cached for runtime (avoids repeated lookups)

            // Default to trade type
            ExchangeType = dto.GiveCurrency == "coins" ? ExchangeType.Purchase : ExchangeType.Trade,

            // Parse cost structure
            Cost = new ExchangeCostStructure
            {
                Resources = dto.GiveAmount > 0 ? new List<ResourceAmount>
            {
                new ResourceAmount
                {
                    Type = (dto.GiveCurrency ?? "coins").ToLower() switch // Optional - defaults to "coins" if missing
                    {
                        "coins" or "coin" => ResourceType.Coins,
                        "food" => ResourceType.Hunger,
                        "health" => ResourceType.Health,
                        "trust" => ResourceType.TrustToken,
                        "diplomacy" => ResourceType.DiplomacyToken,
                        "status" => ResourceType.StatusToken,
                        "shadow" => ResourceType.ShadowToken,
                        _ => ResourceType.Coins
                    },
                    Amount = dto.GiveAmount
                }
            } : new List<ResourceAmount>(),
                TokenRequirements = dto.TokenGate?.Count > 0 ? new Dictionary<ConnectionType, int>() : null,
                ConsumedItemIds = new List<string>() // DEPRECATED: consumedItems never appears in JSON (0% frequency)
            },

            // Parse reward structure
            Reward = new ExchangeRewardStructure
            {
                Resources = dto.ReceiveAmount > 0 ? new List<ResourceAmount>
            {
                new ResourceAmount
                {
                    Type = (dto.ReceiveCurrency ?? "coins").ToLower() switch // Optional - defaults to "coins" if missing
                    {
                        "coins" or "coin" => ResourceType.Coins,
                        "food" => ResourceType.Hunger,
                        "health" => ResourceType.Health,
                        "trust" => ResourceType.TrustToken,
                        "diplomacy" => ResourceType.DiplomacyToken,
                        "status" => ResourceType.StatusToken,
                        "shadow" => ResourceType.ShadowToken,
                        "weight_reduction" => ResourceType.CarryingCapacity,
                        "item" => ResourceType.Item,
                        _ => ResourceType.Coins
                    },
                    Amount = dto.ReceiveAmount
                }
            } : new List<ResourceAmount>(),

                // Handle item rewards (support both legacy single item and new multi-item)
                ItemIds = MergeItemRewards(dto)
            },

            // Default properties
            SingleUse = false,
            SuccessRate = 100,
            IsCompleted = false
        };

        return card;
    }

    /// <summary>
    /// Generate a description from the exchange DTO
    /// </summary>
    private static string GenerateDescription(ExchangeDTO dto)
    {
        string receiveText = !string.IsNullOrEmpty(dto.ReceiveItem)
            ? dto.ReceiveItem
            : $"{dto.ReceiveAmount} {dto.ReceiveCurrency ?? "coins"}"; // Optional - defaults to "coins" if missing

        return $"Trade {dto.GiveAmount} {dto.GiveCurrency ?? "coins"} for {receiveText}"; // Optional - defaults to "coins" if missing
    }

    /// <summary>
    /// Merge legacy single item reward (ReceiveItem) with new multi-item rewards (GrantedItems)
    /// </summary>
    private static List<string> MergeItemRewards(ExchangeDTO dto)
    {
        List<string> items = new List<string>();

        // Support legacy single item field
        if (!string.IsNullOrEmpty(dto.ReceiveItem))
            items.Add(dto.ReceiveItem);

        // Support new multi-item field
        if (dto.GrantedItems != null && dto.GrantedItems.Any())
            items.AddRange(dto.GrantedItems);

        return items;
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
                    Name = "Buy Hunger",
                    Description = "Purchase provisions from the merchant",
                    // HIGHLANDER Sub-Pattern A: Store both ID (for persistence) and Object (for runtime)
                    NpcId = npc.ID,
                    Npc = npc,
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
                    // HIGHLANDER Sub-Pattern A: Store both ID (for persistence) and Object (for runtime)
                    NpcId = npc.ID,
                    Npc = npc,
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
                    }
                    },
                    SuccessRate = 100
                });
                break;
        }

        return exchanges;
    }
}