/// <summary>
/// Handles exchange system for Diplomacy conversations.
/// Manages resource trades and exchange validation.
/// </summary>
public class ExchangeHandler
{
    private readonly TimeManager _timeManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly MessageSystem _messageSystem;

    public ExchangeHandler(
        TimeManager timeManager,
        TokenMechanicsManager tokenManager,
        MessageSystem messageSystem)
    {
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }

    /// <summary>
    /// Execute an exchange with an NPC
    /// </summary>
    public bool ExecuteExchange(ExchangeCard exchange, NPC npc, Player player, PlayerResourceState playerResources, GameWorld gameWorld)
    {
        if (exchange != null)
        { }

        if (exchange == null)
        {
            return false;
        }

        // Validate player can afford
        if (!CanAffordExchange(exchange, playerResources))
        {
            _messageSystem.AddSystemMessage("You don't have enough resources for this exchange", SystemMessageTypes.Warning);
            return false;
        }

        // Apply costs
        if (!ApplyCosts(exchange, player, npc))
        {
            return false;
        }

        // Apply rewards
        ApplyRewards(exchange, player, npc);

        // Generate success message
        string exchangeName = GetExchangeName(exchange);
        _messageSystem.AddSystemMessage($"You {exchangeName} with {npc.Name}", SystemMessageTypes.Success);

        // Handle time advancement for work exchanges
        if (ShouldAdvanceTime(exchange))
        {
            _timeManager.AdvanceSegments(1);
            _messageSystem.AddSystemMessage("Time passes as you work...", SystemMessageTypes.Info);
        }
        exchange.RecordUse();
        return true;
    }

    /// <summary>
    /// Validate if player can afford exchange
    /// </summary>
    public bool CanAffordExchange(ExchangeCard exchange, PlayerResourceState playerResources)
    {
        return exchange.CanAfford(playerResources);
    }

    /// <summary>
    /// Get available exchanges for NPC at location
    /// HIGHLANDER: Accepts Venue object, not string identifier
    /// </summary>
    public List<ExchangeOption> GetAvailableExchanges(NPC npc, Venue currentVenue, PlayerResourceState playerResources)
    {
        List<ExchangeOption> exchanges = new List<ExchangeOption>();

        if (npc.ExchangeDeck == null || !npc.ExchangeDeck.Any())
            return exchanges;

        // Get current time for availability check
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();

        foreach (ExchangeCard card in npc.ExchangeDeck)
        {
            // Check if exchange is available at this Venue and time
            if (!card.IsAvailable(currentVenue, currentTimeBlock))
                continue;

            // Check token requirements (minimum tokens required to even see the exchange)
            if (!CheckTokenRequirements(card, npc))
                continue;

            bool canAfford = CanAffordExchange(card, playerResources);

            exchanges.Add(new ExchangeOption
            {
                ExchangeId = card.Name,  // HIGHLANDER: Use Name as natural key (no separate ID)
                Name = card.Name ?? GetExchangeName(card),
                Description = card.Description,
                Cost = FormatCost(card.GetCostAsList()),
                Reward = FormatReward(card.GetRewardAsList()),
                CanAfford = canAfford,
                ExchangeCard = card
            });
        }

        return exchanges;
    }

    /// <summary>
    /// Apply exchange costs to player
    /// </summary>
    private bool ApplyCosts(ExchangeCard exchange, Player player, NPC npc)
    {
        foreach (ResourceAmount cost in exchange.GetCostAsList())
        {
            switch (cost.Type)
            {
                case ResourceType.Coins:
                    if (player.Coins < cost.Amount)
                        return false;
                    player.Coins -= cost.Amount;
                    break;

                case ResourceType.Health:
                    if (player.Health < cost.Amount)
                        return false;
                    player.Health -= cost.Amount;
                    break;

                case ResourceType.TrustToken:
                    // HIGHLANDER: Pass NPC object directly, not npc.ID
                    if (!_tokenManager.SpendTokens(ConnectionType.Trust, cost.Amount, npc))
                        return false;
                    break;

                case ResourceType.DiplomacyToken:
                    // HIGHLANDER: Pass NPC object directly, not npc.ID
                    if (!_tokenManager.SpendTokens(ConnectionType.Diplomacy, cost.Amount, npc))
                        return false;
                    break;

                case ResourceType.StatusToken:
                    // HIGHLANDER: Pass NPC object directly, not npc.ID
                    if (!_tokenManager.SpendTokens(ConnectionType.Status, cost.Amount, npc))
                        return false;
                    break;

                case ResourceType.ShadowToken:
                    // HIGHLANDER: Pass NPC object directly, not npc.ID
                    if (!_tokenManager.SpendTokens(ConnectionType.Shadow, cost.Amount, npc))
                        return false;
                    break;
            }
        }

        // Apply item costs (consume items from inventory)
        // HIGHLANDER: Use Item objects directly, no string resolution needed
        foreach (Item item in exchange.Cost.ConsumedItems)
        {
            if (!player.Inventory.Contains(item))
            {
                _messageSystem.AddSystemMessage($"Missing required item: {item.Name}", SystemMessageTypes.Danger);
                return false;
            }
            player.Inventory.Remove(item);
        }

        return true;
    }

