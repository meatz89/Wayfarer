/// <summary>
/// Parses exchange DTOs into ExchangeCard entities.
/// Converts JSON exchange definitions into strongly-typed domain objects.
/// </summary>
public static class ExchangeParser
{
    /// <summary>
    /// Parse a single ExchangeDTO into an ExchangeCard
    /// Uses EntityResolver.FindOrCreate for categorical NPC resolution (DDR-006)
    /// </summary>
    public static ExchangeCard ParseExchange(ExchangeDTO dto, EntityResolver entityResolver)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // VALIDATION: Name is REQUIRED field
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"Exchange '{dto.Name}' missing required field 'name'");

        // VALIDATION: ProviderFilter is REQUIRED field
        if (dto.ProviderFilter == null)
            throw new InvalidOperationException($"Exchange '{dto.Name}' missing required 'providerFilter' field");

        // EntityResolver.FindOrCreate pattern - categorical entity resolution
        PlacementFilter providerFilter = SceneTemplateParser.ParsePlacementFilter(dto.ProviderFilter, $"Exchange:{dto.Name}");
        NPC npc = entityResolver.FindOrCreateNPC(providerFilter);

        ExchangeCard card = new ExchangeCard
        {
            Name = dto.Name,
            Description = GenerateDescription(dto),
            // HIGHLANDER: Object reference only (no ID string)
            Npc = npc,  // Categorical resolution via EntityResolver.FindOrCreate

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
                TokenRequirements = dto.TokenGate?.Count > 0 ? new List<TokenCount>() : new List<TokenCount>(),
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
                    Name = "Buy Hunger",
                    Description = "Purchase provisions from the merchant",
                    // HIGHLANDER: Object reference only
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
                    Name = "Rest at Inn",
                    Description = "Pay for a comfortable rest",
                    // HIGHLANDER: Object reference only
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