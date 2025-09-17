using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.Subsystems.ResourceSubsystem;
using Wayfarer.Subsystems.TokenSubsystem;

namespace Wayfarer.Subsystems.ExchangeSubsystem
{
    /// <summary>
    /// Processes exchange execution, applying costs and rewards.
    /// Internal to the Exchange subsystem - not exposed publicly.
    /// </summary>
    public class ExchangeProcessor
    {
        private readonly GameWorld _gameWorld;
        private readonly ResourceFacade _resourceFacade;
        private readonly TokenFacade _tokenFacade;
        private readonly TimeManager _timeManager;
        private readonly MessageSystem _messageSystem;

        public ExchangeProcessor(
            GameWorld gameWorld,
            ResourceFacade resourceFacade,
            TokenFacade tokenFacade,
            TimeManager timeManager,
            MessageSystem messageSystem)
        {
            _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
            _resourceFacade = resourceFacade ?? throw new ArgumentNullException(nameof(resourceFacade));
            _tokenFacade = tokenFacade ?? throw new ArgumentNullException(nameof(tokenFacade));
            _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
            _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        }

        /// <summary>
        /// Process an exchange, applying all costs and rewards
        /// </summary>
        public async Task<ExchangeResult> ProcessExchange(ExchangeData exchange, NPC npc)
        {
            ExchangeResult result = new ExchangeResult
            {
                Success = false,
                CostsApplied = new Dictionary<ResourceType, int>(),
                RewardsGranted = new Dictionary<ResourceType, int>(),
                SideEffects = new List<string>()
            };

            try
            {
                // Apply all costs
                if (!ApplyCosts(exchange, npc, result))
                {
                    // Rollback any partial costs if something fails
                    RollbackCosts(result);
                    result.Message = "Failed to apply exchange costs";
                    return result;
                }

                // Apply all rewards
                ApplyRewards(exchange, npc, result);

                // Process side effects
                await ProcessSideEffects(exchange, npc, result);

                // Generate success message
                result.Success = true;
                result.Message = GenerateSuccessMessage(exchange, npc, result);

                // Send success notification
                _messageSystem.AddSystemMessage(result.Message, SystemMessageTypes.Success);

                // Track exchange completion
                exchange.TimesUsed++;

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ExchangeProcessor] Error processing exchange: {ex.Message}");

                // Attempt rollback on error
                RollbackCosts(result);

                result.Success = false;
                result.Message = $"Exchange failed: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Apply all costs of an exchange
        /// </summary>
        private bool ApplyCosts(ExchangeData exchange, NPC npc, ExchangeResult result)
        {
            Player player = _gameWorld.GetPlayer();

            foreach (ResourceAmount cost in exchange.Costs)
            {
                bool applied = ApplySingleCost(cost, npc, player);
                if (!applied)
                {
                    Console.WriteLine($"[ExchangeProcessor] Failed to apply cost: {cost.Type} x{cost.Amount}");
                    return false;
                }

                // Track what was applied for potential rollback
                result.CostsApplied[cost.Type] = cost.Amount;
            }

            return true;
        }

        /// <summary>
        /// Apply a single cost
        /// </summary>
        private bool ApplySingleCost(ResourceAmount cost, NPC npc, Player player)
        {
            switch (cost.Type)
            {
                case ResourceType.Coins:
                    if (!_resourceFacade.CanAfford(cost.Amount))
                        return false;
                    return _resourceFacade.SpendCoins(cost.Amount, $"Exchange with {npc.Name}");

                case ResourceType.Health:
                    if (_resourceFacade.GetHealth() < cost.Amount)
                        return false;
                    _resourceFacade.TakeDamage(cost.Amount, $"Exchange with {npc.Name}");
                    return true;

                case ResourceType.Attention:
                    TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
                    if (!_resourceFacade.CanAffordAttention(cost.Amount, currentTimeBlock))
                        return false;
                    return _resourceFacade.SpendAttention(cost.Amount, currentTimeBlock, $"Exchange with {npc.Name}");

                case ResourceType.TrustToken:
                    if (!_tokenFacade.HasTokens(ConnectionType.Trust, cost.Amount))
                        return false;
                    return _tokenFacade.SpendTokensWithNPC(ConnectionType.Trust, cost.Amount, npc.ID);

                case ResourceType.CommerceToken:
                    if (!_tokenFacade.HasTokens(ConnectionType.Commerce, cost.Amount))
                        return false;
                    return _tokenFacade.SpendTokensWithNPC(ConnectionType.Commerce, cost.Amount, npc.ID);

                case ResourceType.StatusToken:
                    if (!_tokenFacade.HasTokens(ConnectionType.Status, cost.Amount))
                        return false;
                    return _tokenFacade.SpendTokensWithNPC(ConnectionType.Status, cost.Amount, npc.ID);

                case ResourceType.ShadowToken:
                    if (!_tokenFacade.HasTokens(ConnectionType.Shadow, cost.Amount))
                        return false;
                    return _tokenFacade.SpendTokensWithNPC(ConnectionType.Shadow, cost.Amount, npc.ID);

                case ResourceType.Item:
                    // Handle item costs (need item ID from exchange data)
                    if (!string.IsNullOrEmpty(cost.ItemId))
                    {
                        if (!_resourceFacade.HasItem(cost.ItemId))
                            return false;
                        return _resourceFacade.RemoveItem(cost.ItemId);
                    }
                    return false;

                default:
                    Console.WriteLine($"[ExchangeProcessor] Unknown cost type: {cost.Type}");
                    return true; // Ignore unknown types
            }
        }

        /// <summary>
        /// Apply all rewards of an exchange
        /// </summary>
        private void ApplyRewards(ExchangeData exchange, NPC npc, ExchangeResult result)
        {
            foreach (ResourceAmount reward in exchange.Rewards)
            {
                ApplySingleReward(reward, npc);
                result.RewardsGranted[reward.Type] = reward.Amount;
            }

            // Grant item rewards
            foreach (string itemId in exchange.ItemRewards)
            {
                if (_resourceFacade.AddItem(itemId))
                {
                    result.ItemsGranted.Add(itemId);
                }
                else
                {
                    Console.WriteLine($"[ExchangeProcessor] Failed to grant item: {itemId} (inventory full?)");
                }
            }
        }

        /// <summary>
        /// Apply a single reward
        /// </summary>
        private void ApplySingleReward(ResourceAmount reward, NPC npc)
        {
            switch (reward.Type)
            {
                case ResourceType.Coins:
                    _resourceFacade.AddCoins(reward.Amount, $"Exchange with {npc.Name}");
                    break;

                case ResourceType.Health:
                    _resourceFacade.Heal(reward.Amount, $"Exchange with {npc.Name}");
                    break;

                case ResourceType.Hunger:
                    // Hunger reward means reducing hunger (feeding)
                    _resourceFacade.DecreaseHunger(reward.Amount, $"Exchange with {npc.Name}");
                    break;

                case ResourceType.Attention:
                    // Can't directly add attention through ResourceFacade, would need enhancement
                    Console.WriteLine($"[ExchangeProcessor] Attention reward not implemented: +{reward.Amount}");
                    break;

                case ResourceType.TrustToken:
                    _tokenFacade.AddTokensToNPC(ConnectionType.Trust, reward.Amount, npc.ID);
                    break;

                case ResourceType.CommerceToken:
                    _tokenFacade.AddTokensToNPC(ConnectionType.Commerce, reward.Amount, npc.ID);
                    break;

                case ResourceType.StatusToken:
                    _tokenFacade.AddTokensToNPC(ConnectionType.Status, reward.Amount, npc.ID);
                    break;

                case ResourceType.ShadowToken:
                    _tokenFacade.AddTokensToNPC(ConnectionType.Shadow, reward.Amount, npc.ID);
                    break;

                default:
                    Console.WriteLine($"[ExchangeProcessor] Unknown reward type: {reward.Type}");
                    break;
            }
        }

        /// <summary>
        /// Process any side effects of the exchange
        /// </summary>
        private async Task ProcessSideEffects(ExchangeData exchange, NPC npc, ExchangeResult result)
        {
            // Time advancement for work-like exchanges
            if (ShouldAdvanceTime(exchange))
            {
                int segmentsToAdvance = CalculateTimeAdvancement(exchange);
                _timeManager.AdvanceSegments(segmentsToAdvance);
                result.SideEffects.Add($"Time advanced by {segmentsToAdvance} segment(s)");
                _messageSystem.AddSystemMessage($"Time passes... ({segmentsToAdvance} segment(s))", SystemMessageTypes.Info);
            }

            // Relationship effects
            if (exchange.AffectsRelationship)
            {
                ProcessRelationshipEffects(exchange, npc, result);
            }

            // NPC patience effects
            if (exchange.ConsumesPatience)
            {
                npc.SpendPatience(exchange.PatienceCost);
                result.SideEffects.Add($"{npc.Name} patience reduced");
            }

            // Unlock new exchanges
            if (!string.IsNullOrEmpty(exchange.UnlocksExchangeId))
            {
                result.SideEffects.Add($"New exchange unlocked: {exchange.UnlocksExchangeId}");
            }

            // Story triggers
            if (!string.IsNullOrEmpty(exchange.TriggerEvent))
            {
                result.SideEffects.Add($"Story event triggered: {exchange.TriggerEvent}");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Rollback costs if exchange fails
        /// </summary>
        private void RollbackCosts(ExchangeResult result)
        {
            Player player = _gameWorld.GetPlayer();

            foreach (KeyValuePair<ResourceType, int> cost in result.CostsApplied)
            {
                switch (cost.Key)
                {
                    case ResourceType.Coins:
                        _resourceFacade.AddCoins(cost.Value, "Exchange rollback");
                        break;

                    case ResourceType.Health:
                        _resourceFacade.Heal(cost.Value, "Exchange rollback");
                        break;

                    // Token rollbacks would need to be handled through TokenFacade
                    // Items would need to be re-added through ResourceFacade

                    default:
                        Console.WriteLine($"[ExchangeProcessor] Cannot rollback {cost.Key}");
                        break;
                }
            }

            _messageSystem.AddSystemMessage("Exchange cancelled, costs refunded", SystemMessageTypes.Warning);
        }

        /// <summary>
        /// Check if exchange should advance time
        /// </summary>
        private bool ShouldAdvanceTime(ExchangeData exchange)
        {
            // Work-like exchanges that cost significant attention advance time
            return exchange.AdvancesTime ||
                   exchange.Costs.Any(c => c.Type == ResourceType.Attention && c.Amount >= 3);
        }

        /// <summary>
        /// Calculate how much time to advance
        /// </summary>
        private int CalculateTimeAdvancement(ExchangeData exchange)
        {
            if (exchange.TimeAdvancementHours > 0)
            {
                return exchange.TimeAdvancementHours;
            }

            // Default: 1 segment per 3 attention spent
            int attentionCost = exchange.Costs
                .Where(c => c.Type == ResourceType.Attention)
                .Sum(c => c.Amount);

            return Math.Max(1, attentionCost / 3);
        }

        /// <summary>
        /// Process relationship changes from exchange
        /// </summary>
        private void ProcessRelationshipEffects(ExchangeData exchange, NPC npc, ExchangeResult result)
        {
            // Successful exchanges improve flow
            if (exchange.FlowModifier != 0)
            {
                npc.RelationshipFlow += exchange.FlowModifier;
                result.SideEffects.Add($"Relationship with {npc.Name} {(exchange.FlowModifier > 0 ? "improved" : "worsened")}");
            }

            // Some exchanges might affect connection state directly
            if (exchange.ConnectionStateChange.HasValue)
            {
                // This would need integration with conversation system
                result.SideEffects.Add($"Connection state changed to {exchange.ConnectionStateChange.Value}");
            }
        }

        /// <summary>
        /// Generate a success message for the exchange
        /// </summary>
        private string GenerateSuccessMessage(ExchangeData exchange, NPC npc, ExchangeResult result)
        {
            string baseName = !string.IsNullOrEmpty(exchange.ExchangeName)
                ? exchange.ExchangeName
                : "exchange";

            string message = $"Completed {baseName} with {npc.Name}";

            // Add cost summary
            if (result.CostsApplied.Any())
            {
                List<string> costs = result.CostsApplied
                    .Select(c => $"{c.Value} {GetResourceDisplayName(c.Key)}")
                    .ToList();
                message += $" (Paid: {string.Join(", ", costs)})";
            }

            // Add reward summary
            if (result.RewardsGranted.Any() || result.ItemsGranted.Any())
            {
                List<string> rewards = result.RewardsGranted
                    .Select(r => $"{r.Value} {GetResourceDisplayName(r.Key)}")
                    .ToList();
                rewards.AddRange(result.ItemsGranted);
                message += $" (Received: {string.Join(", ", rewards)})";
            }

            return message;
        }

        /// <summary>
        /// Get display name for a resource type
        /// </summary>
        private string GetResourceDisplayName(ResourceType type)
        {
            return type switch
            {
                ResourceType.Coins => "coins",
                ResourceType.Health => "health",
                ResourceType.Hunger => "food",
                ResourceType.Attention => "attention",
                ResourceType.TrustToken => "trust tokens",
                ResourceType.CommerceToken => "commerce tokens",
                ResourceType.StatusToken => "status tokens",
                ResourceType.ShadowToken => "shadow tokens",
                ResourceType.Item => "items",
                _ => type.ToString().ToLower()
            };
        }
    }
}