    /// <summary>
    /// Apply exchange rewards to player
    /// </summary>
    private void ApplyRewards(ExchangeCard exchange, Player player, NPC npc)
    {
        foreach (ResourceAmount reward in exchange.GetRewardAsList())
        {
            switch (reward.Type)
            {
                case ResourceType.Coins:
                    player.Coins += reward.Amount;
                    break;

                case ResourceType.Health:
                    player.Health = Math.Min(player.MaxHealth, player.Health + reward.Amount);
                    break;

                case ResourceType.Hunger:
                    // Hunger maps to Hunger (0 = not hungry, 100 = very hungry)
                    player.Hunger = Math.Max(0, Math.Min(100, player.Hunger - reward.Amount));
                    break;

                case ResourceType.TrustToken:
                    _tokenManager.AddTokensToNPC(ConnectionType.Trust, reward.Amount, npc);
                    break;

                case ResourceType.DiplomacyToken:
                    _tokenManager.AddTokensToNPC(ConnectionType.Diplomacy, reward.Amount, npc);
                    break;

                case ResourceType.StatusToken:
                    _tokenManager.AddTokensToNPC(ConnectionType.Status, reward.Amount, npc);
                    break;

                case ResourceType.ShadowToken:
                    _tokenManager.AddTokensToNPC(ConnectionType.Shadow, reward.Amount, npc);
                    break;
            }
        }

        // Apply item rewards (grant items to inventory)
        // HIGHLANDER: Use Item objects, not string IDs
        foreach (Item item in exchange.GetItemRewards())
        {
            player.Inventory.Add(item);
        }
    }

    /// <summary>
    /// Check if player has the minimum required tokens for a gated exchange
    /// </summary>
    private bool CheckTokenRequirements(ExchangeCard card, NPC npc)
    {
        // If no token requirements, exchange is available
        if (card.Cost?.TokenRequirements == null || card.Cost.TokenRequirements.Count == 0)
            return true;

        // Check if player has required minimum tokens with this NPC
        // HIGHLANDER: Pass NPC object directly, not npc.ID
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(npc);

        foreach (TokenCount tokenReq in card.Cost.TokenRequirements)
        {
            int currentTokens = npcTokens.ContainsKey(tokenReq.Type)
                ? npcTokens[tokenReq.Type]
                : 0;
            if (currentTokens < tokenReq.Count)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get friendly name for exchange
    /// </summary>
    private string GetExchangeName(ExchangeCard exchange)
    {
        if (!string.IsNullOrEmpty(exchange.Name))
            return exchange.Name;

        // HIGHLANDER: ExchangeCard has NO Id property, use Name as natural key
        // Since Name is required, this fallback should never execute
        return exchange.Name switch
        {
            "food_exchange" => "bought travel provisions",
            "healing_exchange" => "received healing",
            "information_exchange" => "traded information",
            "work_exchange" => "helped with work",
            "favor_exchange" => "traded favors",
            "rest_exchange" => "rested",
            "lodging_exchange" => "secured lodging",
            _ => "completed an exchange"
        };
    }

    /// <summary>
    /// Check if exchange should advance time
    /// </summary>
    private bool ShouldAdvanceTime(ExchangeCard exchange)
    {
        return exchange.AdvancesTime;
    }

    /// <summary>
    /// Format cost for display
    /// </summary>
    private string FormatCost(List<ResourceAmount> costs)
    {
        IEnumerable<string> parts = costs.Select(c => $"{c.Amount} {GetResourceName(c.Type)}");
        return string.Join(", ", parts);
    }

    /// <summary>
    /// Format reward for display
    /// </summary>
    private string FormatReward(List<ResourceAmount> rewards)
    {
        IEnumerable<string> parts = rewards.Select(r => $"{r.Amount} {GetResourceName(r.Type)}");
        return string.Join(", ", parts);
    }

    /// <summary>
    /// Get friendly resource name
    /// </summary>
    private string GetResourceName(ResourceType type)
    {
        return type switch
        {
            ResourceType.Coins => "coins",
            ResourceType.Health => "health",
            ResourceType.Hunger => "hunger",
            ResourceType.TrustToken => "trust",
            ResourceType.DiplomacyToken => "diplomacy",
            ResourceType.StatusToken => "status",
            ResourceType.ShadowToken => "shadow",
            _ => type.ToString().ToLower()
        };
    }

}

/// <summary>
/// Represents an available exchange option (DTO for UI display)
/// </summary>
public class ExchangeOption
{
    public string ExchangeId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Cost { get; set; }
    public string Reward { get; set; }
    public bool CanAfford { get; set; }
    public ExchangeCard ExchangeCard { get; set; }
    public ExchangeValidationResult ValidationResult { get; set; }
}

/// <summary>
