using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles exchange system for Commerce conversations.
/// Manages resource trades and exchange validation.
/// </summary>
public class ExchangeHandler
{
    private readonly TimeManager _timeManager;
    private readonly TimeBlockAttentionManager _timeBlockAttentionManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly MessageSystem _messageSystem;

    public ExchangeHandler(
        TimeManager timeManager,
        TimeBlockAttentionManager timeBlockAttentionManager,
        TokenMechanicsManager tokenManager,
        MessageSystem messageSystem)
    {
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _timeBlockAttentionManager = timeBlockAttentionManager ?? throw new ArgumentNullException(nameof(timeBlockAttentionManager));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }

    /// <summary>
    /// Execute an exchange with an NPC
    /// </summary>
    public bool ExecuteExchange(ExchangeData exchange, NPC npc, Player player, PlayerResourceState playerResources)
    {
        Console.WriteLine($"[ExchangeHandler] Executing exchange: {exchange?.ExchangeName ?? "NULL"}");

        if (exchange == null)
        {
            Console.WriteLine("[ExchangeHandler] Exchange is null");
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
            _timeManager.AdvanceTime(1);
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
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        AttentionManager currentAttentionManager = _timeBlockAttentionManager.GetCurrentAttention(currentTimeBlock);
        int currentAttention = currentAttentionManager?.Current ?? 0;

        return exchange.CanAfford(playerResources, _tokenManager, currentAttention);
    }

    /// <summary>
    /// Get available exchanges for NPC at location
    /// </summary>
    public List<ExchangeOption> GetAvailableExchanges(NPC npc, List<string> spotDomainTags, PlayerResourceState playerResources)
    {
        List<ExchangeOption> exchanges = new List<ExchangeOption>();

        if (npc.ExchangeDeck == null || !npc.ExchangeDeck.HasCardsAvailable())
            return exchanges;

        foreach (ConversationCard card in npc.ExchangeDeck.GetAllCards())
        {
            if (card.Context?.ExchangeData == null)
                continue;

            // Check domain requirements
            if (!CheckDomainRequirements(card, spotDomainTags))
                continue;

            ExchangeData exchange = card.Context.ExchangeData;
            bool canAfford = CanAffordExchange(exchange, playerResources);

            exchanges.Add(new ExchangeOption
            {
                ExchangeId = card.TemplateId,
                Name = exchange.ExchangeName ?? GetExchangeName(exchange),
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
        foreach (KeyValuePair<ResourceType, int> cost in exchange.Cost)
        {
            switch (cost.Key)
            {
                case ResourceType.Coins:
                    if (player.Coins < cost.Value)
                        return false;
                    player.Coins -= cost.Value;
                    break;

                case ResourceType.Health:
                    if (player.Health < cost.Value)
                        return false;
                    player.Health -= cost.Value;
                    break;

                case ResourceType.Attention:
                    TimeBlocks timeBlock = _timeManager.GetCurrentTimeBlock();
                    AttentionManager attentionMgr = _timeBlockAttentionManager.GetCurrentAttention(timeBlock);
                    if (!attentionMgr.TrySpend(cost.Value))
                        return false;
                    break;

                case ResourceType.TrustToken:
                    if (!_tokenManager.SpendTokens(ConnectionType.Trust, cost.Value, npc.ID))
                        return false;
                    break;

                case ResourceType.CommerceToken:
                    if (!_tokenManager.SpendTokens(ConnectionType.Commerce, cost.Value, npc.ID))
                        return false;
                    break;

                case ResourceType.StatusToken:
                    if (!_tokenManager.SpendTokens(ConnectionType.Status, cost.Value, npc.ID))
                        return false;
                    break;

                case ResourceType.ShadowToken:
                    if (!_tokenManager.SpendTokens(ConnectionType.Shadow, cost.Value, npc.ID))
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
        foreach (KeyValuePair<ResourceType, int> reward in exchange.Reward)
        {
            switch (reward.Key)
            {
                case ResourceType.Coins:
                    player.Coins += reward.Value;
                    break;

                case ResourceType.Health:
                    player.Health = Math.Min(100, player.Health + reward.Value);
                    break;

                case ResourceType.Hunger:
                    // Hunger maps to Food (0 = not hungry, 100 = very hungry)
                    player.Food = Math.Max(0, Math.Min(100, player.Food - reward.Value));
                    break;

                case ResourceType.Attention:
                    TimeBlocks timeBlock = _timeManager.GetCurrentTimeBlock();
                    AttentionManager attentionMgr = _timeBlockAttentionManager.GetCurrentAttention(timeBlock);
                    attentionMgr.AddAttention(reward.Value);
                    break;

                case ResourceType.TrustToken:
                    _tokenManager.AddTokensToNPC(ConnectionType.Trust, reward.Value, npc.ID);
                    break;

                case ResourceType.CommerceToken:
                    _tokenManager.AddTokensToNPC(ConnectionType.Commerce, reward.Value, npc.ID);
                    break;

                case ResourceType.StatusToken:
                    _tokenManager.AddTokensToNPC(ConnectionType.Status, reward.Value, npc.ID);
                    break;

                case ResourceType.ShadowToken:
                    _tokenManager.AddTokensToNPC(ConnectionType.Shadow, reward.Value, npc.ID);
                    break;
            }
        }
    }

    /// <summary>
    /// Check if exchange meets domain requirements
    /// </summary>
    private bool CheckDomainRequirements(ConversationCard card, List<string> spotDomains)
    {
        // Domain filtering removed - CardContext doesn't have RequiredDomains
        return true; // No domain filtering for now
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
        // Work exchanges that cost significant attention advance time
        return exchange.Cost.Any(c => c.Key == ResourceType.Attention && c.Value >= 3);
    }

    /// <summary>
    /// Format cost for display
    /// </summary>
    private string FormatCost(List<ResourceExchange> costs)
    {
        IEnumerable<string> parts = costs.Select(c => $"{c.Amount} {GetResourceName(c.ResourceType)}");
        return string.Join(", ", parts);
    }

    /// <summary>
    /// Format reward for display
    /// </summary>
    private string FormatReward(List<ResourceExchange> rewards)
    {
        IEnumerable<string> parts = rewards.Select(r =>
        {
            if (r.IsAbsolute)
                return $"Set {GetResourceName(r.ResourceType)} to {r.Amount}";
            else
                return $"{r.Amount} {GetResourceName(r.ResourceType)}";
        });
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
            ResourceType.Attention => "attention",
            ResourceType.TrustToken => "trust",
            ResourceType.CommerceToken => "commerce",
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
}