using System;
using System.Collections.Generic;
using System.Linq;

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
    public bool ExecuteExchange(ExchangeData exchange, NPC npc, Player player, PlayerResourceState playerResources)
    {
        Console.WriteLine($"[ExchangeHandler] Executing exchange: {exchange?.ExchangeName ?? "NULL"}");
        if (exchange != null)
        {
            Console.WriteLine($"[ExchangeHandler] Exchange Cost: {string.Join(", ", exchange.Costs.Select(c => $"{c.Type}={c.Amount}"))}");
            Console.WriteLine($"[ExchangeHandler] Exchange Reward: {string.Join(", ", exchange.Rewards.Select(r => $"{r.Type}={r.Amount}"))}");
            Console.WriteLine($"[ExchangeHandler] Player Resources: Coins={playerResources.Coins}, Health={playerResources.Health}");
        }

        if (exchange == null)
        {
            Console.WriteLine("[ExchangeHandler] Exchange is null");
            return false;
        }

        // Validate player can afford
        if (!CanAffordExchange(exchange, playerResources))
        {
            Console.WriteLine($"[ExchangeHandler] Player cannot afford exchange");
            _messageSystem.AddSystemMessage("You don't have enough resources for this exchange", SystemMessageTypes.Warning);
            return false;
        }

        // Apply costs
        if (!ApplyCosts(exchange, player, npc))
        {
            Console.WriteLine("[ExchangeHandler] Failed to apply costs");
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

        Console.WriteLine($"[ExchangeHandler] Successfully completed exchange {exchange.ExchangeName}");
        return true;
    }

    /// <summary>
    /// Validate if player can afford exchange
    /// </summary>
    public bool CanAffordExchange(ExchangeData exchange, PlayerResourceState playerResources)
    {
        return exchange.CanAfford(playerResources, _tokenManager);
    }

    /// <summary>
    /// Get available exchanges for NPC at location
    /// </summary>
    public List<ExchangeOption> GetAvailableExchanges(NPC npc, List<string> spotDomainTags, PlayerResourceState playerResources)
    {
        List<ExchangeOption> exchanges = new List<ExchangeOption>();

        if (npc.ExchangeDeck == null || !npc.ExchangeDeck.Any())
            return exchanges;

        // Get current location and time for availability check
        string currentLocationId = spotDomainTags?.FirstOrDefault() ?? string.Empty;
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();

        foreach (ExchangeCard card in npc.ExchangeDeck)
        {
            // Check if exchange is available at this location and time
            if (!card.IsAvailable(currentLocationId, currentTimeBlock))
                continue;

            // Check token requirements (minimum tokens required to even see the exchange)
            if (!CheckTokenRequirements(card, npc))
                continue;

            // Convert ExchangeCard to ExchangeData for compatibility
            ExchangeData exchange = ConvertToExchangeData(card);
            bool canAfford = CanAffordExchange(exchange, playerResources);

            exchanges.Add(new ExchangeOption
            {
                ExchangeId = card.Id,
                Name = card.Name ?? GetExchangeName(exchange),
                Description = card.Description,
                Cost = FormatCost(exchange.GetCostAsList()),
                Reward = FormatReward(exchange.GetRewardAsList()),
                CanAfford = canAfford,
                ExchangeData = exchange
            });
        }

        return exchanges;
    }

    /// <summary>
    /// Apply exchange costs to player
    /// </summary>
    private bool ApplyCosts(ExchangeData exchange, Player player, NPC npc)
    {
        foreach (ResourceAmount cost in exchange.Costs)
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
                    if (!_tokenManager.SpendTokens(ConnectionType.Trust, cost.Amount, npc.ID))
                        return false;
                    break;

                case ResourceType.DiplomacyToken:
                    if (!_tokenManager.SpendTokens(ConnectionType.Diplomacy, cost.Amount, npc.ID))
                        return false;
                    break;

                case ResourceType.StatusToken:
                    if (!_tokenManager.SpendTokens(ConnectionType.Status, cost.Amount, npc.ID))
                        return false;
                    break;

                case ResourceType.ShadowToken:
                    if (!_tokenManager.SpendTokens(ConnectionType.Shadow, cost.Amount, npc.ID))
                        return false;
                    break;
            }
        }

        return true;
    }

    /// <summary>
    /// Apply exchange rewards to player
    /// </summary>
    private void ApplyRewards(ExchangeData exchange, Player player, NPC npc)
    {
        foreach (ResourceAmount reward in exchange.Rewards)
        {
            switch (reward.Type)
            {
                case ResourceType.Coins:
                    player.Coins += reward.Amount;
                    break;

                case ResourceType.Health:
                    player.Health = Math.Min(100, player.Health + reward.Amount);
                    break;

                case ResourceType.Hunger:
                    // Hunger maps to Hunger (0 = not hungry, 100 = very hungry)
                    player.Hunger = Math.Max(0, Math.Min(100, player.Hunger - reward.Amount));
                    break;


                case ResourceType.TrustToken:
                    _tokenManager.AddTokensToNPC(ConnectionType.Trust, reward.Amount, npc.ID);
                    break;

                case ResourceType.DiplomacyToken:
                    _tokenManager.AddTokensToNPC(ConnectionType.Diplomacy, reward.Amount, npc.ID);
                    break;

                case ResourceType.StatusToken:
                    _tokenManager.AddTokensToNPC(ConnectionType.Status, reward.Amount, npc.ID);
                    break;

                case ResourceType.ShadowToken:
                    _tokenManager.AddTokensToNPC(ConnectionType.Shadow, reward.Amount, npc.ID);
                    break;
            }
        }
    }

    /// <summary>
    /// Convert ExchangeCard to ExchangeData for compatibility
    /// </summary>
    private ExchangeData ConvertToExchangeData(ExchangeCard card)
    {
        ExchangeData data = new ExchangeData
        {
            ExchangeName = card.Name,
            TemplateId = card.Id,
            Costs = new List<ResourceAmount>(),
            Rewards = new List<ResourceAmount>()
        };

        // Convert cost structure - copy resource list
        if (card.Cost?.Resources != null)
        {
            data.Costs = new List<ResourceAmount>(card.Cost.Resources);
        }

        // Convert reward structure - copy resource list
        if (card.Reward?.Resources != null)
        {
            data.Rewards = new List<ResourceAmount>(card.Reward.Resources);
        }

        return data;
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
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(npc.ID);

        foreach (KeyValuePair<ConnectionType, int> tokenReq in card.Cost.TokenRequirements)
        {
            int currentTokens = npcTokens.ContainsKey(tokenReq.Key)
                ? npcTokens[tokenReq.Key]
                : 0;
            if (currentTokens < tokenReq.Value)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get friendly name for exchange
    /// </summary>
    private string GetExchangeName(ExchangeData exchange)
    {
        if (!string.IsNullOrEmpty(exchange.ExchangeName))
            return exchange.ExchangeName;

        return exchange.TemplateId switch
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
    private bool ShouldAdvanceTime(ExchangeData exchange)
    {
        // Simple work exchanges advance time
        return false;
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
/// Represents an available exchange option
/// </summary>
public class ExchangeOption
{
    public string ExchangeId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Cost { get; set; }
    public string Reward { get; set; }
    public bool CanAfford { get; set; }
    public ExchangeData ExchangeData { get; set; }
    public ExchangeValidationResult ValidationResult { get; set; }
}

/// <summary>
